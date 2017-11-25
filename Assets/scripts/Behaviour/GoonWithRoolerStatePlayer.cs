using UnityEngine;
using System.Collections;

public class GoonWithRoolerStatePlayer : FollowerAI {
	int Walk = 0;
	int Jump = 27;
	int Idle = 6;
	int Run = 0;//25;
	int Attack = 2;

    override protected void OnStartFollow()
    {
        m_CurrentKeyState = Run;
    }

    override protected void OnStartAttack()
    {
        m_CurrentKeyState = Attack;
    }

    override protected void OnGoIdle()
    {
        m_CurrentKeyState = Idle;
    }

    override protected void OnStopFollow()
    {
        m_CurrentKeyState = Walk;
    }

    override protected void InitAI()
    {
        m_FollowStartDistance = (1024 * Settings.SceneScaling);
        m_FollowEndDistance = (4096 * Settings.SceneScaling);
        m_AttackingDistance = (256 * Settings.SceneScaling);
        m_CurrentKeyState = Idle;
        m_AllowAxis = new Vector3(1, 0, 1); //move horizontally 
    }

    void UpdateDayNight(bool isday)
	{
		Debug.Log("Goon's day:" + isday);
	}
}
