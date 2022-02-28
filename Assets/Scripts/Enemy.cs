using UnityEngine;

public class Enemy : GameBehavior {
	
	[SerializeField]
	Transform model = default; //Currently used to turn the model smoothly as it takes corners.
	//So I'll explain how the turn works here.
	// - We try to find the center of the tile we are on,
	// - Set ModelRoot's position there and then rotate around that
	// - Because ModelRoot is a parent of our cube it will drag the cube with it
	//as we rotate.
	
	EnemyFactory originFactory; 
	
	//Movement variables to store tile information.
	//Initialized in SpawnOn
	GameTile tileFrom, tileTo;
	Vector3 positionFrom, positionTo;
	//Changing direction. Instead of snapping to a new direction just change accelerations.
	Direction direction;
	DirectionChange directionChange;
	float directionAngleFrom, directionAngleTo;
	float progress, progressFactor; //progress is how far along we are to our destination
	//progressFactor modifies our speed for unified speed.
	//e.g. turning around will require travelling around a rotation so we change our progressFactor
	//to speed ourselves up to accomodate that.
	float pathOffset; //Now that we offset our cubes randomly from the center of the tile
	//we need to track that in Enemy or we'll step on some toes when we start moving.
	float speed; //In fact, for every range of values we add to the enemyFactory,
	//Enemy.cs will need to track
	
	public EnemyFactory OriginFactory {
		get => originFactory;
		set {
			Debug.Assert(originFactory == null, "Redefined origin factory!");
			//value is an implicit property that setters have that
            originFactory = value;
		}
	}
	
	//Exposes the scale of the Enemy so that the Tower can see how big the radius of
	//the colliders are so it can take that into account when figuring out
	//if something is in its targetingRange or not.
	public float Scale { get; private set; }
	//Expose the health for other systems to use this. Like Tower for example.
	float Health { get; set; }
	
	public void ApplyDamage (float damage) {
		Debug.Assert(damage >= 0f, "Negative damage applied.");
		Health -= damage;
	}
	
	public override bool GameUpdate () {
		if (Health <= 0f) {
			//OriginFactory.Reclaim(this);
			Recycle();
			return false;
		}
		
		//Progress is added by deltaTime unmodified.
		//So our enemies move one tile per second.
		//When progress is just starting out we'll skip the while loop
		//and go directly to transform.localPosition linear interpolation
		//dependant on our progress variable.
		progress += Time.deltaTime * progressFactor;
		//WHILE: 1 second has not passed
		while (progress >= 1f) {
			//With the creation of PrepareOutro the below two lines are obsolete.
			//The tile shifting the lines handled will now be handled by
			//PrepareNextState. You'll find these exact lines there.
			//tileFrom = tileTo;
			//tileTo = tileTo.NextTileOnPath;
			
			//If our next tile is null then we have hit our destination tile.
			if (tileTo == null) {
				//Send a signal to the OriginFactory to reclaim this thing and end its whole career.
				//OriginFactory.Reclaim(this);
				Game.EnemyReachedDestination();
				Recycle();
				return false;
			}
			//Replaced by PrepareNextState
			//positionFrom = positionTo;
			//positionTo = tileFrom.ExitPoint;
			//transform.localRotation = tileFrom.PathDirection.GetRotation();
			
			//leftover progress was applied to the next state. That's what the -= 1f part means.
			//we cannot do that anymore if we want to even out our speed.
			//progress -= 1f;
			//Now we normalize our progress and apply the new factor once we're in the
			//new state.
			progress = (progress - 1f) / progressFactor;
			PrepareNextState();
			progress *= progressFactor;
		}
		if (directionChange == DirectionChange.None) {
			transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, progress);
		}
		//If direction change then interpolate between the two angles and set the rotation.
		//Interpolating the position is osbolete now that the movement is taken care of
		//by rotation.
		//if (directionChange != DirectionChange.None) {
		else {
			float angle = Mathf.LerpUnclamped(directionAngleFrom, directionAngleTo, progress);
			transform.localRotation = Quaternion.Euler(0f, angle, 0f);
		}
		return true;
	}
	
    public void Initialize (float scale, float speed, float pathOffset, float health) {
		Scale = scale;
		model.localScale = new Vector3(scale, scale, scale);
		this.speed = speed;
		this.pathOffset = pathOffset;
		Health = health;//Health = 100f * scale; //Bigger enemy, more health. 100 is the base. Used to be set here but
		//now it's passed in from outside.
	}
	
	public void SpawnOn (GameTile tile) {
		Debug.Assert(tile.NextTileOnPath != null, "Nowhere to go!", this);
		tileFrom = tile;
		tileTo = tile.NextTileOnPath;
		//Replaced by PrepareIntro
		//positionFrom = tileFrom.transform.localPosition;
		//positionTo = tileFrom.ExitPoint;
		//transform.localRotation = tileFrom.PathDirection.GetRotation();
		progress = 0f;
		PrepareIntro();
	}
	
	//Prepare the tiles for spawning from SpawnOn
	//A list of variable declarations.
	void PrepareIntro () {
		positionFrom = tileFrom.transform.localPosition; //Where we used to be (or are now)
		positionTo = tileFrom.ExitPoint; //The position where we are leaving our tile. A half vector between
		//the destination and origin tile.
		direction = tileFrom.PathDirection;
		directionChange = DirectionChange.None; //Whether we are undergoing a change in direction and will
		//need to prepare for a turn.
		directionAngleFrom = directionAngleTo = direction.GetAngle();
		model.localPosition = new Vector3(pathOffset, 0f);
		transform.localRotation = direction.GetRotation();
		progressFactor = 2f * speed;
	}
	//For aesthetic's sake: We wait until we reach the center of our destination
	//tile. Then we pop out.
	void PrepareOutro () {
		positionTo = tileFrom.transform.localPosition;
		directionChange = DirectionChange.None;
		directionAngleTo = direction.GetAngle();
		model.localPosition = new Vector3(pathOffset, 0f);
		transform.localRotation = direction.GetRotation();
		progressFactor = 2f * speed;
	}
	
	void PrepareNextState () {
		//Set our new From and To Tiles and their positions
		//for we have finished moving one tile and now need our new destination.
		tileFrom = tileTo;
		tileTo = tileTo.NextTileOnPath;
		
		positionFrom = positionTo;
		//tileTo means we have reached our destination tile and therefore:
		//We must prepare to leave.
		if (tileTo == null) {
			PrepareOutro();
			return;
		}
		positionTo = tileFrom.ExitPoint;
		directionChange = direction.GetDirectionChangeTo(tileFrom.PathDirection);
		direction = tileFrom.PathDirection;
		directionAngleFrom = directionAngleTo;
		
		switch (directionChange) {
			case DirectionChange.None: PrepareForward(); break;
			case DirectionChange.TurnRight: PrepareTurnRight(); break;
			case DirectionChange.TurnLeft: PrepareTurnLeft(); break;
			default: PrepareTurnAround(); break;
		}
	}
	
	//Prepare for the direction specified. 
	//The prepare functions also deal with turning.
	//These are the functions that involve GetHalfVector()
	//We must apply our pathoffset to model.localPosition
	//Just to make sure it all stays in line.
	void PrepareForward () {
		transform.localRotation = direction.GetRotation();
		directionAngleTo = direction.GetAngle();
		model.localPosition = new Vector3(pathOffset, 0f);
		progressFactor = speed;
	}
	//The circumference or a circle is equal to 2π times its radius (2πr). A right or 
	//left turn only covers a quarter of that and the radius is ½, so it's ½π × ½.
	//Now with our path offset, we need to change the radius in accordance with the
	//pathOffset. Subtract for right turn, addition for left.
	void PrepareTurnRight () {
		directionAngleTo = directionAngleFrom + 90f;
		model.localPosition = new Vector3(pathOffset - 0.5f, 0f);
		transform.localPosition = positionFrom + direction.GetHalfVector();
		progressFactor = speed / (Mathf.PI * 0.5f * (0.5f - pathOffset));
	}
	void PrepareTurnLeft () {
		directionAngleTo = directionAngleFrom - 90f;
		model.localPosition = new Vector3(pathOffset + 0.5f, 0f);
		transform.localPosition = positionFrom + direction.GetHalfVector();
		progressFactor = speed / (Mathf.PI * 0.5f * (0.5f + pathOffset));
	}
	void PrepareTurnAround () {
		//pathoffset modifies turning around as well. But at low offsets
		//we'll take an incredibly sharp turn, so by preventing an incredibly small
		//pathoffset, from messing around, we get this.
		directionAngleTo = directionAngleFrom + (pathOffset < 0f ? 180f : -180f);
		model.localPosition = new Vector3(pathOffset, 0f);
		transform.localPosition = positionFrom;
		progressFactor = speed / (Mathf.PI * Mathf.Max(Mathf.Abs(pathOffset), 0.2f));
	}
	
	//Because we extend WarEntity override the Recycle method
	public override void Recycle () {
		OriginFactory.Reclaim(this);
	}
}