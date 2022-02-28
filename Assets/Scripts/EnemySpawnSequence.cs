//This code controls which enemies spawn per sequence.
//A wave is just an array of spawn sequences.
//We progress through a sequence the same way we progress through a wave.

//Assets usually contain data that doesn't change but we're totally changing
//here as we progress through the sequence.

using UnityEngine;

[System.Serializable]
public class EnemySpawnSequence {

	[SerializeField]
	EnemyFactory factory = default;

	[SerializeField]
	EnemyType type = EnemyType.Medium;

	[SerializeField, Range(1, 100)]
	int amount = 1;

	[SerializeField, Range(0.1f, 10f)]
	float cooldown = 1f; //Per second.
    
    //Whenever we want to begin progressing through a sequence, we need to get a new state instance for it.
    //Whoever invokes Begin will be responsible for holding onto it.
    public State Begin () => new State(this);
    
    //To progress through a scenario we need to track its state somehow. But EnemySpawnSequence
    //is not attached to an object, it's like a Factory asset.
    //While we can track the sequence using a duplicate we don't need to duplicate the
    //entire asset.
    [System.Serializable] //To make it survive hot reloads, it needs to be serializable.
    public struct State {
        //The struct line above used to be a class. But that public State Begin()
        //creates a new state object every time a sequence starts.
        //More importantly it allocates memory every time.
        //We're avoiding it with a struct but this will only work as long as
        //the state remains small.
        //FURTHERMORE: IT IS A VALUE TYPE. PASSING IT AROUND WILL COPY IT.
        
        //Sequence.State only holds what it needs. Two variables.
        int count;
	    float cooldown;
        
		EnemySpawnSequence sequence;
        
		public State (EnemySpawnSequence sequence) {
			this.sequence = sequence;
		    count = 0;
			cooldown = sequence.cooldown;
		}
		
        //Due to the way we add it's likely we overshoot our cooldowns a bit.
        //So we bleed our remainder time over to the next time we try to increase
        //progress.
		public float Progress (float deltaTime) {
			cooldown += deltaTime;
			while (cooldown >= sequence.cooldown) {
				cooldown -= sequence.cooldown;
				if (count >= sequence.amount) {
					return cooldown;
				}
				count += 1;
                Game.SpawnEnemy(sequence.factory, sequence.type);
			}
			return -1f;
		}
	}
}