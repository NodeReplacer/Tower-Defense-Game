using UnityEngine;

[CreateAssetMenu]
public class WarFactory : GameObjectFactory {
    [SerializeField]
	Explosion explosionPrefab = default;
    
	[SerializeField]
	Shell shellPrefab = default;
    
	//Expose the explosion to the Shell.cs and MortarTower.cs
    //they do need that to actually tell the factory to render the explosion.
    public Explosion Explosion => Get(explosionPrefab);
    
    public Shell Shell => Get(shellPrefab);
    
	T Get<T> (T prefab) where T : WarEntity {
		T instance = CreateGameObjectInstance(prefab);
		instance.OriginFactory = this;
		return instance;
	}

	public void Reclaim (WarEntity entity) {
		Debug.Assert(entity.OriginFactory == this, "Wrong factory reclaimed!");
		Destroy(entity.gameObject);
	}
}