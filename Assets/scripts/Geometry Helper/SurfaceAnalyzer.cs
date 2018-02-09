using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SurfaceAnalyzer {
	
	int facecount = 0;
	public List<Triangle> LodTriangles = null;
	List<Vertex> LodVertices = null;
	List<Vector2i> features = null;

	//compute cost along a edge
	static public float ComputeEdgeCollapseCost(Vertex u,Vertex v) //v is one of neighbor of u
	{
		//float edgelength = (v.position - u.position).magnitude;
		float curvature = 1;
		List<Triangle> sides = new List<Triangle>();
		List<Vertex> edge_ends = new List<Vertex>();

		Vector3 vec = (v.position - u.position).normalized;
		for(int j = 0 ; j < u.neighbor.Count; j++)
		{
			Vector3 vec2 = (u.neighbor[j].position - u.position).normalized;
			if(Vector3.Dot(vec, vec2) == 1.0f)
			{
				edge_ends.Add(u.neighbor[j]);
			}
		}

		for(int e = 0; e < edge_ends.Count; e++)
		{
			for(int i = 0;i < u.face.Count; i++) 
			{
				if(u.face[i].HasVertex(edge_ends[e]))
				{
					sides.Add(u.face[i]);
				}
			}
		}

		if(sides.Count > 1)
		{
			Vector3 n0 = sides[0].normal;
			Vector3 n1 =  sides[1].normal;

			/*if(Vector3.Dot(n0, Vector3.up ) > 0.25f)
			{
				n0 = Vector3.up;
			}

			if(Vector3.Dot(n1, Vector3.up ) > 0.25f)
			{
				n1 = Vector3.up;
			}*/

			curvature =  Vector3.Dot(n0, n1);
		}
		curvature = Mathf.Max(0,curvature);
	
		return  curvature;
	}
	
	//compute cost for all edges from a vertex
	static void ComputeEdgeCostAtPeakVertex(Vertex v, bool peak)
	{
		if(v.neighbor.Count == 0) 
		{
			v.cost = -1f;
			v.collapse = v;
			return;
		}
		v.collapse = v.neighbor[0];
		v.cost = ComputeEdgeCollapseCost(v,v.neighbor[0]);
		float height = v.position.y;
		//search in all edge for max cost
		float[] costs = new float[v.neighbor.Count]; 
		if(peak)
		{
			for(int i = 0; i < v.neighbor.Count; i++) 
			{
				//determine peak by equal and oposite micro ridge
				if((height - v.neighbor[i].position.y) < -0.1f )
				{
					v.cost = 0;
					return;
				}
			}
		}
	}
	
	//compute cost for all edges from a vertex
	static void ComputeEdgeCostAtVertex(Vertex v, bool peak)
	{
		if(v.neighbor.Count == 0) 
		{
			v.cost = -1f;
			v.collapse = v;
			return;
		}
		v.collapse = v.neighbor[0];
		v.cost = ComputeEdgeCollapseCost(v,v.neighbor[0]);
		float height = v.position.y;
		//search in all edge for max cost
		float[] costs = new float[v.neighbor.Count]; 
		if(peak)
		{
			for(int i = 0; i < v.neighbor.Count; i++) 
			{
				/*//determine peak by equal and oposite micro ridge
				if((height - v.neighbor[i].position.y) < -0.15f )
				{
					v.cost = 0;
					return;
				}*/
				
				float c = ComputeEdgeCollapseCost(v,v.neighbor[i]);
				costs[i] = c;
				if(c < v.cost) 
				{
					v.collapse = v.neighbor[i];
					v.cost = c;
				}
			}
			
			Vector3 vec_ridge_forward = (v.collapse.position - v.position).normalized;
			/*Vector3 up = Vector3.Cross(Vector3.right, vec_ridge_forward).normalized;
			float dot = Vector3.Dot (up,Vector3.right);

			if(Mathf.Abs(dot) > 0)
			{
				v.cost = 0;
			}*/
			
			
			/*for(int i = 0; i < v.neighbor.Count; i++) 
			{
				Vector3 vec_ridge_backward = ( v.neighbor[i].position - v.position).normalized;
				float c = costs[i];
				float dot = Vector3.Dot (vec_ridge_forward,vec_ridge_backward);
				if(dot < 0.99f  && Mathf.Abs(v.cost - c) < 0.01f)
				{
					return;
				}
				v.cost = 0;
			}*/
			
		}
		
	}

	public  SurfaceAnalyzer(Mesh mesh)
	{
		LodTriangles = new List<Triangle>();
		LodVertices = new List<Vertex>();
		features = new List<Vector2i>();
	}

	int k = 327;
	public void DebugSurface(Transform transform, List<Edge> edges)
	{
		for(int i = 0; i < edges.Count; i++)
		{
			Debug.DrawLine(transform.TransformPoint(edges[i].PointA), transform.TransformPoint(edges[i].PointB),Color.red);
		}
		//Debug.Log("Debug Room Surface:");
	}
	
	public List<Edge>  Analyze(Vector3[] dsm_vertices, int[]dsm_tris, Color[] maskcolors , bool peak = true)
	{
		// generate vertextriangle network graph in roof surface

		Vector3[] vertices;
		int[]tris;
		facecount = dsm_tris.Length / 3;

		for(int i = 0; i < dsm_vertices.Length; i++)
		{
			Vertex v = new Vertex(dsm_vertices[i],i,i);
			LodVertices.Add(v);
		}

		for(int i = 0; i < facecount; i++)
		{
			int t = 3 * i;

			Vertex v0 =LodVertices[dsm_tris[t + 0]];
			Vertex v1 =LodVertices[dsm_tris[t + 1]];
			Vertex v2 =LodVertices[dsm_tris[t + 2]];

			Triangle tri = new Triangle(v0,v1,v2);
			LodTriangles.Add(tri);
			
			v0.AddFace(tri);
			v1.AddFace(tri);
			v2.AddFace(tri);
		
		}

		List<Edge> edges = new List<Edge>();

		int nedge = 0;
		for(k = 0; k < LodVertices.Count; k++)
		{
			//if(LodVertices[k].deleted) continue;
			for(int n = 0; n < LodVertices[k].neighbor.Count; n++)
			{
				if(LodVertices[k].neighbor[n].deleted) continue;
				if(ComputeEdgeCollapseCost(LodVertices[k],LodVertices[k].neighbor[n]) < 1)
				{
					//LodVertices[k].neighbor[n].deleted = true;

					Vector3 p1 = Vector3.zero;
					Vector3 p2 = Vector3.zero;
					//rectify edge vector with id cheking
					if(LodVertices[k].id < LodVertices[k].neighbor[n].id)
					{

						p1 = LodVertices[k].position;
						p2 = LodVertices[k].neighbor[n].position;

						//
					}
					else
					{
						p1 = LodVertices[k].neighbor[n].position;
						p2 = LodVertices[k].position;

					}
					Vector3 edgevec = (p2 - p1).normalized;
					if(Vector3.Dot(edgevec, Vector3.up) == 0)
					{
						Edge e = new Edge(p1, p2);
						e.AtoB =  edgevec;
						edges.Add(e);
					}
	
				}
			}
		}

		return edges;
	}
	
}
