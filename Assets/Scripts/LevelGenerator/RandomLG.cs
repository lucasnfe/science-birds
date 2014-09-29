using UnityEngine;
using System.Collections.Generic;

public class RandomLG : LevelGenerator {

	class ShiftABGameObject : ABGameObject
	{
		public int type;
		public float posShift;
		public ShiftABGameObject holdingObject;

		public static int GetTypeByTag(string tag)
		{
			if(tag == "Box")
				return 0;

			if(tag == "Circle")
				return 1;

			if(tag == "Rect")
				return 2;

			return -1;
		}
	}

	static readonly int[,] _gameObjectsDependencyGraph = {
		{1, 1, 1},
		{1, 0, 1},
		{1, 0, 1}
	};

	public int _maxStacks = 1;
	public float _maxStackHeight = 10f;

	List<ShiftABGameObject>[] _shiftGameObjects;

	public override List<ABGameObject> GenerateLevel()
	{
		_shiftGameObjects = new List<ShiftABGameObject>[_maxStacks];

		int stackIndex = 0;
		float probToGenerateNextColumn = 1f;

		// Loop to generate the game object stacks
		while(Random.value <= probToGenerateNextColumn)
		{
			// Randomly generate new stack based on the dependency graph
			_shiftGameObjects[stackIndex] = new List<ShiftABGameObject>();
			GenerateNextStack(stackIndex);

			// Reduce probability to create new stack
			probToGenerateNextColumn -= 1f/(float)_maxStacks;
			stackIndex++;
		}

		return ConvertShiftGBtoABGB();
	}

	void GenerateNextStack(int stackIndex)
	{
		List<ShiftABGameObject> stack = _shiftGameObjects[stackIndex];

		float probToStackNextObj = 1f;

		while(Random.value <= probToStackNextObj)
		{
			ShiftABGameObject nextObject = GenerateNextObject(stackIndex);

			if(nextObject == null)
				break;

			stack.Add(nextObject);

			Vector2 currentObjectSize = CalcObjBounds(ABTemplates[nextObject.label]).size;
			probToStackNextObj -= currentObjectSize.y / _maxStackHeight;
		}
	}

	ShiftABGameObject GenerateNextObject(int stackIndex)
	{
		// Generate next object in the stack
		ShiftABGameObject nextObject = new ShiftABGameObject();

		DefineObjectLabel(stackIndex, nextObject);

		if(nextObject.label == -1)
			return null;

		DefineObjectPosition(stackIndex, nextObject);

		return nextObject;
	}

	void DefineObjectPosition(int stackIndex, ShiftABGameObject nextObject)
	{
		List<ShiftABGameObject> stack = _shiftGameObjects[stackIndex];

		Vector2 holdingPosition = Vector2.zero;
		Vector2 currentObjectSize = CalcObjBounds(ABTemplates[nextObject.label]).size;
		
		if(nextObject.holdingObject != null)
		{
			float holdingObjHeight = CalcObjBounds(ABTemplates[nextObject.holdingObject.label]).size.y;

			holdingPosition.x = nextObject.holdingObject.position.x;
			holdingPosition.y = nextObject.holdingObject.position.y + holdingObjHeight/2f;
		}
		else 
		{
			Transform ground = transform.Find("Level/Ground");
			BoxCollider2D groundCollider = ground.GetComponent<BoxCollider2D>();

			holdingPosition.x = Random.Range(0f, 2f);

			if(stack.Count == 0)
			{
				if(stackIndex > 0)
				{
					List<ShiftABGameObject> lastStack = _shiftGameObjects[stackIndex - 1];

					if(lastStack.Count > 0)
					{
						ShiftABGameObject obj = FindWidestObjInStack(stackIndex - 1, holdingPosition.y + currentObjectSize.y);
						holdingPosition.x += obj.position.x + CalcObjBounds(ABTemplates[obj.label]).size.x/2f;
					}

					holdingPosition.x += currentObjectSize.x/2f;
				}
			}
			else if(stackIndex > 0)
			{
				ShiftABGameObject obj = FindWidestObjInStack(stackIndex - 1, holdingPosition.y + currentObjectSize.y);
				holdingPosition.x += obj.position.x + CalcObjBounds(ABTemplates[obj.label]).size.x/2f + currentObjectSize.x/2f;
			}

			holdingPosition.y = ground.position.y + groundCollider.size.y/2.25f;
		}

		nextObject.position.x = holdingPosition.x;
		nextObject.position.y = holdingPosition.y + currentObjectSize.y/2f + Mathf.Epsilon;

		UpdateStackPosition(stackIndex, holdingPosition.x);

		Debug.Log(nextObject.position);
	}

	void DefineObjectLabel(int stackIndex, ShiftABGameObject nextObject)
	{
		List<ShiftABGameObject> stack = _shiftGameObjects[stackIndex];

		int objBelowIndex = stack.Count - 1;
		ShiftABGameObject objectBelow = null;

		if(objBelowIndex >= 0)
			objectBelow = stack[objBelowIndex];

		// If the object below is the ground
		if(objectBelow == null)
		{
			nextObject.label = Random.Range(0, ABTemplates.Length);
			nextObject.holdingObject = objectBelow;
			nextObject.type = ShiftABGameObject.GetTypeByTag(ABTemplates[nextObject.label].tag);
			return;
		}

		// Get list of objects that can be stacked
		List<int> stackableObjects = GetStackableObjects(objectBelow);

		while(stackableObjects.Count > 0)
		{
			int nextObjectLabel = stackableObjects[Random.Range(0, stackableObjects.Count - 1)];
			int nextObjType = ShiftABGameObject.GetTypeByTag(ABTemplates[nextObjectLabel].tag);
			Bounds nextObjBounds = CalcObjBounds(ABTemplates[nextObjectLabel]);

			// Check if there is no stability problems
			if(nextObjType == 0)
			{
				// If next object is a box, check if it can enclose the underneath objects
				int stackCurrtIndex = objBelowIndex;
				float underObjectsHeight = 0f;

				while(stackCurrtIndex >= 0)
				{
					Bounds objBelowBounds = CalcObjBounds(ABTemplates[stack[stackCurrtIndex].label]);

					if(objBelowBounds.size.x <= nextObjBounds.size.x*0.5f)
					{
						if(underObjectsHeight + objBelowBounds.size.y < nextObjBounds.size.y*0.9f)
						{
							underObjectsHeight += objBelowBounds.size.y;
							stackCurrtIndex--;
						}
						else
							break;
					}
					else
						break;
				}

				// Holding object is the ground, so it is safe
				if(stackCurrtIndex < 0)
				{
					nextObject.label = nextObjectLabel;
					nextObject.holdingObject = null;
					nextObject.type = nextObjType;
					return;
				}

				Bounds holdObjBounds = CalcObjBounds(ABTemplates[stack[stackCurrtIndex].label]);

				// Holding object is bigger, so it is safe
				if(holdObjBounds.size.x >= nextObjBounds.size.x)
				{
					nextObject.label = nextObjectLabel;
					nextObject.holdingObject = stack[stackCurrtIndex];
					nextObject.type = nextObjType;
					return;
				}

				nextObject.label = -1;
				nextObject.holdingObject = null;
			}
			else
			{
				Bounds holdObjBounds = CalcObjBounds(ABTemplates[objectBelow.label]);

				// Holding object is bigger, so it is safe
				if(holdObjBounds.size.x >= nextObjBounds.size.x)
				{
					nextObject.label = nextObjectLabel;
					nextObject.holdingObject = objectBelow;
					nextObject.type = nextObjType;
					return;
				}
			}

			stackableObjects.Remove(nextObjectLabel);
		}
	}

	List<int> GetStackableObjects(ShiftABGameObject objectBelow)
	{
		List<int> stackableObjects = new List<int>();

		for(int i = 0; i < ABTemplates.Length; i++)
		{
			int currentObjType = ShiftABGameObject.GetTypeByTag(ABTemplates[i].tag);

			if(_gameObjectsDependencyGraph[currentObjType, objectBelow.type] == 1)
				stackableObjects.Add(i);
		}

		return stackableObjects;
	}

	List<ABGameObject> ConvertShiftGBtoABGB()
	{
		List<ABGameObject> gameObjects = new List<ABGameObject>();

		for(int i = 0; i < _shiftGameObjects.Length; i++)
		{
			if(_shiftGameObjects[i] != null)
			{
				foreach(ShiftABGameObject shiftGameObject in _shiftGameObjects[i])
				{
					ABGameObject baseGameObject = new ABGameObject();

					baseGameObject.label = shiftGameObject.label;
					baseGameObject.position = shiftGameObject.position;

					gameObjects.Add(baseGameObject);
				}
			}
		}

		return gameObjects;
	}

	void UpdateStackPosition(int stackIndex, float offset)
	{
		for(int i = 0; i < _shiftGameObjects[stackIndex].Count; i++)
		{
			_shiftGameObjects[stackIndex][i].position.x = offset;
		}
	}

	ShiftABGameObject FindWidestObjInStack(int stackIndex, float maxHeight = Mathf.Infinity)
	{
		List<ShiftABGameObject> stack = _shiftGameObjects[stackIndex];

		ShiftABGameObject widestObj = stack[0];
		float currentStackHeight = 0f;

		for(int i = 1; i < stack.Count && currentStackHeight <= maxHeight; i++)
		{
			float stackedObjWidth = CalcObjBounds(ABTemplates[stack[i].label]).size.x;
			float widestObjWidth = CalcObjBounds(ABTemplates[widestObj.label]).size.x;

			if(stackedObjWidth > widestObjWidth)
				widestObj = stack[i];

			currentStackHeight += CalcObjBounds(ABTemplates[stack[i].label]).size.y;
		}

		return widestObj;
	}
}
