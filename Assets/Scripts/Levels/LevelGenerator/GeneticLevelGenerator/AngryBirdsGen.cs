using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
/** \class AngryBirdsGen
 *  \brief  Angry Birds GA's genome
 *
 *  Contains the genome of a level, the method to get its hash code and its comparison method.
 */
public class AngryBirdsGen
{
    /**level that the genome represents*/
	public ShiftABLevel level;
    /**constructor of the class, generates a new ShiftABLevel object*/
	public AngryBirdsGen()
	{
		level = new ShiftABLevel();
	}
    /**
    *   Gets the Hash Code of the level based on prime number 17, adding it to the amount of birds on level,
    *   then calculates a subhash based on all the items in level array, and adds it to the hash.
    *   @return int Hash Code for the current genome
    */
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
    /**
     *  Compares this genome to another. First comparing birds amount, then comparing amount of total objects.
     *  If both are equal, compares the height of each stack, and, if equals, the object at each position of each stack.
     *  If everything is equal, they are equal genomes.
     *  @param[in]  otherGen    AngyrBirdsGen to be compared with this one.
     *  @return bool    True if both genome are equal, false otherwise.
     */
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
    /**
     *  overrides the Equals() method and calls the one defined within this class
     *  @param[in]  obj Object to be compared, should be of type AngryBirdsGen.
     *  @return bool    True if passed object is equal to the actual object, false otherwhise.
     */
    public override bool Equals(object obj)
	{
	    // Since our other Equals() method already compares guys, we'll just call it.
	    // if (!(obj is AngryBirdsGen)) return false;
	    return Equals((AngryBirdsGen)obj);
	}
}