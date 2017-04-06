using UnityEngine;
using System.Collections;

public class MeshBuilder  {
	
	public static Mesh CreateRoomMesh(Parser.Tr2Room tr2room, Parser.Tr2Level  leveldata)
	{
		Vector3[] sharedVertices = null;
	
		if(tr2room.RoomData.NumVertices > 0)
		{
			sharedVertices = new Vector3[tr2room.RoomData.NumVertices];
			for(int vertAttribCount = 0; vertAttribCount < tr2room.RoomData.NumVertices; vertAttribCount++)
			{
				float x = tr2room.RoomData.Vertices[vertAttribCount].Vertex.x; 
				float y = tr2room.RoomData.Vertices[vertAttribCount].Vertex.y; 
				float z = tr2room.RoomData.Vertices[vertAttribCount].Vertex.z;
				
				////print("chk vtx x y z:" +x+ " " +y + " " +z);
				
				sharedVertices[vertAttribCount].x = x;
				sharedVertices[vertAttribCount].y =-y;
				sharedVertices[vertAttribCount].z = z; 
			}
		}
		
		//warning: avariable lengh array in a structure can cause access violence
		
		
		//if(tr2room.RoomData.NumRectangles > 0)
		//{
		
		//selected_texObjectIdx = tr2room.RoomData.Rectangles[0].Texture;
		//selected_texObj =  leveldata.ObjectTextures[selected_texObjectIdx];
		//selected_texTileIdx = selected_texObj.Tile;
		
		int strideVertIdx = (tr2room.RoomData.NumRectangles * 4);
		int strideTriIdx = (tr2room.RoomData.NumRectangles* 3 * 2) ;
		
		int numNonsharedVertices = strideVertIdx + (tr2room.RoomData.NumTriangles * 3) ;
		int numNonsharedTris = strideTriIdx + (tr2room.RoomData.NumTriangles  * 3) ;
		
		Vector3[] nonSharedVertices = new Vector3[numNonsharedVertices]; 
		Vector2[] nonSharedUVs = new Vector2[numNonsharedVertices]; 
		Vector2[] nonSharedUV2s = new Vector2[numNonsharedVertices]; 
		int[] nonSharedTris = new int[numNonsharedTris];
		
		//triangles = new int[tr2room.RoomData.NumRectangles * 3 * 2];
		
		for(int rectCount = 0; rectCount < tr2room.RoomData.NumRectangles; rectCount++)
		{
			
			int Idx0 = tr2room.RoomData.Rectangles[rectCount].Vertices0;
			int Idx1 = tr2room.RoomData.Rectangles[rectCount].Vertices1;	
			int Idx2 = tr2room.RoomData.Rectangles[rectCount].Vertices2;
			int Idx3 = tr2room.RoomData.Rectangles[rectCount].Vertices3;
			
			////print ("idx0 - Idx1 - Idx2 - Idx3:" + Idx0 + " " + Idx1 + " " + Idx2 +" " + Idx3);
			
			
			int vertOrUVIdx0 = rectCount * 4 + 0;
			int vertOrUVIdx1 = rectCount * 4 + 1;
			int vertOrUVIdx2 = rectCount * 4 + 2;
			int vertOrUVIdx3 = rectCount * 4 + 3;
			
			
			nonSharedVertices[vertOrUVIdx0] = sharedVertices[Idx0];
			nonSharedVertices[vertOrUVIdx1] = sharedVertices[Idx1];
			nonSharedVertices[vertOrUVIdx2] = sharedVertices[Idx2];
			nonSharedVertices[vertOrUVIdx3] = sharedVertices[Idx3];
			
			ushort texObjectIdx = tr2room.RoomData.Rectangles[rectCount].Texture;
			Parser.Tr2ObjectTexture texObj =  leveldata.ObjectTextures[texObjectIdx];
			ushort texTileIdx = texObj.Tile;  //bind this textile in material?
			
			//if(texTileIdx != prevTexture)
			//{
			//newMatCount +=1;
			//prevTexture = texTileIdx;
			////print("newMatCount:"+ newMatCount);
			//}
			
			nonSharedUVs[vertOrUVIdx0].x = (float) 	TextureUV.AdjustTextureCoordinateX((byte)texObj.Vertices[0].Xpixel,(sbyte)texObj.Vertices[0].Xcoordinate,texTileIdx);
			nonSharedUVs[vertOrUVIdx0].y = (float) 	TextureUV.AdjustTextureCoordinateY((byte)texObj.Vertices[0].Ypixel,(sbyte)texObj.Vertices[0].Ycoordinate,texTileIdx);
			
			nonSharedUVs[vertOrUVIdx1].x = (float) 	TextureUV.AdjustTextureCoordinateX((byte)texObj.Vertices[1].Xpixel,(sbyte)texObj.Vertices[1].Xcoordinate,texTileIdx);
			nonSharedUVs[vertOrUVIdx1].y = (float) 	TextureUV.AdjustTextureCoordinateY((byte)texObj.Vertices[1].Ypixel,(sbyte)texObj.Vertices[1].Ycoordinate,texTileIdx);
			
			nonSharedUVs[vertOrUVIdx2].x = (float) 	TextureUV.AdjustTextureCoordinateX((byte)texObj.Vertices[2].Xpixel,(sbyte)texObj.Vertices[2].Xcoordinate,texTileIdx);
			nonSharedUVs[vertOrUVIdx2].y = (float) 	TextureUV.AdjustTextureCoordinateY((byte)texObj.Vertices[2].Ypixel,(sbyte)texObj.Vertices[2].Ycoordinate,texTileIdx);
			
			nonSharedUVs[vertOrUVIdx3].x = (float) 	TextureUV.AdjustTextureCoordinateX((byte)texObj.Vertices[3].Xpixel,(sbyte)texObj.Vertices[3].Xcoordinate,texTileIdx);
			nonSharedUVs[vertOrUVIdx3].y = (float) 	TextureUV.AdjustTextureCoordinateY((byte)texObj.Vertices[3].Ypixel,(sbyte)texObj.Vertices[3].Ycoordinate,texTileIdx);
			
			////print("uv[Idx0]"+ uv[Idx0].x + " " + uv[Idx0].y);
			////print("uv[Idx1]"+ uv[Idx1].x + " " + uv[Idx1].y);
			
			
			//ushort opacity = texObj.TransparencyFlags;  //isItOpacq
			
			nonSharedTris[rectCount * 6 + 0] = vertOrUVIdx0;
			nonSharedTris[rectCount * 6 + 1] = vertOrUVIdx1;
			nonSharedTris[rectCount * 6 + 2] = vertOrUVIdx2;
			
			nonSharedTris[rectCount * 6 + 3] = vertOrUVIdx2;
			nonSharedTris[rectCount * 6 + 4] = vertOrUVIdx3;
			nonSharedTris[rectCount * 6 + 5] = vertOrUVIdx0;	
			
		}
		
		
		for(int triCount = 0; triCount < tr2room.RoomData.NumTriangles; triCount++)
		{
			
			////print("tr2room.RoomData.NumTriangles"+ tr2room.RoomData.NumTriangles);
			
			int Idx0 = tr2room.RoomData.Triangles[triCount].Vertices0;
			int Idx1 = tr2room.RoomData.Triangles[triCount].Vertices1;	
			int Idx2 = tr2room.RoomData.Triangles[triCount].Vertices2;
			
			////print ("idx0 - Idx1 - Idx2:" + Idx0 + " " + Idx1 + " " + Idx2);
			//[][][][]+[][][]
			int vertOrUVIdx0 = triCount * 3+ 0;
			int vertOrUVIdx1 = triCount * 3+ 1;
			int vertOrUVIdx2 = triCount * 3+ 2;
			
			
			nonSharedVertices[strideVertIdx + vertOrUVIdx0] = sharedVertices[Idx0];
			nonSharedVertices[strideVertIdx + vertOrUVIdx1] = sharedVertices[Idx1];
			nonSharedVertices[strideVertIdx + vertOrUVIdx2] = sharedVertices[Idx2];
			
			
			ushort texObjectIdx = tr2room.RoomData.Triangles[triCount].Texture;
			Parser.Tr2ObjectTexture texObj =  leveldata.ObjectTextures[texObjectIdx];
			ushort texTileIdx = texObj.Tile;  //bind this textile in material?
			
			//if(texTileIdx != prevTexture)
			//{
			//newMatCount +=1;
			//prevTexture = texTileIdx;
			////print("newMatCount:"+ newMatCount);
			//}
			
			nonSharedUVs[strideVertIdx + vertOrUVIdx0].x = (float) 	TextureUV.AdjustTextureCoordinateX((byte)texObj.Vertices[0].Xpixel,(sbyte)texObj.Vertices[0].Xcoordinate,texTileIdx);
			nonSharedUVs[strideVertIdx + vertOrUVIdx0].y = (float) 	TextureUV.AdjustTextureCoordinateY((byte)texObj.Vertices[0].Ypixel,(sbyte)texObj.Vertices[0].Ycoordinate,texTileIdx);
			
			nonSharedUVs[strideVertIdx + vertOrUVIdx1].x = (float) 	TextureUV.AdjustTextureCoordinateX((byte)texObj.Vertices[1].Xpixel,(sbyte)texObj.Vertices[1].Xcoordinate,texTileIdx);
			nonSharedUVs[strideVertIdx + vertOrUVIdx1].y = (float) 	TextureUV.AdjustTextureCoordinateY((byte)texObj.Vertices[1].Ypixel,(sbyte)texObj.Vertices[1].Ycoordinate,texTileIdx);
			
			nonSharedUVs[strideVertIdx + vertOrUVIdx2].x = (float) 	TextureUV.AdjustTextureCoordinateX((byte)texObj.Vertices[2].Xpixel,(sbyte)texObj.Vertices[2].Xcoordinate,texTileIdx);
			nonSharedUVs[strideVertIdx + vertOrUVIdx2].y = (float) 	TextureUV.AdjustTextureCoordinateY((byte)texObj.Vertices[2].Ypixel,(sbyte)texObj.Vertices[2].Ycoordinate,texTileIdx);
			
			
			////print("uv[Idx0]"+ uv[Idx0].x + " " + uv[Idx0].y);
			////print("uv[Idx1]"+ uv[Idx1].x + " " + uv[Idx1].y);
			
			//ushort opacity = texObj.TransparencyFlags;  //isItOpacq
			
			nonSharedTris[strideTriIdx + vertOrUVIdx0] = strideVertIdx + vertOrUVIdx0;
			nonSharedTris[strideTriIdx + vertOrUVIdx1] = strideVertIdx + vertOrUVIdx1;
			nonSharedTris[strideTriIdx + vertOrUVIdx2] = strideVertIdx + vertOrUVIdx2;
			
			////print ("idx0 - Idx1 - Idx2:" + nonSharedTris[strideTriIdx + vertOrUVIdx0]  + " " + 	nonSharedTris[strideTriIdx + vertOrUVIdx1]  + " " + nonSharedTris[strideTriIdx + vertOrUVIdx2] );
		}
		
		////print("leveldata.Rooms[5].RoomData.NumRectangles:"+ tr2room.RoomData.NumRectangles);
		//SetTriangles (triangles : int[], submesh : int) : void
		//generate secondary uv set
		
		for(int uvIdx = 0; uvIdx < nonSharedUVs.Length; uvIdx++)
		{
			nonSharedUV2s[uvIdx] = nonSharedUVs[uvIdx];
			nonSharedUV2s[uvIdx].x+=0.001f;
			nonSharedUV2s[uvIdx].y+=0.001f;
		}
        for (int i = 0; i < nonSharedVertices.Length; i++) { nonSharedVertices[i] = nonSharedVertices[i] * Settings.SceneScaling; }
        Mesh mesh  = new Mesh();
		mesh.Clear();
		mesh.vertices = nonSharedVertices;
		mesh.uv = nonSharedUVs;
		mesh.uv2= nonSharedUV2s;
		mesh.triangles = nonSharedTris;
		//mesh.Optimize();
		mesh.RecalculateNormals();
#if UNITY_EDITOR
		Vector4[] tangents = new Vector4[mesh.vertices.Length];
		computeTangentsAndBinormals(nonSharedVertices,mesh.normals,nonSharedUVs, nonSharedTris ,tangents);
		mesh.tangents = tangents;
		tangents = null;
#endif
		//free some memory
		nonSharedVertices = null;
		nonSharedUVs = null;
		nonSharedUV2s = null;
		nonSharedTris = null;
		
		

		//}

		return mesh;
	}
	
	public static Mesh CreateObjectMesh(Parser.Tr2Mesh tr2mesh , Parser.Tr2Level  leveldata)
	{
		Vector3[] sharedVertices = null;
		if(tr2mesh.NumVertices> 0)
		{
			sharedVertices = new Vector3[tr2mesh.NumVertices];
			for(int vertAttribCount = 0; vertAttribCount < tr2mesh.NumVertices; vertAttribCount++)
			{
				
				
				float x = tr2mesh.Vertices[vertAttribCount].x; 
				float y = tr2mesh.Vertices[vertAttribCount].y; 
				float z = tr2mesh.Vertices[vertAttribCount].z;
				
				////print("chk vtx x y z:" +x+ " " +y + " " +z);
				
				sharedVertices[vertAttribCount].x = x;
				sharedVertices[vertAttribCount].y =-y;
				sharedVertices[vertAttribCount].z = z; 
			}
			
			//uv = new Vector2[leveldata.Rooms[chkRoom].RoomData.NumVertices];
		}
		//warning: a variable lengh array in a structure can cause access violence
		
		//selected_texObjectIdx = leveldata.Rooms[chkRoom].RoomData.Rectangles[0].Texture;
		//selected_texObj =  leveldata.ObjectTextures[selected_texObjectIdx];
		//selected_texTileIdx = selected_texObj.Tile;
		
		int numNonsharedVertices = (tr2mesh.NumTexturedRectangles * 4) +(tr2mesh.NumTexturedTriangles * 3) ;
		int numNonsharedTris = (tr2mesh.NumTexturedRectangles* 3 * 2) +(tr2mesh.NumTexturedTriangles  * 3) ;
		
		Vector3[] nonSharedVertices = new Vector3[numNonsharedVertices]; 
		Vector2[] nonSharedUVs = new Vector2[numNonsharedVertices]; 
		int[] nonSharedTris = new int[numNonsharedTris];
		
		
		//triangles = new int[leveldata.Rooms[chkRoom].RoomData.NumRectangles * 3 * 2];
		
		for(int rectCount = 0; rectCount < tr2mesh.NumTexturedRectangles; rectCount++)
		{
			
			int Idx0 = tr2mesh.TexturedRectangles[rectCount].Vertices0;
			int Idx1 = tr2mesh.TexturedRectangles[rectCount].Vertices1;	
			int Idx2 = tr2mesh.TexturedRectangles[rectCount].Vertices2;
			int Idx3 = tr2mesh.TexturedRectangles[rectCount].Vertices3;
			
			
			int vertOrUVIdx0 = rectCount * 4 + 0;
			int vertOrUVIdx1 = rectCount * 4 + 1;
			int vertOrUVIdx2 = rectCount * 4 + 2;
			int vertOrUVIdx3 = rectCount * 4 + 3;
			
			
			nonSharedVertices[vertOrUVIdx0] = sharedVertices[Idx0];
			nonSharedVertices[vertOrUVIdx1] = sharedVertices[Idx1];
			nonSharedVertices[vertOrUVIdx2] = sharedVertices[Idx2];
			nonSharedVertices[vertOrUVIdx3] = sharedVertices[Idx3];
			
			ushort texObjectIdx = tr2mesh.TexturedRectangles[rectCount].Texture;
			Parser.Tr2ObjectTexture texObj =  leveldata.ObjectTextures[texObjectIdx];
			ushort texTileIdx = texObj.Tile;  //bind this textile in material?
			
			//if(texTileIdx != prevTexture)
			//{
			//newMatCount +=1;
			//prevTexture = texTileIdx;
			////print("newMatCount:"+ newMatCount);
			//}
			
			nonSharedUVs[vertOrUVIdx0].x = (float) TextureUV.AdjustTextureCoordinateX((byte)texObj.Vertices[0].Xpixel,(sbyte)texObj.Vertices[0].Xcoordinate,texTileIdx);
			nonSharedUVs[vertOrUVIdx0].y = (float) TextureUV.AdjustTextureCoordinateY((byte)texObj.Vertices[0].Ypixel,(sbyte)texObj.Vertices[0].Ycoordinate,texTileIdx);
			
			nonSharedUVs[vertOrUVIdx1].x = (float) TextureUV.AdjustTextureCoordinateX((byte)texObj.Vertices[1].Xpixel,(sbyte)texObj.Vertices[1].Xcoordinate,texTileIdx);
			nonSharedUVs[vertOrUVIdx1].y = (float) TextureUV.AdjustTextureCoordinateY((byte)texObj.Vertices[1].Ypixel,(sbyte)texObj.Vertices[1].Ycoordinate,texTileIdx);
			
			nonSharedUVs[vertOrUVIdx2].x = (float) TextureUV.AdjustTextureCoordinateX((byte)texObj.Vertices[2].Xpixel,(sbyte)texObj.Vertices[2].Xcoordinate,texTileIdx);
			nonSharedUVs[vertOrUVIdx2].y = (float) TextureUV.AdjustTextureCoordinateY((byte)texObj.Vertices[2].Ypixel,(sbyte)texObj.Vertices[2].Ycoordinate,texTileIdx);
			
			nonSharedUVs[vertOrUVIdx3].x = (float) TextureUV.AdjustTextureCoordinateX((byte)texObj.Vertices[3].Xpixel,(sbyte)texObj.Vertices[3].Xcoordinate,texTileIdx);
			nonSharedUVs[vertOrUVIdx3].y = (float) TextureUV.AdjustTextureCoordinateY((byte)texObj.Vertices[3].Ypixel,(sbyte)texObj.Vertices[3].Ycoordinate,texTileIdx);
			
			////print("uv[Idx0]"+ uv[Idx0].x + " " + uv[Idx0].y);
			////print("uv[Idx1]"+ uv[Idx1].x + " " + uv[Idx1].y);
			
			
			//ushort opacity = texObj.TransparencyFlags;  //isItOpacq
			
			nonSharedTris[rectCount * 6 + 0] = vertOrUVIdx0;
			nonSharedTris[rectCount * 6 + 1] = vertOrUVIdx1;
			nonSharedTris[rectCount * 6 + 2] = vertOrUVIdx2;
			
			nonSharedTris[rectCount * 6 + 3] = vertOrUVIdx2;
			nonSharedTris[rectCount * 6 + 4] = vertOrUVIdx3;
			nonSharedTris[rectCount * 6 + 5] = vertOrUVIdx0;			
		}

		for(int triCount = 0; triCount < tr2mesh.NumTexturedTriangles; triCount++)
		{
			
			int Idx0 = tr2mesh.TexturedTriangles[triCount].Vertices0;
			int Idx1 = tr2mesh.TexturedTriangles[triCount].Vertices1;	
			int Idx2 = tr2mesh.TexturedTriangles[triCount].Vertices2;
			
			int vertOrUVIdx0 = triCount * 3+ 0;
			int vertOrUVIdx1 = triCount * 3+ 1;
			int vertOrUVIdx2 = triCount * 3+ 2;
			
			int strideVertIdx = (tr2mesh.NumTexturedRectangles * 4);
			int strideTriIdx = (tr2mesh.NumTexturedRectangles* 3 * 2) ;
			
			
			nonSharedVertices[strideVertIdx+vertOrUVIdx0] = sharedVertices[Idx0];
			nonSharedVertices[strideVertIdx+vertOrUVIdx1] = sharedVertices[Idx1];
			nonSharedVertices[strideVertIdx+vertOrUVIdx2] = sharedVertices[Idx2];
			
			
			ushort texObjectIdx = tr2mesh.TexturedTriangles[triCount].Texture;
			Parser.Tr2ObjectTexture texObj =  leveldata.ObjectTextures[texObjectIdx];
			ushort texTileIdx = texObj.Tile;  //bind this textile in material?
			
			//if(texTileIdx != prevTexture)
			//{
			//newMatCount +=1;
			//prevTexture = texTileIdx;
			////print("newMatCount:"+ newMatCount);
			//}
			
			nonSharedUVs[strideVertIdx+vertOrUVIdx0].x = (float) TextureUV.AdjustTextureCoordinateX((byte)texObj.Vertices[0].Xpixel,(sbyte)texObj.Vertices[0].Xcoordinate,texTileIdx);
			nonSharedUVs[strideVertIdx+vertOrUVIdx0].y = (float) TextureUV.AdjustTextureCoordinateY((byte)texObj.Vertices[0].Ypixel,(sbyte)texObj.Vertices[0].Ycoordinate,texTileIdx);
			
			nonSharedUVs[strideVertIdx+vertOrUVIdx1].x = (float) TextureUV.AdjustTextureCoordinateX((byte)texObj.Vertices[1].Xpixel,(sbyte)texObj.Vertices[1].Xcoordinate,texTileIdx);
			nonSharedUVs[strideVertIdx+vertOrUVIdx1].y = (float) TextureUV.AdjustTextureCoordinateY((byte)texObj.Vertices[1].Ypixel,(sbyte)texObj.Vertices[1].Ycoordinate,texTileIdx);
			
			nonSharedUVs[strideVertIdx+vertOrUVIdx2].x = (float) TextureUV.AdjustTextureCoordinateX((byte)texObj.Vertices[2].Xpixel,(sbyte)texObj.Vertices[2].Xcoordinate,texTileIdx);
			nonSharedUVs[strideVertIdx+vertOrUVIdx2].y = (float) TextureUV.AdjustTextureCoordinateY((byte)texObj.Vertices[2].Ypixel,(sbyte)texObj.Vertices[2].Ycoordinate,texTileIdx);
			
			
			////print("uv[Idx0]"+ uv[Idx0].x + " " + uv[Idx0].y);
			////print("uv[Idx1]"+ uv[Idx1].x + " " + uv[Idx1].y);
			
			//ushort opacity = texObj.TransparencyFlags;  //isItOpacq
			
			nonSharedTris[strideTriIdx+vertOrUVIdx0] = strideVertIdx+vertOrUVIdx0;
			nonSharedTris[strideTriIdx+vertOrUVIdx1] = strideVertIdx+vertOrUVIdx1;
			nonSharedTris[strideTriIdx+vertOrUVIdx2] = strideVertIdx+vertOrUVIdx2;
			
		}
        for (int i = 0; i < nonSharedVertices.Length; i++) { nonSharedVertices[i] = nonSharedVertices[i] * Settings.SceneScaling; }
        Mesh mesh  = new Mesh();
		mesh.Clear();
		mesh.vertices = nonSharedVertices;
		
		mesh.uv = nonSharedUVs;
		mesh.triangles = nonSharedTris;
		MeshModifier.VertexWeild(mesh);
#if UNITY_EDITOR
		Vector4[] tangents = new Vector4[mesh.vertices.Length];
		computeTangentsAndBinormals(mesh.vertices,mesh.normals,mesh.uv, mesh.triangles ,tangents);
		mesh.tangents = tangents;
		tangents = null;
#endif
		//free some memory
		nonSharedVertices = null;
		nonSharedUVs = null;
		nonSharedTris = null;
	

		return mesh;
	}
	
	//-----------------------------------------------------------------------------
	// Name: computeTangentVector()
	// Desc: To find a tangent that heads in the direction of +tv, find
	//       the components of both vectors on the tangent surface, and add a 
	//       linear combination of the two projections that head in the +tv 
	//       direction
	//-----------------------------------------------------------------------------
	
	static public Vector4 computeTangentVector(int vidx0 ,int vidx1,int vidx2,Vector3[] verts, Vector3[] norms, Vector2[] uvs)
	{
		Vector3 pVtxA = verts[vidx0];
		Vector3 pVtxB = verts[vidx1];
		Vector3 pVtxC = verts[vidx2];
		
		Vector2 pVtxA_t = uvs[vidx0];
		Vector2 pVtxB_t = uvs[vidx1];
		Vector2 pVtxC_t = uvs[vidx2];
	
		Vector3 vAB = pVtxB - pVtxA;
		Vector3 vAC = pVtxC - pVtxA;
		Vector3 nA = norms[vidx0];
		
		// Components [of vectors to neighboring vertices] that are orthogonal to the
    	// vertex normal[in case normal is not absulutely orthogonal to surface]
    	
    	//where nA is global up axis in surface cordinate system
    	//vAB,vAc any vector relative to suface cordinate system
    	//components of vAB, vAC is Axis alinged. that means they arw orthogonal nA, beacuse nA is third axis
    	 
    	 // Gram-Schmidt orthogonalize
    	 //result = t - n * n.Dot(t); 
    	 
		Vector3 vProjAB = vAB - ( Vector3.Dot( nA, vAB ) * nA ); 
    	Vector3 vProjAC = vAC - ( Vector3.Dot( nA, vAC ) * nA );
    	
    	// tu texture coordinate change  along suface cordinate system's x axis
    	float duAB = pVtxB_t.x - pVtxA_t.x;
    	float duAC = pVtxC_t.x - pVtxA_t.x;

		// tu texture coordinate change  along suface cordinate system's y axis
    	float dvAB = pVtxB_t.y - pVtxA_t.y;
    	float dvAC = pVtxC_t.y - pVtxA_t.y;
    	
    	
    	//calculate handness to determine filp
    	if( (duAC * dvAB) > (duAB * dvAC) )
    	{
       		duAC = -duAC;
        	duAB = -duAB;
    	}
    
   		Vector3 vTangent = (duAC * vProjAB) - (duAB*vProjAC);
    	vTangent.Normalize();
    	
    	Vector4 retval = Vector4.zero;
    	retval.x = vTangent.x;
    	retval.y = vTangent.y;
    	retval.z = vTangent.z;
    	retval.w = 1;
    
		return retval;
	}
	
	
	//-----------------------------------------------------------------------------
	// Name: computeTangentsAndBinormals
	// Desc: For each vertex, create a tangent vector and binormal
	//-----------------------------------------------------------------------------
	static public void computeTangentsAndBinormals(Vector3[] verts,Vector3[] norms,Vector2[] uvs, int[] tris ,Vector4[] vTangents)
	{	
		int nNumIndices = tris.Length;
    	
   	    //const int nNumIndices = 6;
	   //int indices[nNumIndices] = { 0,1,2,  2,3,1 };  //triangles

    	//
		// For every triangle or face, use the indices to find the vertices 
		// that make it up. Then, compute the tangent vector for each one,
		// averaging whenever a vertex happens to be shared amongst triangles.
   		//
    	for( int i = 0; i < nNumIndices; i += 3 )
    	{
			int a = tris[i+0];
        	int b = tris[i+1];
        	int c = tris[i+2];
        
        	// We use += because we want to average the tangent vectors with 
        	// neighboring triangles that share vertices.
			vTangents[a] += computeTangentVector(a,b,c,verts, norms, uvs);
			vTangents[b] += computeTangentVector(b,a,c,verts, norms, uvs);
			vTangents[c] += computeTangentVector(c,a,b,verts, norms, uvs);
		}

   	   //
       // Normalize each tangent vector and create a binormal to pair with it...
       //

    	for(int i = 0; i < verts.Length; ++i )
   		{
        	vTangents[i].Normalize();
        	
        	//D3DXVec3Normalize( &vTangents[i], &vTangents[i] );
        	//D3DXVec3Cross( &g_vBiNormals[i], &D3DXVECTOR3( pVertices[i].nx, pVertices[i].ny, pVertices[i].nz ),&g_vTangents[i] );
   		}
   		 //g_pVertexBuffer->Unlock();
   		 	
	}
	
	static public GameObject CreateZone(string name)
	{
		GameObject retval = null;
		retval = new GameObject(name);
		MeshFilter mf = retval.AddComponent<MeshFilter>();
		Mesh m = mf.mesh;
		m.vertices = new Vector3[]{ new Vector3(0,0,0) , new Vector3(1,1,1), new Vector3(0,0,0), new Vector3(1,0,1) };
        for (int i = 0; i < m.vertices.Length; i++) { m.vertices[i] = m.vertices[i] * Settings.SceneScaling; }
        m.triangles =new int[]{0,1,2};
		BoxCollider collider = retval.AddComponent<BoxCollider>();
		collider.isTrigger = true;
		retval.layer = UnityLayer.Player;
		return retval;
	}
}
