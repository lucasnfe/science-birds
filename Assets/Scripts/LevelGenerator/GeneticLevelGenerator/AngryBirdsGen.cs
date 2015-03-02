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
		gameObjects = new List<LinkedList<ShiftABGameObject>>();
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hash = 17;

			// get hash code for the birds amount
			hash = hash * 23 + birdsAmount.GetHashCode();

			// get hash code for all items in array
		   	for(int i = 0; i < gameObjects.Count; i++)
			{
				if(gameObjects[i].Count > 0)
				{
					for (LinkedListNode<ShiftABGameObject> obj = gameObjects[i].First; obj != gameObjects[i].Last.Next; obj = obj.Next)
					{
						hash = (hash * 23) + obj.Value.Label.GetHashCode();
						hash = (hash * 23) + obj.Value.IsDouble.GetHashCode();
					}
				}
				else
				{
					hash = (hash * 23) + i.GetHashCode();
				}
			}

			return hash;
		}
	}

    public bool Equals(AngryBirdsGen otherGen)
   	{		
		if(birdsAmount != otherGen.birdsAmount)
			return false;

		if(gameObjects.Count != otherGen.gameObjects.Count)
			return false;

	   	for(int i = 0; i < gameObjects.Count; i++)
		{
			if(gameObjects[i].Count != otherGen.gameObjects[i].Count)
				return false;
			
			if(gameObjects[i].Count > 0 && otherGen.gameObjects[i].Count > 0)
			{				
				LinkedListNode<ShiftABGameObject> obj1, obj2;
		
				for (obj1 = gameObjects[i].First, obj2 = otherGen.gameObjects[i].First; 
				     obj1 != gameObjects[i].Last.Next && obj2 != otherGen.gameObjects[i].Last.Next; 
					 obj1 = obj1.Next, obj2 = obj2.Next)
				{
					if(obj1.Value.Label != obj2.Value.Label)
						return false;

					if(obj1.Value.IsDouble != obj2.Value.IsDouble)
						return false;
				}
			}
		}

		return true;
   }

    public override bool Equals(object obj)
    {
        if (obj == null)
            return base.Equals(obj);

        if (!(obj is AngryBirdsGen))

            throw new InvalidCastException("The Object isn't of Type AngryBirdsGen.");
        else
            return Equals(obj as AngryBirdsGen);
    }

    public static bool operator ==(AngryBirdsGen person1, AngryBirdsGen person2)
    {
        return person1.Equals(person2);
    }

    public static bool operator !=(AngryBirdsGen person1, AngryBirdsGen person2)
    {
        return (!person1.Equals(person2));
    }
}