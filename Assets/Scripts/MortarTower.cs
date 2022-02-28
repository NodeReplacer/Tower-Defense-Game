using UnityEngine;

public class MortarTower : Tower {
	
	[SerializeField, Range(0.5f, 2f)]
	float shotsPerSecond = 1f;
	
	//MortarTower is in control of the explosions' size and radii.
	[SerializeField, Range(0.5f, 3f)]
	float shellBlastRadius = 1f;

	[SerializeField, Range(1f, 100f)]
	float shellDamage = 10f;
	
	[SerializeField]
	Transform mortar = default;
	
	public override TowerType TowerType => TowerType.Mortar;
    
    float launchSpeed; //We only need to know our launchSpeed once. Though we will need
    //to change it to keep track of how far our enemy is.
    float launchProgress; //We won't be firing nonstop, we need this to take a break between shots.
    //Like SpawnProgress.
	
	void Awake () {
		OnValidate();
	}

	void OnValidate () {
		float x = targetingRange + 0.25001f;;
		float y = -mortar.position.y;
		launchSpeed = Mathf.Sqrt(9.81f * (y + Mathf.Sqrt(x * x + y * y)));
	}
    
    public override void GameUpdate () {
        
        //Old code used to test out our launch functions on specific points.
		//Launch(new Vector3(3f, 0f, 0f));
		//Launch(new Vector3(0f, 0f, 1f));
		//Launch(new Vector3(1f, 0f, 1f));
		//Launch(new Vector3(3f, 0f, 1f));
        
        launchProgress += shotsPerSecond * Time.deltaTime;
		while (launchProgress >= 1f) {
			if (AcquireTarget(out TargetPoint target)) {
				Launch(target);
				launchProgress -= 1f;
			}
			else {
				launchProgress = 0.999f;
			}
		}
	}
    
    //Our aiming function
	public void Launch (TargetPoint target) {
		//We begin by pointing directly at our target.
        //That straight line can define a right triangle.
        //It's top point is at the mortar's position. The point below is the base.
        Vector3 launchPoint = mortar.position;
		Vector3 targetPoint = target.Position;
		targetPoint.y = 0f;
        
        Vector2 dir;
		dir.x = targetPoint.x - launchPoint.x;
		dir.y = targetPoint.z - launchPoint.z;
        float x = dir.magnitude;
		float y = -launchPoint.y;
        dir /= x;
        
        //After that, crazy math.
        //We launch the shell so that it's flight time is exactly as long as it takes to reach a target.
        //We have a quadratic formula. It's au^2 + bu + c = 0.
        //u = tan
        //a = -gx^2/2s^2
        //b = x
        //c = a-y
        
        float g = 9.81f; //gravity
		float s = launchSpeed; //Launch speed
		float s2 = s * s; //Launch speed squared.

		float r = s2 * s2 - g * (g * x * x + 2f * y * s2); //Our range.
        Debug.Assert(r >= 0f, "Launch velocity insufficient for range!");
        
        //Our launch speed should be just enough to hit the farthest target
        //and no further.
        
		float tanTheta = (s2 + Mathf.Sqrt(r)) / (g * x);
		float cosTheta = Mathf.Cos(Mathf.Atan(tanTheta));
		float sinTheta = cosTheta * tanTheta;
        
        //Face the mortar towards the correct angle and direction
        mortar.localRotation = Quaternion.LookRotation(new Vector3(dir.x, tanTheta, dir.y));
        
        Game.SpawnShell().Initialize(
			launchPoint, targetPoint,
			new Vector3(s * cosTheta * dir.x, s * sinTheta, s * cosTheta * dir.y),
			shellBlastRadius, shellDamage
		);
        
        /*
		//Drawing functions for visualizing.
        Vector3 prev = launchPoint, next;
		for (int i = 1; i <= 10; i++) {
			float t = i / 10f;
			float dx = s * cosTheta * t;
			float dy = s * sinTheta * t - 0.5f * g * t * t;
			next = launchPoint + new Vector3(dir.x * dx, dy, dir.y * dx);
			Debug.DrawLine(prev, next, Color.blue, 1f);
			prev = next;
		}
		
		Debug.DrawLine(launchPoint, targetPoint, Color.yellow, 1f);
        Debug.DrawLine(
			new Vector3(launchPoint.x, 0.01f, launchPoint.z),
			new Vector3(launchPoint.x + dir.x * x, 0.01f, launchPoint.z + dir.y * x),
			Color.white, 1f
		);
		*/
		
	}
}