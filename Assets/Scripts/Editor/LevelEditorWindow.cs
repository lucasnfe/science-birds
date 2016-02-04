using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using System.Collections;

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

class LevelEditor : EditorWindow {

	public BIRDS  _birdsOps;
	public PIGS   _pigsOps;
	public BLOCKS _blocksOps;

	private static GameObject[] _birds;
	private static GameObject[] _pigs;
	private static GameObject[] _blocks;

	private static int _birdsAdded = 0;
	private static Vector3 _slingshotPos;
	private static Vector3 _groundPos;

	[MenuItem ("Window/Level Editor %l")]
	public static void ShowWindow () {
		
		UnityEditor.EditorWindow window = EditorWindow.GetWindow(typeof(LevelEditor));

		ToggleGizmos (false);

		HideLayer (LayerMask.NameToLayer("UI"));

		UnityEngine.Object[] objs1 = Resources.LoadAll("Prefabs/GameWorld/Characters/Birds");

		_birds = new GameObject[objs1.Length];
		for (int i = 0; i < objs1.Length; i++)
			_birds [i] = (GameObject)objs1 [i];

		UnityEngine.Object[] objs2 = Resources.LoadAll("Prefabs/GameWorld/Characters/Pigs");

		_pigs = new GameObject[objs2.Length];
		for (int i = 0; i < objs2.Length; i++)
			_pigs [i] = (GameObject)objs2 [i];

		UnityEngine.Object[] objs3 = Resources.LoadAll("Prefabs/GameWorld/Blocks");

		_blocks = new GameObject[objs3.Length];
		for (int i = 0; i < objs3.Length; i++)
			_blocks [i] = (GameObject)objs3 [i];


		_groundPos = new Vector3 (0f, -2.74f, 0f);
		_slingshotPos = new Vector3 (-7.62f, -1.24f, 1f);

		_birdsAdded = GameObject.Find ("Birds").transform.childCount;

		window.Show ();
	}

	void OnGUI()
	{
		if (GUILayout.Button ("Clear Level")) {

			ClearLevel ();
		}

		_birdsOps = (BIRDS) EditorGUILayout.EnumPopup("Bird to create:", _birdsOps);
		if (GUILayout.Button ("Create Bird", GUILayout.Width (80), GUILayout.Height (20))) {

			CreateBird ();
		}

		_pigsOps = (PIGS) EditorGUILayout.EnumPopup("Pig to create:", _pigsOps);
		if (GUILayout.Button ("Create Pig", GUILayout.Width (80), GUILayout.Height (20))) {

			GameObject pig = InstantiateGameObject (_pigs, (int)_pigsOps);
			pig.transform.parent = GameObject.Find ("Blocks").transform;
		}

		_blocksOps = (BLOCKS) EditorGUILayout.EnumPopup("Block to create:", _blocksOps);
		if (GUILayout.Button ("Create Block", GUILayout.Width (80), GUILayout.Height (20))) {

			GameObject block = InstantiateGameObject (_blocks, (int)_blocksOps);
			block.transform.parent = GameObject.Find ("Blocks").transform;
		}
			
		if (GUILayout.Button ("Save Level")) {

			
		}
	}

	GameObject InstantiateGameObject(GameObject[]source, int index) {

		GameObject cube = (GameObject)PrefabUtility.InstantiatePrefab (source[index]);
		cube.transform.position = Vector3.zero;

		return cube;
	}

	void ClearLevel() {

		_birdsAdded = 0;

		DestroyImmediate (GameObject.Find ("Birds").gameObject);
		GameObject birds = new GameObject ();
		birds.name = "Birds";
		birds.transform.parent = GameObject.Find ("GameWorld").transform;

		DestroyImmediate (GameObject.Find ("Blocks").gameObject);
		GameObject blocks = new GameObject ();
		blocks.name = "Blocks";
		blocks.transform.parent = GameObject.Find ("GameWorld").transform;
	}

	void CreateBird() {

		GameObject bird = InstantiateGameObject (_birds, (int)_birdsOps);
		bird.name = bird.name + "_" + _birdsAdded;
		bird.transform.parent = GameObject.Find ("Birds").transform;

		Vector3 birdsPos = _slingshotPos;

		// From the second Bird on, they are added to the ground
		if(_birdsAdded >= 1)
		{
			birdsPos.y = _groundPos.y;

			for(int i = 0; i < _birdsAdded; i++)
				birdsPos.x -= bird.GetComponent<SpriteRenderer>().bounds.size.x * 2f;
		}

		bird.transform.position = birdsPos;

		_birdsAdded++;
	}

	public static void ToggleGizmos(bool gizmosOn) {

		int val = gizmosOn ? 1 : 0;

		Assembly asm = Assembly.GetAssembly(typeof(Editor));
		Type type = asm.GetType("UnityEditor.AnnotationUtility");

		if (type != null) {

			MethodInfo getAnnotations = type.GetMethod("GetAnnotations", BindingFlags.Static | BindingFlags.NonPublic);
			MethodInfo setGizmoEnabled = type.GetMethod("SetGizmoEnabled", BindingFlags.Static | BindingFlags.NonPublic);
			MethodInfo setIconEnabled = type.GetMethod("SetIconEnabled", BindingFlags.Static | BindingFlags.NonPublic);

			var annotations = getAnnotations.Invoke(null, null);

			foreach (object annotation in (IEnumerable)annotations) {

				Type annotationType = annotation.GetType();
				FieldInfo classIdField = annotationType.GetField("classID", BindingFlags.Public | BindingFlags.Instance);
				FieldInfo scriptClassField = annotationType.GetField("scriptClass", BindingFlags.Public | BindingFlags.Instance);

				if (classIdField != null && scriptClassField != null) {
					int classId = (int)classIdField.GetValue(annotation);

					string scriptClass = (string)scriptClassField.GetValue(annotation);
					setGizmoEnabled.Invoke(null, new object[] { classId, scriptClass, val });
					setIconEnabled.Invoke(null, new object[] { classId, scriptClass, val });
				}
			}
		}
	}

	static void HideLayer(int layerNumber)
	{
		LayerMask layerNumberBinary = 1 << layerNumber;
		LayerMask blocksLayer = 1 << LayerMask.NameToLayer ("Blocks");
		LayerMask birdsLayer = 1 << LayerMask.NameToLayer ("Birds");
		LayerMask pigsLayer = 1 << LayerMask.NameToLayer ("Pigs");

		Tools.visibleLayers = ~layerNumberBinary; 
		Tools.lockedLayers = ~(blocksLayer | birdsLayer | pigsLayer);

		SceneView.RepaintAll();
	}
}