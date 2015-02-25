using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshModifier  {

	static List<int> DuplicateVertexIds = new List<int>();
	static List<Vector3> DuplicateVertices = new List<Vector3>();
	
	static public void FindDuplicateVertices(Mesh srcmesh )
	{
		DuplicateVertexIds = new List<int>();
		DuplicateVertices = new List<Vector3>();
		
		Mesh m = srcmesh;
		Vector3[] Vertices = m.vertices;
		
		for(int i = 0; i < Vertices.Length; i++)
		{
			for(int j = 0; j < Vertices.Length; j++)
			{
				if( i != j)
				{
					if(Vertices[i] == Vertices[j]) //found duplicate!
					{
						bool hasduplicate = false;
						for(int dupid = 0; dupid < DuplicateVertices.Count ;dupid++)
						{
							if(DuplicateVertices[dupid] == Vertices[j] )
							{
								hasduplicate = true;
								break;
							}
						}
						
						if(!hasduplicate)
						{
							DuplicateVertices.Add(Vertices[i]);
							DuplicateVertexIds.Add(i);
						}
						
						break;
					}
				}
			}
		}
		
	}

	static public void VertexWeild(Mesh srcmesh)
	{
		int[] vtxsharedtris = GetSharedTriangles(srcmesh );
		Vector3[] Vertices = srcmesh.vertices;
		Vector2[] uv = srcmesh.uv;
		int[] tris = srcmesh.triangles;
		
		//get shated normals
		srcmesh.triangles = vtxsharedtris;
		srcmesh.RecalculateNormals();
		Vector3[] normals = srcmesh.normals;
		
		for(int i = 0; i < Vertices.Length; i++)
		{
			for(int j = 0; j < DuplicateVertices.Count; j++)
			{
				if(Vertices[i] == DuplicateVertices[j])
				{
					normals[i] =  normals[DuplicateVertexIds[j]];
					//Debug.Log("found duplicate vertices");
					break;
					
				}
			}
		}
		
		srcmesh.Clear();
		srcmesh.vertices = Vertices;
		srcmesh.uv = uv;
		srcmesh.normals = normals;
		srcmesh.triangles  = tris;
	}

	static public int[] GetSharedTriangles(Mesh srcmesh )
	{
		FindDuplicateVertices(srcmesh);
		
		Vector3[] Vertices = srcmesh.vertices;
		int[] vtxsharedtris = srcmesh.triangles;
		int[] tris = new int[vtxsharedtris.Length];
		for(int i = 0; i < tris.Length; i++)
		{
			tris[i] = vtxsharedtris[i];
		}
		
		for(int i = 0; i < tris.Length; i++)
		{
			for(int j = 0; j < DuplicateVertices.Count; j++)
			{
				if(Vertices[tris[i]] == DuplicateVertices[j])
				{
					vtxsharedtris[i] = DuplicateVertexIds[j];
					//Debug.Log("found duplicate vertices");
					break;
				}
			}
		}
		
		return vtxsharedtris;
	}
}
