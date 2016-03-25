using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/** \class ShiftABLevel
 *  \brief  Contains methods to build the level and calculate relevant information about it
 *
 *  Contains level widht, height, width of empty stack, methods to add stacks, set new stack on stack index,
 *  get stack, get amount of stacks, count total of objects, calculate the level bounds, its density, linearity, 
 *  Stacks height and width, get widest object in stack and fix the level size.
 */
public class ShiftABLevel : ABLevel{
    /**List containing all the stacks of blocks (and pigs), each stack is a linked list of ShiftABGameObject*/
	List<LinkedList<ShiftABGameObject>> _shiftGameObjects;
    /**Width of only the playable area of the level, aka, the area which blocks can be created*/
	float _levelPlayableWidth;
    /**Accessor for level playable width variable*/
	public float LevelPlayableWidth {
		get { return _levelPlayableWidth; }
	}
	/**Height of the playable area of the level*/
	const float _levelPlayableHeight = 12f;
    /**Accessor for the level playable height variable*/
	public float LevelPlayableHeight {
		get { return _levelPlayableHeight; }
	}
    /**width of an empty stack*/
	const float _widthOfEmptyStack = 2f;
    /**Accessor for width of empty stack variable*/
	public float WidthOfEmptyStack {
		get { return _widthOfEmptyStack; }
	}
    /**
    *   Constructor method, sets the ground transform object from the game world instance, sets the level left bound
    *   The sling distance from the left bound, the playable width of the level, and creates the list of stacks of objects
    *   (_shiftGameObjects)
    */
	public ShiftABLevel() 
	{
		GameObject ground = GameWorld.Instance._groundTransform.gameObject;	
		
		float levelLeftBound = ground.transform.position.x - ground.GetComponent<Collider2D>().bounds.size.x/2f;
		float slingDistFromLeftBound = GameWorld.Instance._slingshotTransform.position.x - levelLeftBound;
		
		_levelPlayableWidth = (ground.GetComponent<Collider2D>().bounds.size.x - slingDistFromLeftBound) - 1f;

		_shiftGameObjects = new List<LinkedList<ShiftABGameObject>>();
	}
    /**
     *  Adds a stack to the list of stacks
     *  @param[in]  stack   Linked lisk of ShiftABGameObject containing a stack of objects
     */
	public void AddStack(LinkedList<ShiftABGameObject> stack)
	{
		_shiftGameObjects.Add (stack);
	}

    /**
     *  Sets a stack in the given index
     *  @param[in]  stackIndex  index where to set the given stack
     *  @param[in]  stack   Linked lisk of ShiftABGameObject containing a stack of objects
     */
    public void SetStack(int stackIndex, LinkedList<ShiftABGameObject> stack)
	{
		if(stackIndex < _shiftGameObjects.Count)
			_shiftGameObjects[stackIndex] = stack;
	}
    /**
     *  If a valid index is given, returns the stack contained in that index.
     *  @param[in] stackIndex   index of the stack to e returned
     *  @return LinkedList<ShiftABGameObject>   linked list containing a stack of objects, null if invalid index
     */
	public LinkedList<ShiftABGameObject> GetStack(int stackIndex)
	{
		if(stackIndex < _shiftGameObjects.Count)
			return _shiftGameObjects[stackIndex];

		return null;
	}
    /**
     *  Gets the total number of stacks in the level.
     *  @return int Number of stacks.
     */
	public int GetStacksAmount()
	{
		return _shiftGameObjects.Count;
	}

    /**
     *  Counts and returns the total of objects in the level
     *  @return int Total of objects in the level
     */
	public int GetTotalObjectsAmount()
	{
		int objectsAmount = 0;

		for(int i = 0; i < _shiftGameObjects.Count; i++)
			objectsAmount += _shiftGameObjects[i].Count;

		return objectsAmount;
	}

    /**
     *  Calculates the level bounds by summing the width of each stack and getting the height of the highest stack.
     *  @return Bounds  Bound of the level begining in (0,0) and ending with the calculated width and height
     */
	public Bounds GetLevelBounds()
	{
		float width = 0f;
		float height = 0f;
		
		for(int i = 0; i < _shiftGameObjects.Count; i++)
		{		
			width += GetStackWidth(i);
			float columnHeight = GetStackHeight(i);
			
			if(columnHeight > height)
				height = columnHeight;
		}
		
		return new Bounds(Vector2.zero, new Vector2(width, height));
	}
	
    /**
     *  Calculates the level density based on the sum of the largest object's width of each stack, divided by
     *  The level playable width
     *  @return float   density of the level
     */
	public float GetLevelDensity()
	{
		float width = 0f;
		
		for(int i = 0; i < _shiftGameObjects.Count; i++)
		{		
			ShiftABGameObject widestObj = GetWidestObjInStack(i);
			
			if(widestObj != null)
				width += widestObj.GetBounds().size.x;
		}
		
		return width/_levelPlayableWidth;
	}
	
    /**
     *  Calculates the linearity of the level by calculating 1 - the variance of heights divided by the max variance
     *  @return float   The linearity of the level.
     */
	public float GetLevelLinearity()
	{
		float max_variance = 0f;
		float variance = 0f;
		
		float []stacksHeight = new float[_shiftGameObjects.Count];
		
		// creating array 
		for(int i = 0; i < _shiftGameObjects.Count; i++)
			stacksHeight[i] = GetStackHeight(i);
		
		float y_i_avg = ABMath.Average(stacksHeight);
		
		for(int i = 0; i < _shiftGameObjects.Count; i++)
		{	
			variance  += (stacksHeight[i] - y_i_avg) * (stacksHeight[i] - y_i_avg);
			max_variance += (stacksHeight[i] - _levelPlayableHeight) * (stacksHeight[i] - _levelPlayableHeight);
		}
		
		return 1f - Mathf.Sqrt(variance)/Mathf.Sqrt(max_variance);
	}
	
    /**
     *  Gets the total height of a stack by summing the height of each block
     *  @param[in]  stackIndex  index of the stack to calculate the height of
     *  @return float   Total height of the stack
     */
	public float GetStackHeight(int stackIndex)
	{
		LinkedList<ShiftABGameObject> stack = _shiftGameObjects[stackIndex];

		float columnHeight = 0f;
		
		if(stack.Count > 0)
		{
			for (LinkedListNode<ShiftABGameObject> obj = stack.First; obj != stack.Last.Next; obj = obj.Next)
				columnHeight += obj.Value.GetBounds().size.y;
		}
		
		return columnHeight;
	}
	
    /**
     *  Gets the width of the widest object in the stack and returns it as the width of the stack
     *  @param[in]  stackIndex  index of the stack to calculate the width of
     *  @return float   Width of the stack
     */
	public float GetStackWidth(int stackIndex)
	{
		ShiftABGameObject wid = GetWidestObjInStack(stackIndex);
		if(wid != null)
			return wid.GetBounds().size.x;
		
		return _widthOfEmptyStack;
	}

    /**
     *  Gets the widest object in the stack
     *  @param[in]  stackIndex  index of the stack to calculate the widest object of
     *  @return ShiftABGameObject   Widest object in the stack
     */
    public ShiftABGameObject GetWidestObjInStack(int stackIndex)
	{
		LinkedList<ShiftABGameObject> stack = _shiftGameObjects[stackIndex];

		if(stack.Count == 0)
			return null;
		
		ShiftABGameObject widestObj = stack.First.Value;
		
		for (LinkedListNode<ShiftABGameObject> obj = stack.First; obj != stack.Last.Next; obj = obj.Next)
		{
			float stackedObjWidth = obj.Value.GetBounds().size.x;
			float widestObjWidth = widestObj.GetBounds().size.x;
			
			if(stackedObjWidth > widestObjWidth)
				widestObj = obj.Value;
		}
		
		return widestObj;
	}

    /**
     *  Fixes the level size by removing any stacks that surpasses the level playable width
     */
	public void FixLevelSize() 
	{
		for(int i = _shiftGameObjects.Count - 1; i >= 0; i--)
		{
			if(GetLevelBounds().size.x > _levelPlayableWidth)
			{	
				// shiftGameObjects[i].Clear();
				_shiftGameObjects.Remove(_shiftGameObjects[i]);
			}
		}
	}
}
