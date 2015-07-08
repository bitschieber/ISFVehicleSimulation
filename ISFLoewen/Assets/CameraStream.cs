using UnityEngine;
using System.Collections;
using System.Net;
using TcpServerClient;
using System;

public class CameraStream : MonoBehaviour {

	// Use this for initialization
	//bool take = true;
	int resWidth = 320;
	int resHeight = 240;
	//Server _server;
	//Int64 oldTime_ms = 0;
	//int hz = 10;

	void Start () {

		//IPAddress ipaddr = IPAddress.Parse("10.0.0.245");
		//_server = new Server(ipaddr, 5042, 1, dataFromClient);
	
	}
	
	// Update is called once per frame
	void Update () {
		//Int64 currentTime_ms = Convert.ToInt64(Time.time*1000);
		//if (take == true) {

			//if(currentTime_ms-oldTime_ms<(1000/hz))
				//return;

			//oldTime_ms = currentTime_ms;


			//if (_server.getClients().Count > 0)
			//{
				/*
				RenderTexture rt = new RenderTexture (resWidth, resHeight, 24);
				GetComponent<Camera>().targetTexture = rt;
				Texture2D screenShot = new Texture2D (resWidth, resHeight, TextureFormat.RGB24, false);
				GetComponent<Camera>().Render ();
				RenderTexture.active = rt;
				screenShot.ReadPixels (new Rect (0, 0, resWidth, resHeight), 0, 0);
				GetComponent<Camera>().targetTexture = null;
				RenderTexture.active = null; // JC: added to avoid errors
				Destroy (rt);
				byte[] bytes = screenShot.EncodeToJPG ();
				*/
				//screenShot.EncodeToJPG
				//string filename = ScreenShotName (resWidth, resHeight);
				//System.IO.File.WriteAllBytes (filename, bytes);
				//Debug.Log (string.Format ("Took screenshot to: {0}", filename));
				//takeHiResShot = false;

				//_server.sendData(_server.getClients()[0],bytes);
				
			//}
		//}
		//take = false;

	}

	public byte[] getImage()
	{
		RenderTexture rt = new RenderTexture (resWidth, resHeight, 24);
		GetComponent<Camera>().targetTexture = rt;
		Texture2D screenShot = new Texture2D (resWidth, resHeight, TextureFormat.RGB24, false);
		GetComponent<Camera>().Render ();
		RenderTexture.active = rt;
		screenShot.ReadPixels (new Rect (0, 0, resWidth, resHeight), 0, 0);
		GetComponent<Camera>().targetTexture = null;
		RenderTexture.active = null; // JC: added to avoid errors
		Destroy (rt);
		byte[] bytes = screenShot.EncodeToJPG ();
		return bytes;
	}

	/*
	public static string ScreenShotName(int width, int height) {
		return string.Format("{0}/screenshots/screen_{1}x{2}_{3}.png", 
		                     Application.dataPath, 
		                     width, height, 
		                     System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"));
	}
	*/

	/*
	private void dataFromClient(string data)
	{
		//_dataFromClients.Add(data);
	}
	*/
}
