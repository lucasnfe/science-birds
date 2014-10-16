using UnityEngine;
using System.Collections.Generic;

public class RandomLG : LevelGenerator {

	static readonly int[,] _gameObjectsDependencyGraph = {
		{1, 1, 1},
		{1, 0, 1},
		{1, 0, 1}
	};

	public int _maxPigsAmount = 1;

	public float _maxStackWidth = 1f;
	public float _maxStackHeight = 1f;

	// Each object has its own probability for being spawned
	public float []_gameObjectsDuplicateProbability;

	// List of stacks in the level
	private List<LinkedList<ShiftABGameObject>> _shiftGameObjects;
	
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
		_shiftGameObjects = new List<LinkedList<ShiftABGameObject>>();

		int stackIndex = 0;
		float probToGenerateNextColumn = 1f;

		// Loop to generate the game object stacks
		while(Random.value <= probToGenerateNextColumn)
		{
			// Randomly generate new stack based on the dependency graph
			_shiftGameObjects.Add(new LinkedList<ShiftABGameObject>());
			GenerateNextStack(stackIndex);

			// Reduce probability to create new stack
			float widerObjectInStack = FindWidestObjInStack(stackIndex).GetBounds().size.x;
			probToGenerateNextColumn -= widerObjectInStack/_maxStackWidth;
			stackIndex++;
		}

		InsertPigs();

		return ConvertShiftGBtoABGB();
	}

	void GenerateNextStack(int stackIndex)
	{
		LinkedList<ShiftABGameObject> stack = _shiftGameObjects[stackIndex];

		float probToStackNextObj = 1f;

		while(Random.value <= probToStackNextObj)
		{
			ShiftABGameObject nextObject = GenerateNextObject(stackIndex);
			if(nextObject == null)
				break;

			stack.AddLast(nextObject);

			Vector2 currentObjectSize = nextObject.GetBounds().size;
			probToStackNextObj -= currentObjectSize.y / _maxStackHeight;
		}
	}

	ShiftABGameObject GenerateNextObject(int stackIndex)
	{
		// Generate next object in the stack
		ShiftABGameObject nextObject = new ShiftABGameObject();

		if(!DefineObjectLabel(stackIndex, nextObject))
			return null;

		DefineObjectPosition(stackIndex, nextObject);

		return nextObject;
	}

	void DefineObjectPosition(int stackIndex, ShiftABGameObject nextObject)
	{
		LinkedList<ShiftABGameObject> stack = _shiftGameObjects[stackIndex];

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

			holdingPosition.x = Random.Range(0f, 0.5f);

			if(stack.Count == 0)
			{
				if(stackIndex > 0)
				{
					LinkedList<ShiftABGameObject> lastStack = _shiftGameObjects[stackIndex - 1];

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

			holdingPosition.y = ground.position.y + groundCollider.size.y/2.4f;
		}

		Vector2 newPosition = Vector2.zero;
		newPosition.x = holdingPosition.x;
		newPosition.y = holdingPosition.y + currentObjectSize.y/2f;

		nextObject.Position = newPosition;

		if(stack.Count > 0)
			UpdateStackPosition(stackIndex, holdingPosition.x);
	}

	bool DefineObjectLabel(int stackIndex, ShiftABGameObject nextObject)
	{
		LinkedList<ShiftABGameObject> stack = _shiftGameObjects[stackIndex];

		ShiftABGameObject objectBelow = null;

		if(stack.Count - 1 >= 0)
			objectBelow = stack.Last.Value;

		// If the object below is the ground
		if(objectBelow == null)
		{
			nextObject.Label = Random.Range(0, ABTemplates.Length);

			// There is a chance to double the object
			if(Random.value < _gameObjectsDuplicateProbability[nextObject.Label])
				nextObject.IsDouble = true;

			nextObject.HoldingObject = objectBelow;
			return true;
		}

		// Get list of objects that can be stacked
		List<int> stackableObjects = GetStackableObjects(objectBelow);

		while(stackableObjects.Count > 0)
		{
			nextObject.Label = stackableObjects[Random.Range(0, stackableObjects.Count - 1)];
			nextObject.IsDouble = false;

			// There is a chance to double the object
			if(Random.value < _gameObjectsDuplicateProbability[nextObject.Label])
				nextObject.IsDouble = true;

			// Check if there is no stability problems
			if(nextObject.Type == 0)
			{
				nextObject.IsDouble = false;

				// If next object is a box, check if it can enclose the underneath objects
				LinkedListNode<ShiftABGameObject> currentObj = stack.Last;
				float underObjectsHeight = 0f;

				while(currentObj != null)
				{
					Bounds objBelowBounds = currentObj.Value.GetBounds();

					if(objBelowBounds.size.x <= nextObject.GetBounds().size.x*0.5f)
					{
						if(underObjectsHeight + objBelowBounds.size.y < nextObject.GetBounds().size.y*0.9f)
						{
							nextObject.AddObjectInside(currentObj.Value);

							underObjectsHeight += objBelowBounds.size.y;
							currentObj = currentObj.Previous;
						}
						else break;
					}
					else break;
				}

				// Holding object is the ground, so it is safe
				if(currentObj == null)
				{
					nextObject.HoldingObject = null;
					return true;
				}

				Bounds holdObjBounds = currentObj.Value.GetBounds();

				// Holding object is bigger, so it is safe
				if(holdObjBounds.size.x >= nextObject.GetBounds().size.x)
				{
					nextObject.HoldingObject = currentObj.Value;
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

	void InsertPigs()
	{
		int objectsAmount = 0;

		for(int i = 0; i < _shiftGameObjects.Count; i++)
		{
			objectsAmount += _shiftGameObjects[i].Count;
		}

		int totalPigsAdded = 0;
		int pigsAmount = objectsAmount/_maxPigsAmount;
		int pigsPerStack = pigsAmount/_shiftGameObjects.Count + 1;

		for(int i = 0; i < _shiftGameObjects.Count; i++)
		{
			int pigsAddedInStack = 0;
			LinkedList<ShiftABGameObject> stack = _shiftGameObjects[i];

			for (LinkedListNode<ShiftABGameObject> obj = stack.First; obj != stack.Last.Next; obj = obj.Next)
			{
				if(obj.Value.Type == 0)
				{
					// Check if pig dimensions fit inside the box 
					if(obj.Value.GetBounds().size.y - obj.Value.UnderObjectsHeight > _pig.renderer.bounds.size.y*1.2f)
					{
						ShiftABGameObject pig = new ShiftABGameObject();
						pig.Label = ABTemplates.Length;

						Vector2 position = Vector2.zero;

						if(obj.Value.LastObjectInside() != null)
						{
							position = obj.Value.LastObjectInside().Position;
							position.y += obj.Value.LastObjectInside().GetBounds().size.y;

							LinkedListNode<ShiftABGameObject> pigNode = new LinkedListNode<ShiftABGameObject>(pig);
							stack.AddAfter(stack.Find(obj.Value.LastObjectInside()), pigNode);
							pigsAddedInStack++;
						}
						else
						{
							if(obj.Value.HoldingObject != null)
							{
								position = obj.Value.HoldingObject.Position;
								position.y += obj.Value.HoldingObject.GetBounds().size.y/2f;

								LinkedListNode<ShiftABGameObject> pigNode = new LinkedListNode<ShiftABGameObject>(pig);
								stack.AddAfter(stack.Find(obj.Value.HoldingObject), pigNode);
								pigsAddedInStack++;
							}
							else
							{
								Transform ground = transform.Find("Level/Ground");
								BoxCollider2D groundCollider = ground.GetComponent<BoxCollider2D>();

								position.x = obj.Value.Position.x;
								position.y = ground.position.y + groundCollider.size.y/2.4f;

								if(obj.Value.IsDouble)
								{
									float side = 1f;
									if(Random.value < 0.5f)
										side = -1f;

									position.x += (obj.Value.GetBounds().size.x/4f) * side;
								}

								LinkedListNode<ShiftABGameObject> pigNode = new LinkedListNode<ShiftABGameObject>(pig);
								stack.AddFirst(pigNode);
								pigsAddedInStack++;
							}
						}

						position.y += _pig.renderer.bounds.size.y/2f;
						pig.Position = position;
					}
				}
				
				if(pigsAddedInStack == pigsPerStack)
					break;
			}

			if(pigsAddedInStack < pigsPerStack)
			{
				Vector2 position = stack.Last.Value.Position;

				// If last element in stack is already a circle, replace it with a pig
				if(stack.Last.Value.Type == 1)
				{
					position.y -= stack.Last.Value.GetBounds().size.y/2f;

					stack.Last.Value.Label = ABTemplates.Length;
					position.y += _pig.renderer.bounds.size.y/2f;

					stack.Last.Value.Position = position;
					break;
				}

				ShiftABGameObject pig = new ShiftABGameObject();
				pig.Label = ABTemplates.Length;

				position.y += stack.Last.Value.GetBounds().size.y/2f;
				position.y += _pig.renderer.bounds.size.y/2f;
				pig.Position = position;

				LinkedListNode<ShiftABGameObject> pigNode = new LinkedListNode<ShiftABGameObject>(pig);
				stack.AddLast(pigNode);
				pigsAddedInStack++;
			}

			totalPigsAdded += pigsAddedInStack;

			if(totalPigsAdded >= pigsAmount)
				break;
		}
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

		for(int i = 0; i < _shiftGameObjects.Count; i++)
		{
			if(_shiftGameObjects[i] != null)
			{
				foreach(ShiftABGameObject shiftGameObject in _shiftGameObjects[i])
				{
					if(!shiftGameObject.IsDouble)
					{
						ABGameObject baseGameObject = new ABGameObject();
						
						baseGameObject.Label = shiftGameObject.Label;
						baseGameObject.Position = shiftGameObject.Position;
						gameObjects.Add(baseGameObject);
					}
					else
					{
						ABGameObject baseGameObjectA = new ABGameObject();
						
						baseGameObjectA.Label = shiftGameObject.Label;

						Vector2 leftObjPos = shiftGameObject.Position;
						leftObjPos.x -= shiftGameObject.GetBounds().size.x/4f;
						baseGameObjectA.Position = leftObjPos;

						gameObjects.Add(baseGameObjectA);

						ABGameObject baseGameObjectB = new ABGameObject();
						
						baseGameObjectB.Label = shiftGameObject.Label;
						Vector2 rightObjPos = shiftGameObject.Position;
						rightObjPos.x +=  shiftGameObject.GetBounds().size.x/4f;
						baseGameObjectB.Position = rightObjPos;

						gameObjects.Add(baseGameObjectB);
					}
				}
			}
		}

		return gameObjects;
	}

	void UpdateStackPosition(int stackIndex, float offset)
	{
		LinkedList<ShiftABGameObject> stack = _shiftGameObjects[stackIndex];

		for (LinkedListNode<ShiftABGameObject> obj = stack.First; obj != stack.Last.Next; obj = obj.Next)
		{
			Vector2 newPos = obj.Value.Position;
			newPos.x = offset;
			obj.Value.Position = newPos;
		}
	}

	ShiftABGameObject FindWidestObjInStack(int stackIndex, float maxHeight = Mathf.Infinity)
	{
		LinkedList<ShiftABGameObject> stack = _shiftGameObjects[stackIndex];

		float currentStackHeight = 0f;
		ShiftABGameObject widestObj = stack.First.Value;

		for (LinkedListNode<ShiftABGameObject> obj = stack.First.Next; obj != stack.Last.Next && currentStackHeight <= maxHeight; obj = obj.Next)
		{
			float stackedObjWidth = obj.Value.GetBounds().size.x;
			float widestObjWidth = widestObj.GetBounds().size.x;

			if(stackedObjWidth > widestObjWidth)
				widestObj = obj.Value;

			currentStackHeight += obj.Value.GetBounds().size.y;
		}

		return widestObj;
	}
}
