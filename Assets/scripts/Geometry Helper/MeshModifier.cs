using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MeshModifier  {

	static List<int> DuplicateVertexIds = new List<int>();
	static List<Vector3> DuplicateVertices = new List<Vector3>();
	
	static public void CullAlphaFace(ref Mesh mesh, Texture2D texture)
	{
        return;
		int[] triangles = mesh.triangles;
		Vector2[] uvs = mesh.uv;
		int face_count = triangles.Length / 3;
		List<int> face_list = new List<int>();
		Color[] color = texture.GetPixels();
		
		//0 - 2, 3,4,5, 6,7,8
		for(int i = 0; i < face_count; i++)
		{
			float minx = 0;
			float maxx = 0;
			float miny = 0;
			float maxy = 0;
			
			int uvid0 = triangles[3 * i + 0];
	
			
			/*if((uvid0  > uvs.Length) || (uvid1 > uvs.Length) || (uvid2 > uvs.Length))
			{
				
				Debug.Log("face id:" + i + " face count:" + face_count + " uvs.length" + uvs.Length);
			}
			
			continue;*/
			
			 minx = uvs[uvid0].x;
			 maxx = uvs[uvid0].x;
			 miny = uvs[uvid0].y;
			 maxy = uvs[uvid0].y;
				
			//find uv aabb for this face 
			for(int t = 0; t < 3; t++)
			{
				if(minx > uvs[triangles[3 * i + t]].x)
				{
					minx = uvs[triangles[3 * i + t]].x;
				}
				
				if(maxx < uvs[triangles[3 * i + t]].x)
				{
					maxx = uvs[triangles[3 * i + t]].x;
				}
				
				if(miny > uvs[triangles[3 * i + t]].y)
				{
					miny = uvs[triangles[3 * i + t]].y;
				}
				
				if(maxy < uvs[triangles[3 * i + t]].y)
				{
					maxy = uvs[triangles[3 * i + t]].y;
				}
			}
			
			int minxi = (int)(minx * texture.width);
			int maxxi = (int)(maxx * texture.width);
			int minyi = (int)(miny * texture.height);
			int maxyi = (int)(maxy * texture.height);
			
			//convert uv space to screen space
			Vector3 p0 = new Vector3(uvs[triangles[3 * i + 0]].x * texture.width,  0, uvs[triangles[3 * i + 0]].y * texture.height );
			Vector3 p1 = new Vector3(uvs[triangles[3 * i + 1]].x * texture.width,  0, uvs[triangles[3 * i + 1]].y * texture.height );
			Vector3 p2 = new Vector3(uvs[triangles[3 * i + 2]].x * texture.width,  0, uvs[triangles[3 * i + 2]].y * texture.height );
			
			/*IsUVInSideDebug(p0, p1,p2,  Vector3.zero, color, texture.width,  texture.height );
			
			Vector3 vec =((p2 + ((p0 - p2) * 0.5f) - p1) * 1.8f) + p1;
			int idx = (int)vec.z * texture.width + (int)vec.x;
				//color[idx] = Color.red;
			
			if(IsUVInSide( p2, p1,p0, vec ))
			{
				color[idx] = Color.red;
			}*/
			
			bool hasalpha = false;
			
			for(int y = minyi; y < maxyi; y++)
			{
				for(int x = minxi; x < maxxi; x++)
				{
				
					//if(IsUVInSide( p2, p1,p0, new Vector3(x,0, y) ))
					//{
						int idx = y *  texture.width + x;
						
						if(color[idx].a < 0.5 )
						{
							hasalpha = true;
							//color[idx] = Color.yellow;
						    break;
						}
					//}
					
				
				}
				
				if(hasalpha) break;
			}
		
			if(!hasalpha)
			{
				face_list.Add(triangles[3 * i + 0]);
				face_list.Add(triangles[3 * i + 1]);
				face_list.Add(triangles[3 * i + 2]);
			}
		}
		texture.SetPixels(color);
		texture.Apply();
		mesh.triangles =  face_list.ToArray();
		mesh.RecalculateNormals();
	}
	
	public static bool IsUVInSide(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 test_point)
	{
		Vector3 vec1 = (p1 - p0);
		Vector3 vec2 = (p2 - p1);
		Vector3 vec3 = (p0 - p2);
		
		Vector3 in1 = Vector3.Cross(Vector3.up, vec1.normalized).normalized;
		Vector3 in2 = Vector3.Cross(Vector3.up, vec2.normalized).normalized;
		Vector3 in3 = Vector3.Cross(Vector3.up, vec3.normalized).normalized;
		
		if(Vector3.Dot(in1,(test_point - p0).normalized ) < 0) return false;
		if(Vector3.Dot(in2,(test_point - p1).normalized ) < 0) return false;
		if(Vector3.Dot(in3,(test_point - p2).normalized ) < 0) return false;
		
		return true;
	}
	
	
	public static bool IsUVInSideDebug(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 test_point, Color[] color, int width, int height)
	{
		Vector3 vec1 = (p1 - p0);
		Vector3 vec2 = (p2 - p1);
		Vector3 vec3 = (p0 - p2);
		
		float delta1 = vec1.magnitude * 0.01f;
		float delta2 = vec2.magnitude * 0.01f;
		float delta3 = vec3.magnitude * 0.01f;
		
		for(int i = 0; i < 100; i ++)
		{
			Vector3 p = p0 + vec1.normalized * i * delta1;
			
			int idx = (int)p.z * width + (int)p.x;
			color[idx] = Color.red * ((float)i / (float)100);
			
		}
		
		for(int i = 0; i < 100; i ++)
		{
			Vector3 p = p1 + vec2.normalized * i * delta2;
			
			int idx = (int)p.z * width + (int)p.x;
			color[idx] = Color.green* ((float)i / (float)100);
		}
		
		for(int i = 0; i < 100; i ++)
		{
			Vector3 p = p2 + vec3.normalized * i * delta3;
			
			int idx = (int)p.z * width + (int)p.x;
			color[idx] = Color.blue;
		}
		
		
		
		
		
		
		
		/*float insdide = 1;
		
		Vector3 in1 = Vector3.Cross(Vector3.up, vec1);
		Vector3 in2 = Vector3.Cross(Vector3.up, vec2);
		Vector3 in3 = Vector3.Cross(Vector3.up, vec3);
		
		if(Vector3.Dot(in1,test_point - p0 ) < 0) return false;
		if(Vector3.Dot(in2,test_point - p1 ) < 0) return false;
		if(Vector3.Dot(in3,test_point - p2 ) < 0) return false;
		*/
		return true;
	}
	
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

	static public Mesh VertexWeild(Mesh srcmesh)
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

        return srcmesh;

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
