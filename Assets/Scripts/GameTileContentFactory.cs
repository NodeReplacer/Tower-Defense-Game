using UnityEngine;

[CreateAssetMenu]
public class GameTileContentFactory : GameObjectFactory {
    
    [SerializeField]
	GameTileContent destinationPrefab = default;
	[SerializeField]
	GameTileContent emptyPrefab = default;
    [SerializeField]
	GameTileContent wallPrefab = default;
	[SerializeField]
	GameTileContent spawnPointPrefab = default;
	[SerializeField]
	Tower[] towerPrefabs = default;
	
	public void Reclaim (GameTileContent content) {
		Debug.Assert(content.OriginFactory == this, "Wrong factory reclaimed!");
		Destroy(content.gameObject);
	}
    
    public GameTileContent Get (GameTileContentType type) {
		switch (type) {
			case GameTileContentType.Destination: return Get(destinationPrefab);
			case GameTileContentType.Empty: return Get(emptyPrefab);
			case GameTileContentType.Wall: return Get(wallPrefab);
			case GameTileContentType.SpawnPoint: return Get(spawnPointPrefab);
			//case GameTileContentType.Tower: return Get(towerPrefab);
		}
		Debug.Assert(false, "Unsupported non-tower type: " + type);
		return null;
	}
	
	public Tower Get (TowerType type) {
		Debug.Assert((int)type < towerPrefabs.Length, "Unsupported tower type!");
		Tower prefab = towerPrefabs[(int)type];
		Debug.Assert(type == prefab.TowerType, "Tower prefab at wrong index!");
		return Get(prefab);
	}
	
	//This private Get used her returned GameTileContent but the Get above
	//should return Tower for specificity's sake. We can cast the
	//GameTileContent or make this Get method generic.
	//- instantiate the prefab
    //- sets its origin factory should be this gameObject
    //- moves it to the factory scene
    //- returns the instance
    T Get<T> (T prefab) where T : GameTileContent {
		T instance = CreateGameObjectInstance(prefab);
		instance.OriginFactory = this;
		return instance;
	}
}