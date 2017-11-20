using UnityEngine;
using System.Collections;

public class CrowStatePlayer : MonoBehaviour {

	public Transform m_FollowTransform;
	public Transform[] m_MateTransforms;
	public Parser.Tr2Item m_Tr2Item;


	Vector3 m_StartPos = Vector3.zero;
	Vector3 m_TargetPos = Vector3.zero;

	int Walk = 8;
	int Jump = 0;
	int Idle = 2;
	int Run = 8;
	int Attack = 4;

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
	void Update () 
	{
		if(Level.m_Player!= null)
		{
			m_FollowTransform = Level.m_Player.transform;
		}
		
		if(m_FollowTransform!=null)
		{
			
			float dist = (m_FollowTransform.position - m_StartPos).magnitude;
			
			if(dist > (1024 * Settings.SceneScaling) && dist < (10000 * Settings.SceneScaling))
			{
				m_TargetPos = m_FollowTransform.position;
				m_CurrentKeyState = Run;
				if((transform.position - m_TargetPos).magnitude < (1024 * Settings.SceneScaling))
				{
					m_CurrentKeyState = Attack;
				}
				
				
				
				transform.forward = (m_TargetPos - transform.position).normalized;
				transform.position = Vector3.Lerp(transform.position ,m_TargetPos,Time.deltaTime);
			}
			else if((transform.position - m_StartPos).magnitude < (1024 * Settings.SceneScaling))
			{
				m_CurrentKeyState = Idle;
			}
			else
			{
				m_TargetPos = m_StartPos;
				m_CurrentKeyState = Walk;
				
				transform.forward = (m_TargetPos - transform.position).normalized;
				transform.position = Vector3.Lerp(transform.position ,m_TargetPos,Time.deltaTime * 0.25f);
			}
			
		}
		
		m_Animation.Play(""+ m_CurrentKeyState);
		
	}
	
	void UpdateDayNight(bool isday)
	{
		Debug.Log("Crow's day:" + isday);
	}

}
