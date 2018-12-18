using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.Experimental.XR;


[RequireComponent(typeof(ARSessionOrigin))]

public class ARInteractionScript : MonoBehaviour {


	// references
	[SerializeField] TMPro.TMP_Text m_StateText;
	[SerializeField] TMPro.TMP_Text m_CountText;

	[SerializeField] ARPlaneManager m_PlaneManager;
	[SerializeField] ARPointCloudManager m_PointCloudManager;

	[SerializeField] Transform m_robotPrefab;

	[SerializeField] GameObject m_robotObject; 

	//The prefab to instantiate on touch
	public GameObject robotObject{
		get { return m_robotObject;}
		set { m_robotObject = value;}
	}

	//The object instantiated as a result of a successful raycast hit with a plane
	public GameObject createRobot {get; private set;}

	int nHit = 0;//for test
	int nPoints = 0;
	int nPlane = 0;
	List<Vector3> pointList = new List<Vector3>();
	static List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
	ARSessionOrigin m_SessionOrigin;


	//Subcribe callbacks to events
	void OnEnable(){
		ARSubsystemManager.systemStateChanged += ChangeStateText;

		m_PlaneManager.planeUpdated += PlaneUpdatedFunction;
		m_PlaneManager.planeAdded += PlaneAddedFunction;
		m_PlaneManager.planeRemoved += PlaneRemovedFunction;

		m_PointCloudManager.pointCloudUpdated += PointCloudCountFunction;
	}

	//Unubcribe callbacks to events
	void OnDiable(){
		ARSubsystemManager.systemStateChanged -= ChangeStateText;

		m_PlaneManager.planeUpdated -= PlaneUpdatedFunction;
		m_PlaneManager.planeAdded -= PlaneAddedFunction;
		m_PlaneManager.planeRemoved -= PlaneRemovedFunction;

		m_PointCloudManager.pointCloudUpdated -= PointCloudCountFunction;
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
		//m_CountText.text = "PL:" + nPlane + "  PC:" + nPoints + " hit:"+ nHit;
		m_CountText.text = "PL:" + nPlane + " hit:"+ nHit;
	}


	/*
	// Use this for initialization
	void Start () {
		
	}
	*/

	void Awake(){
		m_SessionOrigin = GetComponent<ARSessionOrigin> ();
	}

	// Update is called once per frame
	void Update () {
		
		if (Input.touchCount == 0) {
			return;
		}
	
		// if the raycast hit a plane, then place a robot
		if (m_SessionOrigin.Raycast(Input.GetTouch(0).position, hitResults, UnityEngine.Experimental.XR.TrackableType.Planes)){
			nHit++;  //for test
			displayCountText (); //for test

			var hitPose = hitResults [0].pose;
			if (createRobot == null) {
				createRobot = Instantiate (m_robotObject, hitPose.position, hitPose.rotation);
			} else {
				createRobot.transform.position = hitPose.position;
			}

		}

	}

}
