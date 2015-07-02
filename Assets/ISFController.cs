using UnityEngine;
using System.Collections;
using TcpServerClient;
using System;
using System.Timers;

public struct DATA_SET_MONITOR_t{
	public int  lenkwinkel;
	public int  speed;
};

//public DATASETSIZ = 

public class ISFController : MonoBehaviour {

	static int counter = 0;
	Timer _timer;
	Client _client;
	GameObject _CarWheelFrontLeft = null;
	GameObject _CarWheelFrontRight = null;
	GameObject _Car = null;
	int rotateFrontWheels = 0;
	int _carSpeed_mmSek = 0;
	DATA_SET_MONITOR_t _dataSetOutput;
	public int frameRate = 50;
	Int64 deltaTime_ms = 0;
	float fps = 0;

	float angle = 0;
	float speed = 0;

	public int desiredFPS = 60;
	
	void Awake()
	{
		Application.targetFrameRate = -1;
		QualitySettings.vSyncCount = 0;
	}

	/*
	void Awake ()
	{
		Application.targetFrameRate = 50;
	}
	*/
	// Use this for initialization
	void Start () {
		//StartCoroutine(changeFramerate());

		_dataSetOutput.lenkwinkel = 1500;
		_dataSetOutput.speed = 0;

		_Car = GameObject.Find ("Car");
		_CarWheelFrontLeft = GameObject.Find ("WheelFrontLeft");
		_CarWheelFrontRight = GameObject.Find ("WheelFrontRight");

		_client = new Client ("169.254.151.80", 25002, dataFromServer);

		_timer = new Timer (1);
		_timer.Elapsed += new ElapsedEventHandler (_timer_elapsed);
		_timer.Start ();
		//var thread = System.Threading.Thread(doLogic);
		//thread.Start();
	}

	static void _timer_elapsed (object sender, ElapsedEventArgs args){
		counter ++;
	}

	void doLogic() {
		// do stuff in here
	}

	IEnumerator changeFramerate() {
		yield return new WaitForSeconds(1);
		Application.targetFrameRate = frameRate;
	}


	void OnGUI()
	{		
		GUI.Box(new Rect(10,10,100,90), "FPS:"+fps.ToString());
		//GUI.Box(new Rect(10,10,100,90), "Counter:"+counter.ToString());
	}


	// Update is called once per frame
	void Update () {
		//Application.targetFrameRate = frameRate;
		//_client.sendData ("huhu Server");		
		//if (_CarWheelFrontLeft != null) {
		//}

		/*
		long lastTicks = DateTime.Now.Ticks;
		long currentTicks = lastTicks;
		float delay = 1f / desiredFPS;
		float elapsedTime;
		
		if (desiredFPS <= 0)
			return;
		
		while (true)
		{
			currentTicks = DateTime.Now.Ticks;
			elapsedTime = (float)TimeSpan.FromTicks(currentTicks - lastTicks).TotalSeconds;
			if(elapsedTime >= delay)
			{
				break;
			}
		}
		*/
		if (Input.GetKey ("up")) {
			speed *= 1.5f;
		} else if (Input.GetKey ("down")) {
			speed *= 0.5f;
		} else {
			speed *= 0.9f;
		}
		
		if (Input.GetKey ("left")) {
			print ("down arrow key is held down");
			angle = 1;
		}
		else if (Input.GetKey ("right")) {
			print ("down arrow key is held down");
			angle = -1;
		}
		else{
			angle = 0;
		}


		_CarWheelFrontLeft.transform.Rotate (0, 0, rotateFrontWheels-(270-_CarWheelFrontLeft.transform.eulerAngles.y));
		_CarWheelFrontRight.transform.Rotate (0, 0, rotateFrontWheels-(90-_CarWheelFrontRight.transform.eulerAngles.y));
	
		//calcSpeed
		int VehicleSpeedmmSek = _carSpeed_mmSek;
		_dataSetOutput.speed = VehicleSpeedmmSek;
		_dataSetOutput.lenkwinkel = rotateFrontWheels;
		//_Car.transform.position = calcNewCarPos(_Car.transform.position,VehicleSpeedmmSek,angle);

		//_client.sendData (_dataSetOutput);

		
		Int64 currentTime_ms = Convert.ToInt64(Time.time*1000);
		if(currentTime_ms - deltaTime_ms!=0)
			fps = 1000/(currentTime_ms - deltaTime_ms);
		deltaTime_ms = currentTime_ms;
	}

	void dataFromServer(DATA_SET_MONITOR_t data){

		rotateFrontWheels = data.lenkwinkel; //Convert.ToInt32( (-45)+( (90.0/1000) *( data.lenkwinkel-1000)));
		_carSpeed_mmSek = Convert.ToInt32(data.speed);
		//int i = 0;
	}

	/*
	float calcSpeed(int pwm){
		return 1;

		bool vorwaerts = true;
		if (pwm >= 1450 && pwm <= 1550)
			return 0;
		else if (pwm < 1100 || pwm > 1900)
			return 0;
		else if (pwm > 1500) {
			vorwaerts = true;
		} else {
			vorwaerts = false;
			pwm = 1500+(1500 -pwm);
		}

		double uebersetzungWelleMotor = 7;
		double umfangReifenMeter = Math.PI * 65 / 1000;

		//Berechnung PWM in Umdrehung/s
		double period1 = 1.11102 * Math.Pow (10, 23);
		double period2 = Math.Exp (-31.0161 * (pwm / 1000.0));
		double motorPeriode = period1 * period2;
		double umdrehungenMotorSek = 1000 / motorPeriode;
		double umdrehungWelle = umdrehungenMotorSek / uebersetzungWelleMotor;


		if (vorwaerts == false)
			return Convert.ToSingle(umdrehungWelle * umfangReifenMeter)*-1;
		else
			return Convert.ToSingle(umdrehungWelle * umfangReifenMeter);
		//Berechnung Umdrehung in m/s mit Radumfang
	}
	*/

	Vector3 calcNewCarPos(Vector3 oldPos, int speed_mmSek, float lenkwinkel){
		Vector3 newPos;

		//float moveCM = speed_mmSek * 100;
		float moveCurFrame = ( (1.0f * speed_mmSek) / frameRate)/10; //in cm

		oldPos.z = oldPos.z+ moveCurFrame;
		newPos = oldPos;

		return newPos;
	}
}
