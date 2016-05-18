using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MATERIALS {
	wood, 
	stone, 
	ice 
};

public enum BIRDS  { 
	Red, 
	Green, 
	Blue 
};

public enum PIGS   { 
	BasicSmall, 
	BasicMedium, 
	BasicLarge 
};

public enum BLOCKS { 
	Circle, 
	CircleSmall, 
	RectBig, 
	RectFat, 
	RectMedium, 
	RectSmall, 
	RectTiny,
	SquareHole, 
	SquareSmall,
	SquareTiny,
	Triangle,
	TriangleHole 
};

public enum SLINGSHOT_LINE_POS
{
	SLING,
	BIRD
};

public class ABConstants {

	public static readonly Vector3 SLING_SELECT_POS = new Vector3 (-7.62f, -1.24f, 1f);

	#if UNITY_EDITOR

	public static readonly float MOUSE_SENSIBILITY = 1f;
	public static readonly string LEVELS_FOLDER = "/StreamingAssets/Levels";

	#elif UNITY_STANDALONE_OSX

	public static readonly float MOUSE_SENSIBILITY = 5f;
	public static readonly string LEVELS_FOLDER = "/Resources/Data/StreamingAssets/Levels";

	#elif UNITY_STANDALONE_WIN

	public static readonly float MOUSE_SENSIBILITY = 25f;
	public static readonly string LEVELS_FOLDER = "/StreamingAssets/Levels";

	#endif
}

public class ABWorldAssets {

	public static readonly GameObject[] WOOD_DESTRUCTION_EFFECT  = Resources.LoadAll<GameObject>("Prefabs/GameWorld/Particles/Wood");
	public static readonly GameObject[] STONE_DESTRUCTION_EFFECT = Resources.LoadAll<GameObject>("Prefabs/GameWorld/Particles/Stone");
	public static readonly GameObject[] ICE_DESTRUCTION_EFFECT   = Resources.LoadAll<GameObject>("Prefabs/GameWorld/Particles/Stone");

	public static readonly Dictionary<string, GameObject> BIRDS = LevelLoader.LoadABResource ("Prefabs/GameWorld/Characters/Birds");
	public static readonly Dictionary<string, GameObject> PIGS = LevelLoader.LoadABResource ("Prefabs/GameWorld/Characters/Pigs");
	public static readonly Dictionary<string, GameObject> BLOCKS = LevelLoader.LoadABResource ("Prefabs/GameWorld/Blocks");

	public static readonly GameObject PLATFORM = (GameObject) Resources.Load ("Prefabs/GameWorld/Platform");
	public static readonly GameObject SCORE_POINT = (GameObject) Resources.Load ("Prefabs/GameWorld/ScorePoints");

	public static readonly AudioClip[] WOOD_DAMAGE_CLIP  = Resources.LoadAll<AudioClip>("Audio/Blocks/Wood");
	public static readonly AudioClip[] STONE_DAMAGE_CLIP = Resources.LoadAll<AudioClip>("Audio/Blocks/Stone");
	public static readonly AudioClip[] ICE_DAMAGE_CLIP   = Resources.LoadAll<AudioClip>("Audio/Blocks/Stone");

	public static readonly PhysicsMaterial2D WOOD_MATERIAL  = Resources.Load("Materials/Wood") as PhysicsMaterial2D;
	public static readonly PhysicsMaterial2D STONE_MATERIAL = Resources.Load("Materials/Stone") as PhysicsMaterial2D;
	public static readonly PhysicsMaterial2D ICE_MATERIAL   = Resources.Load("Materials/Ice") as PhysicsMaterial2D;
}
