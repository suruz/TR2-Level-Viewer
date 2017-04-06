using UnityEngine;
using System.Collections;

public class ButtlerStatePlayer : MonoBehaviour {
	
	public Transform m_FollowTransform;
	public Transform[] m_MateTransforms;
	public Parser.Tr2Item m_Tr2Item;

	Vector3 m_StartPos = Vector3.zero;
	Vector3 m_TargetPos = Vector3.zero;
	
	int Walk = 0;
	int Jump = 0;
	int Idle = 0;
	int Run = 0;
	int Attack = 0;
	
	int m_PrevKeyState = 0;
	int m_CurrentKeyState = 0;
	Animation m_Animation = null;
	
	// Use this for initialization
	void Start () 
	{
		m_Animation = GetComponent<Animation>();
		m_CurrentKeyState = Idle;
		m_StartPos = transform.position;
		m_TargetPos = m_StartPos;
		DayNightSystem.AddDayNightEventHandler(UpdateDayNight);
	}

	// Update is called once per frame
	void Update () {
	
		if(Level.m_Player!= null)
		{
			m_FollowTransform = Level.m_Player.transform;
		}
	
		if(m_FollowTransform!=null)
		{
			m_TargetPos = m_FollowTransform.position;
			transform.forward = (m_TargetPos - transform.position).normalized;
			float dist = (m_FollowTransform.position - transform.position).magnitude;
			if(dist > (2048 * Settings.SceneScaling) )
			{
				m_CurrentKeyState = Walk;
				transform.position = Vector3.Lerp(transform.position ,m_TargetPos,Time.deltaTime * 0.05f * Settings.SceneScaling);
			}
			else
			{
				m_CurrentKeyState = Idle;
			}
		}

		m_Animation.Play(""+ m_CurrentKeyState);
	}
	
	void UpdateDayNight(bool isday)
	{
		Debug.Log("Buttler's day:" + isday);
	}
}
