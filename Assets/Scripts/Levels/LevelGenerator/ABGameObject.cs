using UnityEngine;
using System;
using System.Collections;

/** \class ABGameObject
 *  \brief  Manages the game objects, their attributes and some util. methods
 *
 *  Contains objects position, label, check if is pig, get area, bounds, hashcode and check if equals another ABGameObject
 */
public class ABGameObject
{
    /**X, Y position of object*/
	private Vector2 _position;
    /**_position getter and setter*/
	public virtual Vector2 Position
	{ 
		get{ return _position; }
		set{ _position = value; }
	}
	/**label of the game object*/
	private int _label;
    /**_label getter and setter*/
	public virtual int Label
	{ 
		get{ return _label; }
		set{ _label = value; }
	}
    /**Check if this object is a Pig
     *  @return bool true if is a Pig, false otherwise
     */
	public bool IsPig() 
	{ 
		return (Label == GameWorld.Instance.Templates.Length); 
	}
	/**Gets the current object area, multiplying its X size by its Y size
     *  @return float Area of the object
     */
	public float GetArea()
	{
		return GetBounds().size.x * GetBounds().size.y;
	}
    /**Gets the bounds of the object
     *  If a Pig get its bounds, if not, check if object has children
     *  If it has, encapsulate each bound on the Bounds object
     *  If not, just get the object's bounds
     *  At the end, makes the center of the Bounds the object's Position
     *  @return Bounds  Bounds object containing the object (and its children) bounds
     */
	public virtual Bounds GetBounds()
	{
		Bounds composedBounds = new Bounds();

		if(IsPig())
		{
			composedBounds = GameWorld.Instance._pig.GetComponent<Renderer>().bounds;
		}
		else
		{
			GameObject composedObj = GameWorld.Instance.Templates[Label];
			
			if(composedObj.transform.childCount > 0)
			{
				SpriteRenderer[] allRenderers = composedObj.GetComponentsInChildren<SpriteRenderer>(true);
				
				foreach (SpriteRenderer rend in allRenderers) {
					
					composedBounds.Encapsulate(rend.bounds);
				}
			}
			else
			{
				SpriteRenderer objCollider = composedObj.GetComponent<SpriteRenderer>();
				composedBounds = objCollider.bounds;
			}
		}

		composedBounds.center = Position;

		return composedBounds;
	}

    /**Gets the hash code for the object, using prime number 13 and its Label and its X and Y position 
     *  as code parameters
     *  @return int hash code of the object
     */
    public override int GetHashCode()
	{
	    const int prime = 13;
		return Label * prime + (int)Position.x * prime + (int)Position.y * prime;
	}

    /**Gets the current object area, multiplying its X size by its Y size
     *  @return float Area of the object
     */
    public bool Equals(ABGameObject otherGen)
   	{
        if (ReferenceEquals(null, otherGen)) return false;
        if (ReferenceEquals(this, otherGen)) return true;

		return (Label == otherGen.Label && Position == otherGen.Position);
    }

    /**Compare this object with another one and check if they are equal
     *  @return bool True if they are equal, false otherwise
     */
    public override bool Equals(object obj)
	{
	    // Since our other Equals() method already compares guys, we'll just call it.
	    if (!(obj is ABGameObject)) return false;
	    return Equals((ABGameObject) obj);
	}
}
