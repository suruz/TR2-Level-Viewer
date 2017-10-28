using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RoomEx: MonoBehaviour  {
	public SurfaceAnalyzer m_RoomAnalyzer;
	public Parser.Tr2Room m_Tr2Room;
	public Mesh m_Mesh;
	Vector3[] m_RoomVertices;
	public Transform m_Transform;
	int ID = 0;
	public int[] m_SharedTriangles = null;
	List<Edge> m_RoomEdges = null;
	public float m_CeilingHeight = -Mathf.Infinity;
	public float m_FloorHeight = Mathf.Infinity;
	public List<GameObject> m_StaticObjects = null;
	Material m_Material;
	Parser.Tr2Level m_leveldata = null;

	void Start()
	{
		m_RoomVertices = m_Mesh.vertices;
		m_SharedTriangles = MeshModifier.GetSharedTriangles(m_Mesh);
		m_RoomAnalyzer = new SurfaceAnalyzer(m_Mesh);
		m_RoomEdges = m_RoomAnalyzer.Analyze(m_RoomVertices, m_SharedTriangles,null);
		m_Material = GetComponent<MeshRenderer>().sharedMaterial;
	}

	void Update()
	{

	}

	public void  InitRoom(Parser.Tr2Room room, List<GameObject> objects)
	{
		m_leveldata = Level.m_leveldata;

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
}
