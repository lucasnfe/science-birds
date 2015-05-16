using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ShiftABLevel : ABLevel{

	List<LinkedList<ShiftABGameObject>> _shiftGameObjects;

	float _levelPlayableWidth;
	public float LevelPlayableWidth {
		get { return _levelPlayableWidth; }
	}
	
	const float _levelPlayableHeight = 12f;
	public float LevelPlayableHeight {
		get { return _levelPlayableHeight; }
	}

	const float _widthOfEmptyStack = 2f;
	public float WidthOfEmptyStack {
		get { return _widthOfEmptyStack; }
	}

	public ShiftABLevel() 
	{
		GameObject ground = GameWorld.Instance._groundTransform.gameObject;	
		
		float levelLeftBound = ground.transform.position.x - ground.GetComponent<Collider2D>().bounds.size.x/2f;
		float slingDistFromLeftBound = GameWorld.Instance._slingshotTransform.position.x - levelLeftBound;
		
		_levelPlayableWidth = (ground.GetComponent<Collider2D>().bounds.size.x - slingDistFromLeftBound) - 1f;

		_shiftGameObjects = new List<LinkedList<ShiftABGameObject>>();
	}

	public void AddStack(LinkedList<ShiftABGameObject> stack)
	{
		_shiftGameObjects.Add (stack);
	}

	public void SetStack(int stackIndex, LinkedList<ShiftABGameObject> stack)
	{
		if(stackIndex < _shiftGameObjects.Count)
			_shiftGameObjects[stackIndex] = stack;
	}

	public LinkedList<ShiftABGameObject> GetStack(int stackIndex)
	{
		if(stackIndex < _shiftGameObjects.Count)
			return _shiftGameObjects[stackIndex];

		return null;
	}

	public int GetStacksAmount()
	{
		return _shiftGameObjects.Count;
	}

	public int GetTotalObjectsAmount()
	{
		int objectsAmount = 0;

		for(int i = 0; i < _shiftGameObjects.Count; i++)
			objectsAmount += _shiftGameObjects[i].Count;

		return objectsAmount;
	}

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
	
	public float GetStackWidth(int stackIndex)
	{
		ShiftABGameObject wid = GetWidestObjInStack(stackIndex);
		if(wid != null)
			return wid.GetBounds().size.x;
		
		return _widthOfEmptyStack;
	}
	
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
