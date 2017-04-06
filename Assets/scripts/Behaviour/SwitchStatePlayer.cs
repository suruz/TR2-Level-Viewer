using UnityEngine;
using System.Collections;

public class SwitchStatePlayer : MonoBehaviour {

	public Transform m_FollowTransform;
	public Transform[] m_MateTransforms;
	public Parser.Tr2Item m_Tr2Item;
	DoorStatePlayer m_DoorState = null;

	int On = 1;
	int Off = 0;

	int m_PrevKeyState = 0;
	int m_CurrentKeyState = 0;
	Animation m_Animation = null;
	
	bool m_ReadyToSwitch = false;
	Font m_GUIFont = null;
	GUIStyle m_GUIStyle = null;

	
	// Use this for initialization
	void Start () {
	
		m_Animation = GetComponent<Animation>();
		m_CurrentKeyState = Off;
		
		m_Animation.wrapMode = WrapMode.Once;
		m_Animation.Play(""+ m_CurrentKeyState);

	
        //removed collider creation from this behabiour script


		gameObject.layer = UnityLayer.Switch;

		m_GUIFont = (Font)Resources.Load("Font/courbd - gui", typeof(Font));
		m_GUIStyle = new GUIStyle();
		m_GUIStyle.font = m_GUIFont;
		m_GUIStyle.normal.textColor = Color.white;

	}
	
	// Update is called once per frame
	void Update () {
	
		if(m_ReadyToSwitch && m_CurrentKeyState == Off)
		{
			if(Input.GetKeyUp(KeyCode.E))
			{
				Activate();
			}
			
			if(Input.GetMouseButtonDown(0) )
			{
				Activate();
			}
		}
	}

	void OnGUI()
	{
		if(m_ReadyToSwitch && m_CurrentKeyState == Off)
		{
			GUI.Label( new Rect(Screen.width * 0.25f,Screen.height * 0.5f,Screen.width , Screen.height), "Tap to open the door !", m_GUIStyle);
		}

		//if(m_ReadyToSwitch && m_CurrentKeyState == On)
		//{
			//GUI.Label( new Rect(Screen.width * 0.25f,Screen.height * 0.5f,Screen.width , Screen.height), "Tap to close the door !", m_GUIStyle);
		//}
	}
	
	public void Activate()
	{
		if(m_Tr2Item.ActivateObject != null)
		{
			if(m_CurrentKeyState == Off)
			{
				m_CurrentKeyState = On;
			}
			else
			{
				m_CurrentKeyState = Off;
			}
			m_Animation.Play(""+ m_CurrentKeyState);
			
			if(m_DoorState == null)
			{
				m_DoorState = m_Tr2Item.ActivateObject.GetComponent<DoorStatePlayer>();
			}
			if(m_DoorState != null)
			{
				m_DoorState.Activate();
				DoorOpeningCam.SetTarget(m_DoorState.transform);
			}
		}
	}

	void OnTriggerEnter (Collider other) 
	{
		m_ReadyToSwitch = true;
	}

	void OnTriggerExit (Collider other) 
	{
		m_ReadyToSwitch = false;
	}



}
