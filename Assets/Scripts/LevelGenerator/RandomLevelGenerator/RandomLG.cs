using UnityEngine;
using System.Collections.Generic;

public class RandomLG : LevelGenerator {

	static readonly int[,] _gameObjectsDependencyGraph = {
		{1, 1, 1},
		{1, 0, 1},
		{1, 0, 1}
	};

	public int _maxStacks = 1;
	public float _maxStackHeight = 10f;

	List<ShiftABGameObject>[] _shiftGameObjects;

	public int GetTypeByTag(string tag)
	{					
		if(tag == "Box")
			return 0;
		
		if(tag == "Circle")
			return 1;
		
		if(tag == "Rect")
			return 2;
		
		return -1;
	}

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

			Vector2 currentObjectSize = nextObject.GetBounds().size;
			probToStackNextObj -= currentObjectSize.y / _maxStackHeight;
		}
	}

	ShiftABGameObject GenerateNextObject(int stackIndex)
	{
		// Generate next object in the stack
		ShiftABGameObject nextObject = new ShiftABGameObject();

		// There is a chance to double the object
		if(Random.value < 0.5f)
			nextObject.IsDouble = true;

		if(!DefineObjectLabel(stackIndex, nextObject))
			return null;

		DefineObjectPosition(stackIndex, nextObject);

		return nextObject;
	}

	void DefineObjectPosition(int stackIndex, ShiftABGameObject nextObject)
	{
		List<ShiftABGameObject> stack = _shiftGameObjects[stackIndex];

		Vector2 holdingPosition = Vector2.zero;
		Vector2 currentObjectSize = nextObject.GetBounds().size;

		if(nextObject.HoldingObject != null)
		{
			float holdingObjHeight = nextObject.HoldingObject.GetBounds().size.y;

			holdingPosition.x = nextObject.HoldingObject.Position.x;
			holdingPosition.y = nextObject.HoldingObject.Position.y + holdingObjHeight/2f;
		}
		else 
		{
			Transform ground = transform.Find("Level/Ground");
			BoxCollider2D groundCollider = ground.GetComponent<BoxCollider2D>();

			//holdingPosition.x = Random.Range(0f, 2f);

			if(stack.Count == 0)
			{
				if(stackIndex > 0)
				{
					List<ShiftABGameObject> lastStack = _shiftGameObjects[stackIndex - 1];

					if(lastStack.Count > 0)
					{
						ShiftABGameObject obj = FindWidestObjInStack(stackIndex - 1, holdingPosition.y + currentObjectSize.y);
						holdingPosition.x += obj.Position.x + obj.GetBounds().size.x/2f;
					}

					holdingPosition.x += currentObjectSize.x/2f;
				}
			}
			else if(stackIndex > 0)
			{
				ShiftABGameObject obj = FindWidestObjInStack(stackIndex - 1, holdingPosition.y + currentObjectSize.y);
				holdingPosition.x += obj.Position.x + obj.GetBounds().size.x/2f + currentObjectSize.x/2f;
			}

			holdingPosition.y = ground.position.y + groundCollider.size.y/2.25f;
		}

		Vector2 newPosition = Vector2.zero;
		newPosition.x = holdingPosition.x;
		newPosition.y = holdingPosition.y + currentObjectSize.y/2f;

		nextObject.Position = newPosition;

		UpdateStackPosition(stackIndex, holdingPosition.x);
	}

	bool DefineObjectLabel(int stackIndex, ShiftABGameObject nextObject)
	{
		List<ShiftABGameObject> stack = _shiftGameObjects[stackIndex];

		ShiftABGameObject objectBelow = null;

		if(stack.Count - 1 >= 0)
			objectBelow = stack[stack.Count - 1];

		// If the object below is the ground
		if(objectBelow == null)
		{
			nextObject.Label = Random.Range(0, ABTemplates.Length);
			nextObject.HoldingObject = objectBelow;
			return true;
		}

		// Get list of objects that can be stacked
		List<int> stackableObjects = GetStackableObjects(objectBelow);

		while(stackableObjects.Count > 0)
		{
			nextObject.Label = stackableObjects[Random.Range(0, stackableObjects.Count - 1)];

			// Check if there is no stability problems
			if(nextObject.Type == 0)
			{
				// If next object is a box, check if it can enclose the underneath objects
				int stackCurrtIndex = stack.Count - 1;
				float underObjectsHeight = 0f;

				while(stackCurrtIndex >= 0)
				{
					Bounds objBelowBounds = stack[stackCurrtIndex].GetBounds();

					if(objBelowBounds.size.x <= nextObject.GetBounds().size.x*0.5f)
					{
						if(underObjectsHeight + objBelowBounds.size.y < nextObject.GetBounds().size.y*0.9f)
						{
							underObjectsHeight += objBelowBounds.size.y;
							stackCurrtIndex--;
						}
						else break;
					}
					else break;
				}

				// Holding object is the ground, so it is safe
				if(stackCurrtIndex < 0)
				{
					nextObject.HoldingObject = null;
					return true;
				}

				Bounds holdObjBounds = stack[stackCurrtIndex].GetBounds();

				// Holding object is bigger, so it is safe
				if(holdObjBounds.size.x >= nextObject.GetBounds().size.x)
				{
					nextObject.HoldingObject = stack[stackCurrtIndex];
					return true;
				}
			}
			else
			{
				// Holding object is bigger, so it is safe
				if(objectBelow.GetBounds().size.x >= nextObject.GetBounds().size.x)
				{
					nextObject.HoldingObject = objectBelow;
					return true;
				}
			}

			stackableObjects.Remove(nextObject.Label);
		}

		return false;
	}

	List<int> GetStackableObjects(ShiftABGameObject objectBelow)
	{
		List<int> stackableObjects = new List<int>();

		for(int i = 0; i < ABTemplates.Length; i++)
		{
			int currentObjType = GetTypeByTag(ABTemplates[i].tag);

			if(_gameObjectsDependencyGraph[currentObjType, objectBelow.Type] == 1)
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

					baseGameObject.Label = shiftGameObject.Label;
					baseGameObject.Position = shiftGameObject.Position;

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
			Vector2 newPos = _shiftGameObjects[stackIndex][i].Position;
			newPos.x = offset;

			_shiftGameObjects[stackIndex][i].Position = newPos;
		}
	}

	ShiftABGameObject FindWidestObjInStack(int stackIndex, float maxHeight = Mathf.Infinity)
	{
		List<ShiftABGameObject> stack = _shiftGameObjects[stackIndex];

		ShiftABGameObject widestObj = stack[0];
		float currentStackHeight = 0f;

		for(int i = 1; i < stack.Count && currentStackHeight <= maxHeight; i++)
		{
			float stackedObjWidth = stack[i].GetBounds().size.x;
			float widestObjWidth = widestObj.GetBounds().size.x;

			if(stackedObjWidth > widestObjWidth)
				widestObj = stack[i];

			currentStackHeight += stack[i].GetBounds().size.y;
		}

		return widestObj;
	}
}
