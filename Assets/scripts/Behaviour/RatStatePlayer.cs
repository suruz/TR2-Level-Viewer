using UnityEngine;
using System.Collections;

public class RatStatePlayer : FollowerAI {

	int Walk = 8;
	int Jump = 3;
	int Idle = 7;
	int Run = 5;
	int Attack = 4;

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
        m_FollowStartDistance = (512 * Settings.SceneScaling);
        m_FollowEndDistance = (2048 * Settings.SceneScaling);
        m_AttackingDistance = (128 * Settings.SceneScaling);
        m_CurrentKeyState = Idle;
        m_AllowAxis = new Vector3(1, 0, 1); //move horizontally 
    }

    void UpdateDayNight(bool isday)
    {
        Debug.Log("Rat's day:" + isday);
    }

}
