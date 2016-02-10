using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(ABBlock))]
public class BlockEditor : Editor {

	// Update is called once per frame
	public override void OnInspectorGUI() {

		DrawDefaultInspector ();

		ABBlock block = (ABBlock)target;
		UpdateBlockMaterial (block, block._material);
	}

	public static void UpdateBlockMaterial(ABBlock block, MATERIALS material) {

		block._material = material;

		if (material == MATERIALS.wood)
			block.GetComponent<SpriteRenderer> ().sprite = block.GetComponent<ABBlock> ()._woodSprites [0];

		else if (material == MATERIALS.stone)
			block.GetComponent<SpriteRenderer> ().sprite = block.GetComponent<ABBlock> ()._stoneSprites [0];

		else if (material == MATERIALS.ice)
			block.GetComponent<SpriteRenderer> ().sprite = block.GetComponent<ABBlock> ()._iceSprites [0];
	}
}
