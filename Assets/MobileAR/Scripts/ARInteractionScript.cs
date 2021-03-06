﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Experimental.XR;
using UnityEngine.EventSystems;


[RequireComponent(typeof(ARSessionOrigin))]

public class ARInteractionScript : MonoBehaviour {


	// references
	[SerializeField] TMPro.TMP_Text m_StateText;
	[SerializeField] TMPro.TMP_Text m_CountText;

	[SerializeField] ARPlaneManager m_PlaneManager;
	[SerializeField] ARPointCloudManager m_PointCloudManager;

	[SerializeField] GameObject m_robotObject; 
	[SerializeField] GameObject m_ballObject;

	[SerializeField] Light d_light;
	[SerializeField] Image m_colorImage;


	int nPlane = 0;
	int nPoints = 0;
	int nShotBall =0;
	List<Vector3> pointList = new List<Vector3>();

	ARSessionOrigin m_SessionOrigin;
	static List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
	public GameObject createRobot {get; private set;}//The object instantiated as a result of a successful raycast hit with a plane
	public GameObject newBall {get; private set;}
	public float? m_brightness;
	public float? m_colorTemperature;
	public Color? m_colorCorrection;


	//Subcribe callbacks to events
	void OnEnable(){
		ARSubsystemManager.systemStateChanged += ChangeStateText;

		m_PlaneManager.planeUpdated += PlaneUpdatedFunction;
		m_PlaneManager.planeAdded += PlaneAddedFunction;
		m_PlaneManager.planeRemoved += PlaneRemovedFunction;

		m_PointCloudManager.pointCloudUpdated += PointCloudCountFunction;

		ARSubsystemManager.cameraFrameReceived += LightAdjustment;
	}

	//Unubcribe callbacks
	void OnDiable(){
		ARSubsystemManager.systemStateChanged -= ChangeStateText;

		m_PlaneManager.planeUpdated -= PlaneUpdatedFunction;
		m_PlaneManager.planeAdded -= PlaneAddedFunction;
		m_PlaneManager.planeRemoved -= PlaneRemovedFunction;

		m_PointCloudManager.pointCloudUpdated -= PointCloudCountFunction;

		ARSubsystemManager.cameraFrameReceived -= LightAdjustment;
	}

	//Tracking the AR system state 
	void ChangeStateText(ARSystemStateChangedEventArgs stateChanged_Args){
		m_StateText.text = stateChanged_Args.ToString();
	}

	//To get the number of plane when plane added
	void PlaneAddedFunction(ARPlaneAddedEventArgs planeUpdated_Args){
		nPlane = m_PlaneManager.planeCount;
		displayCountText ();
	}
	//To get the number of plane when plane updated
	void PlaneUpdatedFunction(ARPlaneUpdatedEventArgs planeUpdated_Args){
		nPlane = m_PlaneManager.planeCount;
		displayCountText ();
	}

	//To get the number of plane when plane removed
	void PlaneRemovedFunction(ARPlaneRemovedEventArgs planeUpdated_Args){
		nPlane = m_PlaneManager.planeCount;
		displayCountText ();
	}

	//To get the number of points when point cloud updated
	void PointCloudCountFunction(ARPointCloudUpdatedEventArgs pointCloud_Args){
		pointCloud_Args.pointCloud.GetPoints (pointList);
		nPoints = pointList.Count;
		displayCountText ();
	}		

	// Display the result on the screen 
	void displayCountText(){
		//m_CountText.text = "PL:" + nPlane + "  PC:" + nPoints;
		m_CountText.text = "PL:" + nPlane + "  PC:" + nPoints+" shot:"+nShotBall;//for test
	}

	void LightAdjustment(ARCameraFrameEventArgs cameraFrame_Args){
		
		//Check if the lightEstimation varibles are valid and store their value.
		if(cameraFrame_Args.lightEstimation.averageBrightness.HasValue){
			m_brightness = cameraFrame_Args.lightEstimation.averageBrightness.Value;
			d_light.intensity = m_brightness.Value;
		}

		if (cameraFrame_Args.lightEstimation.averageColorTemperature.HasValue) {
			m_colorTemperature = cameraFrame_Args.lightEstimation.averageColorTemperature.Value;
			d_light.colorTemperature = m_colorTemperature.Value;
		}

		if(cameraFrame_Args.lightEstimation.colorCorrection.HasValue){
			m_colorCorrection = cameraFrame_Args.lightEstimation.colorCorrection.Value;
			d_light.color = m_colorCorrection.Value;
			m_colorImage.color = m_colorCorrection.Value*d_light.intensity;
		}
			
	}

	public void ShootBall(){
		newBall = Instantiate<GameObject> (m_ballObject);
		newBall.transform.position = Camera.current.transform.position;
		Rigidbody rigBall = newBall.GetComponent<Rigidbody> ();
		rigBall.AddForce (100 * Camera.current.transform.forward, ForceMode.Impulse);
		nShotBall++;//for test
	}

	void Awake(){
		m_SessionOrigin = GetComponent<ARSessionOrigin> ();
	}

	// Update is called once per frame
	void Update () {
		
		if(Input.touchCount>0 && Input.GetTouch(0).phase == TouchPhase.Began){
			
			if (EventSystem.current.IsPointerOverGameObject (Input.GetTouch(0).fingerId)) {
				return;

			} else {
				// if the raycast hit a plane, then place a robot
				if (m_SessionOrigin.Raycast(Input.GetTouch(0).position, hitResults, UnityEngine.Experimental.XR.TrackableType.PlaneWithinPolygon)){

					//get the touch position on screen
					var hitPose = hitResults [0].pose;

					//Instantiate a robot if it doesn't exist 
					if (createRobot == null) {
						createRobot = Instantiate (m_robotObject, hitPose.position, hitPose.rotation);
						Rigidbody rigRobot = createRobot.GetComponent<Rigidbody> ();
						rigRobot.isKinematic = false;
						rigRobot.velocity = Vector3.zero;
						rigRobot.angularVelocity = Vector3.zero;
					} else {
						createRobot.transform.position = hitPose.position;
					}
				}
			}
		}
		
	}

}
