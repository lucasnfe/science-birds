using UnityEngine;
using System.Collections.Generic;
/** \class  RandomLG
 *  \brief  Creates a level by generating objects and pigs at random and randomly creating the stacks
 *
 *  Contains block types definition, dependency graph, method to generate random level, random objects, random stacks,
 *  define object label, randomly add pigs, convert the shiftABLevel to a playable AB level. Also has a function to copy stacks.
 */
public class RandomLG : ABLevelGenerator {

    /**The types of block: box, circle, rectangle and podium*/
	enum BlockTypes{
		Box,
		Circle,
		Rect,
		Podium
	}
    /**Dependency graph that is used to build a feasible stack*/
	static readonly int[,] _gameObjectsDependencyGraph = {
		{1, 1, 1, 1},
		{1, 0, 1, 1},
		{1, 0, 1, 1},
		{1, 0, 1, 1}
	};

    /**Probability to duplicate an object*/
	public float _duplicateProbability = 0f;
    /**
     *  Converts the tag string to the corresponding int indicating the block type
     *  @param[in]  tag String containing tag of object type
     *  @return int Integer corresponding to the type of the block, -1 if no corresponding tag
     */
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

    /**
     *  Generates the level, calling the GenerateRandomLevel() method and converting it from ShiftGB to ABGB
     *  @return ABLevel A randomly generated Angry Birds Level.
     */
	public override ABLevel GenerateLevel()
	{
		ShiftABLevel randomLevel = GenerateRandomLevel();
		ConvertShiftGBtoABGB(ref randomLevel);
		
		return randomLevel;
	}

    /**
     *  Generates a level randomly by first generating number of birds, then the width of the level, 
     *  then generates random stacks until width is reached. If no objects were added, add one random stack
     *  at one random index from the already created stacks. Them, fix the level size.
     *  @return ShiftABLevel    Random Generated Level
     */
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

    /**
     *  Generates a random stack, creates the height randomly, adds randomly chosen objects
     *  until height is reached, and them randomly adds pigs.
     *  @param[in]  levelPlayableHeight         Max height the playable level has.
     *  @param[in]  levelPlayableWidth          Max width the playable level has.
     *  @param[in]  widthOfEmptyStack           The width and empty stack has.
     *  @return LinkedList<ShiftABGameObject>   The randomly generated stack
     */
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

    /**
     *  Generates a random object in the given stack by calling DefineObjectLabel()
     *  @param[out] stack               The stack to add the object in
     *  @return     ShiftABGameObject   The added object in the stack, null if no object added.
     */
	ShiftABGameObject GenerateObject(ref LinkedList<ShiftABGameObject> stack)
	{
		// Generate next object in the stack
		ShiftABGameObject nextObject = new ShiftABGameObject();
		
		if(!DefineObjectLabel(ref nextObject, ref stack))
			return null;
		
		return nextObject;
	}

    /**
     *  If no object is under this one, generates a random label for the given object in the stack, 
     *  and if the object is not of type podium or circle, ramdonly chooses if it will be duplicated, 
     *  based on duplicate probability and returns true.
     *  If there are objects under the new one, check if there will be stability problems, if it is a box object
     *  Check if the object under this one can be enclosed by it. If it can, adds the object under it now
     *  inside it; After this, check if object below the box is the ground or a bigger one, if it is any ot them, 
     *  the new object is safe, return true, else, remove the new object from the list of stackable objects and
     *  return false.
     *  If the object is not a box, checks if not a podium or circle and if not any of them, randomly chooses if 
     *  it will be doubled. Then check if object bellow is bigger, if it is, return true, else, remove the new object
     *  from the list of stackable objects and return false.
     *  
     *  @param[out] nextObject     A game object to generate a label to and maybe duplicate
     *  @param[out] stack          Stack with the object that the label will be defined.
     *  @return     bool           True if object with the chosen label can be stacked, false otherwise. 
     */
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

    /**
     *  Tries to insert pigs on stack, beginning from the first position of the stack to the last.
     *  If a pig has been added "throws a coin" and see if can add another. If no pig or can add another,
     *  try to check if its dimensions fit inside the box.
     *  If the pig fits, creates a block with the pig label, and a new pig node on the linked list, adds it
     *  after the previous object if the actual analised object has a different value than the first object in stack
     *  else, adds it in the first position of stack, also, if a pig is added, adds it inside the box.
     *  If not a box, check if in last position of the stack is a circle, if it is, replace it with a pig.
     *  If not a circle and a pig is already added, don't add anymore, if no pigs yet, creates a pig node
     *  And adds it to the last position of the stack.
     *  @param[out] stack   Stack in which the pigs will be added.
     */
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

    /**
     *  Calculates the position of the stacks from the ShiftABLevel to a real level, changing the X and Y position
     *  Of them, adding space equals to the widest object of the actual stack between the previous one and it, or
     *  Adding another empty stack width if there is an empty stack. And also offsetting the Y position based
     *  On the height of the ground. Also makes the right adjustments if the object is a double, changing the X
     *  Position of them to place them side to side.
     *  Each object with the position altered is them added to the randomLevel as an ABGameObject.
     *  @param[out] randomLevel The level with the stacks to convert to a playable AB level.
     */
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

    /**
     *  Copies a stack, if empty, just return the new instantiated stack,
     *  else, for each object in original stack, creates a new object with the same attributes and add it to the
     *  copy stack in the same position.
     *  Then, returns the copy stack.
     *  @param[in]  shiftGameObjects                Stack to be copied.
     *  @return     LinkedList<ShiftABGameObject>   Copy Stack.
     */
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

    /**
     *  Creates a list with the index of all objects which can be placed above the passed object.
     *  Checks all the tags for the current object if their index matched with the object below them in
     *  the dependency graph is equal 1. If so, adds the index of templates for this tag to the list.
     *  If not, doesn't add. Returns the list.
     *  @param[in]  objectBelow The object we want to check which tags are stackable with
     *  @return     List<int>   List with the indexes of stackable objects with the passed one.
     */
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
