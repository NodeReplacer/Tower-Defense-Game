using UnityEngine;

public class Game : MonoBehaviour
{
    //NOTE: The list of living enemies is kept track of by
    //EnemyCollection.cs. Updating and removing dead ones.
    //EnemyCollections is serializable. It doesn't extend anything.
    
    [SerializeField]
    Vector2Int boardSize = new Vector2Int(11,11);
    
    [SerializeField]
    GameBoard board = default;
    
    [SerializeField]
	GameTileContentFactory tileContentFactory = default;
    
    //[SerializeField]
	//EnemyFactory enemyFactory = default;
    
    //keep track of the warFactory to spawn the shells independently from the towers
    //that fire it.
    [SerializeField]
	WarFactory warFactory = default;
    
    [SerializeField]
	GameScenario scenario = default;

	GameScenario.State activeScenario;
    
    [SerializeField, Range(0, 100)]
	int startingPlayerHealth = 10; //If we have game scenarios we have player health.
    
    int playerHealth; //We'll be messing with this
    
	//[SerializeField, Range(0.1f, 10f)]
	//float spawnSpeed = 1f;
    
    //float spawnProgress;
    
    //Time manipulation
    [SerializeField, Range(1f, 10f)]
	float playSpeed = 1f;
    
    const float pausedTimeScale = 0f;
    
    //Now that we have different tower types we have to keep track of it
    //for Game.cs to place it properly.
    TowerType selectedTowerType;
    
    //So now we need to keep track of two collections. One for enemies, the other
    //for not.
    GameBehaviorCollection enemies = new GameBehaviorCollection();
	GameBehaviorCollection nonEnemies = new GameBehaviorCollection();
    
    //Sends out a ray from the Camera to the Input.mousePosition
    //This ray gets whatever is at the tile position and it does not
    //care if the mouse button is down or not because our Update
    //method  reads whatever the mouse is touching at the time of a 
    //mouse button press.
    Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);
    
    //To keep track of each shell fired by the tower it needs a reference
    //to itself to run SpawnShell.
    static Game instance;
    
    public static Shell SpawnShell () {
		Shell shell = instance.warFactory.Shell;
		instance.nonEnemies.Add(shell);
		return shell;
	}
    
    public static Explosion SpawnExplosion () {
		Explosion explosion = instance.warFactory.Explosion;
		instance.nonEnemies.Add(explosion);
		return explosion;
	}
    
    void OnEnable () {
		instance = this;
	}
    
    void Awake () {
		playerHealth = startingPlayerHealth;
        board.Initialize(boardSize, tileContentFactory);
        board.ShowGrid = true;
        activeScenario = scenario.Begin();
    }
    
    void OnValidate() {
        if (boardSize.x < 2) {
            boardSize.x = 2;
        }
        if (boardSize.y < 2) {
            boardSize.y = 2;
        }
    }
    void Update () {
        //Just input getters
		if (Input.GetMouseButtonDown(0)) {
			HandleTouch();
		}
        else if (Input.GetMouseButtonDown(1)) {
			HandleAlternativeTouch();
		}
        if (Input.GetKeyDown(KeyCode.V)) {
			board.ShowPaths = !board.ShowPaths;
		}
        if (Input.GetKeyDown(KeyCode.G)) {
			board.ShowGrid = !board.ShowGrid;
		}
        
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
			selectedTowerType = TowerType.Laser;
		}
		else if (Input.GetKeyDown(KeyCode.Alpha2)) {
			selectedTowerType = TowerType.Mortar;
		}
        
        if (Input.GetKeyDown(KeyCode.Space)) { //The pause
			Time.timeScale =
				Time.timeScale > pausedTimeScale ? pausedTimeScale : playSpeed;
		}
        else if (Time.timeScale > pausedTimeScale) {
			Time.timeScale = playSpeed;
		}
		
		if (Input.GetKeyDown(KeyCode.B)) {
			BeginNewGame();
		}
        
        if (playerHealth <= 0 && startingPlayerHealth > 0) {
			Debug.Log("Defeat!");
			BeginNewGame();
		}
        if (!activeScenario.Progress() && enemies.IsEmpty) {
			Debug.Log("Victory!");
			BeginNewGame();
			activeScenario.Progress();
		}
        
		/*
        spawnProgress += spawnSpeed * Time.deltaTime;
		while (spawnProgress >= 1f) {
			spawnProgress -= 1f;
			SpawnEnemy();
		}
        */
		
        enemies.GameUpdate();
        Physics.SyncTransforms(); //Enemies are spawned at 0,0 for an instant.
        //if a tower would be near the world origin, for a split moment it would catch the
        //enemy there and take a shot at them. We don't want that, so if we synchronze our transforms.
        //
        board.GameUpdate();
        nonEnemies.GameUpdate();
	}
    
    //Run when you click the mouse button
	void HandleTouch () {
		GameTile tile = board.GetTile(TouchRay);
		if (tile != null) {
			if (Input.GetKey(KeyCode.LeftShift)) {
				board.ToggleTower(tile, selectedTowerType);
			}
			else {
				board.ToggleWall(tile);
			}
		}
	}
    void HandleAlternativeTouch () {
		GameTile tile = board.GetTile(TouchRay);
		if (tile != null) {
			if (Input.GetKey(KeyCode.LeftShift)) {
				board.ToggleDestination(tile);
			}
			else {
				board.ToggleSpawnPoint(tile);
			}
		}
	}
    
    //SpawnEnemy is now called in EnemySpawnSequence.State.
    public static void SpawnEnemy (EnemyFactory factory, EnemyType type) {
		GameTile spawnPoint = instance.board.GetSpawnPoint(
			Random.Range(0, instance.board.SpawnPointCount)
		);
		Enemy enemy = factory.Get(type);
		enemy.SpawnOn(spawnPoint);
		instance.enemies.Add(enemy);
	}
    
    //Runs multiple clear functions to uhh clear a bunch of things.
    void BeginNewGame () {
        playerHealth = startingPlayerHealth;
		enemies.Clear();
		nonEnemies.Clear();
		board.Clear();
		activeScenario = scenario.Begin();
	}
    
    public static void EnemyReachedDestination () {
		instance.playerHealth -= 1;
	}
    
    /*
    //Game used to spawn enemies, it does not do so anymore.
    
    //SpawnEnemy and Enemy.SpawnOn can be batched together. SpawnOn is a function that belongs to
    //Enemy.cs that literally spawns the object attached to a script wherever we've been given.
    void SpawnEnemy () {
		GameTile spawnPoint = board.GetSpawnPoint(Random.Range(0, board.SpawnPointCount));
		Enemy enemy = enemyFactory.Get((EnemyType)(Random.Range(0, 3)));
		enemy.SpawnOn(spawnPoint);
        enemies.Add(enemy);
	}
    */
	
}