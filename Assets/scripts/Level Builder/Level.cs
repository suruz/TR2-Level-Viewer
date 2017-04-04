using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class Level  
{
	static RoomEx[] m_RoomExs = null;
	static GameObject m_LevelRoot = null;
	static Texture2D m_LevelTextureTile = null;
	static public Parser.Tr2Level m_leveldata = null;
	static public string m_LevelName;

	//resources
	static List<Tr2Moveable> m_DynamicPrefabs = null;
	static List <Parser.Tr2StaticMesh> m_StaticPrefabs = null;

	//Actors
	static List <Actor> m_Actors = new List<Actor>();
	public static GameObject m_Player = null;
	//instances
	static List<Parser.Tr2Item> m_MovableInstances = null;

	//Process room/sector data
	//Transform m_CurrentActiveRoom = null;
	RoomEx m_CurrentActiveRoom = null;

	Vector3 m_PrevPlayPos = Vector3.zero;
	Vector3 m_CurrentPlayPos = Vector3.zero;
	public float m_DistancedCrossed = 0;
	
	public  delegate bool AttachBehaviourScript(GameObject AI, int ObjectID, GameObject player, Parser.Tr2Item tr2item);
	public static AttachBehaviourScript m_OnAttachBehabiourScript = AICallBackHandler.OnAttachingBehaviourToObject;
	
	public Level(Parser.Tr2Level leveldata)
	{
		m_leveldata = leveldata;
		m_LevelRoot = new GameObject ("Level " + m_LevelName);
		//m_LevelRoot.AddComponent(typeof (MeshFilter));
		//m_LevelRoot.AddComponent(typeof (MeshRenderer));

		if(m_leveldata!=null && m_leveldata.NumRooms > 0)
		{
			m_LevelTextureTile = TextureUV.GenerateTextureTile (m_leveldata);
			m_RoomExs = new RoomEx[m_leveldata.NumRooms];

			m_DynamicPrefabs = BuildDynamicPrefabObjects();
			m_StaticPrefabs = BuildStaticPrefabObjects();

			//determine animation clip size for each movable object
			for(int i = 0; i < m_DynamicPrefabs.Count - 1; i++)
			{
				int startoffset0 = m_DynamicPrefabs[i].Animation;
				int startoffset1 = m_DynamicPrefabs[i + 1].Animation;
				m_DynamicPrefabs[i].NumClips = startoffset1 - startoffset0; 
			}
			if(m_DynamicPrefabs.Count > 0)
			{
				int startoffset0 = m_DynamicPrefabs[m_DynamicPrefabs.Count - 1].Animation;
				m_DynamicPrefabs[m_DynamicPrefabs.Count - 1].NumClips = (int) m_leveldata.NumAnimations - 1 - startoffset0;
			}

			//attach animation and their state change
			for(int i = 0; i < m_DynamicPrefabs.Count; i++)
			{
				List<TRAnimationClip> clips = Animator.AttachAnimation(m_DynamicPrefabs[i], m_leveldata);
				AnimationStateMapper.BuildMap(clips, m_DynamicPrefabs[i].UnityAnimation, m_leveldata);
			}

			//attach 3DText Box to movable objects  to mark their ID
			if(Settings.ShowObjectID)
			{
				for(int i = 0; i < m_DynamicPrefabs.Count; i++)
				{
					if(m_leveldata.Text3DPrefav!=null)
					{
						TextMesh text3d = (TextMesh)GameObject.Instantiate(m_leveldata.Text3DPrefav);
						text3d.transform.position = m_DynamicPrefabs[i].UnityObject.transform.position + Vector3.up * 1000;
						text3d.transform.parent =   m_DynamicPrefabs[i].UnityObject.transform;
						text3d.characterSize = 100;
						text3d.text = "" + m_DynamicPrefabs[i].ObjectID;
					}
				}
			}

			//build rooms
			for(int i = 0 ; i < m_leveldata.NumRooms; i++)
			{
				Parser.Tr2Room tr2room = leveldata.Rooms[i];  
				Mesh roommesh = MeshBuilder.CreateRoomMesh(tr2room, m_leveldata);
				Vector3 position = new Vector3(m_leveldata.Rooms[i].info.x,0,m_leveldata.Rooms[i].info.z);
				GameObject go = CreateRoom(roommesh, position, i);
				go.transform.parent = m_LevelRoot.transform;
				m_RoomExs[i] = go.AddComponent<RoomEx>();
				//build room object
				List <GameObject> objects = InstantiateStaticObjects(tr2room, i);  
				m_RoomExs[i].InitRoom(tr2room, objects);
			}

			m_MovableInstances =  InstantiateDynamicObjects();
			SetupTrigers();
			//attach components to m_MovableInstances
			for(int i = 0; i < m_MovableInstances.Count; i++)
			{
				InitialiseInstance(m_MovableInstances[i]);
			}
	
		}
	}
	
	//TODO: Determine Unity Pro / Free version and use Transparent/Cutout/Diffuse if pro otherwise
	//use Diffuse in material
	
	GameObject CreateRoom(Mesh mesh, Vector3 position, int roomidx)
	{
		GameObject go = new GameObject("room" + roomidx);
		Renderer renderer = go.AddComponent<MeshRenderer>();
		MeshFilter mf = go.AddComponent<MeshFilter>();
		mf.mesh = mesh;

		go.transform.position = position;
		go.transform.rotation = Quaternion.identity;
		if(Settings.PlatformUnityPro)
		{
			renderer.material = new Material(Shader.Find("Transparent/Cutout/Diffuse"));
		}
		else
		{
			renderer.material = new Material(Shader.Find("Diffuse"));
		}
		renderer.material.mainTexture =  m_LevelTextureTile;
		renderer.material.color = new Color(1f,1f,1f,1.0f);
		renderer.castShadows = !Settings.EnableIndoorShadow;
		//renderer.material.SetTexture("_BumpMap", Bumptex);*/

		//check for inertia tensor calculation!
		if(mesh.bounds.extents.y == 0 || mesh.bounds.extents.x == 0 || mesh.bounds.extents.z == 0)
		{
			BoxCollider cldr = go.AddComponent<BoxCollider>();
			cldr.isTrigger = true;
		}
		else
		{
			#if UNITY_5_3_OR_NEWER
			MeshCollider cldr  = go.AddComponent<MeshCollider>();
			//cldr.isTrigger = true;
			#else
			MeshCollider cldr  = go.AddComponent<MeshCollider>();
			cldr.isTrigger = true;
			#endif
		}

		/*Rigidbody rb = go.AddComponent<Rigidbody>();
		rb.isKinematic = true;
		rb.useGravity = false;*/
		
		//go.AddComponent<RoomCollision>();
		return go;
	}

	GameObject CreateObject(Mesh mesh, Vector3 position, Quaternion rotation, string name)
	{
		GameObject go = new GameObject(name);
		Renderer renderer = go.AddComponent<MeshRenderer>();
		MeshFilter mf = go.AddComponent<MeshFilter>();
		mf.mesh = mesh;
		
		go.transform.position = position;
		go.transform.rotation = rotation;
		if(Settings.PlatformUnityPro)
		{
			renderer.material = new Material(Shader.Find("Transparent/Cutout/Diffuse"));
		}
		else
		{
			renderer.material = new Material(Shader.Find("Diffuse"));
		}
		
		renderer.material.mainTexture =  m_LevelTextureTile;
		renderer.material.color = new Color(1f,1f,1f,1.0f);
		return go;
	}

	GameObject CreateObjectWithID(int idx, Vector3 position, Quaternion rotation, string name )
	{
		Parser.Tr2Mesh tr2mesh = m_leveldata.Meshes[idx];
		Mesh objmesh = MeshBuilder.CreateObjectMesh(tr2mesh,m_leveldata);
		return CreateObject(objmesh,position,rotation,name);
	}

	GameObject[] CreateMultiPartObject(Tr2Moveable tr2movable)
	{
		GameObject[] parts = new GameObject[tr2movable.NumMeshes];
		// run through all the meshes init transforms
		for (int i = 0; i < tr2movable.NumMeshes; i++) 
		{
			int itemMeshIdx =	(int)(tr2movable.StartingMesh + i); // mesh id in tr2  mesh table
			if(itemMeshIdx > m_leveldata.NumMeshes-1)
			{
				itemMeshIdx = (int)(m_leveldata.NumMeshes-1);
			}
			
			parts[i] = CreateObjectWithID(itemMeshIdx, Vector3.zero, Quaternion.identity, "objPart:" + itemMeshIdx);
			parts[i].transform.parent = null;
			
		}
		
		return parts;
	}
	
	List<Parser.Tr2StaticMesh>  BuildStaticPrefabObjects()
	{
		List<Parser.Tr2StaticMesh> objects = new List<Parser.Tr2StaticMesh>();
		
		for (int k = 0; k < (int)m_leveldata.NumStaticMeshes; k++) 
		{
			Parser.Tr2StaticMesh tr2staticmesh = m_leveldata.StaticMeshes[k];
			int itemIdx = (int)tr2staticmesh.StartingMesh;
			
			//m_leveldata.StaticMeshes[k]
			tr2staticmesh.UnityObject = CreateObjectWithID(itemIdx,Vector3.zero, Quaternion.identity, "__staticBase" + k );
			tr2staticmesh.UnityObject.transform.parent = m_LevelRoot.transform;
			tr2staticmesh.UnityObject.AddComponent<MeshCollider>();
			tr2staticmesh.UnityObject.SetActiveRecursively(false);
			
			objects.Add(tr2staticmesh);
		}
		return objects;
	}

	List<Tr2Moveable>  BuildDynamicPrefabObjects()
	{
		List<Tr2Moveable> objects = new List<Tr2Moveable>();

		for(int MovableObjectIdx = 0; MovableObjectIdx < m_leveldata.Moveables.Length;  MovableObjectIdx++)
		{
			Tr2Moveable tr2movable =  m_leveldata.Moveables[MovableObjectIdx];
			int startclipid = tr2movable.Animation; 
			if(startclipid > m_leveldata.Animations.Length) continue;

			GameObject[] parts = CreateMultiPartObject(tr2movable);
			Transform[] transformtree = new Transform[parts.Length];
			for(int i = 0; i < parts.Length; i++)
			{
				transformtree[i] = parts[i].transform;
				if(tr2movable.ObjectID != 0)
				{
					MeshCollider mf = parts[i].AddComponent<MeshCollider>();
				}
			}

			//creat a place holder gameObject and make it root transform ;
			tr2movable.UnityObject = new GameObject ("prefab type:" + MovableObjectIdx);
			GameObject objRoot = tr2movable.UnityObject;
			objRoot.transform.parent = m_LevelRoot.transform;
			objRoot.transform.Translate(Vector3.zero);
			objRoot.transform.Rotate(Vector3.zero);

			//add unity animation components
			tr2movable.UnityAnimation = objRoot.AddComponent<Animation>();
			tr2movable.UnityAnimation.wrapMode = WrapMode.Loop;
			tr2movable.TransformsTree = transformtree;
			tr2movable.AnimationStartOffset = startclipid;
			tr2movable.AnimClips = new List<TRAnimationClip>();
			objects.Add(tr2movable);

			//build mesh tree with stack
			ComputionModel.StackInit();

			//setup parent transform
			Transform Parent = transformtree[0];
			Parent.Translate(Vector3.zero); 
			Parent.Rotate(Vector3.zero);
			Parent.parent = objRoot.transform;

			int animRootId = 0;
			for (int i = 0; i < tr2movable.NumMeshes; i++) 
			{
				if (i != 0)   // first mesh - position to world coordinates, set rotation
				{
					Vector3 meshPos = Vector3.zero;

					// tr2movable.MeshTree is a byte offset into MeshTrees[],
					// so we have to do a little converting here...
					
					int offsetMeshTree = (int) tr2movable.MeshTree;
					int Idx = (i-1)*4 + offsetMeshTree;
					meshPos.x = (float)m_leveldata.MeshTrees[Idx  + 1]; 
					meshPos.y =-(float)m_leveldata.MeshTrees[Idx  + 2];
					meshPos.z = (float)m_leveldata.MeshTrees[Idx  + 3];
					
					int flagVal1 = (int)(( m_leveldata.MeshTrees[Idx  + 0]) & 0x01);
					int flagVal2 = (int)((m_leveldata.MeshTrees[Idx  + 0]) & 0x02);
					
					if(flagVal1 > 0 && flagVal2 > 0)
					{
						//print("poping - pushing ");
						animRootId = ComputionModel.Pop();
						Parent =transformtree[animRootId];
						animRootId = ComputionModel.Push(animRootId);
					}
					else
					{
						if (flagVal1 > 0)  // pop last saved anchor
						{
							//print("poping "+i);
							animRootId = ComputionModel.Pop();
							Parent =transformtree[animRootId];
						}
						else
						{
							
							Parent = transformtree[i-1];
						}
						
						if (flagVal2 > 0)  // push new anchor save
						{
							
							//print("pushing "+i);
							animRootId = ComputionModel.Push(i-1);
							//prevParent = transformtree[animRootId];
						}
					}
					
					transformtree[i].parent = Parent;
					transformtree[i].localPosition = meshPos;;
					//transformtree[i].localRotation = relRot;
					
				}
			}
			objRoot.SetActiveRecursively(false);
		}

		return objects;
	}


	List <GameObject> InstantiateStaticObjects(Parser.Tr2Room tr2room, int roomidx)
	{
		List <GameObject> objects = new List <GameObject>();
		
		for(int staticMeshCount = 0;  staticMeshCount < tr2room.NumStaticMeshes; staticMeshCount++)
		{
			for (int k = 0; k < m_StaticPrefabs.Count; k++) 
			{
				//room static meshes are instantce of Level mesh asset
				//choose only the meshe from whole level that belongs to this room
				if (tr2room.StaticMeshes[staticMeshCount].ObjectID == m_leveldata.StaticMeshes[k].ObjectID)
				{
					Vector3 meshPos;
					meshPos.x = tr2room.StaticMeshes[staticMeshCount].x; 
					meshPos.y= -tr2room.StaticMeshes[staticMeshCount].y;
					meshPos.z = tr2room.StaticMeshes[staticMeshCount].z;
					
					Vector3 rot;
					rot.x = 0;
					rot.y = ((tr2room.StaticMeshes[staticMeshCount].Rotation >> 14) & 0x03) * 90; 
					rot.z = 0;
					int itemIdx = (int)m_leveldata.StaticMeshes[k].StartingMesh;
					
					GameObject go = (GameObject)GameObject.Instantiate(m_StaticPrefabs[k].UnityObject,meshPos,Quaternion.Euler(rot) );
					go.name = "room" + roomidx + "__staticBase" + k + "__meshBase"+itemIdx;
					go.transform.parent = m_LevelRoot.transform;
					go.SetActiveRecursively(true);
					objects.Add(go);
				}
			}
		}
		return objects;
	}

	List <Parser.Tr2Item>  InstantiateDynamicObjects()
	{
		List <Parser.Tr2Item> objects = new List <Parser.Tr2Item>();
		
		for (int j = m_leveldata.NumItems - 1; j >= 0; j--) 
		{
			uint objId = (uint)m_leveldata.Items[j].ObjectID;
			//determine if item pointing to a sprite object going through all sprites
			int i = 0; // "Could be a Sprite" flag (0 == "could be a sprite")
			
			if (m_leveldata.EngineVersion == Parser.TR2VersionType.TombRaider_1) 
			{
				if(m_leveldata.Items[j].Intensity1 == -1) // it's a mesh, not a sprite
				{
					i = (int)m_leveldata.NumSpriteSequences;   // skip the sprite search "for" loop that follows
				}
			}
			
			// for othercases where m_leveldata.Items[j].Intensity1 != -1
			// search the SpriteSequence list (if we didn't already decide that it's a mesh) 
			
			for ( ; i < (int)m_leveldata.NumSpriteSequences; ++i) 
			{
				if (m_leveldata.SpriteSequences[i].ObjectID == objId) break; //search till we find one sprite
			}
			
			if (i == (int)m_leveldata.NumSpriteSequences) //search exhusted? not in SpriteSequences[] (or we know it's not a sprite)
			{  
				// search the Moveable list
				for (i = 0; i < m_DynamicPrefabs.Count; ++i)
				{
					if (m_DynamicPrefabs[i].ObjectID == objId) 
					{
						break;
					}
				}
				
				GameObject movableItem = (GameObject) GameObject.Instantiate(m_DynamicPrefabs[i].UnityObject);
				movableItem.name = "Object";
				movableItem.transform.parent = m_LevelRoot.transform;
				movableItem.transform.position = new Vector3(m_leveldata.Items[j].x ,-m_leveldata.Items[j].y,m_leveldata.Items[j].z );
				float rot = ((m_leveldata.Items[j].Angle >> 14) & 0x03) * 90;
				movableItem.transform.rotation = Quaternion.Euler(0,rot,0);
				m_leveldata.Items[j].UnityObject = movableItem;
				m_leveldata.Items[j].ObjectBase = m_DynamicPrefabs[i];
				objects.Add(m_leveldata.Items[j]);

				if(m_DynamicPrefabs[i].ObjectID == 0)
				{
					m_Player = movableItem;
				}
			}
		}
		
		return objects;
	}
	
	void InitialiseInstance(Parser.Tr2Item tr2item)
	{
		GameObject go = tr2item.UnityObject;
		go.name +=" " + tr2item.ObjectID;
		if(tr2item.ObjectID == 0 )
		{
			//playable character found!
			m_Player = go;
			m_Player.layer = UnityLayer.Player;
			m_PrevPlayPos = m_Player.transform.position;
			m_Player.transform.parent = null;

			if(m_leveldata.Camera !=null)
			{
				m_leveldata.Camera.target= m_Player.transform;
			}
			LaraStatePlayer stateplayer = m_Player.AddComponent<LaraStatePlayer>();
			stateplayer.tranimations = m_DynamicPrefabs[0].AnimClips;
			m_Player.name = "Lara";

			GameObject FlashLight = new GameObject("Fire Torch");
			FlashLight.AddComponent <FlashLightStatePlayer>();
			Light lt = FlashLight.AddComponent<Light>();
			lt.type = LightType.Spot;
			lt.range = 10000;
			lt.spotAngle = 70;
			lt.intensity = 1;

			FlashLight.transform.parent = m_Player.transform.FindChild("objPart:0");//.Find("objPart:7").Find("objPart:14");
			FlashLight.transform.position = FlashLight.transform.parent.position;
			FlashLight.transform.forward =  FlashLight.transform.parent.forward;
			lt.enabled = false;
		
			Player player = go.AddComponent<Player>();
			player.m_Tr2Item = tr2item;
			HealthMonitor healthmon = go.AddComponent<HealthMonitor>();
			PlayerCollisionHandler playercollider =  go.AddComponent<PlayerCollisionHandler>();

			//Initialise Current Active Room for player
			player.m_Room = SetRoomForPlayer();
			go.SetActiveRecursively(true);
		}
		//check if we have any custom behabiour  script for object
		else if(m_OnAttachBehabiourScript!= null && !m_OnAttachBehabiourScript(tr2item.UnityObject,tr2item.ObjectID,m_Player,tr2item) ) 
		{	
			go.AddComponent<DefaultStatePlayer>(); // user did not attached any custom behabiour. so use default one
		}
			
	}
	
	RoomEx SetRoomForPlayer()
	{
		if(m_RoomExs.Length > 0 && m_Player!=null )
		{
			RaycastHit hit = new RaycastHit();
			Transform roomtransform = null;
			Transform Lara = m_Player.transform;

			#if UNITY_5_3_OR_NEWER
				int mask = Physics.DefaultRaycastLayers & ~(MaskedLayer.Switch | MaskedLayer.Player);
			#else
				int mask = Physics.kDefaultRaycastLayers & ~(MaskedLayer.Switch | MaskedLayer.Player);
			#endif

			if(Physics.Raycast(Lara.position + Vector3.up * 50, -Vector3.up, out hit,14096,mask ))
			{
				roomtransform = hit.transform; Debug.Log("SetRoomForPlayer");
			}
			else
			{
				Debug.Log("failed SetRoomForPlayer!");
			}
			
			for(int r = 0; r < m_RoomExs.Length; r++)
			{
				if(m_RoomExs[r].m_Transform == roomtransform)
				{
					m_CurrentActiveRoom = m_RoomExs[r];
					break;
				}
			}
		}

		return m_CurrentActiveRoom;
	}

	public void Update()
	{
		/*if(m_RoomExs.Length > 0 && m_Player!=null )
		{
			RaycastHit hit = new RaycastHit();
			Transform roomtransform = null;
			Transform Lara = m_Player.transform;

			int mask = Physics.DefaultRaycastLayers & ~(MaskedLayer.Switch | MaskedLayer.Player);
			if(Physics.Raycast(Lara.position, -Vector3.up, out hit,14096,mask ))
			{
				roomtransform = hit.transform;
			}

			//room changed?
			if(m_CurrentActiveRoom.m_Transform!= m_RoomExs[r].m_Transform)
			{
				m_CurrentActiveRoom = m_RoomExs[r].m_Transform;
			}
		}*/
		
		//Debug.Log("Room length:" + m_leveldata.Rooms.Length);
		//if(m_CurrentActiveRoom!=null)
		//{
			//m_CurrentActiveRoom.DebugRoomSurface();
		//}
	}


	void SetupTrigers()
	{
		if(m_leveldata.Rooms.Length > 0 && m_Player!=null )
		{
			Transform Lara = m_Player.transform;

			for(int r = 0; r < m_leveldata.Rooms.Length; r++)
			{
				Vector3 room_world_position = m_RoomExs[r].transform.position;
				
				//IN TR2 sector is scaned vertically (depth first order)
				
				int width = m_leveldata.Rooms[r].NumXsectors;
				int depth = m_leveldata.Rooms[r].NumZsectors;
				
				
				int numsector = width * depth;
				int sectorid = 0;
				
				
				for(int sectorz = 0; sectorz < depth; sectorz++)
				{
					for (int sectorx = 0; sectorx < width; sectorx++)
					{
						sectorid = sectorx * depth + sectorz;
						Parser.Tr2RoomSector[] sector = m_leveldata.Rooms[r].SectorList;
						int fdid = sector[sectorid].FDindex;
						ushort fd = m_leveldata.FloorData[fdid];
						
				
						//determine if floor data is a single function
						//Function:         bits 0..7 (0x00FF)
						//Sub Function:    	bits 8..14 (0x7F00)
						//EndData:          bit 15 (0x8000) 
		
						//Debug.Log(sectorid  +"-> Floor Data indx :" + sector[sectorid].FDindex);
									
						int triger_item_index = - 1;
						int target_item_index = - 1;
					
						if((fd & 0x00FF) == 0x4) // sector acts as triger
						{
							int subfunc = (int)((fd & 0x7F00) >> 8);	    	  
							if(subfunc == 2) // sector includes a triger item ( for example switch
							{
								int activation_mask = (int)((m_leveldata.FloorData[fdid + 1] &  0x3e00) >> 9);
								//Debug.Log("activation_mask " + activation_mask);
								ushort fdlist0 = m_leveldata.FloorData[fdid + 2];
								int triger_funtion = (int)((fdlist0 & 0x3C00) >> 10);
								//Debug.Log("triger_funtion" + triger_funtion);
								if(triger_funtion == 0) //FDfunction 0x00: Activate or deactivate item
								{
									triger_item_index =  (int)(fdlist0 & 0x03FF); 	//FDfunction Operand (bits 0..9): Item index 
									//Debug.Log("Switch ID " + m_leveldata.Items[triger_item_index].ObjectID);
									m_leveldata.Items[triger_item_index].UnityObject.name = "Switch";
								}
							
								//get trigger target
								fdlist0 = m_leveldata.FloorData[fdid + 3];
								triger_funtion = (int)((fdlist0 & 0x3C00) >> 10);
								if(triger_funtion == 0) 
								{
									target_item_index =  (int)(fdlist0 & 0x03FF); 
									//Debug.Log("Target ID " + m_leveldata.Items[target_item_index].ObjectID);
									m_leveldata.Items[target_item_index].UnityObject.name = "Target";
									m_leveldata.Items[triger_item_index].ActivateObject = m_leveldata.Items[target_item_index].UnityObject;
									m_leveldata.Items[triger_item_index].UnityObject.name += "[T" + m_leveldata.Items[target_item_index].ObjectID + "]";
								}
				
							}
							else if(subfunc == 0)
							{
								ushort fdlist0 = m_leveldata.FloorData[fdid + 2];
								int triger_funtion = (int)((fdlist0 & 0x3C00) >> 10);
								//Debug.Log("triger_funtion" + triger_funtion);
							
								if(triger_funtion == 0) //FDfunction 0x00: Activate or deactivate item
								{
									triger_item_index =  (int)(fdlist0 & 0x03FF); 	//FDfunction Operand (bits 0..9): Item index 
									//Debug.Log("Enemy ID " + triger_item_index);
								}
							}
						
						}
					
						if((int)(fd & 0x8000) == 0x8000)
						{
						
							switch(fd & 0x00FF)
							{
								case 0x5: Debug.Log("Die");
									Vector3 sector_world_position = new Vector3(room_world_position.x + (sectorx * 1024), 0, room_world_position.z + (sectorz * 1024));
								
									GameObject diezone = MeshBuilder.CreateZone("Die Zone");
									diezone.transform.position = sector_world_position;
									diezone.transform.localScale = new Vector3(1024, 1024, 1024);
								   // diezone.transform.parent = m_RoomExs[r].transform;
								
								break;
								
								case 0x6: Debug.Log("Climb");
								// m_leveldata.Rooms[r].info.
									sector_world_position = new Vector3(room_world_position.x + (sectorx * 1024), 0, room_world_position.z + (sectorz * 1024));
								
									GameObject climbzone = MeshBuilder.CreateZone("Climb Zone");
									climbzone.transform.position = sector_world_position;
									climbzone.transform.localScale = new Vector3(1024, 1024, 1024);
								    //climbzone.transform.parent = m_RoomExs[r].transform;
									
								break;
							}
						}
				
					}

				}
				
			}
			
		}
	}

	
}
