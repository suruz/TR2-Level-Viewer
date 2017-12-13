using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomEx: MonoBehaviour  {
	SurfaceAnalyzer m_RoomAnalyzer;
	Parser.Tr2Room m_Tr2Room;
    Mesh m_Mesh;
	Vector3[] m_RoomVertices;
	public Transform m_Transform;
	int ID = 0;
	int[] m_SharedTriangles = null;
	List<Edge> m_RoomEdges = null;
	float m_CeilingHeight = -Mathf.Infinity;
	float m_FloorHeight = Mathf.Infinity;
	public List<GameObject> m_StaticObjects = null;
	public Material m_Material;
    //Parser.Tr2Level m_leveldata = null;
    public Vector3 m_CenterPoint = Vector3.zero;
    public Bounds m_RoomBound; // made public for serialization
    public short Flags = 0;
    Vector3[] m_PortalPolygon;
    public enum RoomType
    {
        Land = 0,
        DeepWater,
        ShalloWater,
        WaterFilled,
        Ice
    }

    public RoomType m_RoomType = RoomType.Land;

    void Start()
	{
        m_Mesh = GetComponent<MeshFilter>().mesh;
        m_RoomVertices = m_Mesh.vertices;
		m_SharedTriangles = MeshModifier.GetSharedTriangles(m_Mesh);
		m_RoomAnalyzer = new SurfaceAnalyzer(m_Mesh);
		m_RoomEdges = m_RoomAnalyzer.Analyze(m_RoomVertices, m_SharedTriangles,null);
		m_Material = GetComponent<MeshRenderer>().sharedMaterial;
	}

	void Update()
	{
        if(m_PortalPolygon != null)
        for(int i = 0; i < m_PortalPolygon.Length; i++)
        {
            int id = (i + 1) % m_PortalPolygon.Length;
            Debug.DrawLine(transform.TransformPoint(m_PortalPolygon[i]), transform.TransformPoint(m_PortalPolygon[id]), Color.yellow);
        }
	}

	public void  InitRoom(Parser.Tr2Room room, List<GameObject> objects)
	{
	
#if UNITY_EDITOR
		m_Mesh = GetComponent<MeshFilter>().sharedMesh;
#else
		m_Mesh = GetComponent<MeshFilter>().mesh;
#endif 
		
		m_Transform = transform;
		m_StaticObjects = objects;
		m_Tr2Room = room;

        //These are not serialised by Editor. Move them to Start()
        //m_RoomVertices = m_Mesh.vertices;
        //m_SharedTriangles = MeshModifier.GetSharedTriangles(m_Mesh);
        //m_RoomAnalyzer = new SurfaceAnalyzer(m_Mesh);
        //m_RoomEdges = m_RoomAnalyzer.Analyze(m_RoomVertices, m_SharedTriangles,null);

        /*
         * Update: Calculate center point of room, useful for vertex modultion
         * */

        float room_width = room.NumXsectors * 1024 * Settings.SceneScaling;
        float room_depth = room.NumZsectors * 1024 * Settings.SceneScaling;
        float bottom = (-room.info.yBottom * Settings.SceneScaling);
        float surface = (-room.info.yTop * Settings.SceneScaling);
        float x = transform.position.x;
        float z = transform.position.z;
        m_CenterPoint = new Vector3(x, bottom, z) + new Vector3(room_width, (surface - bottom) * 2, room_depth) * 0.5f;// - transform.position; //in world space

        m_RoomBound = new Bounds(m_CenterPoint, Vector3.zero);
        m_RoomBound.SetMinMax(new Vector3(x, bottom, z), new Vector3(x + room_width, surface, z + room_depth));

        m_RoomType = DetectRoomType(room, m_RoomBound);
        Flags = m_Tr2Room.Flags;

        m_PortalPolygon = GetWaterPortal(room);
    }

	public void DebugRoomSurface()
	{
		m_RoomAnalyzer.DebugSurface(m_Transform, m_RoomEdges);
	}
	public void DebugRoomSurface(List<Edge> Edges )
	{
		m_RoomAnalyzer.DebugSurface(m_Transform, Edges);
	}
	
	
	public bool HitTest(Vector3 origin, Vector3 dir ,ref Vector3 normal, float raylength = 200)
	{
		Vector3 hitpoint = Vector3.zero;
		int faceid = Physic3D.RayCast(m_Transform, m_RoomVertices, m_SharedTriangles,origin,dir, raylength, ref hitpoint);
		if(faceid != -1)
		{
			Triangle face = m_RoomAnalyzer.LodTriangles[faceid];
			normal = face.normal;
		}
		
		return (faceid != -1);
	}

	public List<Edge> RayCast(Vector3 origin, Vector3 dir , float raylength = 200)
	{
		//first select a face from face network
		//then find a edge that is normal to ground. 
				
		//if  normal going upward and found next edge diagonal - > go to mirror face
		//if normal going upward found next edge horizontal - > next edge lader step
		//                                                  -> if lader step found chek it is topmost go to mirror face 
		//else

		//if normal going downward and found next edge horizontal - > go to mirror face
		//f normal going downward and found next edge diagonal - > third edge is lader step
		//                                                   -> if lader step found chek it is topmost go to mirror face 

		Vector3 hitpoint = Vector3.zero;
		int faceid = Physic3D.RayCast(m_Transform, m_RoomVertices, m_SharedTriangles,origin,dir, raylength, ref hitpoint);

		//find all edeges connected to faceid
		List<Edge> edges = new List<Edge>();
		if(faceid != -1)
		{
			//Debug.Log("Room.RayCast:" + faceid);
			Triangle face = m_RoomAnalyzer.LodTriangles[faceid];
			int ntrycount = 0;

			Edge TempEdge = null;
			Edge platformEdge = null;
			while(platformEdge == null)
			{
				for(int k = 0; k < face.vertex.Length; k++)
				{
					int nextid = (k + 1) % face.vertex.Length;
					Vector3 upvec = (face.vertex[nextid].position - face.vertex[k].position).normalized;
					float dot = Vector3.Dot(Vector3.up,upvec);

					if(dot == 1.0f) //goin upward
					{
						//check if next edge is diagonal
						int diagonalvtx0 = nextid;
						int diagonalvtx1 = (nextid + 1) % face.vertex.Length;
						Vector3 diagvec = (face.vertex[diagonalvtx1].position - face.vertex[diagonalvtx0].position).normalized;
						bool isdiag = (Vector3.Dot(upvec, diagvec) != 0);
						bool gotomirrorface = false;

						if(!isdiag) //found ladder step
						{
							Edge e = new Edge(face.vertex[diagonalvtx0].position, face.vertex[diagonalvtx1].position);
							e.AtoB =  diagvec;
							TempEdge = e;

						   //check if this is top most step
							if(SurfaceAnalyzer.ComputeEdgeCollapseCost(face.vertex[diagonalvtx0],face.vertex[diagonalvtx1]) < 1)
							{
							    platformEdge = e;
								break;
							}
							else
							{
								gotomirrorface = true;
							}

						}
						else
						{
							gotomirrorface = true;
						}

						if(gotomirrorface)
						{
							List<Triangle> faces = face.vertex[diagonalvtx0].face;
							for(int i = 0; i < faces.Count; i++)
							{
								if(faces[i].HasVertex(face.vertex[diagonalvtx1]) && !faces[i].HasVertex(face.vertex[k]))
								{
									face  = faces[i];
									break;
								}
							}
						}
					}
					else if(dot == -1.0f)
					{
						//check if next edge is diagonal
						int diagonalvtx0 = nextid;
						int diagonalvtx1 = (nextid + 1) % face.vertex.Length;
						Vector3 diagvec = (face.vertex[diagonalvtx1].position - face.vertex[diagonalvtx0].position).normalized;
						bool isdiag = (Vector3.Dot(upvec, diagvec) != 0);
						bool gotomirrorface = false;
						
						if(!isdiag) //found lowest ladder step
						{
							gotomirrorface = true;
						}
						else
						{
							Edge e = new Edge(face.vertex[diagonalvtx1].position, face.vertex[k].position);
						    e.AtoB =  (face.vertex[k].position - face.vertex[diagonalvtx1].position ).normalized;
							TempEdge = e;

							//check if this is top most step
							if(SurfaceAnalyzer.ComputeEdgeCollapseCost(face.vertex[diagonalvtx1],face.vertex[k]) < 1)
							{
							    platformEdge = e;
								break;
							}
							else
							{
								gotomirrorface = true;
							}
						}

						if(gotomirrorface)
						{
							List<Triangle> faces = face.vertex[diagonalvtx1].face;
							for(int i = 0; i < faces.Count; i++)
							{
								if(faces[i].HasVertex(face.vertex[k]) && !faces[i].HasVertex(face.vertex[diagonalvtx0]))
								{
									face  = faces[i];
									break;
								}
							}
						}

					}
				}

				ntrycount ++;
				if(ntrycount > 20)
				{
					if(TempEdge!= null)
					{
						platformEdge = TempEdge;
					}
					break;
				}
			}
		
			if(platformEdge != null) edges.Add(platformEdge);
		}

		return edges;
	}
	
	public  Material GetRoomMaterial()
	{
		return m_Material;
	}
	
	//Added method GetRoomMaterial() to modify shared mat

    public Vector3 GetCenterPoint()
    {
        return m_CenterPoint;
    }

    public Bounds GetBound()
    {
        return m_RoomBound;
    }


    RoomType DetectRoomType(Parser.Tr2Room room, Bounds b )
    {
        if ((room.Flags & 1) == 1) //water room below, bug: direct use of Flags causing error
        {
            //calculate water depth
            b = GetBound();
            if (b.size.y > 0.75)
            {
                return RoomType.DeepWater;
            }
            else
            {
                return RoomType.ShalloWater;
            }

        }
        return RoomType.Land;
    }

    public RoomType GetRoomType()
    {
        return m_RoomType;
    }

    public static Vector3[] GetWaterPortal(Parser.Tr2Room room)
    {
        Vector3[] portal_polygon = null;
        float surface = (-room.info.yTop * Settings.SceneScaling); //get scaled room surface point
        int Flags = room.Flags;
        if (Flags == 72  || (Flags & 1) == 1)
        {
            if (room.Portals != null)
            {
                for (int p = 0; p < room.Portals.Length; p++)
                {
                    Parser.Tr2RoomPortal port = room.Portals[p];
                    Parser.Tr2Vertex n = port.Normal;
                    if ((Vector3.Dot(new Vector3(n.y, n.y, n.z), Vector3.right) > 0.85f) && (port.Vertices != null))  // choose only horizontal portals
                    {
                        portal_polygon = new Vector3[port.Vertices.Length];
                        for (int i = 0; i < portal_polygon.Length; i++)
                        {
                            portal_polygon[i] = new Vector3(port.Vertices[i].x * Settings.SceneScaling, surface, port.Vertices[i].z * Settings.SceneScaling);
                        }

                        break;
                    }

                }
            }
        }

        return portal_polygon;
    }
}
