using UnityEngine;
using System.Collections;

public class TRexStatePlayer : FollowerAI {

	int Walk = 2;
	int Jump = 5;
	int Idle = 0;
	int Run = 1;
	int Attack = 6;

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
        m_FollowStartDistance = (2048 * Settings.SceneScaling);
        m_FollowEndDistance = (6000 * Settings.SceneScaling);
        m_AttackingDistance = (1024 * Settings.SceneScaling);
        m_CurrentKeyState = Idle;
        m_AllowAxis = new Vector3(1, 0, 1); //move horizontally 
    }

}
