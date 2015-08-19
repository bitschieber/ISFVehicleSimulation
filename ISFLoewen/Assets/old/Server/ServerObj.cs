using UnityEngine;
using System.Collections;
using SharpConnect;
using AssemblyCSharp;
using TestClient;
using System.Text;
using System;
using System.Runtime.InteropServices;
using SimulationUtils;

namespace Simulation
{


	public class ServerObj : MonoBehaviour {


		Connector con;
		Client _client;
		byte[] bytes = { 0, 200, 1, 23 }; // a byte array contains non-ASCII (or non-readable) characters
		private  static int _dataentry_t_size = System.Runtime.InteropServices.Marshal.SizeOf(typeof(DataEntry_t));
		DataEntry_t[] values = new DataEntry_t[3];
		byte[] result;
		//public GameObject[] _ultraschallSensoren = GameObject.FindGameObjectsWithTag ("UltraschallSensor");

		public GameObject[] _ultraschallSensoren;
		void Start() {
			_ultraschallSensoren = GameObject.FindGameObjectsWithTag("UltraschallSensor");

			foreach (GameObject sensor in _ultraschallSensoren) {
				//int val = sensor.GetComponent<UltraschallSensor>().value;
				//byte id = sensor.GetComponent<UltraschallSensor>().id;
				//values [id].value = val;
				//values [id].id = id;
				//(int)sensor.transform.position.x;
				//Instantiate(respawnPrefab, respawn.transform.position, respawn.transform.rotation);
			}
			//respawns = GameObject.FindGameObjectsWithTag("Respawn");

			//con = new Connector ();
			//con.fnConnectResult ("127.0.0.1", 25000, "bla");
			//con.testCon ("127.0.0.1", 25000);
			//cl = new Client ("127.0.0.1", 25000);

			/*
			values [0].id = 0;
			values [0].value = 444;
			values [1].id = 1;
			values [1].value = 333;
			values [2].id = 2;
			values [2].value = 9999;
			*/



			//result = new byte[values.Length * _dataentry_t_size];
			//values.CopyTo (result, 0);
			//result = new byte[values.Length * _dataentry_t_size];
			//Buffer.BlockCopy(values, 0, result, 0, result.Length);

			_client = new Client("169.254.151.80", 25000);
		}
		
		// Update is called once per frame
		void Update () {

			/*
			if (_ultraschallSensoren == null) {				
				try{
					_ultraschallSensoren = GameObject.FindGameObjectsWithTag ("UltraschallSensor");
				}
				catch(Exception ex){
					Debug.Log(ex.Message);
				}
			}
			*/

			//if (respawns == null)
			//_ultraschallSensoren = GameObject.FindGameObjectsWithTag("UltraschallSensor");

			//GameObject respawnPrefab;


			foreach (GameObject sensor in _ultraschallSensoren) {
				//int val = sensor.GetComponent<UltraschallSensor>().value;
				//int id = sensor.GetComponent<UltraschallSensor>().id;
				//values [id].value = val;
					//(int)sensor.transform.position.x;
				//Instantiate(respawnPrefab, respawn.transform.position, respawn.transform.rotation);
			}

			if(values!=null)
				result = Utils.StructureToByteArray(values);


			/*
			Hier müssen alle Werte in ein Format gebracht werden(ID und Value) und an den Server gesendet werden.
			Anschließend muss gepruft werden ob bekannte Werte überschrieben werden sollen 
			(Client gucken ob Daten da sind. evtl. mit einer Packetnummer)
			Die Objekte müssen dann dementsprechend verschoben werden

			Zum Testen vielleicht die Framerate runterdrehen :-)
			*/



			//_client.sendData("Data From Unity :-)");
			
			//string s1 = Encoding.UTF8.GetString(bytes);
			this._client.sendData(Utils.ByteArrayToString(result));
		}

	}
}
