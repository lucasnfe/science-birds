using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class AngryBirdsGen
{
	public ShiftABLevel level;

	public AngryBirdsGen()
	{
		level = new ShiftABLevel();
	}

	public override int GetHashCode()
	{
		unchecked {
		    const int prime = 17;

			int hash = prime + level.birdsAmount;

			// get hash code for all items in array
			for(int i = 0; i < level.GetStacksAmount(); i++)
			{
				int subHash = 0;
			
				if(level.GetStack(i).Count > 0)
				
					for(LinkedListNode<ShiftABGameObject> obj1 = level.GetStack(i).First; obj1 != level.GetStack(i).Last.Next; obj1 = obj1.Next)
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
		if(level.birdsAmount != otherGen.level.birdsAmount)
			return false;

		// the amount of stacks must be the same
		if(level.GetTotalObjectsAmount() != otherGen.level.GetTotalObjectsAmount())
			return false;

		for(int i = 0; i < level.GetStacksAmount() && i < otherGen.level.GetStacksAmount(); i++)
		{
			// the height of each stack must be the same
			if(level.GetStack(i).Count != otherGen.level.GetStack(i).Count)
				return false;
			
			if(level.GetStack(i).Count > 0)
			{
				LinkedListNode<ShiftABGameObject> obj1 = level.GetStack(i).First;
				LinkedListNode<ShiftABGameObject> obj2 = otherGen.level.GetStack(i).First;

				for(; obj1 != level.GetStack(i).Last.Next && obj2 != otherGen.level.GetStack(i).Last.Next; obj1 = obj1.Next, obj2 = obj2.Next)
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