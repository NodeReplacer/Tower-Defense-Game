using UnityEngine;

//A very important class, to be sure, but we needed to settle our tiles, their content,
//factories, the enemies, and their paths before we started worrying about
public class LaserTower : Tower {
    //Expose a reference to the turret's transform component.
    [SerializeField]
	Transform turret = default, laserBeam = default;
    
    [SerializeField, Range(1f, 100f)]
	float damagePerSecond = 10f;//Used in void Shoot(). But passed to the target Enemy's
    //ApplyDamage function to actually effect the enemy.
    
    TargetPoint target;
    
    //How wide is our beam? Find out now. Honestly I've jsut learned that leaving things
    //even when the variable name is self-explanatory has me vaguely uncomfortable.
    //I'm kind of fine with it but also not really.
    Vector3 laserBeamScale;
    
    //Override the TowerType getter from Tower.cs because the tower type is
    //TowerType.Laser
    public override TowerType TowerType => TowerType.Laser;
    
	void Awake () {
		laserBeamScale = laserBeam.localScale;
	}
    
    public override void GameUpdate () {
		if (TrackTarget(ref target) || AcquireTarget(out target)) {
			//Debug.Log("Locked on target!");
			Shoot();
		}
        else {
			laserBeam.localScale = Vector3.zero;
		}
	}
    
    void Shoot () {
		Vector3 point = target.Position;
		turret.LookAt(point);
        //Laserbeam cube must match orientation of turret
        laserBeam.localRotation = turret.localRotation;
        
        float d = Vector3.Distance(turret.position, point);
		//scale the z directin of the laserbeam cube. In relative terms to our turret
        //the z directin of laserbeam will be pointing at our target once we
        //finished matching their rotations above.
        laserBeamScale.z = d;
		laserBeam.localScale = laserBeamScale;
        laserBeam.localPosition = turret.localPosition + 0.5f * d * laserBeam.forward;
        target.Enemy.ApplyDamage(damagePerSecond * Time.deltaTime);
	}
}