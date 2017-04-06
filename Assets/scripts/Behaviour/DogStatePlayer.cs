using UnityEngine;
using System.Collections;

public class DogStatePlayer : MonoBehaviour {

	public Transform m_FollowTransform;
	public Transform[] m_MateTransforms;
	public Parser.Tr2Item m_Tr2Item;


	Vector3 m_StartPos = Vector3.zero;
	Vector3 m_TargetPos = Vector3.zero;

	int Walk = 1;
	int Jump = 11;
	int Idle = 10;
	int Run = 14;
	int Attack = 18;

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
			Vector3 follow = (m_FollowTransform.position - m_StartPos);
			float dist = follow.magnitude;
			
			if(dist > (1024 * Settings.SceneScaling) && dist < (6000 * Settings.SceneScaling))
			{
				m_TargetPos = m_FollowTransform.position - follow.normalized * 512 * Settings.SceneScaling;
				m_CurrentKeyState = Run;
				if((transform.position - m_TargetPos).magnitude < (256 * Settings.SceneScaling))
				{
					m_CurrentKeyState = Attack;
				}
				
				
				
				Vector3 fwrd = (m_FollowTransform.position - transform.position).normalized;
				fwrd.y = 0;
				transform.forward = fwrd;
				transform.position = Vector3.Lerp(transform.position ,m_TargetPos,Time.deltaTime);
			}
			else if((transform.position - m_StartPos).magnitude < ( 256 * Settings.SceneScaling))
			{
				m_CurrentKeyState = Idle;
			}
			else
			{
				m_TargetPos = m_StartPos;
				m_CurrentKeyState = Walk;
				
				Vector3 fwrd = (m_TargetPos - transform.position).normalized;
				fwrd.y = 0;
				transform.forward = fwrd;
				transform.position = Vector3.Lerp(transform.position ,m_TargetPos,Time.deltaTime * 0.25f * Settings.SceneScaling);
			}
			
		}
		
		m_Animation.Play(""+ m_CurrentKeyState);

	}
	
	void UpdateDayNight(bool isday)
	{
		Debug.Log("Dog's day:" + isday);
	}
}
