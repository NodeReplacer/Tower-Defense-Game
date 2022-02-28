using UnityEngine;
using System.Collections.Generic;

public class GameBoard : MonoBehaviour {
	
	[SerializeField]
	Transform ground = default;
	
    [SerializeField]
    GameTile tilePrefab = default;
    
    [SerializeField]
	Texture2D gridTexture = default;
    
	Vector2Int size; //For now, it lies along the x and z planes.
	
	GameTile[] tiles;
	
	List<GameTile> spawnPoints = new List<GameTile>(); //Our list of spawn points, can be any number
	//of them. Can't use a queue because we can't afford to lose track of our spawn points.
	
	//We create a list of what content needs to be updated too, because we are already
	//in charge of the tiles and their content. 
	List<GameTileContent> updatingContent = new List<GameTileContent>();
	
	//Our search will be breadth first (instead of A* pathfinding) because we need to find multiple paths
    //towards the destination tile.
    //Anyways, it is important to process tiles in the same order that they're added to the frontier so
    //we make a Queue.
    Queue<GameTile> searchFrontier = new Queue<GameTile>();
	
	//contentFactory gives the responsibility of setting the content of the tiles to
    //the GameBoard.
    GameTileContentFactory contentFactory;
	
    bool showPaths, showGrid;
    public bool ShowPaths {
		get => showPaths;
		set {
			showPaths = value;
			if (showPaths) {
				foreach (GameTile tile in tiles) {
					tile.ShowPath();
				}
			}
			else {
				foreach (GameTile tile in tiles) {
					tile.HidePath();
				}
			}
		}
	}
    public bool ShowGrid {
		get => showGrid;
		set {
			showGrid = value;
			Material m = ground.GetComponent<MeshRenderer>().material;
			if (showGrid) {
				m.mainTexture = gridTexture;
                m.SetTextureScale("_MainTex", size);
			}
			else {
				m.mainTexture = null;
			}
		}
	}
	
	public int SpawnPointCount => spawnPoints.Count;
	
    //Create the entire gameboard.
    // - Create each tile, store them in the tiles array (so we can use them later to find paths)
    //      - the two for loops do this
	// - Establish the relationship each tile has with the other (MakeEastWestNeighbors, etc.).
    public void Initialize (Vector2Int size, GameTileContentFactory contentFactory) {
		this.size = size;
        this.contentFactory = contentFactory;
		ground.localScale = new Vector3(size.x, size.y, 1f);
        
        Vector2 offset = new Vector2((size.x - 1) * 0.5f, (size.y - 1) * 0.5f);
		
        tiles = new GameTile[size.x * size.y];
        for (int i = 0, y = 0; y < size.y; y++) {
			for (int x = 0; x < size.x; x++, i++) {
				GameTile tile = tiles[i] = Instantiate(tilePrefab);
				tile.transform.SetParent(transform, false);
				tile.transform.localPosition = new Vector3(x - offset.x, 0f, y - offset.y);
                if (x > 0) {
					GameTile.MakeEastWestNeighbors(tile, tiles[i - 1]);
				}
				if (y > 0) {
					GameTile.MakeNorthSouthNeighbors(tile, tiles[i - size.x]);
				}
                //If tile.IsAlternative is true if x (as in the x value) is an even number.
                //The specific way this works is that & is an AND operator.
                //We take every individual binary bit of the number and AND them.
                //In this case we are ANDing each one with 1.
                //The last part is a mask which checks the LAST bit of the number
                //specifically if it is 0. (All even numbers have 0 as their lsat bit).
                tile.IsAlternative = (x & 1) == 0;
                if ((y & 1) == 0) {
					tile.IsAlternative = !tile.IsAlternative;
				}
                //tile.Content = contentFactory.Get(GameTileContentType.Empty);
			}
		}
        //ToggleDestination(tiles[tiles.Length / 2]);
		//ToggleSpawnPoint(tiles[0]);
		Clear();
	}
    
	//This GameUpdate is necessary now that we Update each tile to track our towers
	//firing.
	public void GameUpdate () {
		for (int i = 0; i < updatingContent.Count; i++) {
			updatingContent[i].GameUpdate();
		}
	}
	
	//Send out a ray towards our mouse position.
    public GameTile GetTile (Ray ray) {
		if (Physics.Raycast(ray, out RaycastHit hit, float.MaxValue, 1)) {
			int x = (int)(hit.point.x + size.x * 0.5f);
			int y = (int)(hit.point.z + size.y * 0.5f);
			if (x >= 0 && x < size.x && y >= 0 && y < size.y) {
				return tiles[x + y * size.x];
			}
		}
        return null;
	}
    
    bool FindPaths () {
        //IF YOU USE foreach ON LISTS YOU WIND UP WITH MEMORY POLLUTION.
		foreach (GameTile tile in tiles) {
			//With multiple possible destinations we need to check each tile
            //if it is a destination.
            if (tile.Content.Type == GameTileContentType.Destination) {
				tile.BecomeDestination();
				searchFrontier.Enqueue(tile);
			}
			else {
                tile.ClearPath();
            }
		}
        //We've erased all of the elements in the queue of searchFrontier.
        if (searchFrontier.Count == 0) {
            return false;
        }
        //searchFrontier is an array of tiles that we've added to the path but
        //haven't grown a path out of yet. So we need to keep track of them
        //because we aren't done with them yet.
        while (searchFrontier.Count > 0) {
            //Dequeue just pops a unit out of the queue it is acting upon.
            GameTile tile = searchFrontier.Dequeue();
            if (tile != null) {
                //This is where we use GameTile.IsAlternative
                if (tile.IsAlternative) {
					searchFrontier.Enqueue(tile.GrowPathNorth());
					searchFrontier.Enqueue(tile.GrowPathSouth());
					searchFrontier.Enqueue(tile.GrowPathEast());
					searchFrontier.Enqueue(tile.GrowPathWest());
				}
                else {
					searchFrontier.Enqueue(tile.GrowPathWest());
					searchFrontier.Enqueue(tile.GrowPathEast());
					searchFrontier.Enqueue(tile.GrowPathSouth());
					searchFrontier.Enqueue(tile.GrowPathNorth());
				}
            }
        }
        
        //This is a check to ensure that the tiles
        foreach (GameTile tile in tiles) {
			if (!tile.HasPath) {
				return false;
			}
		}
        
        if (showPaths) {
            foreach (GameTile tile in tiles) {
                tile.ShowPath();
            }
        }
        return true;
	}
	
	//The board takes care of the SpawnPoints but NOT THE ENEMIES COMING OUT OF THEM.
	public GameTile GetSpawnPoint (int index) {
		return spawnPoints[index];
	}
	
	//recently added to empty all tiles, clear the spawn points, and updating content.
	public void Clear () {
		foreach (GameTile tile in tiles) {
			tile.Content = contentFactory.Get(GameTileContentType.Empty);
		}
		spawnPoints.Clear();
		updatingContent.Clear();
		ToggleDestination(tiles[tiles.Length / 2]);
		ToggleSpawnPoint(tiles[0]);
	}
	
	//Below is the series of "Toggle" methods that corresponds to 
	//various button presses that modify the GameBoard
	
    //If tile is empty get a GameTileContentType.Destination from the
    //GameTileContentFactory called contentFactory.
    //and vice versa.
	public void ToggleDestination (GameTile tile) {  
        if (tile.Content.Type == GameTileContentType.Destination) {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
            if (!FindPaths()) {
				tile.Content = contentFactory.Get(GameTileContentType.Destination);
				FindPaths();
			}
        }
        else if (tile.Content.Type == GameTileContentType.Empty) {
            tile.Content = contentFactory.Get(GameTileContentType.Destination);
            FindPaths();
        }
    }
    //Similar to the ToggleDestination method above. In fact, almost completely identical.
    public void ToggleWall (GameTile tile) {
		if (tile.Content.Type == GameTileContentType.Wall) {
			tile.Content = contentFactory.Get(GameTileContentType.Empty);
			FindPaths();
		}
		else if (tile.Content.Type == GameTileContentType.Empty) {
			tile.Content = contentFactory.Get(GameTileContentType.Wall);
			if (!FindPaths()) {
				tile.Content = contentFactory.Get(GameTileContentType.Empty);
				FindPaths();
			}
		}
	}
	public void ToggleSpawnPoint (GameTile tile) {
		if (tile.Content.Type == GameTileContentType.SpawnPoint) {
			if (spawnPoints.Count > 1) {
				spawnPoints.Remove(tile);
				tile.Content = contentFactory.Get(GameTileContentType.Empty);
			}
		}
		else if (tile.Content.Type == GameTileContentType.Empty) {
			tile.Content = contentFactory.Get(GameTileContentType.SpawnPoint);
			spawnPoints.Add(tile);
		}
	}
	public void ToggleTower (GameTile tile, TowerType towerType) {
		if (tile.Content.Type == GameTileContentType.Tower) {
			updatingContent.Remove(tile.Content);
			//It used to be that replacing a tower was as simple as just looking
			//at a tower and removing it, but with different types
			//we want it to directly replace the old tower with the new one
			//instead of just deleting.
			if (((Tower)tile.Content).TowerType == towerType) {
				tile.Content = contentFactory.Get(GameTileContentType.Empty);
				FindPaths();
			}
			else {
				tile.Content = contentFactory.Get(towerType);
				updatingContent.Add(tile.Content);
			}
		}
		else if (tile.Content.Type == GameTileContentType.Empty) {
			tile.Content = contentFactory.Get(towerType);
			//if (!FindPaths()) {
			if (FindPaths()) {
				updatingContent.Add(tile.Content);
			}
			else {
				tile.Content = contentFactory.Get(GameTileContentType.Empty);
				FindPaths();
			}
		}
		else if (tile.Content.Type == GameTileContentType.Wall) {
			tile.Content = contentFactory.Get(towerType);
			updatingContent.Add(tile.Content);
		}
	}
}