using UnityEngine;
using System.Collections;
/** \class LevelSource
 *  \brief  Abstract class containing basic information for Levels
 *
 *  Contains Current Level getter and setter, Level Limit getter and a Next Level getter
 */
public abstract class LevelSource : MonoBehaviour {

    /**Getter and setter for actual level*/
	public static int CurrentLevel { get; set; }
    /**Returns the limit of the level*/
	public virtual int LevelLimit() {

		return 0;
	}
    /**Adds 1 to the CurrentLevel and returns it*/
	public virtual ABLevel NextLevel() {

		CurrentLevel = CurrentLevel + 1;
		return null;
	}
}
