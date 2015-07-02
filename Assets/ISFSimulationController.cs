using UnityEngine;
using System.Collections;
using TcpServerClient;
using System.Net;
using System.Net.Sockets;
using System;
using System.Runtime.InteropServices;

public enum SIMULATION_COMMAND{SIMUCOM_UNKNOWN, SIMUCOM_SEND_IMAGE,SIMUCOM_UPDATE_DATA};
public struct DATA_SET_TO_SIMULATION_t{
	UInt32 start_sequence;
	public SIMULATION_COMMAND command;
	public Int32 steering_angle; //Grad
	public Int32 speed_mms; //mm/s
	public UInt32 timediff; //mm/s
	UInt32 end_sequence;
};

public class ISFSimulationController : MonoBehaviour {

	


	private Server _server;
	private GameObject _goCamera;
	private GameObject _car;
	private GameObject _tempCar;
	//private Vector3 _tempCarPosition;
	//private Quaternion _tempCarRotation;
	private CameraStream _cameraStream;
	private bool newScreen = false;
	private bool init = true;
	private bool moveCar = false;
	private float _moveCarMmDistance = 0;
	private float _moveCarAngle = 0;

	//Axis	
	private GameObject _wheelFrontLeft;
	private GameObject _wheelBackLeft;
	//private GameObject _backAxisMiddle;
	private GameObject _cubeAxisFront;
	private GameObject _cubeAxisBack;
	private GameObject _steeringCutPoint;
	private GameObject _carKreisbahnPoint;
	private GameObject _kreisbahnRoot;
	private GameObject _golf;
	private GameObject _carRoot;

	//private int _moveCarMsTime = 0;

	private float _moveTempDistance = 2;
	private long _oldTimeTicks = DateTime.Now.Ticks;
	private long _oldTimeTicksOszi = DateTime.Now.Ticks;

	private byte[] _cameraImage;

	
	
	LineRenderer lineRenderer;

	// Use this for initialization
	void Start () {
		IPAddress ipaddr = IPAddress.Parse("127.0.0.1");
		_server = new Server (ipaddr, 4545, 1, dataFromServer);
		
		_goCamera = GameObject.Find("MainCamera");
		_cameraStream = (CameraStream) _goCamera.GetComponent(typeof(CameraStream));

		_tempCar = new GameObject ();
		_car = GameObject.Find("Car");

		_wheelFrontLeft = GameObject.Find ("WheelFrontLeft");
		_wheelBackLeft = GameObject.Find ("WheelBackLeft");
		//_backAxisMiddle = GameObject.Find ("BackAxis");
		_cubeAxisFront = GameObject.Find ("CubeAxisFront");
		_cubeAxisBack = GameObject.Find ("CubeAxisBack");
		_steeringCutPoint = GameObject.Find("SteeringCutPoint");
		_carKreisbahnPoint = GameObject.Find ("CarKreisbahnPoint");
		_kreisbahnRoot = GameObject.Find ("KreisbahnAussenrand");
		_carRoot = GameObject.Find ("CarRoot");
		_golf = GameObject.Find ("Golf");

		
		//_wheelFrontLeft.transform.Rotate(new Vector3(0,1,0),-2);

		lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.SetColors(Color.green, Color.blue);
		lineRenderer.SetWidth(0.2F, 0.2F);
		lineRenderer.transform.Rotate (new Vector3 (90, 0, 0));
	}
	
	// Update is called once per frame
	private Vector3 end;
	private Vector3 end2;
	private Vector3 _steeringCutPointPos = new Vector3();
	private TimeSpan elapsedSpan;
	private bool left = false;
	private bool firstTurn =true;
	//private Quaternion _wheelRotation = new Quaternion(;
	void Update () {

		/*
		elapsedSpan = new TimeSpan(DateTime.Now.Ticks - _oldTimeTicks);
		if (elapsedSpan.Milliseconds > 100) {
			moveCar = true;			
			_oldTimeTicks = DateTime.Now.Ticks;
		}

		elapsedSpan = new TimeSpan(DateTime.Now.Ticks - _oldTimeTicksOszi);
		if (elapsedSpan.Milliseconds > 500) {
			if(firstTurn==true){
				_wheelFrontLeft.transform.Rotate(new Vector3(0,1,0),1);
				firstTurn = false;
			}
			else if(left==true){
				_wheelFrontLeft.transform.Rotate(new Vector3(0,1,0),-2);
			}
			else{
				_wheelFrontLeft.transform.Rotate(new Vector3(0,1,0),2);
			}
			left = !left;
			_oldTimeTicksOszi = DateTime.Now.Ticks;
		}
		*/
		
		if (moveCar == true) {

			float oldAngle = _wheelFrontLeft.transform.localEulerAngles.y;
			if(oldAngle >=180){
				oldAngle = oldAngle - 360;
			}			
			
			_wheelFrontLeft.transform.Rotate(new Vector3(0,1,0), (oldAngle * (-1) ));
			_wheelFrontLeft.transform.Rotate(new Vector3(0,1,0), _moveCarAngle);



			float mag = (_steeringCutPoint.transform.position - _wheelFrontLeft.transform.position).magnitude;


			_cubeAxisFront.transform.position = _wheelFrontLeft.transform.position;
			_cubeAxisFront.transform.rotation = _wheelFrontLeft.transform.rotation;
			
			_cubeAxisBack.transform.position = _wheelBackLeft.transform.position;
			_cubeAxisBack.transform.rotation = _wheelBackLeft.transform.rotation;
			
			_cubeAxisFront.transform.Translate (-100.0f, 0.0f, 0.0f);
			_cubeAxisBack.transform.Translate (-100.0f, 0.0f, 0.0f);

			
			if (_wheelFrontLeft != null) {
				end = _cubeAxisFront.transform.position;
				Debug.DrawLine (_wheelFrontLeft.transform.position, end, Color.green);

				end = _cubeAxisBack.transform.position;
				Debug.DrawLine (_wheelBackLeft.transform.position, end, Color.yellow);
				if(ClosestPointsOnTwoLines(out _steeringCutPointPos, out end2 ,_wheelFrontLeft.transform.position,_wheelFrontLeft.transform.position-_cubeAxisFront.transform.position,_wheelBackLeft.transform.position,_wheelBackLeft.transform.position-_cubeAxisBack.transform.position) == true)
				{
					_steeringCutPoint.transform.position = _steeringCutPointPos;
				}
				else{
					_steeringCutPointPos = new Vector3(0,0,0);
				}
				_steeringCutPoint.transform.position = _steeringCutPointPos;
				//_carKreisbahnPoint.transform.position = _wheelBackLeft.transform.position;
			}

			if (_wheelFrontLeft.transform.localEulerAngles.y == 0) { //StraightForward
				drawLine(_carRoot.transform.position,100);
			} else {
				drawCircle (_steeringCutPointPos, mag);
			}


			if (_wheelFrontLeft.transform.localEulerAngles.y == 0) { //StraightForward
				_carRoot.transform.Translate(0,0,_moveCarMmDistance);
			}
			else{
				float distance = 0;
				float angle = calcRotateFromSpeed(_moveCarMmDistance,mag);
				if(_wheelFrontLeft.transform.localEulerAngles.y>=180)
					angle = angle*(-1);

				Debug.Log("Rad:"+mag.ToString());
				_carRoot.transform.RotateAround(_steeringCutPoint.transform.position,new Vector3(0,1,0),angle);
			}

			moveCar = false;
		}

		if (newScreen == true) {
			_cameraImage = _cameraStream.getImage();
		}
		
		newScreen = false;
	}


	void drawCircle(Vector3 center, double radius)
	{
		double r = radius;

		float theta_scale = 0.01f;             //Set lower to add more points
		int size = (int)((2.0f * Math.PI) / theta_scale); //Total number of points in circle.		
		lineRenderer.SetVertexCount(size);

		//lineRenderer.material = new Material(Shader.Find("Particles/Additive"));
		int i = 0;
		for(float theta = 0; theta < 2 * Math.PI; theta += 0.1f) {
			double x = r*Math.Cos(theta);
			double y = r*Math.Sin(theta);
			
			Vector3 pos = new Vector3((float)x, 0,(float)y);
			pos += center;

			if(i>=size)
				break;
			lineRenderer.SetPosition(i, pos);
			i+=1;
		}
	}

	void drawLine(Vector3 start, double len)
	{
		lineRenderer.SetVertexCount(2);
		lineRenderer.SetPosition(0, start);
		lineRenderer.SetPosition(1, start+(new Vector3(0,0,100)));
	}

	float calcRotateFromSpeed(float speed_mms,float rad)
	{
		float angle = 0;
		speed_mms = speed_mms;

		angle = (57.29f / rad)*speed_mms;

		return angle;
	}


	void dataFromServer(byte[] data, int len)
	{
		DATA_SET_TO_SIMULATION_t _dataFromsimulationController = ByteArrayToNewStuff (data);
		/*
		System.Text.ASCIIEncoding enc = new System.Text.ASCIIEncoding ();
		string message = enc.GetString (data,0,len);
		*/
		if (_dataFromsimulationController.command == SIMULATION_COMMAND.SIMUCOM_SEND_IMAGE) {

			newScreen = true;

			while(newScreen==true){
			}

			//byte[] senddata = enc.GetBytes("HALLO From SIMU");
			if(_server.getClients().Count>0)
			{
				TcpClient c = _server.getClients()[0];
				//_server.sendData(c,_cameraImage);
				//foreach (TcpClient c in _server.getClients()) {

				if(!_server.sendData(c,_cameraImage))
				{
					_server.removeClient(c);
				}

			}
		}
		else if (_dataFromsimulationController.command == SIMULATION_COMMAND.SIMUCOM_UPDATE_DATA) {

			moveCarFunc(_dataFromsimulationController.speed_mms, _dataFromsimulationController.steering_angle, _dataFromsimulationController.timediff);

			newScreen = true;
			while(newScreen==true){
			}
			
			//byte[] senddata = enc.GetBytes("HALLO From SIMU");
			if(_server.getClients().Count>0)
			{
				TcpClient c = _server.getClients()[0];
				//_server.sendData(c,_cameraImage);
				//foreach (TcpClient c in _server.getClients()) {
				
				if(!_server.sendData(c,_cameraImage))
				{
					_server.removeClient(c);
				}
				
			}
		}
	}

	private void moveCarFunc(int speed, int steeringAngle, uint timediff)
	{
		/*

		_moveCarMmDistance = (float)((s / 1000) * timediff);
		_moveCarAngle = (float)steeringAngle;
		*/
		//_moveCarMsTime = timediff;

		_moveCarMmDistance = (float)((speed / 1000) * timediff);
		_moveCarMmDistance = _moveCarMmDistance / 10;
		_moveCarAngle = (float)steeringAngle;

		moveCar = true;

		while (moveCar==true) {
		}

		//_tempCar.transform.Rotate (0, steeringAngle, 0);
		//_tempCar.transform.Translate (0, 0, (float)((s / 1000) * timediff));


		//_tempCarPosition.z += (float)((s/1000)*timediff);
		//_tempCarRotation.y += steeringAngle;

		//transform.Rotate(Vector3.up, -turnSpeed * Time.deltaTime);
	}

	DATA_SET_TO_SIMULATION_t ByteArrayToNewStuff(byte[] bytes)
	{
		GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
		DATA_SET_TO_SIMULATION_t stuff = (DATA_SET_TO_SIMULATION_t)Marshal.PtrToStructure(
			handle.AddrOfPinnedObject(), typeof(DATA_SET_TO_SIMULATION_t));
		handle.Free();
		return stuff;
	}

	public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2){
		
		intersection = Vector3.zero;
		
		Vector3 lineVec3 = linePoint2 - linePoint1;
		Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
		Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);
		
		float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);
		
		//Lines are not coplanar. Take into account rounding errors.
		if((planarFactor >= 0.00001f) || (planarFactor <= -0.00001f)){
			
			return false;
		}
		
		//Note: sqrMagnitude does x*x+y*y+z*z on the input vector.
		float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
		
		if((s >= 0.0f) && (s <= 1.0f)){
			
			intersection = linePoint1 + (lineVec1 * s);
			return true;
		}
		
		else{
			return false;       
		}
	}

	//Two non-parallel lines which may or may not touch each other have a point on each line which are closest
	//to each other. This function finds those two points. If the lines are not parallel, the function 
	//outputs true, otherwise false.
	public static bool ClosestPointsOnTwoLines(out Vector3 closestPointLine1, out Vector3 closestPointLine2, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2){
		
		closestPointLine1 = Vector3.zero;
		closestPointLine2 = Vector3.zero;
		
		float a = Vector3.Dot(lineVec1, lineVec1);
		float b = Vector3.Dot(lineVec1, lineVec2);
		float e = Vector3.Dot(lineVec2, lineVec2);
		
		float d = a*e - b*b;
		
		//lines are not parallel
		if(d != 0.0f){
			
			Vector3 r = linePoint1 - linePoint2;
			float c = Vector3.Dot(lineVec1, r);
			float f = Vector3.Dot(lineVec2, r);
			
			float s = (b*f - c*e) / d;
			float t = (a*f - c*b) / d;
			
			closestPointLine1 = linePoint1 + lineVec1 * s;
			closestPointLine2 = linePoint2 + lineVec2 * t;
			
			return true;
		}
		
		else{
			return false;
		}
	}	
}
