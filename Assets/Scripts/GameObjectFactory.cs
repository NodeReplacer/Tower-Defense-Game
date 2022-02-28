//A common base class, taking from GameTileContentFactory
//It's central function is to check if a scene is loaded
// - handle the special case of being in editor
// - and once that is all done, put all GameObjects that are created
// into the scene that owns this factory
// - Usually we won't use this component directly, we'll pass it down
// to subfactories that are more specialized.

//THIS IS NOT A FULLY FUNCTIONING FACTORY UNTO ITSELF
//Making it abstract will make it impossible to make object instances out of it (attach as a component)

using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class GameObjectFactory : ScriptableObject {
    
	Scene scene;
    //This function creates and returns an instance
    //and takes care of scene management.
    //"protected" means that the function is only accessible to this class
    //and those that extend it.
	protected T CreateGameObjectInstance<T> (T prefab) where T : MonoBehaviour {
		if (!scene.isLoaded) {
			if (Application.isEditor) {
				scene = SceneManager.GetSceneByName(name);
				if (!scene.isLoaded) {
					scene = SceneManager.CreateScene(name);
				}
			}
			else {
				scene = SceneManager.CreateScene(name);
			}
		}
		T instance = Instantiate(prefab);
		SceneManager.MoveGameObjectToScene(instance.gameObject, scene);
		return instance;
	}
}