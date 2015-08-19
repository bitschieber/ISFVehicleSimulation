using System;
using UnityEngine;

    [RequireComponent(typeof (CarController))]
    public class CarUserControl : MonoBehaviour
    {
        private CarController m_Car; // the car controller we want to use


        private void Awake()
        {
            // get the car controller
            m_Car = GetComponent<CarController>();
        }


        private void FixedUpdate()
        {
            // pass the input to the car!
			float h = 10f;
				float v = 10f;
			float handbrake = 0.5f;
			m_Car.Move(h, v, v, handbrake);
        }
    }
