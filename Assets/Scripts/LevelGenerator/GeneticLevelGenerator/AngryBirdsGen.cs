using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AngryBirdsGen
{
	public int birdsAmount;
	public List<LinkedList<ShiftABGameObject>> gameObjects;

	public AngryBirdsGen()
	{
		birdsAmount = 0;
		gameObjects = new List<LinkedList<ShiftABGameObject>>();
	}

	public override int GetHashCode()
	{
		unchecked {
		    const int prime = 17;

			int hash = prime + birdsAmount;

			// get hash code for all items in array
		   	for(int i = 0; i < gameObjects.Count; i++)
			{
				int subHash = 0;
			
				if(gameObjects[i].Count > 0)
				
					for(LinkedListNode<ShiftABGameObject> obj1 = gameObjects[i].First; obj1 != gameObjects[i].Last.Next; obj1 = obj1.Next)
						subHash = subHash * 3 + obj1.Value.GetHashCode();
					
				hash = hash * prime + subHash;
			}

			return hash;
		}
	}

    public bool Equals(AngryBirdsGen otherGen)
   	{
        // if (ReferenceEquals(null, otherGen)) return false;
        // if (ReferenceEquals(this, otherGen)) return true;
		
		// the amount of birds must be the same
		if(birdsAmount != otherGen.birdsAmount)
			return false;

		// the amount of stacks must be the same
		if(gameObjects.Count != otherGen.gameObjects.Count)
			return false;

	   	for(int i = 0; i < gameObjects.Count && i < otherGen.gameObjects.Count; i++)
		{
			// the height of each stack must be the same
			if(gameObjects[i].Count != otherGen.gameObjects[i].Count)
				return false;
			
			if(gameObjects[i].Count > 0)
			{
				LinkedListNode<ShiftABGameObject> obj1 = gameObjects[i].First;
				LinkedListNode<ShiftABGameObject> obj2 = otherGen.gameObjects[i].First;

				for(; obj1 != gameObjects[i].Last.Next && obj2 != otherGen.gameObjects[i].Last.Next; obj1 = obj1.Next, obj2 = obj2.Next)
				{
					if(!obj1.Value.Equals(obj2.Value))
						return false;
				}
			}
		}

		return true;
   }
 
	public override bool Equals(object obj)
	{
	    // Since our other Equals() method already compares guys, we'll just call it.
	    // if (!(obj is AngryBirdsGen)) return false;
	    return Equals((AngryBirdsGen)obj);
	}
}