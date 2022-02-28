using UnityEngine;

//A giant enumerator that we'll be using for a bunch of direction values that we'll be using.
//We don't even use all of them a lot but even so it's useful to have solely for readability
public enum Direction {
	North, East, South, West
}
public enum DirectionChange {
	None, TurnRight, TurnLeft, TurnAround
}

public static class DirectionExtensions {
	
	//A whole load of other value arrays that keep track of which way is where.
	//These are sorted out in the "Get[Blank]" functions located below
	//It uses the direction
	static Quaternion[] rotations = {
		Quaternion.identity,
		Quaternion.Euler(0f, 90f, 0f),
		Quaternion.Euler(0f, 180f, 0f),
		Quaternion.Euler(0f, 270f, 0f)
	};

	static Vector3[] halfVectors = {
		Vector3.forward * 0.5f,
		Vector3.right * 0.5f,
		Vector3.back * 0.5f,
		Vector3.left * 0.5f
	};

	public static Quaternion GetRotation (this Direction direction) {
		return rotations[(int)direction];
	}
    
    public static DirectionChange GetDirectionChangeTo (
		this Direction current, Direction next
	) {
		if (current == next) {
			return DirectionChange.None;
		}
		else if (current + 1 == next || current - 3 == next) {
			return DirectionChange.TurnRight;
		}
		else if (current - 1 == next || current + 3 == next) {
			return DirectionChange.TurnLeft;
		}
		return DirectionChange.TurnAround;
	}
    
    public static float GetAngle (this Direction direction) {
		return (float)direction * 90f;
	}
	public static Vector3 GetHalfVector (this Direction direction) {
		return halfVectors[(int)direction];
	}
}