using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScoreHud : ABSingleton<ScoreHud> {

	private ABParticleSystem  _scoreEmitter;

	// Use this for initialization
	void Start () {
	
		_scoreEmitter = GetComponent<ABParticleSystem> ();
		_scoreEmitter.SetParciclesParent (transform);
	}

	public void SpawnScorePoint(uint point, Vector3 position) {

		ABParticle scoreParticle = _scoreEmitter.ShootParticle ();
		scoreParticle.transform.rotation = Quaternion.identity;
		scoreParticle.transform.position = position;

		Text pointText = scoreParticle.GetComponent<Text>();
		pointText.text = point.ToString();

		HUD.Instance.AddScore (point);
	}
}
