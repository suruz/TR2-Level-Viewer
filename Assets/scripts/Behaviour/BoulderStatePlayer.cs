using UnityEngine;
using System.Collections;

public class BoulderStatePlayer : MonoBehaviour {

	public Transform m_FollowTransform;
	public Transform[] m_MateTransforms;
	public Parser.Tr2Item m_Tr2Item;
	DoorStatePlayer m_DoorState = null;

	int On = 1;
	int Off = 0;
	int Pulling = 0;
	int Pushing = 0;


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

		/*BoxCollider collider = gameObject.AddComponent<BoxCollider>();
		collider.size = new Vector3(1024, 1024, 512);
		collider.center =  new Vector3(0, 512, 256);
		collider.isTrigger = true;
		gameObject.layer = 8;

		m_GUIFont = Resources.Load<Font>("Font/courbd - gui");
		m_GUIStyle = new GUIStyle();
		m_GUIStyle.font = m_GUIFont;
		m_GUIStyle.normal.textColor = Color.white;*/

	}
	
	// Update is called once per frame
	void Update () {
	

	}

	void OnGUI()
	{

	}
	
	public void Activate()
	{

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
