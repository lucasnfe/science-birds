using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(Block))]
public class BlockEditor : Editor {

	// Update is called once per frame
	public override void OnInspectorGUI() {

		DrawDefaultInspector ();

		Block block = (Block)target;
		UpdateBlockMaterial (block, block._material);
	}

	public static void UpdateBlockMaterial(Block block, MATERIALS material) {

		block._material = material;

		if (material == MATERIALS.wood)
			block.GetComponent<SpriteRenderer> ().sprite = block.GetComponent<Block> ()._woodSprites [0];

		else if (material == MATERIALS.stone)
			block.GetComponent<SpriteRenderer> ().sprite = block.GetComponent<Block> ()._stoneSprites [0];

		else if (material == MATERIALS.ice)
			block.GetComponent<SpriteRenderer> ().sprite = block.GetComponent<Block> ()._iceSprites [0];
	}
}
