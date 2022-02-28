using UnityEngine;

public class TargetPoint : MonoBehaviour {

	public Enemy Enemy { get; private set; }

	public Vector3 Position => transform.position;
    
	//A getter that gives a random length
	public static TargetPoint RandomBuffered =>
		GetBuffered(Random.Range(0, BufferedCount));
	
	//An entire section of code harvested from Tower.cs
	
	const int enemyLayerMask = 1 << 9;
	//Physics.OverlapCapsule allocates an array every time its run.
    //But we only need one so why do that? 
    //We have 100 now, to allow for a wider range of targets
    //that we can randomly attack.
    //We keep track of how many targets we actually hit so there's now
    //worry about referencing the 99th array value when we
    //only hit one thing.
	static Collider[] buffer = new Collider[100];

	public static int BufferedCount { get; private set; }
	
	public static bool FillBuffer (Vector3 position, float range) {
		Vector3 top = position;
		top.y += 3f;
		BufferedCount = Physics.OverlapCapsuleNonAlloc(
			position, top, range, buffer, enemyLayerMask
		);
		return BufferedCount > 0;
	}

	public static TargetPoint GetBuffered (int index) {
		var target = buffer[index].GetComponent<TargetPoint>();
		Debug.Assert(target != null, "Targeted non-enemy!", buffer[0]);
		return target;
	}
	//THIS IS THE END OF THE CODE BLOCK TAKEN FROM Tower.cs
	
	
    //We'll be using a target point to check it.
    void Awake () {
		Enemy = transform.root.GetComponent<Enemy>();
		Debug.Assert(Enemy != null, "Target point without Enemy root!", this);
        Debug.Assert(GetComponent<SphereCollider>() != null,"Target point without sphere collider!", this);
        Debug.Assert(gameObject.layer == 9, "Target point on wrong layer!", this);
	}
}