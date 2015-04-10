using UnityEngine;
using System.Collections.Generic;

public class RandomLG : ABLevelGenerator {

	static readonly int[,] _gameObjectsDependencyGraph = {
		{1, 1, 1, 1},
		{1, 0, 1, 1},
		{1, 0, 1, 1},
		{1, 0, 1, 1}
	};

	public float _duplicateProbability = 0f;

	protected float _levelPlayableWidth;
	protected const float _levelPlayableHeight = 12f;
	protected const float _widthOfEmptyStack = 2f;

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
	
	public override void Start()
	{
		GameObject ground = GameWorld.Instance._ground.gameObject;	
		
		float levelLeftBound = ground.transform.position.x - ground.collider2D.bounds.size.x/2f;
		float slingDistFromLeftBound = GameWorld.Instance._slingshot.position.x - levelLeftBound;
		
		_levelPlayableWidth = (ground.collider2D.bounds.size.x - slingDistFromLeftBound) - 1f;
		
		base.Start();
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

		float currentObjectHeight = 0f;
			
		// Next stack height
		float randomHeight = Mathf.Abs(ABMath.RandomGaussian(_levelPlayableHeight/2f, _levelPlayableHeight));
		float nextHeight = (_widthOfEmptyStack + randomHeight) % _levelPlayableHeight;
		
		// The stack will be empty if its height is lower than 1f
		if(nextHeight < 1f)
			return;
						
		for(float height = 0f; height + currentObjectHeight <= nextHeight; height += currentObjectHeight)
		{
			ShiftABGameObject nextObject = GenerateNextObject(stackIndex, ref shiftGameObjects);
			if(nextObject == null)
				break;
		
			stack.AddLast(nextObject);

			currentObjectHeight = nextObject.GetBounds().size.y;
		}
	}
	
	protected void FixLevelSize(ref List<LinkedList<ShiftABGameObject>> shiftGameObjects) {
		
		int n = shiftGameObjects.Count;
		
		for(int i = n - 1; i >= 0; i--)
		{
			if(GetLevelBounds(shiftGameObjects).size.x > _levelPlayableWidth)
			{	
				// shiftGameObjects[i].Clear();
				shiftGameObjects.Remove(shiftGameObjects[i]);
			}
		}
	}

	protected List<LinkedList<ShiftABGameObject>> GenerateRandomLevel() {
		
		List<LinkedList<ShiftABGameObject>> shiftGameObjects = new List<LinkedList<ShiftABGameObject>>();
		
		int stackIndex = 0;
		int totalObjsAdded = 0;
		float widerObjectInStack = 0;
			
		float randomWidth = Mathf.Abs(ABMath.RandomGaussian(_levelPlayableWidth/2f, _levelPlayableWidth/2f));
		float nextWidth = (_widthOfEmptyStack + randomWidth) % _levelPlayableWidth;
		
		// Loop to generate the game object stacks		
		for(float width = 0f; width + widerObjectInStack < nextWidth; width += widerObjectInStack)
		{
			// Randomly generate new stack based on the dependency graph
			shiftGameObjects.Add(new LinkedList<ShiftABGameObject>());
			
			// Build a new stack
			GenerateNextStack(stackIndex, ref shiftGameObjects);
			
			// Add pigs to the level structure
			InsertPigs(stackIndex, ref shiftGameObjects);	
			
			widerObjectInStack = GetStackWidth(shiftGameObjects[stackIndex]);
			totalObjsAdded += shiftGameObjects[stackIndex].Count;
			stackIndex++;
		}
		
		if(totalObjsAdded == 0)
		{
			int randomStack = Random.Range(0, shiftGameObjects.Count);
			GenerateNextStack(randomStack, ref shiftGameObjects);
			InsertPigs(randomStack, ref shiftGameObjects);
		}
		
		FixLevelSize(ref shiftGameObjects);
		
		return shiftGameObjects;
	}

	protected void InsertPigs(int i, ref List<LinkedList<ShiftABGameObject>> shiftGameObjects)
	{
		int pigsAdded = 0;
		
		if(shiftGameObjects[i].Count == 0)
			return;

		for (LinkedListNode<ShiftABGameObject> obj = shiftGameObjects[i].First; obj != shiftGameObjects[i].Last.Next; obj = obj.Next)
		{			
			if(obj.Value.Type == 0 && !obj.Value.IsDouble)
			{
				if(pigsAdded > 0 && Random.value < 0.5f)
					continue;
				
				// Check if pig dimensions fit inside the box 
				if(Mathf.Abs (obj.Value.GetEmptyScapeInside().y - obj.Value.UnderObjectsHeight) > GameWorld.Instance._pig.renderer.bounds.size.y)
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
						else
							shiftGameObjects[i].AddFirst(pigNode);
					}
					
					pigsAdded++;
					obj.Value.AddObjectInside(pig);
				}
			}
			
			if(obj == shiftGameObjects[i].Last)
			{	
				if(pigsAdded > 0 && Random.value < 0.5f)
					continue;
				
				// If last element in stack is already a circle, replace it with a pig
				if(shiftGameObjects[i].Last.Value.Type == 1)
				{
					ShiftABGameObject pig = new ShiftABGameObject();
					pig.Label = GameWorld.Instance.Templates.Length;
					shiftGameObjects[i].Last.Value = pig;
					
					pigsAdded++;
				}
				else
				{	
					if(pigsAdded > 0)
						break;
					
					ShiftABGameObject pig = new ShiftABGameObject();
					pig.Label = GameWorld.Instance.Templates.Length;
					LinkedListNode<ShiftABGameObject> pigNode = new LinkedListNode<ShiftABGameObject>(pig);	
						
					shiftGameObjects[i].AddLast(pigNode);
					pigsAdded++;
					
					break;
				}
			}
		}
	}

	protected List<ABGameObject> ConvertShiftGBtoABGB(List<LinkedList<ShiftABGameObject>> shiftGameObjects)
	{			
		float offsetX, offsetY;
		float slingPos = GameWorld.Instance._slingshot.position.x;
		offsetX = slingPos + _levelPlayableWidth/2f - GetLevelBounds(shiftGameObjects).size.x/2.25f;
		
		List<ABGameObject> gameObjects = new List<ABGameObject>();

		for(int i = 0; i < shiftGameObjects.Count; i++)
		{
			if(i > 0)
			{
				if(shiftGameObjects[i - 1].Count > 0)
				{
					ShiftABGameObject wid = GetWidestObjInStack(shiftGameObjects[i - 1]);
					offsetX = wid.Position.x + wid.GetBounds().size.x/2f;
				}
				else
					offsetX += _widthOfEmptyStack/2f;
			}
				
			if(shiftGameObjects[i].Count > 0)
			{	
				offsetX += GetWidestObjInStack(shiftGameObjects[i]).GetBounds().size.x/2f;
				
				for (LinkedListNode<ShiftABGameObject> obj = shiftGameObjects[i].First; obj != shiftGameObjects[i].Last.Next; obj = obj.Next)
				{
					// Define object position 
					if(obj.Previous != null)

						offsetY = obj.Previous.Value.Position.y + 
							obj.Previous.Value.GetBounds().size.y/2f - obj.Value.UnderObjectsHeight;
					else
						offsetY = GameWorld.Instance._ground.collider2D.bounds.center.y + 
							GameWorld.Instance._ground.collider2D.bounds.size.y/2f;
					
					offsetY += obj.Value.GetBounds().size.y/2f;
					obj.Value.Position = new Vector2(offsetX, offsetY);

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
				offsetX += _widthOfEmptyStack/2f;
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
			width += GetStackWidth(shiftGameObjects[i]);
			float columnHeight = GetStackHeight(shiftGameObjects[i]);
			
			if(columnHeight > height)
				height = columnHeight;
		}
		
		return new Bounds(Vector2.zero, new Vector2(width, height));
	}
		
	protected float GetLevelDensity(List<LinkedList<ShiftABGameObject>> shiftGameObjects)
	{
		float width = 0f;
				
		for(int i = 0; i < shiftGameObjects.Count; i++)
		{		
			ShiftABGameObject widestObj = GetWidestObjInStack(shiftGameObjects[i]);
						
			if(widestObj != null)
				width += widestObj.GetBounds().size.x;
		}
		
		return width/_levelPlayableWidth;
	}
	
	protected float GetLevelLinearity(List<LinkedList<ShiftABGameObject>> shiftGameObjects)
	{
		float max_variance = 0f;
		float variance = 0f;

		float []stacksHeight = new float[shiftGameObjects.Count];
				
		// creating array 
		for(int i = 0; i < shiftGameObjects.Count; i++)
			stacksHeight[i] = GetStackHeight(shiftGameObjects[i]);
		
		float y_i_avg = ABMath.Average(stacksHeight);
		
		for(int i = 0; i < shiftGameObjects.Count; i++)
		{	
			variance  += (stacksHeight[i] - y_i_avg) * (stacksHeight[i] - y_i_avg);
			max_variance += (stacksHeight[i] - _levelPlayableHeight) * (stacksHeight[i] - _levelPlayableHeight);
		}
							
		return 1f - Mathf.Sqrt(variance)/Mathf.Sqrt(max_variance);
	}
	
	protected float GetStackHeight(LinkedList<ShiftABGameObject> shiftGameObjects)
	{
		float columnHeight = 0f;
		
		if(shiftGameObjects.Count > 0)
		{
			for (LinkedListNode<ShiftABGameObject> obj = shiftGameObjects.First; obj != shiftGameObjects.Last.Next; obj = obj.Next)
				columnHeight += obj.Value.GetBounds().size.y;
		}
		
		return columnHeight;
	}
	
	protected float GetStackWidth(LinkedList<ShiftABGameObject> shiftGameObjects)
	{		
		ShiftABGameObject wid = GetWidestObjInStack(shiftGameObjects);
		if(wid != null)
			return wid.GetBounds().size.x;
			
		return _widthOfEmptyStack;
	}
	
	protected ShiftABGameObject GetWidestObjInStack(LinkedList<ShiftABGameObject> shiftGameObjects)
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
		
		if(stack.Count > 0)
		{
			objectBelow = stack.Last.Value;
		}
		else
		{
			// If the object below is the ground
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
			nextObject.UnderObjectsHeight = 0f;
			nextObject.IsDouble = false;
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
