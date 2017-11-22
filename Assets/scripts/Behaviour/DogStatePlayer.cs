using UnityEngine;
using System.Collections;

public class DogStatePlayer : FollowerAI {

	int Walk = 1;
	int Jump = 11;
	int Idle = 10;
	int Run = 14;
	int Attack = 18;

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
		Debug.Log("Dog's day:" + isday);
	}
}
