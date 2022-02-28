//We progress through a wave the same way we do so in a Sequence.
//But with a slight change. Sequences spawn enemies, waves spawn
//sequences, scenarios spawn waves. Up the ladder we go.

using UnityEngine;

[CreateAssetMenu]
public class EnemyWave : ScriptableObject {

	[SerializeField]
	EnemySpawnSequence[] spawnSequences = {
		new EnemySpawnSequence()
	};
	
	public State Begin() => new State(this);

	[System.Serializable]
	public struct State {

		EnemyWave wave;

		int index;

		EnemySpawnSequence.State sequence;

		public State (EnemyWave wave) {
			this.wave = wave;
			index = 0;
			Debug.Assert(wave.spawnSequences.Length > 0, "Empty wave!");
			sequence = wave.spawnSequences[0].Begin(); //This is where the sequence is created.
		}
		
		public float Progress (float deltaTime) {
			//sequence.Progress returns our cooldown given deltaTime
			//Basically, if there's any time remaining, which is what the
			//next while loop checks for.
			deltaTime = sequence.Progress(deltaTime);
			while (deltaTime >= 0f) {
				//If no sequences remain
				if (++index >= wave.spawnSequences.Length) {
					//return the time.
					return deltaTime;
				}
				sequence = wave.spawnSequences[index].Begin();
				deltaTime = sequence.Progress(deltaTime);
			}
			return -1f;
		}
	}
}