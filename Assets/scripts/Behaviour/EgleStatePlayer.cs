using UnityEngine;
using System.Collections;

public class EgleStatePlayer : FollowerAI {

	int Walk = 10;
	int Jump = 0;
	int Idle = 7;
	int Run = 10;
	int Attack = 0;

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
        m_FollowEndDistance = (8096 * Settings.SceneScaling);
        m_AttackingDistance = (256 * Settings.SceneScaling);
        m_CurrentKeyState = Idle;
        m_AllowAxis = new Vector3(1, 1, 1); //move freely
    }

    void UpdateDayNight(bool isday)
	{
		Debug.Log("Egle's day:" + isday);
	}

}
