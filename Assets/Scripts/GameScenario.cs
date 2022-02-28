//A GameScenario is an array of waves which in turn is an array of spheres.

using UnityEngine;

[CreateAssetMenu]
public class GameScenario : ScriptableObject {

	[SerializeField]
	EnemyWave[] waves = {};
    
    [SerializeField, Range(0, 10)]
	int cycles = 1; //And now we can repeat some scenarios.
    
    [SerializeField, Range(0f, 1f)]
	float cycleSpeedUp = 0.5f; //To boost difficulty between cycles.
    
    //GameScenarios create EnemyWaves which create Sequences which
    //then spawn Enemies. Capiche?
    
    public State Begin () => new State(this);
    
	[System.Serializable]
	public struct State {

		GameScenario scenario;

		int cycle, index;
        
        float timeScale;
        
		EnemyWave.State wave;

		public State (GameScenario scenario) {
			this.scenario = scenario;
            cycle = 0;
			index = 0;
            timeScale = 1f;
			Debug.Assert(scenario.waves.Length > 0, "Empty scenario!");
			wave = scenario.waves[0].Begin();
		}
        
        //We are the top level, Progress doesn't require a parameter unlike the other two
        //times we did this.
        public bool Progress () {
			float deltaTime = wave.Progress(timeScale * Time.deltaTime);
			while (deltaTime >= 0f) {
				if (++index >= scenario.waves.Length) {
                    if (++cycle >= scenario.cycles && scenario.cycles > 0) {
                        //We don't need to return the time either,
                        //but do need to indicate if the scenario is finished or not.
                        return false;
                    }
					index = 0;
                    timeScale += scenario.cycleSpeedUp;
				}
				wave = scenario.waves[index].Begin();
				deltaTime = wave.Progress(deltaTime);
			}
			return true;
		}
	}
}