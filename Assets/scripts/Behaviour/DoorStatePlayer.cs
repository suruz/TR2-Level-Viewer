using UnityEngine;
using System.Collections;

public class DoorStatePlayer : MonoBehaviour {

	public Transform m_FollowTransform;
	public Transform[] m_MateTransforms;
	public Parser.Tr2Item m_Tr2Item;

	public int On = 2;
	int Off = 0;
	
	int m_PrevKeyState = 0;
	int m_CurrentKeyState = 0;
	Animation m_Animation = null;
	
	// Use this for initialization
	void Start () {
		
		m_Animation = GetComponent<Animation>();
		On = m_Animation.GetClipCount() - 1;
		m_CurrentKeyState = Off;

		m_Animation.wrapMode = WrapMode.ClampForever;
		m_Animation.Play(""+ m_CurrentKeyState);
	}
	
	// Update is called once per frame
	void Update () {

	}
	
	public void Activate()
	{
		//if(m_CurrentKeyState == On) return;

		if(m_CurrentKeyState == Off)
		{
			m_CurrentKeyState = On;
		}
		else
		{
			m_CurrentKeyState = Off;
		}
		if(m_Animation !=null)
		{
			m_Animation.wrapMode = WrapMode.ClampForever;
			m_Animation.Play("" + m_CurrentKeyState);
		}
			
			

	}

}
