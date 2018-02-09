using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public delegate void PositionChangedDelegate(Vector3 position);

public class Player : ObjectExt
{

    event GUIUpdateDelegate OnGUIUpdate;
    event GUIPlayerHealthDelegate OnGUIPlayerHealth;
    event PositionChangedDelegate OnPositionChanged;

    protected float m_DistancedCrossed = 0;
    protected float m_Health = 100;
    protected float m_Height = 775.0f * Settings.SceneScaling;
    protected float m_JumpHeight = 736 * Settings.SceneScaling;
    protected float m_JumpHeight2 = 761 * Settings.SceneScaling;
    protected float m_EdgeGrabHeight = 736.0f * Settings.SceneScaling;

    Physic3D physics = null;
    bool m_bJumping = false;
    bool m_bPullingUp = false;
    bool m_bStandingUp = false;
    bool m_bWalkingUp = false;
    float m_JumpStartTime = 0.0f;

    bool m_bFreeFall = false;
    float m_FreeFallStartTime = 0.0f;
    float m_FreeFallStartHeight = 256 * Settings.SceneScaling;
    float m_GroundHeight = 0.0f;
	float m_GroundHeightLast = 0.0f;
    //float m_OnAirHeight = 0.0f;
    float m_PullUpHeight = 0.0f;
    float m_WalkUpHeight = 0.0f;

    Vector3 JumpVector = Vector3.zero;
    Vector3 PullUpTarget = Vector3.zero; //Manually heightens root pivot up to this position while jumping/walking up

    Vector3 m_FreePosition = Vector3.zero;
    float m_PlayerYaw = 0;
    float m_PlayerTilt = 0;
    public LaraStatePlayer m_AnimStatePlayer = null;

    //Mouse Controller
    float mouse_dx;
    float mouse_dy;
    float m_maxHorizontalDisplacement = Settings.SceneScaling * Settings.SceneScaling * 2500;
    float m_maxForwardRayLength = 100 * Settings.SceneScaling;
    float m_maxDownRayLength = 1000000f;
    float m_maxUpRayLength = 200 * Settings.SceneScaling;

    float m_minDownRayLength = 200 * Settings.SceneScaling;
    float m_minUpRayLength = 200 * Settings.SceneScaling;

    bool m_bWallHitTest = false;
	Transform m_LarasHeap;
	float m_HipHeightAdjust = -0.1f;
    Transform m_LarasHead;
	
	float m_WaterLevel = 0;
    bool m_bDivingIntoWater = false; //to handle diving transions from water surface. if enabled,let finish it;
	
	//Vector3 m_LarasHeapPosition;
    // Use this for initialization
    void Start()
    {
        m_Transform = transform;
        OnGUIUpdate += GUIManager.HandleGUIUpdate;
        OnGUIPlayerHealth += GUIManager.HandleGUIPlayerHealth;
        OnPositionChanged += PositionChanged;

        //GotoKeyMapper.IdleState();
        KeyMapper.OnKeyDown += StateCodeHandler;
        KeyMapper.OnKeyIdle += IdleStateHandler;
        Mouse.m_OnMouseMove += OnMouseMove;

        physics = new Physic3D(m_Transform.position);
        physics.g = 5000 * Settings.SceneScaling;
        //prevy = thistransform.position.y;
        LaraStatePlayer.OnJump += JumpHandler;
        LaraStatePlayer.OnJumping += JumpingHandler;
        LaraStatePlayer.OnMovement += MovementHandler;
        //LaraStatePlayer.OnPrimaryAction += PrimaryActionHandler; //this callback is not needed anymore

        //m_FreePosition = m_Transform.position;
        //m_PrevPlayPos = m_Transform.position;
        //m_GroundHeight = m_Transform.position.y;
		m_LarasHeap = m_Transform.Find("objPart:0");
        m_LarasHead = m_LarasHeap.Find("objPart:7").Find("objPart:14");
                                                  //m_LarasHeapPosition = m_LarasHeap.position - Vector3.up * 0.25f;
        SetInitialPosition(m_Transform.position, transform.forward);

        physics.OnReachedMaximumHeight += DidReachedMaxJumpHeight;
    }

    void SetInitialPosition(Vector3 pos, Vector3 dir)
    {
        pos += dir * 128 * Settings.SceneScaling;
        m_FreePosition = pos;
        m_PrevPlayPos = pos;
        m_GroundHeight = pos.y;
        m_Transform.position = pos;
        PositionChanged(pos);
    }

    List<Edge> facing_edeges = new List<Edge>();

    // Update is called once per frame
    void Update()
    {
        if ((m_PrevPlayPos - m_Transform.position).sqrMagnitude > m_maxHorizontalDisplacement)
        {
            m_DistancedCrossed += (m_Transform.position - m_PrevPlayPos).magnitude / (1024.0f * Settings.SceneScaling);
            OnGUIUpdate(new Rect(0, Screen.height - 25, Screen.width, 100), "" + m_DistancedCrossed.ToString("f2"));
            m_Health -= m_DistancedCrossed * 0.00005f;
            OnGUIPlayerHealth(new Rect(0, 25, Screen.width, 100), "" + m_Health.ToString("f2"));
            PositionChanged(m_Transform.position);
            m_PrevPlayPos = m_Transform.position;
        }

        //m_Room.DebugRoomSurface(facing_edeges);

        InputUpdate();

        //Update locomotion physics;

        if (!IsAvoidingFall() && !m_bFreeFall)
        {
           // simple height fixing
             if (m_SwimState == SwimmingState.None)
             {
                   float h = Mathf.Lerp(m_Transform.position.y, m_GroundHeight, Time.deltaTime * 10.0f);// + 0.1f;
                   m_Transform.position = new Vector3(m_Transform.position.x, h, m_Transform.position.z);
             }
		}
       //update freefall physics. update should be continueous, freefall update should not be event drivent
       if (m_bFreeFall)
       {
           m_Transform.position = physics.UpdateFreeFall(Time.time - m_FreeFallStartTime);

           if (m_GroundHeight > m_Transform.position.y)
           {
               m_bFreeFall = false;
               m_Transform.position = new Vector3(m_Transform.position.x, m_GroundHeight, m_Transform.position.z);
           }
       }

        if (m_bPullingUp)
        {
            OnPullingUp();
        }

        if (m_bStandingUp)
        {
            OnStandingUp();
        }

        if (m_bWalkingUp)
        {
            OnWalkingUp();
        }
    }
	
	//This is commissioned event fired independently directly on Update
    void PositionChanged(Vector3 position)
    {
        //TODO: Detect Room Change with Raycasting Room
        //FIXED:Heighten raycast origin to handle floor collision through raycast. m_Transform.position + Vector3.up
		//TODO: following Raycast can be done by SeekGround()
        RaycastHit hit = new RaycastHit();
#if (UNITY_5_3_OR_NEWER || UNITY_5_3)
        int mask = Physics.DefaultRaycastLayers & ~(MaskedLayer.Switch | MaskedLayer.Player);
#else
		int mask = Physics.kDefaultRaycastLayers & ~(MaskedLayer.Switch | MaskedLayer.Player);
#endif
        if (Physics.Raycast(m_Transform.position + Vector3.up * 10 * Settings.SceneScaling, -Vector3.up, out hit, m_maxDownRayLength, mask))
        {
            //room changed?
            if (hit.transform != null)
            {
                //there may be lara's current room == null,  first check for this
                if (m_Room != null)
                {
                    if (hit.transform != m_Room.transform)
                    {
                        RoomEx room = hit.transform.GetComponent<RoomEx>();
                        if (room != null) //All hit objects need not to be a room
                        {
                            m_Room = room;
                            if (m_Room != null)
                            {
                               // Debug.Log("m_Room" + m_Room.name + "room flag " + m_Room.Flags);
                            }
                        }
                    }
                }
                else
                {
                    RoomEx room = hit.transform.GetComponent<RoomEx>();
                    if (room != null) //All hit objects need not to be a room
                    {
                        m_Room = room;
                        //Debug.Log("m_Room" + m_Room.name);
                    }

                }
            }
        }

        //may hit ground / may hit water
        if (m_Room != null)
        {
            SetSwimState(m_Room);
        }

        FreeFallHandler();  //free falling will continue until hit ground height
    }

    int PrimaryActionHandler()
    {
        //Debug.Log("Primary Action Handler");

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
        Vector3 heappos = GetHipPosition();
        if (m_Room == null)
        {
            Debug.LogError("Player room is not set");
            return retval;
        }
        List<Edge> horizontal_edeges = m_Room.RayCast(heappos, transform.forward, m_maxForwardRayLength * 2);  // ray length must be larger than minimum collision length m_maxCollisionRayLength

        if (horizontal_edeges.Count > 0)
        {
            for (int i = 0; i < horizontal_edeges.Count; i++)
            {
                facing_edeges.Add(horizontal_edeges[i]);

                //float dot = Vector3.Dot(m_Room.transform.TransformDirection(horizontal_edeges[i].AtoB), transform.forward);
                //bool facing = (dot < 0.1f && dot > -0.1f);

                Vector3 ep0 = m_Room.transform.TransformPoint(horizontal_edeges[i].PointA);
                Vector3 ep1 = m_Room.transform.TransformPoint(horizontal_edeges[i].PointB);
                Vector3 edge = ep1 - ep0;
                Vector3 toplayer = m_Transform.position - ep0;

                float pullupheight = ep0.y - m_Transform.position.y;
                m_PullUpHeight = pullupheight;

                if (pullupheight > this.m_Height && pullupheight < this.m_Height * 3.5f)
                {
                    JumpVector = Vector3.up * (pullupheight - m_JumpHeight); //Jump controlled by JumpVector. If length of JumpVector > 0 m_bPullingUp will be true  
                    PullUpTarget = m_Transform.position + JumpVector; // Move animated Lara upto this position
                    retval = KeyMapper.Jump;

                    Debug.Log("pulling up high");
                }
                else if (pullupheight < this.m_Height) // walkup height in normal condition, or,  bellow water surface in swimstate
                {
                    Debug.Log("pulling  up");
                    PullUpTarget = m_Transform.position; // Move animated Lara upto this position

                    if (((this.m_Height - pullupheight) < (100 * Settings.SceneScaling)) && (m_SwimState == SwimmingState.None))
                    {
                        ResetJump();
                        m_bPullingUp = false; //we dont need pull her in this height
                        m_bStandingUp = true;
                        retval = KeyMapper.PullUpHigh;

                        Debug.Log("pulling   up on platform");
                    }
                    else if (m_SwimState == SwimmingState.InWaterSurface)
                    {
                        ResetJump();
                        m_bPullingUp = false; //we dont need pull her in this height
                        m_bStandingUp = true;
                        retval = KeyMapper.PullUpHigh;

                        Debug.Log("pulling   up from water");
                    }
                    else if ((m_SwimState == SwimmingState.None) ||(m_SwimState == SwimmingState.InShallowWater))
                    {
                        ResetJump();
                        m_bPullingUp = false; //we dont need pull her in this height
                        m_bWalkingUp = true;
                        retval = KeyMapper.WalkUp; //consider under water
                        Debug.Log("walking  up");
                    }
                }

                /*
                 * bool inside_edge = (Vector3.Dot (edge.normalized, toplayer.normalized ) > 0);
				//bool facing = (Vector3.Dot (edge.normalized, toplayer.normalized ) > 0);
				//if(ep0.y > m_Transform.position.y)
				//{
					facing_edeges.Add(horizontal_edeges[i]);
					//hasGrabableEdge = true;
					//break;
				//}
				
				if((ep0.y - m_Transform.position.y) > this.m_Height )
				{
					hasGrabableEdge = true;

					return KeyMapper.PullUpLow;
				}*/

            }
        }

        if(retval == KeyMapper.Idle)
        {
            Debug.Log("Too high!!!");
        }

        return retval;
    }

    //FreeFallHandler is called regularly on position changed, and checks and updates ground height on the fly
    void FreeFallHandler()
    {

        if ( IsAvoidingFall() || StopFalling())
        {
            m_bFreeFall = false;
            return;
        }
		
		SeekGround();
		
		if((m_GroundHeight != m_GroundHeightLast) || IsDiving()) // ground height changed. Time to check if we call free fall( fixed: allowed free dive)
		{
			if (!m_bFreeFall && ((m_Transform.position.y - m_GroundHeight > m_FreeFallStartHeight)) )  //m_bJumping check is not needed. Its already checked by  IsAvoidingFall();
            {
                //try free fall
                m_bFreeFall = true;
                physics.StartFreeFall(m_Transform.position);
                m_FreeFallStartTime = Time.time;
                //Debug.Log("Start Free Fall");
                
            }
			m_GroundHeightLast = m_GroundHeight;
		}

        //update free fall physics : possible bug here, free fall update should not be  depended upon position changed
        //if movement stops, free fall will stop as well, this is not intended, move following code to unity Update()
        /*if (m_bFreeFall)
		{
			m_Transform.position = physics.UpdateFreeFall(Time.time - m_FreeFallStartTime);
      
        	if (m_GroundHeight > m_Transform.position.y)
        	{
             	m_bFreeFall = false;
             	m_Transform.position = new Vector3(m_Transform.position.x, m_GroundHeight, m_Transform.position.z);
        	}
		}*/
		
		 /*
             * TODO:
             * Extension: check here for possible room change ( e.g dive in water room)
             * 
             * */
		
           //TODO:
            //Extension: set other stopping condition for free fall (e.g. water hit)
            //Usially ground-room collision check is not performed while free fall
            //now we need determine ground type before we fall on ground surface.
		
        //Debug.Log("Free Fall Handler");

    }

    void JumpHandler(Vector3 From, Vector3 To, Quaternion rot, float sign)
    {

        if (SeekRoof() != null) return;

        m_JumpStartTime = Time.time;

        //JumpVector
        if (JumpVector.sqrMagnitude != 0)
        {
            m_bPullingUp = true;
            m_bJumping = physics.CalculateCurve(From, JumpVector, rot, sign);
            //Debug.Log("Jump Physics Handler" + "pulling up");

        }
        else
        {
            m_bJumping = physics.CalculateCurve(From, To, rot, sign);
            //Debug.Log("Jump Physics Handler" + "Normal Jump");
        }

        
        if (m_bJumping)
		{
			InitialiseGround();
		}
		
    }
	
	//This is commissioned event fired by LaraStatePlayer, it is not conditioned be this.m_bJumping
	//This can only be stoped by LaraStatePlayer. Using stoping condition here is useless
    void JumpingHandler(Vector3 dir)
    {
        //Debug.Log("JumpingHandler");

        Collider roof = SeekRoof();
        if (roof!=null)
        {
            Debug.LogWarning("Head Collision With: " + roof.name);
            StopImmediate(roof.gameObject);
            m_Transform.position = m_FreePosition;
            m_GroundHeight = m_GroundHeight + 0.1f; //force freefall handler to check ground...if ground not changed since last jump
            ResetJump();
        }
        else if (m_Transform.position.y < m_GroundHeight) //landed
        {
            //Debug.Log("Landed");
            StopImmediate(m_Room.gameObject);
            m_Transform.position = new Vector3(m_Transform.position.x, m_GroundHeight, m_Transform.position.z);
            ResetJump();
        }
		
		    //update jump physics
           m_Transform.position = physics.UpdateJump(Time.time - m_JumpStartTime);
           RecordFreePosition(); // record unconditional free position        

        SeekGround();  //Keep a eye on ground height changes   


    }

    void OnReachPullUpTarget()
    {
        //MoveForward
        if (m_AnimStatePlayer != null)
        {
            m_bPullingUp = false;
            m_bStandingUp = true;
            m_AnimStatePlayer.OnPullUp(KeyMapper.PullUpHigh);
        }
    }

    void OnPullingUp()
    {
        if (m_bJumping)
        {
            //check for hip displacement from ground
            float diff = (m_Transform.position - PullUpTarget).magnitude;
            //Debug.Log("Pullig UP" + diff);
            if (diff < 1f * Settings.SceneScaling)
            {
                ResetJump();
                OnReachPullUpTarget();
            }
        }
        else
        {
            //Pulling up without jump
            OnReachPullUpTarget();
        }
    }

    void OnStandingUp()
    {
		bool StandedUp = false;
		if (m_SwimState == SwimmingState.None)
		{
			float diff = m_LarasHeap.position.y - m_Transform.position.y;
        	if (diff > (1100 * Settings.SceneScaling)) //ful standing position
        	{
				StandedUp = true;
				m_bStandingUp = false;
				FlickerPos = new Vector3(m_LarasHeap.position.x, PullUpTarget.y + m_JumpHeight, m_LarasHeap.position.z);
			}

            if (m_AnimStatePlayer != null && diff < 0.01)
            {
                m_AnimStatePlayer.PlayPullUpSFX();
            }
        }
		
 		if (m_SwimState == SwimmingState.InWaterSurface)
		{
			float diff = m_LarasHeap.position.y - m_Transform.position.y;
        	if (diff > 0.4f) //ful standing position
        	{
				StandedUp = true;
				m_bStandingUp = false;
			    FlickerPos = new Vector3(m_LarasHeap.position.x, PullUpTarget.y, m_LarasHeap.position.z);
               
            }

            if (m_AnimStatePlayer != null && diff < 0.01)
            {
                m_AnimStatePlayer.PlayPullUpSFX();
            }

        }
		
		if(StandedUp)
		{
			MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mr in mrs)
            {
               mr.enabled = false;
            }
            Invoke("Flickring", 0.05f);
		}
		 
    }


    Vector3 FlickerPos;

    void OnWalkingUp()
    {
        float diff = m_LarasHeap.position.y - m_Transform.position.y;
        //Debug.Log("OnWalkingUp:" + diff);

        if (diff > (935 * Settings.SceneScaling)) //ful standing position
        {
            m_bWalkingUp = false;
			MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
            foreach (MeshRenderer mr in mrs)
            {
               mr.enabled = false;
            }
            m_AnimStatePlayer.PlayWalkUpSFX();
            FlickerPos = new Vector3(m_LarasHeap.position.x, PullUpTarget.y + m_PullUpHeight, m_LarasHeap.position.z);
            Invoke("Flickring", 0.05f);
            
        }
    }

    void Flickring()
    {
        //gameObject.SetActive(true);
        MeshRenderer[] mrs = GetComponentsInChildren<MeshRenderer>();
        foreach (MeshRenderer mr in mrs)
        {
            mr.enabled = true;
        }

        m_Transform.position = FlickerPos;
        SetInitialPosition(m_Transform.position, m_Transform.forward);

        //if (m_SwimState == SwimmingState.InWaterSurface)
        //{
        SetSwimStateNone();
           //Grab ledge and get outof water
        //}
		Debug.Log("Post pull up stoping");
		StopImmediate(null);
		
    }

    void MovementHandler(Vector3 dir, float speed)
    {
        m_Transform.position = m_Transform.position + dir * speed * Settings.SceneScaling;
        
       	RaycastHit hit2 = new RaycastHit();
#if (UNITY_5_3_OR_NEWER || UNITY_5_3)
        int mask = Physics.DefaultRaycastLayers & ~(MaskedLayer.Switch | MaskedLayer.Player);
#else
		int mask = Physics.kDefaultRaycastLayers & ~(MaskedLayer.Switch | MaskedLayer.Player);
#endif
		
		Vector3 heappos = GetHipPosition();
        bool collision = Physics.Raycast(heappos, dir, out hit2, m_maxForwardRayLength, mask);
        if (collision)
        {
            StopImmediate(hit2.collider.gameObject);
            m_Transform.position = m_FreePosition;
            m_bWallHitTest = true;
        }
            
		RecordFreePosition(); // record unconditional free position           
		Debug.DrawRay(heappos, dir * m_maxForwardRayLength * 100, Color.green);
        
        //Debug.Log("MovementHandler");
    }

    public void StateCodeHandler(int keystate, int otherkey, float time)
    {
        if (IsAvoidingFall() || IsDiving()) return;
        if (m_AnimStatePlayer != null)
        {

            //try initiate pullup satecode due to hitting wall except primary action (e.g. pull switch)
            /*
             * folloing condition is useless: if (keystate == KeyMapper.Run) true, then (keystate != KeyMapper.PrimaryAction)  also true
             * if (keystate == KeyMapper.Run && keystate != KeyMapper.PrimaryAction) { }
             * 
             */

            if (keystate == KeyMapper.Run /*&& keystate != KeyMapper.PrimaryAction*/) 
            {
                if (m_bWallHitTest)
                {
                   // Debug.Log("Hit Test:");
                    m_bWallHitTest = false;
                    keystate = PrimaryActionHandler();
					//Debug.Log("Swimm State:" + m_SwimState);
                }
            }
			
			if(m_SwimState == SwimmingState.InWaterSurface && keystate == KeyMapper.Jump && keystate != KeyMapper.PrimaryAction)
			{
                //m_Transform.position = new Vector3(m_Transform.position.x, m_Transform.position.y - m_Height * 0.5f, m_Transform.position.z);
                //SetSwimStateDeepWater();
                //StopImmediate(null);
                //SetSwimStateNone();
                //FreeFallHandler();
                if (!m_bDivingIntoWater)
                {
                    m_bDivingIntoWater = true;
                    SetSwimStateDiving();
                    FreeFallHandler();
                 
                }

            }
			else
			{
				//m_AnimStatePlayer.StateCodeHandler(keystate, otherkey, time);
			}

            m_AnimStatePlayer.StateCodeHandler(keystate, otherkey, time);
        }
    }

    public void IdleStateHandler(int keystate, float time)
    {
        //m_FreePosition = m_Transform.position;
        if (IsAvoidingFall()) return;
        if (m_AnimStatePlayer != null)
        {
            m_AnimStatePlayer.IdleStateHandler(keystate, time);
        }
    }

    public void InputUpdate()
    {
        if (m_bFreeFall || IsAvoidingFall()|| IsDiving()) 
        {
            return;
        }


        m_PlayerYaw += mouse_dx * 0.5f;
        mouse_dx = 0;
        //Debug.Log("m_PlayerYaw " + m_PlayerYaw);

        if (m_SwimState == SwimmingState.InDeepWater)
        {
            m_PlayerTilt -= mouse_dy;
            mouse_dy = 0;
        }
        else //dont tilt up in water surface or on land
        {
            m_PlayerTilt = 0;
        }

        transform.rotation = Quaternion.Euler(m_PlayerTilt, m_PlayerYaw, 0.0f);


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


    public void SetSwimState(RoomEx room)
    {
		//return;
        if (IsAvoidingFall() &&/*bugfix: except jump*/ (m_bJumping == false)) //dont pull into swimming state when trying to getout of water
        {
            if (m_SwimState == SwimmingState.InWaterSurface)
            {
                //Debug.Log("Getting outof water");
            }
            return;
        }

        if (room == null) return;

        if (room != null )
		{
			float surface = room.GetCenterPoint().y;
			if(surface < m_WaterLevel)
			{
				m_WaterLevel = surface;
				SetSwimStateNone();
				Debug.Log("Water Level Changed");
				return;
			}
		}
		
		if (room != null && (m_bFreeFall || m_bJumping)  && (m_SwimState == SwimmingState.None || m_SwimState == SwimmingState.Diving)) //set initial swim state (works only when current room type is Land, not water
        {
            RoomEx.RoomType type = room.GetRoomType();
			m_WaterLevel = room.GetCenterPoint().y;
            //Debug.Log("Room Type:" + type);
			Vector3 heappos = GetHipPosition();
			
			Bounds room_bound = room.GetBound();
			float min_fall_height = room_bound.min.y + room_bound.size.y * 0.1f;
			
			if ((m_Transform.position.y < (m_WaterLevel - m_Height * 0.3f)) && (type == RoomEx.RoomType.DeepWater))  //enter swimming state machine
            {
                if (m_AnimStatePlayer != null)
                {
                    m_AnimStatePlayer.PlayDiveSFX();
                }
                SetSwimStateDeepWater();
                StopImmediate(null);
            }
            else if ((m_Transform.position.y < min_fall_height) && (type == RoomEx.RoomType.ShalloWater))
            {
                if (heappos.y < m_WaterLevel) //hip is inside water
                {
                    if (m_AnimStatePlayer != null)
                    {
                        m_AnimStatePlayer.PlayDiveSFX();
                    }

                    SetSwimStateShallowWater();
                    StopImmediate(null);
                }
                else
                {


                }
                    
             }
			
			
		}
		
	

        if (room != null && ((room.Flags & 1) == 1))
        {
            /*if(m_SwimState != SwimmingState.InShallowWater)
			{
			    SetSwimStateShallowWater();
			}*/

            float surface = room.GetCenterPoint().y;

            //if ((surface - transform.position.y) > 0.1f && (m_SwimState == SwimmingState.InWaterSurface))
            //{
                //SetSwimStateDeepWater();
            //}

            if (m_SwimState == SwimmingState.InDeepWater)
            {
                if (transform.position.y >= (surface - 0.05f))
                {
                    SetSwimStateSurfaceWater();
                }
            }

            /*if((surface - m_Transform.position.y) >  0.35f  && (m_SwimState == SwimmingState.None))
			{
				SetSwimStateShallowWater();
			}*/

            if (m_SwimState == SwimmingState.InWaterSurface)
            {

                //Grab ledge and get outof water
				if(m_bStandingUp || m_bPullingUp)
				{
					//m_SwimState = SwimmingState.None;
				}
            }

            //define shallow water analyzing tub depth


            //Debug.Log("room.m_Tr2Room.info.yTop " + (-room.m_Tr2Room.info.yTop * Settings.SceneScaling));
            //Debug.Log("room.m_Tr2Room.info.yBottom " + (-room.m_Tr2Room.info.yBottom * Settings.SceneScaling));

        }
        else
        {
           //SetSwimStateNone();
           //StopImmediate();
        }


        /*
         * [from state jumping/diving]
         * 
         * dive in shallow water ->
         * 
         * dive in deep water - >
         * 
         * */

        /*
         * 
         * [from state in shallow water]
         * 
         * Jump->
         * trytorun->
         * 
         * */


        /*
         * 
         * [from state deep water]
         * 
         * swim->
         * swim to shallow water->
         * move to surface->
         * 
         * */


        /*
         * 
         * [from state surface]
         * 
         * move-> grab platform -> pull up
         * 
         * */

        //Debug.Log("Swimm State:" + m_SwimState);

    }

    void SetSwimStateDeepWater()
    {
		//Debug.Log("SetSwimStateDeepWater");
        m_SwimState = SwimmingState.InDeepWater;
        if (m_AnimStatePlayer != null)
        {
            m_AnimStatePlayer.SetSwimState(m_SwimState);
        }
        PlayerCollisionHandler.ResizeSwimmCollider();
		m_HipHeightAdjust = 0.0f;
        m_bDivingIntoWater = false;
    }

    void SetSwimStateShallowWater()
    {
		//Debug.Log("SetSwimStateShallowWater");
        m_SwimState = SwimmingState.InShallowWater;
        if(m_AnimStatePlayer!= null)
		{
        	m_AnimStatePlayer.SetSwimState(m_SwimState);
		}
        PlayerCollisionHandler.ResizeNormalCollider();
		m_HipHeightAdjust = -0.1f;
    }

    void SetSwimStateSurfaceWater()
    {
		//Debug.Log("SetSwimStateSurfaceWater");
        m_SwimState = SwimmingState.InWaterSurface;
        if(m_AnimStatePlayer!= null)
		{
        	m_AnimStatePlayer.SetSwimState(m_SwimState);
		}
        PlayerCollisionHandler.ResizeNormalCollider();
		m_HipHeightAdjust = 0.0f;
    }

    void SetSwimStateNone()
    {
		//Debug.Log("SetSwimStateNone: Setting Normal Physics (e.g standing jump)");
        m_SwimState = SwimmingState.None;
        if(m_AnimStatePlayer!= null)
		{
        	m_AnimStatePlayer.SetSwimState(m_SwimState);
		}
        PlayerCollisionHandler.ResizeNormalCollider();
		m_HipHeightAdjust = -0.1f;
        if (physics != null)
        {
            physics.SetFreeFallSpeed(2.5f);
        }
    }

    void SetSwimStateDiving()
    {
        //Debug.Log("SetSwimStateDiving: Diving");
        m_SwimState = SwimmingState.Diving;
        if (m_AnimStatePlayer != null)
        {
            m_AnimStatePlayer.SetSwimState(m_SwimState);
        }
        PlayerCollisionHandler.ResizeSwimmCollider();
        m_HipHeightAdjust = 0.0f;
        if (physics != null)
        {
            physics.SetFreeFallSpeed(0.085f);
        }
    }

    void OnDestroy()
    {
        //release event handlers
        KeyMapper.OnKeyDown -= StateCodeHandler;
        KeyMapper.OnKeyIdle -= IdleStateHandler;
        Mouse.m_OnMouseMove -= OnMouseMove;

        //prevy = thistransform.position.y;
        LaraStatePlayer.OnJump -= JumpHandler;
        LaraStatePlayer.OnJumping -= JumpingHandler;
        LaraStatePlayer.OnMovement -= MovementHandler;
        //LaraStatePlayer.OnPrimaryAction -= PrimaryActionHandler; //this callback is not needed anymore
    }

    bool IsAvoidingFall()
    {
        return   (m_bJumping || IsPullingUp()) ;
    }
	
	bool IsPullingUp()
	{
		return (m_bPullingUp || m_bWalkingUp || m_bStandingUp);
	}

    bool IsDiving()
    {
        return m_bDivingIntoWater;
    }

    void ResetJump()
    {
        m_bJumping = false;
        JumpVector = Vector3.zero;

        /*
         * following is a bug. ResetJump does not mean we have to stop pull up. Rather m_bPullingUp can be used post jump action
         * disable m_bPullingUp where it needs
         * 
         * m_bPullingUp = false;
         * */

    }

    void SeekGround()
	{
		#if (UNITY_5_3_OR_NEWER || UNITY_5_3)
        int mask = Physics.DefaultRaycastLayers & ~(MaskedLayer.Switch | MaskedLayer.Player);
#else
		int mask = Physics.kDefaultRaycastLayers & ~(MaskedLayer.Switch | MaskedLayer.Player);
#endif
        RaycastHit hit = new RaycastHit();
		
		
        Vector3 origin = GetHipPosition() - Vector3.forward * m_maxForwardRayLength * 0.25f;

        if (Physics.Raycast(origin, -Vector3.up, out hit, m_maxDownRayLength, mask))
        {
            if (hit.transform.root == this.transform)
            {
                Debug.Log("Warning! Self Collision With:" + this.transform.name);
                return;
            }
			m_GroundHeight = hit.point.y;
			
           
           Debug.DrawRay(origin,-Vector3.up * m_maxDownRayLength , Color.red);
         }
		
		
		//return -1;
	}

    Collider SeekRoof()
    {
        RaycastHit hit2 = new RaycastHit();
#if (UNITY_5_3_OR_NEWER || UNITY_5_3)
        int mask = Physics.DefaultRaycastLayers & ~(MaskedLayer.Switch | MaskedLayer.Player);
#else
		int mask = Physics.kDefaultRaycastLayers & ~(MaskedLayer.Switch | MaskedLayer.Player);
#endif
        Vector3 ray_origin = GetHeadPositon() - Vector3.forward * m_maxForwardRayLength * 0.25f;
        Physics.Raycast(ray_origin, Vector3.up, out hit2, m_minUpRayLength, mask);
        Debug.DrawRay(ray_origin, Vector3.up * m_minUpRayLength , Color.red);
        return hit2.collider;
    }

    bool SeekRoof(ref Collider roof)
    {
        RaycastHit hit2 = new RaycastHit();
#if (UNITY_5_3_OR_NEWER || UNITY_5_3)
        int mask = Physics.DefaultRaycastLayers & ~(MaskedLayer.Switch | MaskedLayer.Player);
#else
		int mask = Physics.kDefaultRaycastLayers & ~(MaskedLayer.Switch | MaskedLayer.Player);
#endif
        Vector3 ray_origin = GetHeadPositon() - Vector3.forward * m_maxForwardRayLength * 0.25f;
        bool collision = Physics.Raycast(ray_origin, Vector3.up, out hit2, m_minUpRayLength, mask);
        roof = hit2.collider;
        Debug.DrawRay(ray_origin, Vector3.up * m_minUpRayLength, Color.red);

        return collision;

 
    }

    bool StopFalling()
	{
		return (m_SwimState == SwimmingState.InDeepWater) || (m_SwimState == SwimmingState.InWaterSurface);
	}
	
	void StopImmediate(GameObject other)
	{
        ResetJump();
        if (m_AnimStatePlayer != null)
		{
			m_AnimStatePlayer.OnCollision(other);
		}
        m_bDivingIntoWater = false;
	}
	
	void InitialiseGround()
	{
		m_GroundHeight = m_Transform.position.y ;//- m_Height * 2;  //setup initial freefall height much bellow foot height
		SetSwimStateNone();
	}
	
	void RecordFreePosition()
	{
		 // bug fix: unconditional record of m_FreePosition
         m_FreePosition = m_Transform.position;
	}
	
	Vector3 GetHipPosition()
	{
		return m_LarasHeap.position + Vector3.up * m_HipHeightAdjust;
	}

    Vector3 GetHeadPositon()
    {
        return m_LarasHead.position;
    }

    void DidReachedMaxJumpHeight()
    {
        //Debug.Log("Reached Max Height");
        //Time to check we are diving in water!

        if (m_Room != null)
        {
            RoomEx.RoomType type = m_Room.GetRoomType();
            //Debug.Log("Are we diving in :" + type);

            if(type == RoomEx.RoomType.DeepWater)
            {
                //change animation state dive
                if(m_AnimStatePlayer != null)
                {
                    m_AnimStatePlayer.Dive();
                }
            }
        }
    }

}
