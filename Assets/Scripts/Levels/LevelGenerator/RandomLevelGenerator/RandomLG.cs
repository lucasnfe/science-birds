using UnityEngine;
using System.Collections.Generic;

public class RandomLG : ABLevelGenerator {

	enum BlockTypes{
		Box,
		Circle,
		Rect,
		Podium
	}

	static readonly int[,] _gameObjectsDependencyGraph = {
		{1, 1, 1, 1},
		{1, 0, 1, 1},
		{1, 0, 1, 1},
		{1, 0, 1, 1}
	};

	public float _duplicateProbability = 0f;

	public int GetTypeByTag(string tag)
	{					
		if(tag == "Box")
			return (int)BlockTypes.Box;
		
		if(tag == "Circle")
			return (int)BlockTypes.Circle;
		
		if(tag == "Rect")
			return (int)BlockTypes.Rect;
		
		if(tag == "Podium")
			return (int)BlockTypes.Podium;
		
		return -1;
	}


	public override ABLevel GenerateLevel()
	{
		ShiftABLevel randomLevel = GenerateRandomLevel();
		ConvertShiftGBtoABGB(ref randomLevel);
		
		return randomLevel;
	}

	protected ShiftABLevel GenerateRandomLevel() {
		
		ShiftABLevel randomLevel = new ShiftABLevel();
		randomLevel.birdsAmount = Random.Range(0, ABLevel.BIRDS_MAX_AMOUNT); 
		
		int stackIndex = 0;
		int totalObjsAdded = 0;
		float widerObjectInStack = 0;
		
		float randomWidth = Mathf.Abs(ABMath.RandomGaussian(randomLevel.LevelPlayableWidth/2f, randomLevel.LevelPlayableWidth/2f));
		float nextWidth = (randomLevel.WidthOfEmptyStack + randomWidth) % randomLevel.LevelPlayableWidth;
		
		// Loop to generate the game object stacks		
		for(float width = 0f; width + widerObjectInStack < nextWidth; width += widerObjectInStack)
		{
			// Randomly generate new stack based on the dependency graph
			LinkedList<ShiftABGameObject> stack = 
				GenerateStack(randomLevel.LevelPlayableHeight, randomLevel.LevelPlayableWidth, randomLevel.WidthOfEmptyStack);

			randomLevel.AddStack(stack);
			
			widerObjectInStack = randomLevel.GetStackWidth(stackIndex);
			totalObjsAdded += randomLevel.GetTotalObjectsAmount();
			stackIndex++;
		}
		
		if(totalObjsAdded == 0)
		{
			int randomStack = Random.Range(0, randomLevel.GetStacksAmount());
			LinkedList<ShiftABGameObject> stack = 
				GenerateStack(randomLevel.LevelPlayableHeight, randomLevel.LevelPlayableWidth, randomLevel.WidthOfEmptyStack);

			randomLevel.SetStack(randomStack, stack);
		}
		
		randomLevel.FixLevelSize();
		return randomLevel;
	}

	public LinkedList<ShiftABGameObject> GenerateStack(float levelPlayableHeight, float levelPlayableWidth, float widthOfEmptyStack)
	{		
		LinkedList<ShiftABGameObject> stack = new LinkedList<ShiftABGameObject>();

		float currentObjectHeight = 0f;
			
		// Next stack height
		float randomHeight = Mathf.Abs(ABMath.RandomGaussian(levelPlayableHeight/2f, levelPlayableWidth));
		float nextHeight = (widthOfEmptyStack + randomHeight) % levelPlayableHeight;
		
		// The stack will be empty if its height is lower than 1f
		if(nextHeight < 1f)
			return stack;
						
		for(float height = 0f; height + currentObjectHeight <= nextHeight; height += currentObjectHeight)
		{
			ShiftABGameObject nextObject = GenerateObject(ref stack);
			if(nextObject == null)
				break;
		
			stack.AddLast(nextObject);

			currentObjectHeight = nextObject.GetBounds().size.y;
		}

		// Add pigs to the level structure
		InsertPigs(ref stack);

		return stack;
	}

	ShiftABGameObject GenerateObject(ref LinkedList<ShiftABGameObject> stack)
	{
		// Generate next object in the stack
		ShiftABGameObject nextObject = new ShiftABGameObject();
		
		if(!DefineObjectLabel(ref nextObject, ref stack))
			return null;
		
		return nextObject;
	}
	
	bool DefineObjectLabel(ref ShiftABGameObject nextObject, ref LinkedList<ShiftABGameObject> stack)
	{
		ShiftABGameObject objectBelow = null;
		
		if(stack.Count > 0)
		{
			objectBelow = stack.Last.Value;
		}
		else
		{
			// If the object below is the ground
			nextObject.Label = Random.Range(0, GameWorld.Instance.Templates.Length);
			
			// There is a chance to double the object
			if(nextObject.Type != (int)BlockTypes.Podium && 
			   nextObject.Type != (int)BlockTypes.Circle && 
			   Random.value < _duplicateProbability)
				nextObject.IsDouble = true;
			
			return true;
		}
		
		// Get list of objects that can be stacked
		List<int> stackableObjects = GetStackableObjects(objectBelow);
		
		while(stackableObjects.Count > 0)
		{
			nextObject.UnderObjectsHeight = 0f;
			nextObject.IsDouble = false;
			nextObject.Label = stackableObjects[Random.Range(0, stackableObjects.Count - 1)];
			
			// Check if there is no stability problems
			if(nextObject.Type == (int)BlockTypes.Box)
			{
				// If next object is a box, check if it can enclose the underneath objects
				LinkedListNode<ShiftABGameObject> currentObj = stack.Last;
				float underObjectsHeight = 0f;
				
				while(currentObj != null)
				{
					Bounds objBelowBounds = currentObj.Value.GetBounds();
					
					if(objBelowBounds.size.x < nextObject.GetEmptyScapeInside().x)
					{
						if(underObjectsHeight + objBelowBounds.size.y < nextObject.GetEmptyScapeInside().y)
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

				// Holding object is bigger, so it is safe
				if(currentObj.Value.GetArea() >= nextObject.GetArea())
					return true;
			}
			else
			{
				// There is a chance to double the object
				if(objectBelow.GetBounds().size.x >= 2f * nextObject.GetBounds().size.x)
					
					if(nextObject.Type != (int)BlockTypes.Podium && 
					   nextObject.Type != (int)BlockTypes.Circle &&
					   Random.value < _duplicateProbability)
						nextObject.IsDouble = true;
				
				if(objectBelow.GetArea() > nextObject.GetArea())
					return true;
			}
			
			stackableObjects.Remove(nextObject.Label);
		}
		
		return false;
	}

	protected void InsertPigs(ref LinkedList<ShiftABGameObject> stack)
	{
		int pigsAdded = 0;

		if(stack.Count == 0)
			return;

		for (LinkedListNode<ShiftABGameObject> obj = stack.First; obj != stack.Last.Next; obj = obj.Next)
		{			
			if(obj.Value.Type == (int)BlockTypes.Box && !obj.Value.IsDouble)
			{
				if(pigsAdded > 0 && Random.value < 0.5f)
					continue;
				
				// Check if pig dimensions fit inside the box 
				if(Mathf.Abs (obj.Value.GetEmptyScapeInside().y - obj.Value.UnderObjectsHeight) > GameWorld.Instance._pig.GetComponent<Renderer>().bounds.size.y)
				{
					ShiftABGameObject pig = new ShiftABGameObject();
					pig.Label = GameWorld.Instance.Templates.Length;
					
					LinkedListNode<ShiftABGameObject> pigNode = new LinkedListNode<ShiftABGameObject>(pig);
					
					if(obj.Value.UnderObjectsHeight > 0f)
					{			
						stack.AddBefore(obj, pigNode);
					}
					else
					{
						if(obj.Value != stack.First.Value)
							stack.AddAfter(obj.Previous, pigNode);
						else
							stack.AddFirst(pigNode);
					}
					
					pigsAdded++;
					obj.Value.AddObjectInside(pig);
				}
			}
			
			if(obj == stack.Last)
			{	
				if(pigsAdded > 0 && Random.value < 0.5f)
					continue;
				
				// If last element in stack is already a circle, replace it with a pig
				if(stack.Last.Value.Type == (int)BlockTypes.Circle)
				{
					ShiftABGameObject pig = new ShiftABGameObject();
					pig.Label = GameWorld.Instance.Templates.Length;
					stack.Last.Value = pig;
					
					pigsAdded++;
				}
				else
				{	
					if(pigsAdded > 0)
						break;
					
					ShiftABGameObject pig = new ShiftABGameObject();
					pig.Label = GameWorld.Instance.Templates.Length;
					LinkedListNode<ShiftABGameObject> pigNode = new LinkedListNode<ShiftABGameObject>(pig);	
						
					stack.AddLast(pigNode);
					pigsAdded++;
					
					break;
				}
			}
		}
	}

	protected void ConvertShiftGBtoABGB(ref ShiftABLevel randomLevel)
	{			
		float offsetX, offsetY;
		float slingPos = GameWorld.Instance._slingshotTransform.position.x;
		offsetX = slingPos + randomLevel.LevelPlayableWidth/2f - randomLevel.GetLevelBounds().size.x/2.25f;

		for(int i = 0; i < randomLevel.GetStacksAmount(); i++)
		{
			if(i > 0)
			{
				if(randomLevel.GetStack(i - 1).Count > 0)
				{
					ShiftABGameObject wid = randomLevel.GetWidestObjInStack(i - 1);
					offsetX = wid.Position.x + wid.GetBounds().size.x/2f;
				}
				else
					offsetX += randomLevel.WidthOfEmptyStack/2f;
			}
				
			if(randomLevel.GetStack(i).Count > 0)
			{	
				offsetX += randomLevel.GetWidestObjInStack(i).GetBounds().size.x/2f;
				
				for (LinkedListNode<ShiftABGameObject> obj = randomLevel.GetStack(i).First; obj != randomLevel.GetStack(i).Last.Next; obj = obj.Next)
				{
					// Define object position 
					if(obj.Previous != null)

						offsetY = obj.Previous.Value.Position.y + 
							obj.Previous.Value.GetBounds().size.y/2f - obj.Value.UnderObjectsHeight;
					else
						offsetY = GameWorld.Instance._groundTransform.GetComponent<Collider2D>().bounds.center.y + 
							GameWorld.Instance._groundTransform.GetComponent<Collider2D>().bounds.size.y/2f;
					
					offsetY += obj.Value.GetBounds().size.y/2f;
					obj.Value.Position = new Vector2(offsetX, offsetY);

					if(!obj.Value.IsDouble)
					{
						ABGameObject baseGameObject = new ABGameObject();
						
						baseGameObject.Label =  obj.Value.Label;
						baseGameObject.Position =  obj.Value.Position;

						randomLevel.gameObjects.Add(baseGameObject);
					}
					else
					{
						ABGameObject baseGameObjectA = new ABGameObject();
						
						baseGameObjectA.Label =  obj.Value.Label;

						Vector2 leftObjPos = obj.Value.Position;
						leftObjPos.x -=  obj.Value.GetBounds().size.x/4f;
						baseGameObjectA.Position = leftObjPos;

						randomLevel.gameObjects.Add(baseGameObjectA);

						ABGameObject baseGameObjectB = new ABGameObject();
						
						baseGameObjectB.Label =  obj.Value.Label;
						Vector2 rightObjPos =  obj.Value.Position;
						rightObjPos.x +=   obj.Value.GetBounds().size.x/4f;
						baseGameObjectB.Position = rightObjPos;

						randomLevel.gameObjects.Add(baseGameObjectB);
					}
				}
			}
			else
				offsetX += randomLevel.WidthOfEmptyStack/2f;
		}
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

}
