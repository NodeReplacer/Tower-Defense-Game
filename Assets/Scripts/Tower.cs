using UnityEngine;

//A very important class, but we needed to settle our tiles, their content,
//factories, the enemies, and their paths before we started worrying about
//the towers

//It's now an abstract class to make room for multiple types of towers.
//We'll have to remove the laser related functions from this.
public abstract class Tower : GameTileContent {
    [SerializeField, Range(1.5f, 10.5f)]
	protected float targetingRange = 1.5f;
    
    //This will usually get overridden in each individual tower's script.
    public abstract TowerType TowerType { get; }
    
    protected bool TrackTarget (ref TargetPoint target) {
		if (target == null) {
			return false;
		}
        //TrackTarget is called every GameUpdate so if a target eventually leaves our maximum range
        //we will obviously stop tracking it as a target.
        Vector3 a = transform.localPosition;
		Vector3 b = target.Position;
        //A complex bit of math to track targets. It's the Pythagorean Theorem.
        //We should square root our result but we don't need that level of precision.
        //Anyway, given two straight lines (in the form of float x and float z)
        //If adding the square of them together is less than our targeting range squared (float r)
        //Then we say it's out of range. We're just not square rooting anything. We don't need that
        //precision.
		float x = a.x - b.x;
		float z = a.z - b.z;
        //+ 0.125f is done to take into account the radius of our collider.
        //Though scale is randomly changed so we need to take that into account. 
		float r = targetingRange + 0.125f * target.Enemy.Scale;
		if (x * x + z * z > r * r) {
			target = null;
			return false;
		}
		return true;
	}
    
    //Get all colliders in a sphere range around the tower equal to the targetingRange of
    //the tower.
    //We've changed it to a capsule to ignore elevation now.
    protected bool AcquireTarget (out TargetPoint target) {
		if (TargetPoint.FillBuffer(transform.localPosition, targetingRange)) {
			target = TargetPoint.RandomBuffered;
			return true;
		}
        target = null;
		return false;
	}
    
    //This draws a selected tower's range.
    void OnDrawGizmosSelected () {
		Gizmos.color = Color.yellow;
		Vector3 position = transform.localPosition;
		position.y += 0.01f;
		Gizmos.DrawWireSphere(position, targetingRange);
	}
}