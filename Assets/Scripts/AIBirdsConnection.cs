// SCIENCE BIRDS: A clone version of the Angry Birds game used for 
// research purposes
// 
// Copyright (C) 2016 - Lucas N. Ferreira - lucasnfe@gmail.com
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>
//

﻿using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections;
using System.Collections.Generic;
using System;
using SimpleJSON;
using UnityEngine.SceneManagement;

delegate IEnumerator Handler(JSONNode data);

public class Message {

	public string data;
	public string time;
}

public class AIBirdsConnection : ABSingleton<AIBirdsConnection> {

	Dictionary<String, Handler> handlers;
	WebSocket socket;

	IEnumerator Click(JSONNode data) {

		yield return new WaitForEndOfFrame ();

		float clickX = data[2]["x"].AsFloat;
		float clickY = Screen.height - data[2]["y"].AsFloat;

		Vector2 clickPos = new Vector2 (clickX, clickY);

		HUD.Instance.SimulateInputEvent = 1;
		HUD.Instance.SimulateInputPos = clickPos;
		HUD.Instance.SimulateInputDelta = clickPos;

		string id = data [0];
		string message = "[" + id + "," + "{}" + "]";

	#if UNITY_WEBGL && !UNITY_EDITOR
		socket.Send(System.Text.Encoding.UTF8.GetBytes(message));	
	#else
		socket.Send(message);	
	#endif
	}

	IEnumerator Drag(JSONNode data) {

		yield return new WaitForEndOfFrame ();

		float dragX = data[2]["x"].AsFloat;
		float dragY = data[2]["y"].AsFloat;

		float dragDX = dragX + data[2]["dx"].AsFloat;
		float dragDY = dragY + data[2]["dy"].AsFloat;

		Vector2 dragPos = new Vector2 (dragX, Screen.height - dragY);
		Vector2 deltaPos = new Vector2 (dragDX, Screen.height - dragDY);

		Debug.Log ("POS = " + dragPos);
		Debug.Log ("DRAG = " + deltaPos);

		HUD.Instance.SimulateInputEvent = 1;
		HUD.Instance.SimulateInputPos = dragPos;
		HUD.Instance.SimulateInputDelta = deltaPos;

		string id = data [0];
		string message = "[" + id + "," + "{}" + "]";

	#if UNITY_WEBGL && !UNITY_EDITOR
		socket.Send(System.Text.Encoding.UTF8.GetBytes(message));
	#else
		socket.Send(message);	
	#endif

	}

	IEnumerator MouseWheel(JSONNode data) {

		yield return new WaitForEndOfFrame ();

		float delta = data[2]["delta"].AsFloat;

		HUD.Instance.CameraZoom (-delta);

		string id = data [0];
		string message = "[" + id + "," + "{}" + "]";


	#if UNITY_WEBGL && !UNITY_EDITOR
		socket.Send(System.Text.Encoding.UTF8.GetBytes(message));
	#else
		socket.Send(message);	
	#endif

	}

	IEnumerator Screenshot(JSONNode data) {

		yield return new WaitForEndOfFrame ();

		Texture2D screenshot = new Texture2D (Screen.width, Screen.height, TextureFormat.ARGB32, true);
		screenshot.ReadPixels (new Rect (0, 0, Screen.width, Screen.height), 0, 0, true);
		screenshot.Apply();

		string image = System.Convert.ToBase64String (screenshot.EncodeToPNG ());
	
		string id = data [0];

		Message msg = new Message ();
		msg.data = "data:image/png;base64," + image;
		msg.time = DateTime.Now.ToString ();

		string json = JsonUtility.ToJson (msg);
		string message = "[" + id + "," + json + "]";

	#if UNITY_WEBGL && !UNITY_EDITOR
		socket.Send(System.Text.Encoding.UTF8.GetBytes(message));
	#else
		socket.Send(message);	
	#endif

	}

	IEnumerator SelectLevel(JSONNode data) {

		yield return new WaitForEndOfFrame ();

		int levelIndex = data[2]["levelIndex"].AsInt;

		Debug.Log ("Level index:" + levelIndex);

		LevelList.Instance.SetLevel(levelIndex - 1);
		ABSceneManager.Instance.LoadScene ("GameWorld");

		string id = data [0];
		string message = "[" + id + "," + "{}" + "]";

	#if UNITY_WEBGL && !UNITY_EDITOR
		socket.Send(System.Text.Encoding.UTF8.GetBytes(message));
	#else
		socket.Send(message);	
	#endif

	}

	IEnumerator LoadScene(JSONNode data) {

		yield return new WaitForEndOfFrame ();

		string scene = data[2]["scene"];
		ABSceneManager.Instance.LoadScene (scene);

		string id = data [0];
		string message = "[" + id + "," + "{}" + "]";

	#if UNITY_WEBGL && !UNITY_EDITOR
		socket.Send(System.Text.Encoding.UTF8.GetBytes(message));
	#else
		socket.Send(message);	
	#endif

	}

	IEnumerator Score(JSONNode data) {

		yield return new WaitForEndOfFrame ();

		string id = data [0];

		Message msg = new Message ();
		msg.data = HUD.Instance.GetScore ().ToString();
		msg.time = DateTime.Now.ToString ();

		string json = JsonUtility.ToJson (msg);
		string message = "[" + id + "," + json + "]";

	#if UNITY_WEBGL && !UNITY_EDITOR
		socket.Send(System.Text.Encoding.UTF8.GetBytes(message));
	#else
		socket.Send(message);	
	#endif
	}

	IEnumerator GameState(JSONNode data) {

		yield return new WaitForEndOfFrame ();

		string id = data [0];

		string currentScence = SceneManager.GetActiveScene().name;

		if (currentScence == "GameWorld") {

			if (ABGameWorld.Instance.LevelCleared ()) {

				currentScence = "LevelCleared";
			} 
			else if (ABGameWorld.Instance.LevelFailed ()) {

				currentScence = "LevelFailed";
			}
		}

		Message msg = new Message ();
	
		msg.data = currentScence;
		msg.time = DateTime.Now.ToString ();

		string json = JsonUtility.ToJson (msg);
		string message = "[" + id + "," + json + "]";

	#if UNITY_WEBGL && !UNITY_EDITOR
		socket.Send(System.Text.Encoding.UTF8.GetBytes(message));
	#else
		socket.Send(message);	
	#endif
	}

	public void InitHandlers() {

		handlers = new Dictionary<string, Handler> ();

		handlers ["click"]        = Click;
		handlers ["drag"]         = Drag;
		handlers ["mousewheel"]   = MouseWheel;
		handlers ["screenshot"]   = Screenshot;
		handlers ["gamestate"]    = GameState;
		handlers ["loadscene"]    = LoadScene;
		handlers ["selectlevel"]  = SelectLevel;
		handlers ["score"]        = Score;
	}

	// Use this for initialization
	IEnumerator Start () {

		DontDestroyOnLoad (this.gameObject);

		InitHandlers ();

		socket = new WebSocket(new Uri("ws://localhost:9000/"));
		yield return StartCoroutine(socket.Connect());

		while (true) {
			
			string reply = socket.RecvString();
		
			if (reply != null) {

				JSONNode data = JSON.Parse(reply);

				string type = data [1];

//				Debug.Log("Received message: " + type);

				if (handlers[type] != null) {

					StartCoroutine(handlers [type] (data));
				} 
				else {
					
					Debug.Log("Invalid message: " + type);
				}
			}

			if (socket.error != null) {

				Debug.Log ("Error: " + socket.error);

				yield return new WaitForSeconds (1);

				socket = new WebSocket(new Uri("ws://localhost:9000/"));
				yield return StartCoroutine(socket.Connect());
			}

			yield return 0;
		}

//		socket.Close();
	}
}
