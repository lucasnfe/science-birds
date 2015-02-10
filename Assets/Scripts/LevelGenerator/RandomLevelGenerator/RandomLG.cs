using UnityEngine;
using System.Collections.Generic;

public class RandomLG : ABLevelGenerator {

	static readonly int[,] _gameObjectsDependencyGraph = {
		{1, 1, 1, 1},
		{1, 0, 1, 1},
		{1, 0, 1, 1},
		{1, 0, 1, 1}
	};

	private float _offsetX, _offsetY;
	
	public int _birdsMaxAmount = 0;
	public float _maxStackWidth = 1f;
	public float _maxStackHeight = 1f;
	public float _levelBeginning = 0f;
	public float _duplicateProbability = 0f;
	public float _widthOfEmptyStack = 1;
	public float _probabilityOfEmptyStack = 0f;

	public int GetTypeByTag(string tag)
	{					
		if(tag == "Box")
			return 0;
		
		if(tag == "Circle")
			return 1;
		
		if(tag == "Rect")
			return 2;
		
		if(tag == "Podium")
			return 3;
		
		return -1;
	}

	public override ABLevel GenerateLevel()
	{
		ABLevel abLevel = new ABLevel();
		
		abLevel.birdsAmount = Random.Range(0, _birdsMaxAmount);
		
		List<LinkedList<ShiftABGameObject>> gameObjects = GenerateRandomLevel();
		
		abLevel.width = GetLevelBounds(gameObjects).size.x;
		abLevel.height = GetLevelBounds(gameObjects).size.y;
		
		abLevel.gameObjects = ConvertShiftGBtoABGB(gameObjects);
					
		return abLevel;
	}

	public void GenerateNextStack(int stackIndex, ref List<LinkedList<ShiftABGameObject>> shiftGameObjects)
	{
		LinkedList<ShiftABGameObject> stack = shiftGameObjects[stackIndex];

		float probToStackNextObj = 1f;

		while(Random.value <= probToStackNextObj)
		{
			ShiftABGameObject nextObject = GenerateNextObject(stackIndex, ref shiftGameObjects);
			if(nextObject == null)
				continue;
		
			stack.AddLast(nextObject);

			Vector2 currentObjectSize = nextObject.GetBounds().size;
			probToStackNextObj -= currentObjectSize.y / _maxStackHeight;
		}
	}

	protected List<LinkedList<ShiftABGameObject>> GenerateRandomLevel() {
		
		List<LinkedList<ShiftABGameObject>> shiftGameObjects = new List<LinkedList<ShiftABGameObject>>();
		
		int stackIndex = 0;
		float probToGenerateNextColumn = 1f;
		
		_offsetX = _levelBeginning;
		
		// Loop to generate the game object stacks
		while(Random.value <= probToGenerateNextColumn)
		{
			// Randomly generate new stack based on the dependency graph
			shiftGameObjects.Add(new LinkedList<ShiftABGameObject>());
			
			// The stack may be empty with a certain probability
			if(Random.value >= _probabilityOfEmptyStack)
				GenerateNextStack(stackIndex, ref shiftGameObjects);
			
			// Add pigs to the level structure
			InsertPigs(stackIndex, ref shiftGameObjects);
			
			// Reduce probability to create new stack
			float widerObjectInStack = 0;
			
			if(shiftGameObjects[stackIndex].Count > 0)
			{
				widerObjectInStack = FindWidestObjInStack(shiftGameObjects[stackIndex]).GetBounds().size.x;
				probToGenerateNextColumn -= widerObjectInStack/_maxStackWidth;
			}
		
			stackIndex++;
		}
		
		return shiftGameObjects;
	}

	protected void InsertPigs(int i, ref List<LinkedList<ShiftABGameObject>> shiftGameObjects)
	{
		if(shiftGameObjects[i].Count == 0)
			return;

		for (LinkedListNode<ShiftABGameObject> obj = shiftGameObjects[i].First; obj != shiftGameObjects[i].Last.Next; obj = obj.Next)
		{
			if(obj == shiftGameObjects[i].Last)
			{
				// If last element in stack is already a circle, replace it with a pig
				if(shiftGameObjects[i].Last.Value.Type == 1)
				{
					shiftGameObjects[i].Last.Value.IsDouble = false;
					shiftGameObjects[i].Last.Value.Label = GameWorld.Instance.Templates.Length;
				}
				else
				{				
					ShiftABGameObject pig = new ShiftABGameObject();
					pig.Label = GameWorld.Instance.Templates.Length;
					
					LinkedListNode<ShiftABGameObject> pigNode = new LinkedListNode<ShiftABGameObject>(pig);
					shiftGameObjects[i].AddLast(pigNode);

					break;
				}
			}

			if(obj.Value.Type == 0)
			{
				// Check if pig dimensions fit inside the box 
				if(Mathf.Abs (obj.Value.GetBounds().size.y - obj.Value.UnderObjectsHeight) > GameWorld.Instance._pig.renderer.bounds.size.y * 1.35f)
				{
					ShiftABGameObject pig = new ShiftABGameObject();
					pig.Label = GameWorld.Instance.Templates.Length;
					
					LinkedListNode<ShiftABGameObject> pigNode = new LinkedListNode<ShiftABGameObject>(pig);
					
					if(obj.Value.UnderObjectsHeight > 0f)
					{
						shiftGameObjects[i].AddBefore(obj, pigNode);
					}
					else
					{
						if(obj.Value != shiftGameObjects[i].First.Value)
						
							shiftGameObjects[i].AddAfter(obj.Previous, pigNode);
						
						else if(!obj.Value.IsDouble)
						
							shiftGameObjects[i].AddFirst(pigNode);
					}
					
					obj.Value.AddObjectInside(pig);
				}
			}
		}
	}

	protected List<ABGameObject> ConvertShiftGBtoABGB(List<LinkedList<ShiftABGameObject>> shiftGameObjects)
	{
		_offsetX = _levelBeginning;

		List<ABGameObject> gameObjects = new List<ABGameObject>();

		for(int i = 0; i < shiftGameObjects.Count; i++)
		{
			if(i > 0 && shiftGameObjects[i - 1].Count > 0)
			{
				ShiftABGameObject wid = FindWidestObjInStack(shiftGameObjects[i - 1]);
				_offsetX = wid.Position.x + wid.GetBounds().size.x/2f;
			}
				
			if(shiftGameObjects[i].Count > 0)
			{	
				_offsetX += FindWidestObjInStack(shiftGameObjects[i]).GetBounds().size.x/2f;
		
				for (LinkedListNode<ShiftABGameObject> obj = shiftGameObjects[i].First; obj != shiftGameObjects[i].Last.Next; obj = obj.Next)
				{
					// Define object position 
					if(obj.Previous != null)

						_offsetY = obj.Previous.Value.Position.y + 
							obj.Previous.Value.GetBounds().size.y/2f - obj.Value.UnderObjectsHeight;
					else
						_offsetY = GameWorld.Instance._ground.collider2D.bounds.center.y + 
							GameWorld.Instance._ground.collider2D.bounds.size.y/2f;
					
					_offsetY += obj.Value.GetBounds().size.y/2f;
					obj.Value.Position = new Vector2(_offsetX, _offsetY);

					if(!obj.Value.IsDouble)
					{
						ABGameObject baseGameObject = new ABGameObject();
						
						baseGameObject.Label =  obj.Value.Label;
						baseGameObject.Position =  obj.Value.Position;

						gameObjects.Add(baseGameObject);
					}
					else
					{
						ABGameObject baseGameObjectA = new ABGameObject();
						
						baseGameObjectA.Label =  obj.Value.Label;

						Vector2 leftObjPos = obj.Value.Position;
						leftObjPos.x -=  obj.Value.GetBounds().size.x/4f;
						baseGameObjectA.Position = leftObjPos;

						gameObjects.Add(baseGameObjectA);

						ABGameObject baseGameObjectB = new ABGameObject();
						
						baseGameObjectB.Label =  obj.Value.Label;
						Vector2 rightObjPos =  obj.Value.Position;
						rightObjPos.x +=   obj.Value.GetBounds().size.x/4f;
						baseGameObjectB.Position = rightObjPos;

						gameObjects.Add(baseGameObjectB);
					}
				}
			}
			else
				_offsetX += _widthOfEmptyStack;
		}

		return gameObjects;
	}

	protected LinkedList<ShiftABGameObject> CopyStack(LinkedList<ShiftABGameObject> shiftGameObjects)
	{
		LinkedList<ShiftABGameObject> newStack = new LinkedList<ShiftABGameObject>();

		if(shiftGameObjects.Count == 0)
			return newStack;

		for (LinkedListNode<ShiftABGameObject> obj = shiftGameObjects.First; obj != shiftGameObjects.Last.Next; obj = obj.Next)
		{
			ShiftABGameObject newAbGameObject = new ShiftABGameObject();
			newAbGameObject.Label = obj.Value.Label;
			newAbGameObject.IsDouble = obj.Value.IsDouble;
			newAbGameObject.Position = obj.Value.Position;
			newAbGameObject.UnderObjectsHeight = obj.Value.UnderObjectsHeight;

			newStack.AddLast(newAbGameObject);
		}

		return newStack;
	}
	
	protected Bounds GetLevelBounds(List<LinkedList<ShiftABGameObject>> shiftGameObjects)
	{
		float width = 0f;
		float height = 0f;
		
		for(int i = 0; i < shiftGameObjects.Count; i++)
		{		
			ShiftABGameObject widestObj = FindWidestObjInStack(shiftGameObjects[i]);
						
			if(widestObj != null)
				
				width += widestObj.GetBounds().size.x;
			else
				width += _widthOfEmptyStack;
			
			float columnHeight = 0f;
			
			if(shiftGameObjects.Count > 0)
			{
				columnHeight = 0f;

				for (LinkedListNode<ShiftABGameObject> obj = shiftGameObjects[i].First; obj != shiftGameObjects[i].Last; obj = obj.Next)
					columnHeight += obj.Value.GetBounds().size.y;
			}
			
			if(columnHeight > height)
				height = columnHeight;
		}
		
		return new Bounds(Vector2.zero, new Vector2(width, height));
	}
	
	protected ShiftABGameObject FindWidestObjInStack(LinkedList<ShiftABGameObject> shiftGameObjects)
	{
		if(shiftGameObjects.Count == 0)
			return null;

		ShiftABGameObject widestObj = shiftGameObjects.First.Value;

		for (LinkedListNode<ShiftABGameObject> obj = shiftGameObjects.First; obj != shiftGameObjects.Last.Next; obj = obj.Next)
		{
			float stackedObjWidth = obj.Value.GetBounds().size.x;
			float widestObjWidth = widestObj.GetBounds().size.x;

			if(stackedObjWidth > widestObjWidth)
				widestObj = obj.Value;
		}

		return widestObj;
	}

	List<int> GetStackableObjects(ShiftABGameObject objectBelow)
	{
		List<int> stackableObjects = new List<int>();
		
		for(int i = 0; i < GameWorld.Instance.Templates.Length; i++)
		{
			int currentObjType = GetTypeByTag(GameWorld.Instance.Templates[i].tag);
			
			if(_gameObjectsDependencyGraph[currentObjType, objectBelow.Type] == 1)
				stackableObjects.Add(i);
		}
		
		return stackableObjects;
	}

	ShiftABGameObject GenerateNextObject(int stackIndex, ref List<LinkedList<ShiftABGameObject>> shiftGameObjects)
	{
		// Generate next object in the stack
		ShiftABGameObject nextObject = new ShiftABGameObject();
		
		if(!DefineObjectLabel(stackIndex, ref nextObject, ref shiftGameObjects))
			return null;
		
		return nextObject;
	}
	
	bool DefineObjectLabel(int stackIndex, ref ShiftABGameObject nextObject, ref List<LinkedList<ShiftABGameObject>> shiftGameObjects)
	{
		LinkedList<ShiftABGameObject> stack = shiftGameObjects[stackIndex];
		
		ShiftABGameObject objectBelow = null;
		
		if(stack.Count - 1 >= 0)
			objectBelow = stack.Last.Value;
		
		// If the object below is the ground
		if(objectBelow == null)
		{
			nextObject.Label = Random.Range(0, GameWorld.Instance.Templates.Length);
			
			// There is a chance to double the object
			if(nextObject.Type != 3 && Random.value < _duplicateProbability)
				nextObject.IsDouble = true;
			
			return true;
		}
		
		// Get list of objects that can be stacked
		List<int> stackableObjects = GetStackableObjects(objectBelow);
		
		while(stackableObjects.Count > 0)
		{
			nextObject = new ShiftABGameObject();
			nextObject.Label = stackableObjects[Random.Range(0, stackableObjects.Count - 1)];

			// Check if there is no stability problems
			if(nextObject.Type == 0)
			{
				// If next object is a box, check if it can enclose the underneath objects
				LinkedListNode<ShiftABGameObject> currentObj = stack.Last;
				float underObjectsHeight = 0f;
				
				while(currentObj != null)
				{
					Bounds objBelowBounds = currentObj.Value.GetBounds();
					
					if(objBelowBounds.size.x < nextObject.GetBounds().size.x*0.5f)
					{
						if(underObjectsHeight + objBelowBounds.size.y < nextObject.GetBounds().size.y*0.5f)
						{
							nextObject.AddObjectInside(currentObj.Value);
							underObjectsHeight += objBelowBounds.size.y;
							currentObj = currentObj.Previous;
						}
						else break;
					}
					else break;
				}

				nextObject.UnderObjectsHeight = underObjectsHeight;
				
				// Holding object is the ground, so it is safe
				if(currentObj == null)
					return true;
				
				Bounds holdObjBounds = currentObj.Value.GetBounds();
				
				// Holding object is bigger, so it is safe
				if(holdObjBounds.size.x >= nextObject.GetBounds().size.x)
					return true;
			}
			else
			{
				// There is a chance to double the object
				if(objectBelow.GetBounds().size.x >= 2f * nextObject.GetBounds().size.x && Random.value < _duplicateProbability)
				
					if(nextObject.Type != 3)
						nextObject.IsDouble = true;
					
				if(objectBelow.GetArea() > nextObject.GetArea())	
					return true;
			}
			
			stackableObjects.Remove(nextObject.Label);
		}
		
		return false;
	}
}
