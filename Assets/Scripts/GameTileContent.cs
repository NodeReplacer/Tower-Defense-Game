using UnityEngine;

[SelectionBase]
public class GameTileContent : MonoBehaviour {
    
	[SerializeField]
	GameTileContentType type = default;
	
	public GameTileContentType Type => type;
	
	GameTileContentFactory originFactory;
	
	public bool BlocksPath => Type == GameTileContentType.Wall || Type == GameTileContentType.Tower;
	
	//I've done something like this before with an Object Factory.
	//Whatever this script is attached to will send itself back to 
	//the factory in a Recycle method.
	
	//This assumes the existence of a GameTileContentFactory.cs file with a method called Reclaim.
	//OriginFactory is whatever factory was responsible for creating this GameObject the component
	//is attached to.
	public GameTileContentFactory OriginFactory {
		get => originFactory;
		set {
			Debug.Assert(originFactory == null, "Redefined origin factory!");
			originFactory = value;
		}
	}
	public void Recycle () {
		originFactory.Reclaim(this);
	}
	public virtual void GameUpdate () {
	}
}