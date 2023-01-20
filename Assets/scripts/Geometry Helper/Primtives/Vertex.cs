using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Vertex {
	public Vector3 position; 
	public Vector3 normal; 
	public int id; 
	public int proxyid; 
	
	public List<Vertex> neighbor; //vertex reffence // may be updated by face
	public List<Triangle> face; 
	public float cost = 0; 
	public Vertex  collapse = null;
	public bool deleted = false;
	
	public Vertex(Vector3 v,int _id , int _proxyid)
	{
		position = v;
		id = _id;
		proxyid = _proxyid;
		
		neighbor = new List<Vertex>();
		face = new List<Triangle>();
		cost = 0;
		collapse = null;
		
	}
	public void deVertex ()
	{

    while( neighbor.Count > 0) {
		
			neighbor[0].neighbor.Remove( this );
			neighbor.Remove( neighbor[0] );
    }
    deleted = true;
	}
	
	public bool IsBorder () 
	{
		int j  = 0;;
        int n   = neighbor.Count;
        Vertex nb  = null;
        int face_len = 0;
        Triangle f  = null;
        int count  = 0;
    
    	for (int i   = 0; i < n; ++i) 
		{
			count = 0;
			nb = neighbor[i];
			face_len = face.Count;
			for (j = 0; j < face_len; ++j) {
				f = face[j];
				if (f.HasVertex( nb ))
					++count;
			}
			if (count == 1) return true;
		}
		return false;
	}
	
	
	public void RemoveFace (Triangle tri )
	{
		face.Remove(tri);
	}
	
	public void AddFace(Triangle tri)
	{
		int n  = face.Count;
		bool duplicate = false;
		for(int j = 0; j < n; ++j)
		{
			if(face[j] == tri )
			{
				duplicate = true;
				break;  
			}
			
			//break; //cause unreachable :)
		}
		if(duplicate == false)
		{
			face.Add(tri);
			//Debug.Log(v.id);
		}
		
		//face.Add(tri);
	}
	
	
	public void AddNeighbor(Vertex v)
	{
		int n  = neighbor.Count;
		bool duplicate = false;
		for(int j = 0; j < n; ++j)
		{
			if(neighbor[j].position == v.position )
			{
				duplicate = true;
				break;  
			}
			
			//break; //cause unreachable :)
		}
		if(duplicate == false)
		{
			neighbor.Add(v);
			//Debug.Log(v.id);
		}
		
		
		
		/*
		int i = 0;
		int foundAt  = -1;
		int n  = neighbor.Count;
		
		for (i = 0; i < n; ++i)
			if (neighbor[i] == v) {foundAt = i; break;}
		
		if (foundAt == -1)
			neighbor.Add( v );*/
		
		
	}
	
	public void RemoveNeighbor(Vertex v)
	{
		
	}

	public void RemoveIfNonNeighbor(Vertex v)
	{
		
		bool invalid = false;
		for(int i = 0; i < neighbor.Count; i++)
		{
			if(neighbor[i].position == v.position)
			{
				invalid = true;
				//neighbor[i] = vnew;
				break;
			}
		}
		
		if(invalid)
		{
			//well see if any existing face attached to this vertex still referencing this neighbor vold
			
			for (int i = 0; i < face.Count; ++i) 
			{
				if (face[i].HasVertex(v)) return;
			}
			
			//no ? then remove it
			neighbor.Remove(v);
			
			//Debug.Log("RemoveIfNonNeighbor");
		}
	}
}