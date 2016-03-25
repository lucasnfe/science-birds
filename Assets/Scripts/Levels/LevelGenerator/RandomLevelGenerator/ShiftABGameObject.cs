using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/** \class ShiftABGameObject
 *  \brief  Contains utilities for shifting the place of a game object in the world
 *
 *  Contains a random level generator, the label and type of object, the heigth of the object from the ground.
 *  also a method to check empty space inside object, to check its bounds, add an object inside it get its hash code
 *  and comparing methods.
 */
public class ShiftABGameObject : ABGameObject
{
    /**A Random Level Generator object*/
	protected RandomLG _randomLG;
    /**Constructor setting the random level generator from the level generator game object component*/
	public ShiftABGameObject()
	{
		_randomLG = GameObject.Find("LevelGenerator").GetComponent<RandomLG>();
	}
	/**Accessor to check if object is a double object or not*/
	public bool IsDouble{ get; set; }

    /**type of the object*/
	private int _type;
    /*Accessor for type variable**/
	public int Type 
	{ 
		get{ return _type; } 
	}
    /**Total heigth of objects counting the ones under it from the ground*/
	private float _underObjectsHeight;
    /**Accessor for the under objects height variable*/
	public float UnderObjectsHeight 
	{ 
		get{ return _underObjectsHeight; } 
		set{ _underObjectsHeight = value; }
	}
	/**Accessor for label, in the setter, if object not block nor pig throws exception*/
	public override int Label
	{
		get { return base.Label; }
		set
		{ 
			base.Label = value;
			
			if(value < GameWorld.Instance.Templates.Length)
				_type = _randomLG.GetTypeByTag(GameWorld.Instance.Templates[value].tag); 
			else if(value == GameWorld.Instance.Templates.Length)
				_type = _randomLG.GetTypeByTag(GameWorld.Instance._pig.tag);
			else	
				 throw new System.InvalidOperationException("Invalid label.");
		}
	}
	
    /**
     *  Calculates the empty space inside a box block.
     *  @return Vector2 BoxCollider2D size, representing the empty space inside the box.
     */
	public Vector2 GetEmptyScapeInside()
	{		
		if(Label < GameWorld.Instance.Templates.Length)
		{
			BoxCollider2D emptyScape = GameWorld.Instance.Templates[Label].GetComponent<BoxCollider2D>();
			
			if(emptyScape != null && emptyScape.isTrigger)
				return emptyScape.size;
		}
		
		return Vector2.zero;
	}

    /**
     *  Gets the bounds of the block, if a duplicated block, doubles the x size.
     *  @return Bounds  Object containing the bounds of the block
     */
	public override Bounds GetBounds()
	{
		Bounds baseBounds = base.GetBounds();
		
		if(IsDouble)
		{
			Vector2 doubleSize = baseBounds.size;
			doubleSize.x *= 2f;
			baseBounds.size = doubleSize;
		}
		
		return baseBounds;
	}
	
    /**
     *  Adds an object inside this one. Sums the height of it to the height of objects under this one.
     *  @param[in]  obj ShiftABGameObject containing object to be added.
     */
	public void AddObjectInside(ShiftABGameObject obj)
	{
		_underObjectsHeight += obj.GetBounds().size.y;
	}
	
    /**
     *  Gets the hash code for the object, based on prime number 17, its label, its type and if is a double.
     *  @return int The hash code of the block.
     */
	public override int GetHashCode()
	{
	    const int prime = 17;				
		
		int hash = 1;
		
		hash = hash * prime + Label;
		hash = hash * prime + Type;
		hash = hash * prime + (IsDouble ? 1 : 0);
	
		return hash;
	}
	
    /**
     *  Compares this object with another ShiftABGameObject
     *  Uses the label and if is double and the type for comparison
     *  @return bool    true if both objects are equal, false otherwise
     */
    public bool Equals(ShiftABGameObject otherGen)
   	{		
        // if (ReferenceEquals(null, otherGen)) return false;
        // if (ReferenceEquals(this, otherGen)) return true;
		return (Label == otherGen.Label && IsDouble == otherGen.IsDouble && Type == otherGen.Type);
    }

    /**
     *  Compares this object with another object, using a cast to this class and using the Equals() method.
     *  @return bool    true if both objects are equal, false otherwise
     */
    public override bool Equals(object obj)
	{
	    // Since our other Equals() method already compares guys, we'll just call it.
	    // if (!(obj is ShiftABGameObject)) return false;
	    return Equals((ShiftABGameObject) obj);
	}
}