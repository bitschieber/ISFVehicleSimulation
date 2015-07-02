using UnityEngine;
using System.Collections;

public class CarTransformation : MonoBehaviour {

	
	float angle = 0;
	float speed = 0;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
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

		this.transform.Translate(Vector3.forward * speed);
		this.transform.Rotate(new Vector3(0,angle,0));
	}
}
