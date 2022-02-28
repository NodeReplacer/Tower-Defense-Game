using UnityEngine;

public class GameTile : MonoBehaviour {
	[SerializeField]
	Transform arrow = default;
    
    GameTile north,east,west,south, nextOnPath;
    int distance;
	
    //Eventually all tiles will have a path. This will mean that their distance (from the destination)
    //Will not be the MaxValue anymore.
    public bool HasPath => distance != int.MaxValue;
    
    //A quick way to figure grow a path. We will work backwards from our desired destination tile
    //Each tile increasing in distance by 1 every time we travel.
    public GameTile GrowPathNorth () => GrowPathTo(north, Direction.South);
	public GameTile GrowPathEast () => GrowPathTo(east, Direction.West);
	public GameTile GrowPathSouth () => GrowPathTo(south, Direction.North);
	public GameTile GrowPathWest () => GrowPathTo(west, Direction.East);
    
	//Enemies must know where to go next. So we use a getter method
	public GameTile NextTileOnPath => nextOnPath;
	
    //This is just to break things up visually.
    //Usually the shortest path to a destination is to go in
    //on direction until you are level with your destination  (vertically or horizontall)
    //then move down a straight line to your destination.
    //That looks terrible.
    public bool IsAlternative { get; set; }
    
	//A point on the tile that a moving object will move through to exit the tile.
	public Vector3 ExitPoint { get; private set; }
	
	GameTileContent content;
	
	public Direction PathDirection { get; private set; }
	
	public GameTileContent Content {
		get => content; //The getter returns the content of the tile. Very simple.
		//Setter recycles its previous content and positions the new content.
		set {
			Debug.Assert(value != null, "Null assigned to content!");
			if (content != null) {
				content.Recycle();
			}
			content = value;
			content.transform.localPosition = transform.localPosition;
		}
	}
    //We'll be using these to determine the facing of our arrows.
    static Quaternion
		northRotation = Quaternion.Euler(90f, 0f, 0f),
		eastRotation = Quaternion.Euler(90f, 90f, 0f),
		southRotation = Quaternion.Euler(90f, 180f, 0f),
		westRotation = Quaternion.Euler(90f, 270f, 0f);
	
    //The relationship between tiles is symmetrical. If a tile is the eastern neighbour of one tile
    //then the second tile is the wetsern neighbour of the first tile.
    public static void MakeEastWestNeighbors (GameTile east, GameTile west) {
		//Once this relationship is established it should never change.
        //This debug.assert statement warns me if the relationship is being changed.
        Debug.Assert(west.east == null && east.west == null, "Redefined neighbors!");
        west.east = east;
		east.west = west;
	}
    public static void MakeNorthSouthNeighbors (GameTile north, GameTile south) {
		Debug.Assert(
			south.north == null && north.south == null, "Redefined neighbors!"
		);
		south.north = north;
		north.south = south;
	}
    //Each time we decide to find a path we need to clean out our path data.
    //A basic reset to prevent bugs from fusing previous data.
    public void ClearPath() {
        distance = int.MaxValue;
        nextOnPath = null;
    }
    
    //It's only possible to find a path if we have a destination.
    public void BecomeDestination () {
		distance = 0;
		nextOnPath = null;
		//The exit point for a destination point is its center.
		ExitPoint = transform.localPosition;
	}
    
    GameTile GrowPathTo (GameTile neighbor, Direction direction) {
		if (!HasPath || neighbor == null || neighbor.HasPath) {
			return null;
		}
		neighbor.distance = distance + 1;
		neighbor.nextOnPath = this;
		
		//The edge between two tiles is found by averaging their position
		//With the creation of GetHalfVector located in Direction.cs
		//We don't need to average between two tiles (or get them either)
		//neighbor.ExitPoint = (neighbor.transform.localPosition + transform.localPosition) * 0.5f;
		
		neighbor.ExitPoint = neighbor.transform.localPosition + direction.GetHalfVector();
		neighbor.PathDirection = direction;
		//the below code used to be a simple
		//return neighbor; but the inclusion of walls
		//means something blocks pathfinding
		//so now we must check.
		//return neighbor.Content.Type != GameTileContentType.Wall ? neighbor : null;
		return neighbor.Content.BlocksPath ? null : neighbor;
	}
    
    public void ShowPath () {
		if (distance == 0) {
			arrow.gameObject.SetActive(false);
			return;
		}
		arrow.gameObject.SetActive(true);
		//This nextOnPath checks if this tile's next on path is in a correct direction.
        arrow.localRotation =
			nextOnPath == north ? northRotation :
			nextOnPath == east ? eastRotation :
			nextOnPath == south ? southRotation :
			westRotation;
	}
	
	public void HidePath () {
		arrow.gameObject.SetActive(false);
	}
}