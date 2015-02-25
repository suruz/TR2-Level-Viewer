using UnityEngine;
using System.Collections;

public class DoorOpeningCam : MonoBehaviour {
	
	static Transform m_CurrentDoor = null;
	static Transform m_LastDoor = null;
	Camera m_DoorCam = null;
	Camera m_MainCam = null;
	
	// Use this for initialization
	void Start () {
		m_DoorCam = GetComponent<Camera>();
		m_MainCam = Camera.main;
		m_DoorCam.depth = m_MainCam.depth + 1;
		m_DoorCam.fieldOfView = m_MainCam.fieldOfView;
		m_DoorCam.far = m_MainCam.far;
		m_DoorCam.clearFlags = m_MainCam.clearFlags;
		m_DoorCam.cullingMask = m_MainCam.cullingMask;
		m_DoorCam.enabled = false;
	}
	
	// Update is called once per frame
	void Update () {
	
		if(m_CurrentDoor != null && (m_CurrentDoor != m_LastDoor))
		{
			Invoke("ViewDoorOpening", 0.1f);
			Invoke("EndView", 2f);
			m_LastDoor = m_CurrentDoor;
		}
		
	}
	
	void ViewDoorOpening()
	{
		if(m_DoorCam!=null) 
		{
			m_DoorCam.transform.position =m_CurrentDoor.position + (Vector3.up * 700) + (m_CurrentDoor.forward * 2048);
			Vector3 frwd = (m_CurrentDoor.position - m_DoorCam.transform.position).normalized;
			frwd.y = 0;
			m_DoorCam.transform.forward = frwd;
			m_DoorCam.enabled = true;
		}
	}
	
	void EndView()
	{
		if(m_DoorCam!=null) m_DoorCam.enabled = false;
	}
	
	public static void SetTarget(Transform _Door)
	{
		if(_Door != null)
		{
			if((Level.m_Player.transform.position  -  _Door.transform.position).magnitude > 4096)
			{
				m_CurrentDoor = _Door;
			}
		}
	}
}
