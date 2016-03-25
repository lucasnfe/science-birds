using UnityEngine;
using System.Collections;
/** \class LevelList
 *  \brief  Class containing an array of levels
 *
 *  Contains a list of levels, a list of levels source, a limit for the number of levels, a method to return next level
 *  in the list.
 */
public class LevelList : LevelSource {

    /**List of ABLevels*/
	static ABLevel []_levels;
    /**Max number of levels in list*/
	public int _listLimit;
    /**True if needs to shuffle the levels source (.xml file name) in the levelsSource list*/
	public bool _shuffle;
    /**list with the source of the levels (.xml file name)*/
	public TextAsset []_levelsSource;

    /**
     *  Getter for the max number of levels
     *  @return int limit of the list of levels.
     */
	public override int LevelLimit() {

		return _listLimit;
	}

	/**
     *  Gets the next level. If currently no level was loaded, creates the list of levels object with the length of
     *  the levels source array. Then, if it needs to be shuffled, shuffle the levels source array. Finally,
     *  For each level Source loads the Level from the xml file to the list of levels.
     *  If there already was a level, then checks if limit of levels has been reached, if not, gets the next level
     *  from the levels list, and adds 1 to its index, calling the base's (LevelSource) NextLevel() and returns the 
     *  object corresponding to the next level.
     *  @return ABLevel the next level to be played.    
     */
	public override ABLevel NextLevel() {
	
		if(CurrentLevel == 0) {
			_levels = new ABLevel[_levelsSource.Length];

			if(_shuffle)
				ABArrayUtils.Shuffle(_levelsSource);

			for(int i = 0; i < _levelsSource.Length; i++)
				_levels[i] = LevelLoader.LoadXmlLevel(_levelsSource[i].text);
		}

		if(CurrentLevel > _listLimit - 1)
			return null;

		ABLevel nextLevel = _levels[CurrentLevel];
		base.NextLevel();

		return nextLevel;
	}
}
