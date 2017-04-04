using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void PositionChangedDelegate(Vector3 position);

public class Player : ObjectExt {
	
	event GUIUpdateDelegate OnGUIUpdate;
	event GUIPlayerHealthDelegate OnGUIPlayerHealth;
	event PositionChangedDelegate OnPositionChanged;

	protected float m_DistancedCrossed = 0;
	protected float m_Health = 100;
	protected float m_Height = 775.0f;
	protected float m_JumpHeight = 736;//870.0f;
	protected float m_JumpHeight2 = 761;//870.0f;
	protected float m_EdgeGrabHeight = 736.0f;

	Physic3D physics = null;
	bool m_bJumping = false;
	bool m_bPullingUp = false;
	bool m_bStandingUp = false;
	bool m_bWalkingUp = false;
	float m_JumpStartTime = 0.0f;

	bool m_bFreeFall = false;
	float m_FreeFallStartTime = 0.0f;
	float m_FreeFallStartHeight = 256;
	float m_GroundHeight = 0.0f;
	//float m_OnAirHeight = 0.0f;
	float m_PullUpHeight = 0.0f;
	float m_WalkUpHeight = 0.0f;

	Vector3 JumpTarget = Vector3.zero;
	Vector3 PullUpTarget = Vector3.zero;

	Vector3 m_FreePosition = Vector3.zero;
	float m_PlayerYaw = 0;
	LaraStatePlayer m_AnimStatePlayer = null;
	
	//Mouse Controller
	float mouse_dx;
	float mouse_dy;

	// Use this for initialization
	void Start () {
		m_Transform = transform;
		OnGUIUpdate += GUIManager.HandleGUIUpdate;
		OnGUIPlayerHealth += GUIManager.HandleGUIPlayerHealth;
		OnPositionChanged += PositionChanged;

		//GotoKeyMapper.IdleState();
		KeyMapper.OnKeyDown += StateCodeHandler;
		KeyMapper.OnKeyIdle += IdleStateHandler;
		Mouse.m_OnMouseMove += OnMouseMove;

		physics = new Physic3D(transform.position); 
		physics.g = 5000;
		//prevy = thistransform.position.y;
		LaraStatePlayer.OnJump += JumpHandler;
		LaraStatePlayer.OnJumping += JumpingHandler;
		LaraStatePlayer.OnMovement += MovementHandler;
		LaraStatePlayer.OnPrimaryAction += PrimaryActionHandler;

		//m_FreePosition = transform.position;
		//m_PrevPlayPos = m_Transform.position;
		//m_GroundHeight = transform.position.y;

		SetInitialPosition(transform.position, transform.forward);
	}

	void SetInitialPosition(Vector3 pos, Vector3 dir)
	{
		pos += dir * 128;
		m_FreePosition = pos;
		m_PrevPlayPos = pos;
		m_GroundHeight = pos.y;
		transform.position = pos;
		PositionChanged(pos);
	}

	List<Edge> facing_edeges = new List<Edge>();

	// Update is called once per frame
	void Update () 
	{
		if((m_PrevPlayPos - m_Transform.position).sqrMagnitude > 2500)
		{
			m_DistancedCrossed += (m_Transform.position - m_PrevPlayPos).magnitude /1024.0f;
			OnGUIUpdate(new Rect(0,Screen.height - 25,Screen.width,100), "" + m_DistancedCrossed.ToString("f2"));
			m_Health -= m_DistancedCrossed * 0.00005f;
			OnGUIPlayerHealth(new Rect(0, 25,Screen.width,100), "" + m_Health.ToString("f2"));
			PositionChanged( m_Transform.position);
			m_PrevPlayPos = m_Transform.position;
		}

		//m_Room.DebugRoomSurface(facing_edeges);

		if(m_AnimStatePlayer == null)
		{
			m_AnimStatePlayer = GetComponent<LaraStatePlayer>();
			//m_AnimStatePlayer.enabled = true;
		}

		InputUpdate();

		//Update locomotion physics;
	
		if(!m_bJumping && !m_bPullingUp && !m_bWalkingUp && !m_bStandingUp)
		{
			if(m_bFreeFall)
			{
				transform.position = physics.UpdateFreeFall(Time.time - m_FreeFallStartTime);
				//m_bFreeFall = (m_GroundHeight > transform.position.y) ;
				if(m_GroundHeight > transform.position.y)
				{
					m_bFreeFall = false;
					transform.position = new Vector3(transform.position.x,m_GroundHeight,transform.position.z);
				}
			}
			else // simple height fixing
			{
				float h = Mathf.Lerp(transform.position.y ,m_GroundHeight, Time.deltaTime * 10.0f);// + 0.1f;
				transform.position = new Vector3(transform.position.x,h,transform.position.z);
			}
		}
	
		if(m_bJumping)
		{
			transform.position = physics.UpdateJump(Time.time - m_JumpStartTime);
		}

		if(m_bPullingUp)
		{
			OnPullingUp();
		}

		if(m_bStandingUp)
		{
			OnStandingUp();
		}

		if(m_bWalkingUp)
		{
			OnWalkingUp();
		}
	}
	
	void PositionChanged(Vector3 position)
	{
		//TODO: Detect Room Change with Raycasting Room
		//FIXED:Heighten raycast origin to handle floor collision through raycast. m_Transform.position + Vector3.up
		RaycastHit hit = new RaycastHit();

		#if UNITY_5_3_OR_NEWER
		int mask = Physics.DefaultRaycastLayers & ~(MaskedLayer.Switch | MaskedLayer.Player);
		#else
		int mask = Physics.kDefaultRaycastLayers & ~(MaskedLayer.Switch | MaskedLayer.Player);
		#endif

		if(Physics.Raycast(m_Transform.position + Vector3.up * 10, -Vector3.up, out hit,14096,mask ))
		{
			//room changed?
			if(hit.transform!=null &&  hit.transform != m_Room.transform)
			{
				RoomEx room = hit.transform.GetComponent<RoomEx>();
				if(room != null) //All hit objects need not to be a room
				{
					m_Room = room;
					Debug.Log("m_Room" + m_Room.name);
				}
			}
		}
		FreeFallHandler();
	}

	int PrimaryActionHandler()
	{
		Debug.Log("Primary Action Handler");

		int retval = KeyMapper.Idle;

		//if true animation state player will change state for primary animation
		//pull up animations:
		//PullUpLow: When Obstackle Height == Lara's Height
		//KeyMapper.Walk Up[50]: When Obstackle Height == Lara's heap
		//Auto jump and grab[172] + pullup[PullUpHigh]: When Obstackle Height == twice Lara's Height
		//Manual jump and grab[172] + pullup[PullUpHigh]: When Obstackle Height > twice Lara's Height
		//Manual jump and grab[172] + pullupAcrobatic[PullUpAcrobatic]: When Obstackle Height > twice Lara's Height
		//simi left[136]: 
		//simi right[137]:
		//jump grab[150]:
		//switching[197]
		//61,62 : walk back

		//TODO:
		//All jump target must be cleared after successfull pullup. Other wise normal jump
		//will be effected
		
		bool hasGrabableEdge = false;
		Transform t = m_Transform.FindChild("objPart:0");
		List<Edge> horizontal_edeges = m_Room.RayCast(t.position, transform.forward, 200);
	
		if(horizontal_edeges.Count > 0)
		{
			for(int i = 0; i < horizontal_edeges.Count; i++)
			{
				facing_edeges.Add(horizontal_edeges[i]);

				//float dot = Vector3.Dot(m_Room.transform.TransformDirection(horizontal_edeges[i].AtoB), transform.forward);
				//bool facing = (dot < 0.1f && dot > -0.1f);

				Vector3 ep0 = m_Room.transform.TransformPoint(horizontal_edeges[i].PointA);
				Vector3 ep1 = m_Room.transform.TransformPoint(horizontal_edeges[i].PointB);
				Vector3 edge = ep1 - ep0;
				Vector3 toplayer = transform.position - ep0;

				float pullupheight = ep0.y - transform.position.y;
				m_PullUpHeight = pullupheight;
			
				if( pullupheight > this.m_Height  && pullupheight < this.m_Height + 1024)
				{
					JumpTarget = Vector3.up * (pullupheight - m_JumpHeight);
					PullUpTarget = transform.position + Vector3.up * (pullupheight - m_JumpHeight);
					retval =  KeyMapper.Jump;
				}
				else if( pullupheight > (this.m_Height * 2.0f)  && pullupheight < ((this.m_Height * 2.0f)  + 1024))
				{
					JumpTarget = Vector3.up * (pullupheight - m_JumpHeight);
					PullUpTarget = transform.position + Vector3.up * (pullupheight - m_JumpHeight);
					retval =  KeyMapper.Jump;
				}
				else if(ep0.y < (transform.position.y + this.m_Height))
				{
					if(((transform.position.y + this.m_Height) - ep0.y) < 100)
					{
						JumpTarget = Vector3.zero;
						PullUpTarget = transform.position;
						m_bPullingUp = true;

						retval =  KeyMapper.Idle;
					}
					else
					{
						JumpTarget = Vector3.zero;
						PullUpTarget = transform.position;
						m_bWalkingUp = true;
						retval = KeyMapper.WalkUp;
					}
				}

				/*bool inside_edge = (Vector3.Dot (edge.normalized, toplayer.normalized ) > 0);
				//bool facing = (Vector3.Dot (edge.normalized, toplayer.normalized ) > 0);
				//if(ep0.y > transform.position.y)
				//{
					facing_edeges.Add(horizontal_edeges[i]);
					//hasGrabableEdge = true;
					//break;
				//}

				if((ep0.y - transform.position.y) > this.m_Height )
				{
					hasGrabableEdge = true;

					return KeyMapper.PullUpLow;
				}*/

			}
		}
	
		return retval;
	}

	void FreeFallHandler()
	{
		if(m_bPullingUp || m_bWalkingUp || m_bStandingUp)
		{
			m_bFreeFall = false;
			return;
		}
	
		#if UNITY_5_3_OR_NEWER
		int mask = Physics.DefaultRaycastLayers & ~(MaskedLayer.Switch | MaskedLayer.Player);
		#else
		int mask = Physics.kDefaultRaycastLayers & ~(MaskedLayer.Switch | MaskedLayer.Player);
		#endif

		RaycastHit hit = new RaycastHit();

		Transform t = m_Transform.FindChild("objPart:0");
		Vector3 origin = t.position + Vector3.up * 0 - Vector3.forward * 50;

		if(Physics.Raycast(origin, -Vector3.up, out hit,14096,mask ))
		{
			if(hit.transform.root == this.transform)
			{
				Debug.Log("Warning! Self Collision With:" + this.transform.name);
				return;
			}

			if(!m_bFreeFall && !m_bJumping)
			{
				if(hit.point.y != m_GroundHeight)//try free fall
				{
					m_bFreeFall = (transform.position.y - hit.point.y > m_FreeFallStartHeight) ;
					physics.StartFreeFall(transform.position);
					m_FreeFallStartTime = Time.time;
					Debug.Log("Start Free Fall");
				}
			}
			
			//Debug.DrawRay(origin,-Vector3.up * 14096 , Color.red);
			m_GroundHeight = hit.point.y;
		}

		//Debug.Log("Free Fall Handler");
	
	}
	
	void JumpHandler(Vector3 From, Vector3 To, Quaternion rot , float sign)
	{
		m_JumpStartTime = Time.time;

		//JumpTarget
		if(JumpTarget.sqrMagnitude != 0)
		{
			m_bPullingUp = true;
			m_bJumping = physics.CalculateCurve(From,JumpTarget,rot,sign); 

		}
		else
		{
			m_bJumping = physics.CalculateCurve(From,To,rot,sign);  
		}
		//Debug.Log("Jump Physics Handler" + m_bJumping);
	}

	void JumpingHandler(Vector3 dir)
	{
		if(m_AnimStatePlayer != null)
		{
			RaycastHit hit2 = new RaycastHit();

			#if UNITY_5_3_OR_NEWER
			int mask = Physics.DefaultRaycastLayers & ~(MaskedLayer.Switch | MaskedLayer.Player);
			#else
			int mask = Physics.kDefaultRaycastLayers & ~(MaskedLayer.Switch | MaskedLayer.Player);
			#endif

			bool collision = Physics.Raycast(transform.position + Vector3.up * 400, dir, out hit2,150,mask );
			
			if(collision)
			{
				m_AnimStatePlayer.OnCollision(hit2.collider);
				transform.position = m_FreePosition;
				m_GroundHeight = m_GroundHeight + 0.1f; //force freefall handler to check ground...if gorund not changed since last jump
				m_bJumping = false;
				JumpTarget = Vector3.zero;
			}
			else if(transform.position.y < m_GroundHeight) //landed
			{
				m_AnimStatePlayer.OnCollision(hit2.collider);
				transform.position = new Vector3(transform.position.x,m_GroundHeight,transform.position.z);
				m_bJumping = false;
				JumpTarget = Vector3.zero;
			}
			//else //bug fix
			//{
				m_FreePosition = transform.position;
			//}
		}

		//Debug.Log("Jumping");
		//Debug.DrawRay(transform.position + Vector3.up * 400, dir * 150);
	}

	void OnReachPullUpTarget()
	{
		m_bJumping = false;
		//MoveForward
		if(m_AnimStatePlayer != null)
		{
			m_bStandingUp = true;
			m_AnimStatePlayer.OnPullUp(KeyMapper.PullUpHigh);
		}
	}

	void OnPullingUp()
	{
		if(m_bJumping && m_bPullingUp)
		{
			//check for hip displacement from ground
			float diff = (transform.position - PullUpTarget).magnitude;
			Debug.Log("Pullig UP" + diff);
			if(diff < 1f )
			{
				OnReachPullUpTarget();
			}
		}
		else if(m_bPullingUp)
		{
			//Pulling up without jump
			OnReachPullUpTarget();
		}
	}
	
	void OnStandingUp()
	{
		Transform t = m_Transform.FindChild("objPart:0");
		float diff = t.position.y - m_Transform.position.y;
		if(diff > 1140) //ful standing position
		{
			m_bPullingUp = false;
			m_bStandingUp = false;
			JumpTarget = Vector3.zero;

			if(m_AnimStatePlayer != null)
			{

				MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
				foreach(MeshRenderer mr in mrs)
				{
					mr.enabled = false;
				}
				
				FlickerPos = new Vector3(t.position.x, PullUpTarget.y + m_JumpHeight, t.position.z);
				m_AnimStatePlayer.OnPullUp(KeyMapper.Idle);
				Invoke("Flickring", 0.00025f);

			}
		}
		else
		{
			Debug.Log("Stading UP!" + diff);
		}
	}


	Vector3 FlickerPos;

	void OnWalkingUp()
	{
		Transform t = m_Transform.FindChild("objPart:0");
		float diff = t.position.y - m_Transform.position.y;
		Debug.Log("OnWalkingUp:" + diff);

		if(diff > 935) //ful standing position
		{
			m_bWalkingUp = false;
			if(m_AnimStatePlayer != null)
			{
				MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
				foreach(MeshRenderer mr in mrs)
				{
					mr.enabled = false;
				}

				FlickerPos = new Vector3(t.position.x, PullUpTarget.y + m_PullUpHeight, t.position.z);
				m_AnimStatePlayer.OnPullUp(KeyMapper.Idle);
				Invoke("Flickring", 0.00025f);
			}
		}
	}

	void Flickring()
	{
		//gameObject.SetActive(true);
		MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
		foreach(MeshRenderer mr in mrs)
		{
			mr.enabled = true;
		}

		m_Transform.position = FlickerPos;
		SetInitialPosition(m_Transform.position, m_Transform.forward);
	}

	void MovementHandler(Vector3 dir, float speed)
	{
		transform.position = transform.position + dir * speed;
		if(m_AnimStatePlayer != null)
		{
			RaycastHit hit2 = new RaycastHit();

			#if UNITY_5_3_OR_NEWER
			int mask = Physics.DefaultRaycastLayers & ~(MaskedLayer.Switch | MaskedLayer.Player);
			#else
			int mask = Physics.kDefaultRaycastLayers & ~(MaskedLayer.Switch | MaskedLayer.Player);
			#endif

			Transform t = m_Transform.FindChild("objPart:0");
			bool collision = Physics.Raycast(t.position, dir, out hit2,150,mask );

			if(collision)
			{
				m_AnimStatePlayer.OnCollision(hit2.collider);
				transform.position = m_FreePosition;
			}
			//else //bug fix
			//{
				m_FreePosition = transform.position;
			//}
		}
	
		//Debug.Log("MovementHandler");
	}
	
	public void StateCodeHandler(int keystate, int otherkey, float time)
	{
		if(m_bFreeFall || m_bJumping || m_bPullingUp || m_bWalkingUp || m_bStandingUp) return;
		if(m_AnimStatePlayer != null)
		{
			//try initiate pullup satecode (17) for running(16) and hitting wall
			if(keystate == KeyMapper.Run && m_Room!=null)
			{
				Vector3 normal = Vector3.up;
				Transform t = m_Transform.FindChild("objPart:0");
				bool hit = m_Room.HitTest(t.position, m_Transform.forward,ref normal, 200);
				if(hit)
				{
					Debug.Log("Wall Hit Test:" + hit);
					keystate = 17;
				}
			}
		
			m_AnimStatePlayer.StateCodeHandler(keystate,otherkey,time);
		}
	}

	public void IdleStateHandler(int keystate , float time)
	{
		//m_FreePosition = transform.position;
		if(m_bJumping || m_bPullingUp || m_bWalkingUp|| m_bStandingUp) return;
		if(m_AnimStatePlayer != null)
		{
			m_AnimStatePlayer.IdleStateHandler(keystate , time);
		}
	}

	public void InputUpdate()
	{
		if(!m_bJumping && !m_bPullingUp && !m_bWalkingUp && !m_bPullingUp)
		{
			m_PlayerYaw += mouse_dx * 0.5f;
			mouse_dx = 0;
			transform.rotation = Quaternion.Euler(0.0f,m_PlayerYaw,0.0f);
		}
	}
	
	void OnMouseMove(float dx, float dy)
	{
		mouse_dx = dx;
		mouse_dy = dy;
	}

	/*function OnCollisionEnter(collision : Collision) {
		// Debug-draw all contact points and normals
		for (var contact : ContactPoint in collision.contacts)
			Debug.DrawRay(contact.point, contact.normal, Color.white);
		
		// Play a sound if the coliding objects had a big impact.		
		if (collision.relativeVelocity.magnitude > 2)
			audio.Play();
	}


	function OnTriggerEnter (other : Collider) {
		Destroy(other.gameObject);
	}*/

}
