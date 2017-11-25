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
	int Idle = 2;
	int Run = 0;
	int Attack = 0;
	
	int m_PrevKeyState = 0;
	int m_CurrentKeyState = 0;
	Animation m_Animation = null;
    Transform m_Transform;
    // Use this for initialization
    void Start () 
	{
		m_Animation = GetComponent<Animation>();
		m_CurrentKeyState = Idle;
		m_StartPos = transform.position;
		m_TargetPos = m_StartPos;
		DayNightSystem.AddDayNightEventHandler(UpdateDayNight);
        m_Transform = transform;
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
            Vector3 direction = (m_TargetPos - m_Transform.position);
            direction.y = 0;
            m_Transform.forward = direction.normalized;
        
      
			if(direction.magnitude > (2024 * Settings.SceneScaling) )
			{
				m_CurrentKeyState = Walk;
                Vector3 pos = Vector3.Lerp(m_Transform.position, m_TargetPos, Time.deltaTime * 0.15f);
                pos.y = m_TargetPos.y;
                m_Transform.position = pos;
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
