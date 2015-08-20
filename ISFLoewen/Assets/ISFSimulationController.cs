using UnityEngine;
using System.Collections;
using TcpServerClient;
using System.Net;
using System.Net.Sockets;
using System;
using System.Runtime.InteropServices;
using UnityEngine.UI;


//Enum für zum ändern der Sicht
public enum CAMERAS{CAMERA_FRONT, CAMERA_TOP, CAMERA_BACK, CAMERA_VIEW1};
//Enums für zum ändern der Strecke
public enum COURSES{COURSE_LABOR, COURSE_CUP2014, COURSE_CUP2014_PARKING01, COURSE_CUP2014_OBSTACLES01, CORUSE_STRAIGHT_50_METRES};


public enum SIMULATION_COMMAND{SIMUCOM_UNKNOWN, SIMUCOM_SEND_IMAGE,SIMUCOM_UPDATE_DATA};
public enum GPIO_PIN{GPIO_PIN01 = 0,GPIO_PIN02 = 1,GPIO_PIN03 = 2,GPIO_PIN04 = 3,GPIO_PIN05 = 4,GPIO_PIN06 = 5,GPIO_PIN07 = 6,GPIO_PIN08 = 7};



/*
 * Datenstrukturen für den Datenaustausch mit ISFCarDesktop
 */
public struct DATA_SET_TO_SIMULATION_t{
	UInt32 start_sequence;
	public SIMULATION_COMMAND command;
	public Int32 steering_angle; //Grad
	public Int32 speed_mms; //mm/s
	public UInt32 timediff; //mm/s
	public UInt32 gpio_state;
	UInt32 end_sequence;
};

public enum DATA_HEADER_TYPE{IMAGE_JPEG, SIMULATION_OUTPUT};

public struct DATA_HEADER_SET{
	UInt32 start_sequence;
	public DATA_HEADER_TYPE type;
	public UInt32 length;
	UInt32 end_sequence;
};





public class ISFSimulationController : MonoBehaviour {

	public InputField ipAddressField;
	public Button btnStartServer;
	public Button btnReset;
	public Button btnChangeCamera;
	public Button btnChangeCourse;

	//Objekte hinter denen jeweils die Kameras hinterlegt sind
	private CAMERAS _currentCamera = CAMERAS.CAMERA_BACK;
	private Camera _cameraBack;
	private Camera _cameraFront;
	private Camera _cameraTop;
	private Camera _cameraView01;

	//Objekte hinter denen jeweils die Strecken hinterlegt sind
	private COURSES _currentCourse = COURSES.COURSE_LABOR;
	private GameObject _courseLabor;
	private GameObject _courseCup2014;
	private GameObject _courseCup2014Parking01;
	private GameObject _courseCup2014Obstacles01;
	private GameObject _courseStraight50Metres;

	private Server _server = null;
	private GameObject _goCamera;
	private GameObject _car;
	private GameObject _tempCar;
	private CameraStream _cameraStream;
	private bool newScreen = false;
	private bool moveCar = false;
	private float _moveCarMmDistance = 0;
	private float _moveCarAngle = 0;

	//Tires
	private GameObject _tireFrontLeft;
	private GameObject _tireFrontRight;
	//Axis	
	private GameObject _wheelFrontLeft;
	private GameObject _wheelFrontRight;
	private GameObject _wheelBackLeft;
	//private GameObject _backAxisMiddle;
	private GameObject _cubeAxisFront;
	private GameObject _cubeAxisBack;
	private GameObject _steeringCutPoint;
	//private GameObject _golf;
	private GameObject _carRoot;
	private GameObject _posStraighForeward;
	//Lights
	private uint _moveCarLights;
	private GameObject _lightIndicatorLeft;
	private GameObject _lightIndicatorRight;
	private GameObject _lightDrive;
	private GameObject _lightBreak;
	private GameObject _lightBackward;


	private byte[] _cameraImage;	
	LineRenderer lineRenderer;
	
	// Update is called once per frame
	private Vector3 end;
	private Vector3 end2;
	private Vector3 _steeringCutPointPos = new Vector3();
	private TimeSpan elapsedSpan;
	private GameObject currentRotationWheel;
	private float _cutPointLineDirection = -100.0f;

	// Use this for initialization
	void Start () {

		//Die Kameras im System finden und den Objekten zuweisen
		_cameraBack = GameObject.Find ("CameraBack").GetComponent<Camera> ();
		_cameraFront = GameObject.Find ("CameraFront").GetComponent<Camera> ();
		_cameraTop = GameObject.Find ("CameraTop").GetComponent<Camera> ();
		_cameraView01 = GameObject.Find ("Camera01").GetComponent<Camera> ();
		_goCamera = GameObject.Find("Camera01");
		_cameraStream = (CameraStream) _goCamera.GetComponent(typeof(CameraStream));

		//Die Strecken im System finden und den Objekten zuweisen
		_courseLabor = GameObject.Find ("CourseLabor");
		_courseCup2014 = GameObject.Find ("CourseCup2014");
		_courseCup2014Parking01 = GameObject.Find ("CourseCup2014Parking01");
		_courseCup2014Obstacles01 = GameObject.Find ("CourseCup2014Obstacles01");
		_courseStraight50Metres = GameObject.Find ("CoruseStraight50Metres");		
		_currentCourse = COURSES.COURSE_LABOR;
		_courseCup2014.SetActive(false);
		_courseCup2014Parking01.SetActive(false);
		_courseCup2014Obstacles01.SetActive(false);
		_courseStraight50Metres.SetActive(false);


		_tempCar = new GameObject ();
		_car = GameObject.Find("Car");
		
		//Tires
		_tireFrontLeft = GameObject.Find ("TireFrontLeft");
		_tireFrontRight = GameObject.Find ("TireFrontRight");
		
		//Axis
		_wheelFrontLeft = GameObject.Find ("WheelFrontLeft");
		_wheelFrontRight = GameObject.Find ("WheelFrontRight");
		_wheelBackLeft = GameObject.Find ("WheelBackLeft");
		//_backAxisMiddle = GameObject.Find ("BackAxis");
		_cubeAxisFront = GameObject.Find ("CubeAxisFront");
		_cubeAxisBack = GameObject.Find ("CubeAxisBack");
		_steeringCutPoint = GameObject.Find("SteeringCutPoint");
		_carRoot = GameObject.Find ("CarRoot");
		//_golf = GameObject.Find ("Golf");
		_posStraighForeward = GameObject.Find ("PosStraighForeward");

		//Lights
		_moveCarLights = 0;
		_lightIndicatorLeft  = GameObject.Find ("LightIndicatorLeft"); _lightIndicatorLeft.SetActive (false);
		_lightIndicatorRight = GameObject.Find ("LightIndicatorRight"); _lightIndicatorRight.SetActive (false);
		_lightDrive = GameObject.Find ("LightDrive"); _lightDrive.SetActive (false);
		_lightBreak = GameObject.Find ("LightBreak");_lightBreak.SetActive (false);
		_lightBackward = GameObject.Find ("LightBackward");_lightBackward.SetActive (false);
		
		lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.SetColors(Color.green, Color.blue);
		lineRenderer.SetWidth(0.2F, 0.2F);
		lineRenderer.transform.Rotate (new Vector3 (90, 0, 0));
		
		btnStartServer.GetComponentInChildren<Text>().text = "Start Server";

		btnStartServer.onClick.AddListener(() => {
			//handle click here
			if(_server!=null){
				_server.disconnect();
				_server = null;
				btnStartServer.GetComponentInChildren<Text>().text = "Start Server";
			}
			else{
				IPAddress ipaddr = IPAddress.Parse(ipAddressField.text);
				_server = new Server (ipaddr, 4545, 1, dataFromServer);
				btnStartServer.GetComponentInChildren<Text>().text = "Close Server";
			}
		});

		//Setzt das Fahrzeug zurück auf Position 0,0,0
		btnReset.onClick.AddListener(() => {
			//handle click here
			_carRoot.transform.position = new Vector3(0,_carRoot.transform.position.y,0);
			_carRoot.transform.rotation = new Quaternion(0,0,0,0);
		});


		_cameraTop.enabled = false;
		_cameraFront.enabled = false;
		_cameraBack.enabled = false;

		//Button für die Auswahl der anzuzeigenen Sicht
		btnChangeCamera.onClick.AddListener(() => {
			//handle click here
			if(_currentCamera == CAMERAS.CAMERA_BACK){
				_cameraBack.enabled = false;
				_cameraFront.enabled = true;
				_currentCamera = CAMERAS.CAMERA_FRONT;
			}
			else if(_currentCamera == CAMERAS.CAMERA_FRONT){
				_cameraFront.enabled = false;
				_cameraTop.enabled = true;
				_currentCamera = CAMERAS.CAMERA_TOP;
			}
			else if(_currentCamera == CAMERAS.CAMERA_TOP){
				_cameraTop.enabled = false;
				_cameraView01.enabled = true;
				_currentCamera = CAMERAS.CAMERA_VIEW1;
			}
			else if(_currentCamera == CAMERAS.CAMERA_VIEW1){
				_cameraView01.enabled = false;
				_cameraBack.enabled = true;
				_currentCamera = CAMERAS.CAMERA_BACK;
			}
		});

		//Button für die Auswahl der anzuzeigenen Strecke
		btnChangeCourse.onClick.AddListener(() => {
			//handle click here
			if(_currentCourse == COURSES.COURSE_LABOR){
				_courseLabor.SetActive(false);
				_courseCup2014.SetActive(true);
				_currentCourse = COURSES.COURSE_CUP2014;
			}
			else if(_currentCourse == COURSES.COURSE_CUP2014){
				_courseCup2014.SetActive(false);
				_courseCup2014Parking01.SetActive(true);
				_currentCourse = COURSES.COURSE_CUP2014_PARKING01;
			}
			else if(_currentCourse == COURSES.COURSE_CUP2014_PARKING01){
				_courseCup2014Parking01.SetActive(false);
				_courseCup2014Obstacles01.SetActive(true);
				_currentCourse = COURSES.COURSE_CUP2014_OBSTACLES01;
			}
			else if(_currentCourse == COURSES.COURSE_CUP2014_OBSTACLES01){
				_courseCup2014Obstacles01.SetActive(false);
				_courseStraight50Metres.SetActive(true);
				_currentCourse = COURSES.CORUSE_STRAIGHT_50_METRES;
			}
			else if(_currentCourse == COURSES.CORUSE_STRAIGHT_50_METRES){
				_courseStraight50Metres.SetActive(false);
				_courseLabor.SetActive(true);
				_currentCourse = COURSES.COURSE_LABOR;
			}
		});
	}


	void Update () {

		_wheelFrontRight.transform.rotation = _wheelFrontLeft.transform.rotation;
		float wheelAngle = 0;
		wheelAngle = _wheelFrontLeft.transform.localEulerAngles.y;
		if(Math.Abs(wheelAngle)<0.05)
			wheelAngle = 0;

		if (wheelAngle > 180) {
			currentRotationWheel = _wheelFrontLeft;
			_cutPointLineDirection = -100.0f;
		} else {
			currentRotationWheel = _wheelFrontRight;
			_cutPointLineDirection = 100.0f;
		}

		float mag = (_steeringCutPoint.transform.position - currentRotationWheel.transform.position).magnitude;
		
		
		_cubeAxisFront.transform.position = currentRotationWheel.transform.position;
		_cubeAxisFront.transform.rotation = currentRotationWheel.transform.rotation;
		
		_cubeAxisBack.transform.position = _wheelBackLeft.transform.position;
		_cubeAxisBack.transform.rotation = _wheelBackLeft.transform.rotation;
		
		_cubeAxisFront.transform.Translate (_cutPointLineDirection, 0.0f, 0.0f);
		_cubeAxisBack.transform.Translate (_cutPointLineDirection, 0.0f, 0.0f);

		end = _cubeAxisFront.transform.position;
		Debug.DrawLine (currentRotationWheel.transform.position, end, Color.green);
			
		end = _cubeAxisBack.transform.position;
		Debug.DrawLine (_wheelBackLeft.transform.position, end, Color.yellow);
		if(ClosestPointsOnTwoLines(out _steeringCutPointPos, out end2 ,currentRotationWheel.transform.position,currentRotationWheel.transform.position-_cubeAxisFront.transform.position,_wheelBackLeft.transform.position,_wheelBackLeft.transform.position-_cubeAxisBack.transform.position) == true)
		{
			_steeringCutPoint.transform.position = _steeringCutPointPos;
		}
		else{
			_steeringCutPointPos = new Vector3(0,0,0);
		}
		_steeringCutPoint.transform.position = _steeringCutPointPos;

		_tireFrontLeft.transform.Rotate (new Vector3(0,1,0),-_tireFrontLeft.transform.localEulerAngles.y) ;
		_tireFrontLeft.transform.Rotate (new Vector3(0,1,0),_wheelFrontLeft.transform.localEulerAngles.y) ;
		_tireFrontRight.transform.Rotate (new Vector3(0,1,0),-_tireFrontRight.transform.localEulerAngles.y) ;
		_tireFrontRight.transform.Rotate (new Vector3(0,1,0),_wheelFrontRight.transform.localEulerAngles.y) ;


		/*
		 * Only when Step from SimulationController
		 * */
		if (moveCar == true) {

			float oldAngle = _wheelFrontLeft.transform.localEulerAngles.y;
			if(oldAngle >=180){
				oldAngle = oldAngle - 360;
			}			

			//Lichter anhand der GPIOs schalten
			setCarLights(_moveCarLights);

			//Vorderräder auf gewünschten Lenkwinkel setzen
			//Erst auf 0 Grad setzen und dann auf gewünschten Lenkwinkel
			_wheelFrontLeft.transform.Rotate(new Vector3(0,1,0), (oldAngle * (-1) ));
			_wheelFrontLeft.transform.Rotate(new Vector3(0,1,0), _moveCarAngle);
			_wheelFrontRight.transform.Rotate(new Vector3(0,1,0), (oldAngle * (-1) ));
			_wheelFrontRight.transform.Rotate(new Vector3(0,1,0), _moveCarAngle);


			if (wheelAngle == 0) { //Wenn das Fahrzeug nur geradeaus fahren soll
				_carRoot.transform.Translate(0,0,_moveCarMmDistance);
			}
			else{//Falls eine Rotation durchgeführt werden muss
				float angle = calcRotateFromSpeed(_moveCarMmDistance,mag);
				if(wheelAngle>=180)
					angle = angle*(-1);

				Debug.Log("Rad:"+mag.ToString());
				//Das gesamte Auto um den berechneten Schnittpunkt drehen.
				_carRoot.transform.RotateAround(_steeringCutPoint.transform.position,new Vector3(0,1,0),angle);
			}

			moveCar = false;
		}

		
		if (newScreen == true) {
			_cameraImage = _cameraStream.getImage();
		}		
		newScreen = false;
	}

	//Berechnung des Winkels um das sich das Fahrzeug dreht, unter Berücksichtigung der zurückgelegten Strecke auf dem Kreisbogen.
	float calcRotateFromSpeed(float speed_mms,float rad)
	{
		return ( (180.0f/(float)Math.PI) / rad)*speed_mms;
	}

	void dataFromServer(byte[] data, int len)
	{
		DATA_SET_TO_SIMULATION_t _dataFromsimulationController = ByteArrayToNewStuff (data);
		bool sendImage = false;

		//Prüfen, um welchen Befehl es sich handelt
		if (_dataFromsimulationController.command == SIMULATION_COMMAND.SIMUCOM_SEND_IMAGE) {
			
			sendImage = true;
		}
		else if (_dataFromsimulationController.command == SIMULATION_COMMAND.SIMUCOM_UPDATE_DATA) {

			moveCarFunc(_dataFromsimulationController.speed_mms, _dataFromsimulationController.steering_angle, _dataFromsimulationController.timediff);
			_moveCarLights = _dataFromsimulationController.gpio_state;

			sendImage = true;

		}

		//Send Image to Client
		if (sendImage == true) {
			newScreen = true;
			while(newScreen==true){
			}
			
			DATA_HEADER_SET header = new DATA_HEADER_SET();
			header.type = DATA_HEADER_TYPE.IMAGE_JPEG;
			header.length = (UInt32)_cameraImage.Length;
			
			int hSize = Marshal.SizeOf(header);
			
			byte[] sendData = new byte[hSize+ header.length];
			Array.Copy(getBytes(header),sendData,hSize);
			Array.Copy(_cameraImage,0,sendData,hSize,header.length);
			if(_server.getClients().Count>0)
			{
				TcpClient c = _server.getClients()[0];
				
				if(!_server.sendData(c,sendData))
				{
					_server.removeClient(c);
				}
				
			}
		}

	}

	//Je nach GPIO-Zustand werden hier die entsprechenden Lichter an- und ausgeschaltet
	private void setCarLights(uint gpioStates){
		if( (gpioStates & (1 << (int)GPIO_PIN.GPIO_PIN01)) >0)
			_lightIndicatorLeft.SetActive (true);
		else
			_lightIndicatorLeft.SetActive (false);

		if( (gpioStates & (1 << (int)GPIO_PIN.GPIO_PIN02)) >0)
			_lightIndicatorRight.SetActive (true);
		else
			_lightIndicatorRight.SetActive (false);

		if((gpioStates & (1 << (int)GPIO_PIN.GPIO_PIN03))>0)
			_lightDrive.SetActive (true);
		else
			_lightDrive.SetActive (false);

		if((gpioStates & (1 << (int)GPIO_PIN.GPIO_PIN04)) >0)
			_lightBreak.SetActive (true);
		else
			_lightBreak.SetActive (false);

		if((gpioStates & (1 << (int)GPIO_PIN.GPIO_PIN05)) >0)
			_lightBackward.SetActive (true);
		else
			_lightBackward.SetActive (false);
	}

	//Parameter für geschwindigkeit und Lenkwinkel setzen und warten bis neu gerendert wird -> Update()
	//Innerhalb der Update() Methode wird auf diese Parameter zugeriffen und das Fahrzeug positioniert
	private void moveCarFunc(int speed, int steeringAngle, uint timediff)
	{
		float mmInUnity = 0;
		mmInUnity = ((float)(speed / 1000.0f)) / 10;
		_moveCarMmDistance = mmInUnity * timediff;
		_moveCarAngle = (float)steeringAngle;

		moveCar = true;
		while (moveCar==true) {
		}
	}

	DATA_SET_TO_SIMULATION_t ByteArrayToNewStuff(byte[] bytes)
	{
		GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
		DATA_SET_TO_SIMULATION_t stuff = (DATA_SET_TO_SIMULATION_t)Marshal.PtrToStructure(
			handle.AddrOfPinnedObject(), typeof(DATA_SET_TO_SIMULATION_t));
		handle.Free();
		return stuff;
	}

	byte[] getBytes(DATA_HEADER_SET str) {
		int size = Marshal.SizeOf(str);
		byte[] arr = new byte[size];
		
		IntPtr ptr = Marshal.AllocHGlobal(size);
		Marshal.StructureToPtr(str, ptr, true);
		Marshal.Copy(ptr, arr, 0, size);
		Marshal.FreeHGlobal(ptr);
		return arr;
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
