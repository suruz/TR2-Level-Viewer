using UnityEngine;
using System.Collections;

public class AIState
{
    public int Walk = 8;
    public int Jump = 3;
    public int Idle = 7;
    public int Run = 5;
    public int Attack = 4;

    public AIState(int walk, int jump, int idle, int run, int attack)
    {
        Walk = walk;
        Jump = jump;
        Idle = idle;
        Run = run;
        Attack = attack;
    }
}

public class FollowerAI : MonoBehaviour {

    public Transform m_FollowTransform;
    public Transform[] m_MateTransforms;
    public Parser.Tr2Item m_Tr2Item;

    Vector3 m_StartPos = Vector3.zero;
    Vector3 m_TargetPos = Vector3.zero;
    Vector3 m_CurrentPos = Vector3.zero;
    int m_PrevKeyState = 0;
    protected int m_CurrentKeyState = 0;
    Animation m_Animation = null;
    Transform m_Transform;

    bool m_IsTargetInRange = false;
    bool m_CanAttackTarget = false;

    protected float m_FollowStartDistance = 0;
    protected float m_FollowEndDistance = 0;
    protected float m_AttackingDistance = 0;
    protected Vector3 m_AllowAxis = Vector3.one;
    // Use this for initialization
    void Start ()
    {
        m_Animation = GetComponent<Animation>();
        InitAI();
        m_StartPos = transform.position;
        m_TargetPos = m_StartPos;
        m_Transform = transform;
    }
	
	// Update is called once per frame
	void Update () {

        if (Level.m_Player != null)
        {
            m_FollowTransform = Level.m_Player.transform;
        }

        if (m_FollowTransform != null)
        {
            Vector3 follow = ScaleVector((m_FollowTransform.position - m_StartPos), m_AllowAxis);
            float dist = follow.magnitude;
            if (dist > m_FollowStartDistance && dist < m_FollowEndDistance)
            {
                m_TargetPos = m_StartPos + follow - follow.normalized * 512 * Settings.SceneScaling;
              
                OnStartFollow();
                if ((m_Transform.position - m_TargetPos).magnitude < m_AttackingDistance )
                {
                    OnStartAttack();
                }

                Vector3 fwrd = (m_FollowTransform.position - m_Transform.position).normalized;
                fwrd.y = 0;
                m_Transform.forward = fwrd;
                m_Transform.position = Vector3.Lerp(m_Transform.position, m_TargetPos, Time.deltaTime);
            }
            else if ((m_Transform.position - m_StartPos).magnitude < (256 * Settings.SceneScaling))
            {
                OnGoIdle();
            }
            else
            {
                m_TargetPos = m_StartPos;
                OnStopFollow();

                Vector3 fwrd = (m_TargetPos - m_Transform.position).normalized;
                fwrd.y = 0;
                m_Transform.forward = fwrd;
                m_Transform.position = Vector3.Lerp(m_Transform.position, m_TargetPos, Time.deltaTime * 0.25f);
            }

        }

        m_Animation.Play("" + m_CurrentKeyState);
    }

    virtual protected AIState SetAIState()
    {
        return null;
    }

    virtual protected void OnStartFollow()
    {

    }

    virtual protected void OnStartAttack()
    {

    }

    virtual protected void OnGoIdle()
    {

    }

    virtual protected void OnStopFollow()
    {

    }

    virtual protected void InitAI()
    {
     
    }

    Vector3 ScaleVector(Vector3 a, Vector3 b)
    {
        return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
    }

}
