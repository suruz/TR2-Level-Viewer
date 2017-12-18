using UnityEngine;
using System.Collections;

public class MeshBuilder
{

    public static Mesh CreateRoomMesh(Parser.Tr2Room tr2room, Parser.Tr2Level leveldata, ref bool has_water)
    {
        Vector3[] sharedVertices = null;
        has_water = false;
        byte[] is_water_vertex = null;
        bool has_water_face = false;
        if (tr2room.RoomData.NumVertices > 0)
        {
            int LightMode = tr2room.LightMode;
            int NumVertices = tr2room.RoomData.NumVertices; // optimized for field access
            Parser.Tr2VertexRoom[] Vertices = tr2room.RoomData.Vertices;
            sharedVertices = new Vector3[NumVertices];
            is_water_vertex = new byte[sharedVertices.Length];
            for (int vertAttribCount = 0; vertAttribCount < NumVertices; vertAttribCount++)
            {
                float x = Vertices[vertAttribCount].Vertex.x;
                float y = Vertices[vertAttribCount].Vertex.y;
                float z = Vertices[vertAttribCount].Vertex.z;

                ////print("chk vtx x y z:" +x+ " " +y + " " +z);

                sharedVertices[vertAttribCount].x = x;
                sharedVertices[vertAttribCount].y = -y;
                sharedVertices[vertAttribCount].z = z;
                is_water_vertex[vertAttribCount] = 10;
                if ((Vertices[vertAttribCount].Attributes & 0x8000L) == 0x8000L)
                {
                    is_water_vertex[vertAttribCount] = 1;
                    has_water = true;
                }
            }
       

            //warning: avariable lengh array in a structure can cause access violence


            //if(tr2room.RoomData.NumRectangles > 0)
            //{

            //selected_texObjectIdx = tr2room.RoomData.Rectangles[0].Texture;
            //selected_texObj =  leveldata.ObjectTextures[selected_texObjectIdx];
            //selected_texTileIdx = selected_texObj.Tile;
            int NumRectangles = tr2room.RoomData.NumRectangles; // optimized for field access
            int strideVertIdx = (NumRectangles * 4);
            int strideTriIdx = (NumRectangles * 3 * 2);

            int numNonsharedVertices = strideVertIdx + (tr2room.RoomData.NumTriangles * 3);
            int numNonsharedTris = strideTriIdx + (tr2room.RoomData.NumTriangles * 3);

            Vector3[] nonSharedVertices = new Vector3[numNonsharedVertices];
            Vector2[] nonSharedUVs = new Vector2[numNonsharedVertices];
            Vector2[] nonSharedUV2s = new Vector2[numNonsharedVertices];
            Color[] nonSharedColor = new Color[numNonsharedVertices];

            int[] nonSharedTris = new int[numNonsharedTris];

            //triangles = new int[num_rectangles * 3 * 2];
            Parser.Tr2Face4[] Rectangles = tr2room.RoomData.Rectangles; // optimized for field access
            for (int rectCount = 0; rectCount < NumRectangles; rectCount++)
            {

                int Idx0 = Rectangles[rectCount].Vertices0;
                int Idx1 = Rectangles[rectCount].Vertices1;
                int Idx2 = Rectangles[rectCount].Vertices2;
                int Idx3 = Rectangles[rectCount].Vertices3;

                ////print ("idx0 - Idx1 - Idx2 - Idx3:" + Idx0 + " " + Idx1 + " " + Idx2 +" " + Idx3);


                int vertOrUVIdx0 = rectCount * 4 + 0;
                int vertOrUVIdx1 = rectCount * 4 + 1;
                int vertOrUVIdx2 = rectCount * 4 + 2;
                int vertOrUVIdx3 = rectCount * 4 + 3;

                if (has_water)
                {
                    if (IsFaceInWater(is_water_vertex, Idx0, Idx1, Idx2, Idx3)) //if all vertices are in water
                    {
                        continue;
                    }
                }
                else
                {
                    if (IsFaceInWater(leveldata, Rectangles[rectCount].Texture))
                    {
                        has_water_face = true;
                        continue;
                    }
                }

                nonSharedVertices[vertOrUVIdx0] = sharedVertices[Idx0];
                nonSharedVertices[vertOrUVIdx1] = sharedVertices[Idx1];
                nonSharedVertices[vertOrUVIdx2] = sharedVertices[Idx2];
                nonSharedVertices[vertOrUVIdx3] = sharedVertices[Idx3];

                if (has_water)
                {
                    //Added vertex color for lighting effect
                    //Debug.Log("Light Atrrib0" + (Vertices[Idx0].Attributes & 0x1f));
                    //Debug.Log("Light Atrrib1" + (Vertices[Idx1].Attributes & 0x1f));
                    //Debug.Log("Light Atrrib2" + (Vertices[Idx2].Attributes & 0x1f));
                }

                //Added vertex color for lighting effect
                nonSharedColor[vertOrUVIdx0] = Color.white * (1 - Vertices[Idx0].Lighting2 * 1.220852154804053e-4f);// ((0.5f - ((float)(Vertices[Idx0].Attributes & 0x1f) / 32f)) + 0.5f);
                nonSharedColor[vertOrUVIdx1] = Color.white * (1 - Vertices[Idx1].Lighting2 * 1.220852154804053e-4f);// ((0.5f - ((float)(Vertices[Idx1].Attributes & 0x1f) / 32f)) + 0.5f);
                nonSharedColor[vertOrUVIdx2] = Color.white * (1 - Vertices[Idx2].Lighting2 * 1.220852154804053e-4f);// ((0.5f - ((float)(Vertices[Idx2].Attributes & 0x1f) / 32f)) + 0.5f);
                nonSharedColor[vertOrUVIdx3] = Color.white * (1 - Vertices[Idx3].Lighting2 * 1.220852154804053e-4f);// ((0.5f - ((float)(Vertices[Idx3].Attributes & 0x1f) / 32f)) + 0.5f);

                ushort texObjectIdx = Rectangles[rectCount].Texture;
                if (texObjectIdx >= leveldata.ObjectTextures.Length) continue; //fixed:  outof bound exception for Parser.Tr2Level.ObjectTextures
                Parser.Tr2ObjectTexture texObj = leveldata.ObjectTextures[texObjectIdx];
                ushort texTileIdx = texObj.Tile;  //bind this textile in material?

                //if(texTileIdx != prevTexture)
                //{
                //newMatCount +=1;
                //prevTexture = texTileIdx;
                ////print("newMatCount:"+ newMatCount);
                //}

                SetFaceUVs(nonSharedUVs, vertOrUVIdx0, vertOrUVIdx1, vertOrUVIdx2, vertOrUVIdx3, texObj);

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

            Parser.Tr2Face3[] Triangles = tr2room.RoomData.Triangles; // optimized for field access
            int NumTriangles = tr2room.RoomData.NumTriangles;
            for (int triCount = 0; triCount < NumTriangles; triCount++)
            {

                ////print("tr2room.RoomData.NumTriangles"+ tr2room.RoomData.NumTriangles);

                int Idx0 = Triangles[triCount].Vertices0;
                int Idx1 = Triangles[triCount].Vertices1;
                int Idx2 = Triangles[triCount].Vertices2;

                ////print ("idx0 - Idx1 - Idx2:" + Idx0 + " " + Idx1 + " " + Idx2);
                //[][][][]+[][][]
                int vertOrUVIdx0 = triCount * 3 + 0;
                int vertOrUVIdx1 = triCount * 3 + 1;
                int vertOrUVIdx2 = triCount * 3 + 2;

                if (has_water)
                {
                    if (IsFaceInWater(is_water_vertex, Idx0, Idx1, Idx2)) //if all vertices are in water
                    {
                        continue;
                    }
                }
                else
                {
                    if (IsFaceInWater(leveldata, Triangles[triCount].Texture))
                    {
                        has_water_face = true;
                        continue;
                    }
                }

                nonSharedVertices[strideVertIdx + vertOrUVIdx0] = sharedVertices[Idx0];
                nonSharedVertices[strideVertIdx + vertOrUVIdx1] = sharedVertices[Idx1];
                nonSharedVertices[strideVertIdx + vertOrUVIdx2] = sharedVertices[Idx2];

                if (has_water)
                {
                    //Added vertex color for lighting effect
                    //Debug.Log("Light Atrrib0" + (Vertices[Idx0].Attributes & 0x1f));
                    //Debug.Log("Light Atrrib1" + (Vertices[Idx1].Attributes & 0x1f));
                    //Debug.Log("Light Atrrib2" + (Vertices[Idx2].Attributes & 0x1f));
                }

                nonSharedColor[strideVertIdx + vertOrUVIdx0] = Color.white * (1 - Vertices[Idx0].Lighting2 * 1.220852154804053e-4f);////Color.white * ((0.5f - ((float)(Vertices[Idx0].Attributes & 0x1f) / 32f)) + 0.5f);
                nonSharedColor[strideVertIdx + vertOrUVIdx1] = Color.white * (1 - Vertices[Idx1].Lighting2 * 1.220852154804053e-4f);////Color.white * ((0.5f - ((float)(Vertices[Idx1].Attributes & 0x1f) / 32f)) + 0.5f);
                nonSharedColor[strideVertIdx + vertOrUVIdx2] = Color.white * (1 - Vertices[Idx2].Lighting2 * 1.220852154804053e-4f);////Color.white * ((0.5f - ((float)(Vertices[Idx2].Attributes & 0x1f) / 32f)) + 0.5f);


                ushort texObjectIdx = Triangles[triCount].Texture;
                if (texObjectIdx >= leveldata.ObjectTextures.Length) continue; //fixed:  outof bound exception for Parser.Tr2Level.ObjectTextures
                Parser.Tr2ObjectTexture texObj = leveldata.ObjectTextures[texObjectIdx];

                //if(texTileIdx != prevTexture)
                //{
                //newMatCount +=1;
                //prevTexture = texTileIdx;
                ////print("newMatCount:"+ newMatCount);
                //}

                SetFaceUVs(nonSharedUVs, strideVertIdx + vertOrUVIdx0, strideVertIdx + vertOrUVIdx1, strideVertIdx + vertOrUVIdx2, texObj);


                ////print("uv[Idx0]"+ uv[Idx0].x + " " + uv[Idx0].y);
                ////print("uv[Idx1]"+ uv[Idx1].x + " " + uv[Idx1].y);

                //ushort opacity = texObj.TransparencyFlags;  //isItOpacq

                nonSharedTris[strideTriIdx + vertOrUVIdx0] = strideVertIdx + vertOrUVIdx0;
                nonSharedTris[strideTriIdx + vertOrUVIdx1] = strideVertIdx + vertOrUVIdx1;
                nonSharedTris[strideTriIdx + vertOrUVIdx2] = strideVertIdx + vertOrUVIdx2;

                ////print ("idx0 - Idx1 - Idx2:" + nonSharedTris[strideTriIdx + vertOrUVIdx0]  + " " + 	nonSharedTris[strideTriIdx + vertOrUVIdx1]  + " " + nonSharedTris[strideTriIdx + vertOrUVIdx2] );
            }
            if(has_water_face) { has_water = true; }
            ////print("leveldata.Rooms[5].RoomData.NumRectangles:"+ tr2room.RoomData.NumRectangles);
            //SetTriangles (triangles : int[], submesh : int) : void
            //generate secondary uv set

            for (int i = 0; i < nonSharedVertices.Length; i++) { nonSharedVertices[i] = nonSharedVertices[i] * Settings.SceneScaling; }
            Mesh mesh = new Mesh();
            mesh.Clear();
            mesh.vertices = nonSharedVertices;
            mesh.uv = nonSharedUVs;
            mesh.uv2 = nonSharedUVs;
            mesh.colors = nonSharedColor;
            mesh.triangles = nonSharedTris;
            //mesh.Optimize();
            mesh.RecalculateNormals();
#if UNITY_EDITOR
            Vector4[] tangents = new Vector4[mesh.vertices.Length];
            computeTangentsAndBinormals(nonSharedVertices, mesh.normals, nonSharedUVs, nonSharedTris, tangents);
            mesh.tangents = tangents;
            tangents = null;
#endif
            //free some memory
            nonSharedVertices = null;
            nonSharedUVs = null;
            nonSharedUV2s = null;
            nonSharedTris = null;
            nonSharedColor = null;



            //}

            return mesh;
        }

        return new Mesh(); //empty mesh
    }

    public static Mesh CreateRoomWaterMesh(Parser.Tr2Room tr2room, Parser.Tr2Level leveldata)
    {
        Vector3[] sharedVertices = null;
        byte[] is_water_vertex = null;
        bool has_water = false;
        if (tr2room.RoomData.NumVertices > 0)
        {
            int NumVertices = tr2room.RoomData.NumVertices;
            sharedVertices = new Vector3[NumVertices];
            is_water_vertex = new byte[sharedVertices.Length];
            Parser.Tr2VertexRoom[] Vertices = tr2room.RoomData.Vertices;
            for (int vertAttribCount = 0; vertAttribCount < NumVertices; vertAttribCount++)
            {
                float x = Vertices[vertAttribCount].Vertex.x;
                float y = Vertices[vertAttribCount].Vertex.y;
                float z = Vertices[vertAttribCount].Vertex.z;

                ////print("chk vtx x y z:" +x+ " " +y + " " +z);

                sharedVertices[vertAttribCount].x = x;
                sharedVertices[vertAttribCount].y = -y;
                sharedVertices[vertAttribCount].z = z;

                is_water_vertex[vertAttribCount] = 10;
                if ((Vertices[vertAttribCount].Attributes & 0x8000L) == 0x8000L)
                {
                    is_water_vertex[vertAttribCount] = 1;
                    has_water = true;
                }

            }
        }

        //warning: avariable lengh array in a structure can cause access violence


        //if(tr2room.RoomData.NumRectangles > 0)
        //{

        //selected_texObjectIdx = tr2room.RoomData.Rectangles[0].Texture;
        //selected_texObj =  leveldata.ObjectTextures[selected_texObjectIdx];
        //selected_texTileIdx = selected_texObj.Tile;
        int NumRectangles = tr2room.RoomData.NumRectangles;
        int strideVertIdx = (NumRectangles * 4);
        int strideTriIdx = (NumRectangles * 3 * 2);

        int numNonsharedVertices = strideVertIdx + (tr2room.RoomData.NumTriangles * 3);
        int numNonsharedTris = strideTriIdx + (tr2room.RoomData.NumTriangles * 3);

        Vector3[] nonSharedVertices = new Vector3[numNonsharedVertices];
        Vector2[] nonSharedUVs = new Vector2[numNonsharedVertices];
        Vector2[] nonSharedUV2s = new Vector2[numNonsharedVertices];
        int[] nonSharedTris = new int[numNonsharedTris];

        Parser.Tr2Face4[] Rectangles = tr2room.RoomData.Rectangles;

        for (int rectCount = 0; rectCount < NumRectangles; rectCount++)
        {

            int Idx0 = Rectangles[rectCount].Vertices0;
            int Idx1 = Rectangles[rectCount].Vertices1;
            int Idx2 = Rectangles[rectCount].Vertices2;
            int Idx3 = Rectangles[rectCount].Vertices3;

            ////print ("idx0 - Idx1 - Idx2 - Idx3:" + Idx0 + " " + Idx1 + " " + Idx2 +" " + Idx3);


            int vertOrUVIdx0 = rectCount * 4 + 0;
            int vertOrUVIdx1 = rectCount * 4 + 1;
            int vertOrUVIdx2 = rectCount * 4 + 2;
            int vertOrUVIdx3 = rectCount * 4 + 3;

            if (has_water)
            {
                if (!IsFaceInWater(is_water_vertex, Idx0, Idx1, Idx2, Idx3)) //if not all vertices are in water
                {
                    continue;
                }
            }
            else
            {
                if (!IsFaceInWater(leveldata, Rectangles[rectCount].Texture))
                {
                    continue;
                }
            }

            nonSharedVertices[vertOrUVIdx0] = sharedVertices[Idx0];
            nonSharedVertices[vertOrUVIdx1] = sharedVertices[Idx1];
            nonSharedVertices[vertOrUVIdx2] = sharedVertices[Idx2];
            nonSharedVertices[vertOrUVIdx3] = sharedVertices[Idx3];

            ushort texObjectIdx = Rectangles[rectCount].Texture;
            if (texObjectIdx >= leveldata.ObjectTextures.Length) continue; //fixed:  outof bound exception for Parser.Tr2Level.ObjectTextures
            Parser.Tr2ObjectTexture texObj = leveldata.ObjectTextures[texObjectIdx];
            ushort texTileIdx = texObj.Tile;  //bind this textile in material?

            //if(texTileIdx != prevTexture)
            //{
            //newMatCount +=1;
            //prevTexture = texTileIdx;
            ////print("newMatCount:"+ newMatCount);
            //}

            SetFaceUVs(nonSharedUVs, vertOrUVIdx0, vertOrUVIdx1, vertOrUVIdx2, vertOrUVIdx3, texObj);

            ////print("uv[Idx0]"+ uv[Idx0].x + " " + uv[Idx0].y);
            ////print("uv[Idx1]"+ uv[Idx1].x + " " + uv[Idx1].y);


            //ushort opacity = texObj.TransparencyFlags;  //isItOpacq

            //generate secondary uv for animation by going through all animated texture group and checking existance of 
            //texObjectIdx in that group
            //leveldata.AnimatedTextures[]  // this is varable length record of animated texture group

            int texture_idx = GetAnimatedTextureIndex(leveldata.AnimatedTextures, leveldata.NumAnimatedTextures, texObjectIdx);
            if (texture_idx > -1)
            {
                texObj = leveldata.ObjectTextures[texture_idx];
                SetFaceUVs(nonSharedUV2s, vertOrUVIdx0, vertOrUVIdx1, vertOrUVIdx2, vertOrUVIdx3, texObj);
            }


            nonSharedTris[rectCount * 6 + 0] = vertOrUVIdx0;
            nonSharedTris[rectCount * 6 + 1] = vertOrUVIdx1;
            nonSharedTris[rectCount * 6 + 2] = vertOrUVIdx2;

            nonSharedTris[rectCount * 6 + 3] = vertOrUVIdx2;
            nonSharedTris[rectCount * 6 + 4] = vertOrUVIdx3;
            nonSharedTris[rectCount * 6 + 5] = vertOrUVIdx0;

        }

        int NumTriangles = tr2room.RoomData.NumTriangles;
        Parser.Tr2Face3[] Triangles = tr2room.RoomData.Triangles;
        for (int triCount = 0; triCount < tr2room.RoomData.NumTriangles; triCount++)
        {

            ////print("tr2room.RoomData.NumTriangles"+ tr2room.RoomData.NumTriangles);

            int Idx0 = Triangles[triCount].Vertices0;
            int Idx1 = Triangles[triCount].Vertices1;
            int Idx2 = Triangles[triCount].Vertices2;

            ////print ("idx0 - Idx1 - Idx2:" + Idx0 + " " + Idx1 + " " + Idx2);
            //[][][][]+[][][]
            int vertOrUVIdx0 = triCount * 3 + 0;
            int vertOrUVIdx1 = triCount * 3 + 1;
            int vertOrUVIdx2 = triCount * 3 + 2;

            if (has_water)
            {
                if (!IsFaceInWater(is_water_vertex, Idx0, Idx1, Idx2)) //if not all vertices are in water
                {
                    continue;
                }
            }
            else
            {
                if (!IsFaceInWater(leveldata, Triangles[triCount].Texture))
                {
                    continue;
                }
            }


            nonSharedVertices[strideVertIdx + vertOrUVIdx0] = sharedVertices[Idx0];
            nonSharedVertices[strideVertIdx + vertOrUVIdx1] = sharedVertices[Idx1];
            nonSharedVertices[strideVertIdx + vertOrUVIdx2] = sharedVertices[Idx2];

            ushort texObjectIdx = Triangles[triCount].Texture;
            if (texObjectIdx >= leveldata.ObjectTextures.Length) continue; //fixed:  outof bound exception for Parser.Tr2Level.ObjectTextures
            Parser.Tr2ObjectTexture texObj = leveldata.ObjectTextures[texObjectIdx];

            //if(texTileIdx != prevTexture)
            //{
            //newMatCount +=1;
            //prevTexture = texTileIdx;
            ////print("newMatCount:"+ newMatCount);
            //}

            SetFaceUVs(nonSharedUVs, strideVertIdx + vertOrUVIdx0, strideVertIdx + vertOrUVIdx1, strideVertIdx + vertOrUVIdx2, texObj);
            int texture_idx = GetAnimatedTextureIndex(leveldata.AnimatedTextures, leveldata.NumAnimatedTextures, texObjectIdx);
            if (texture_idx > -1)
            {
                texObj = leveldata.ObjectTextures[texture_idx];
                SetFaceUVs(nonSharedUV2s, strideVertIdx + vertOrUVIdx0, strideVertIdx + vertOrUVIdx1, strideVertIdx + vertOrUVIdx2, texObj);
            }

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

        for (int i = 0; i < nonSharedVertices.Length; i++) { nonSharedVertices[i] = nonSharedVertices[i] * Settings.SceneScaling; }
        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.vertices = nonSharedVertices;
        mesh.uv = nonSharedUVs;
        mesh.uv2 = nonSharedUV2s;
        mesh.triangles = nonSharedTris;
        //mesh.Optimize();
        mesh.RecalculateNormals();
#if UNITY_EDITOR
        Vector4[] tangents = new Vector4[mesh.vertices.Length];
        computeTangentsAndBinormals(nonSharedVertices, mesh.normals, nonSharedUVs, nonSharedTris, tangents);
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


    public static Mesh CreateObjectMesh(Parser.Tr2Mesh tr2mesh, Parser.Tr2Level leveldata)
    {
        Vector3[] sharedVertices = null;
        if (tr2mesh.NumVertices > 0)
        {
            sharedVertices = new Vector3[tr2mesh.NumVertices];
            for (int vertAttribCount = 0; vertAttribCount < tr2mesh.NumVertices; vertAttribCount++)
            {


                float x = tr2mesh.Vertices[vertAttribCount].x;
                float y = tr2mesh.Vertices[vertAttribCount].y;
                float z = tr2mesh.Vertices[vertAttribCount].z;

                ////print("chk vtx x y z:" +x+ " " +y + " " +z);

                sharedVertices[vertAttribCount].x = x;
                sharedVertices[vertAttribCount].y = -y;
                sharedVertices[vertAttribCount].z = z;
            }

            //uv = new Vector2[leveldata.Rooms[chkRoom].RoomData.NumVertices];
        }
        //warning: a variable lengh array in a structure can cause access violence

        //selected_texObjectIdx = leveldata.Rooms[chkRoom].RoomData.Rectangles[0].Texture;
        //selected_texObj =  leveldata.ObjectTextures[selected_texObjectIdx];
        //selected_texTileIdx = selected_texObj.Tile;
        int NumTexturedTriangles = tr2mesh.NumTexturedTriangles;
        int NumTexturedRectangles = tr2mesh.NumTexturedRectangles;
        int numNonsharedVertices = (NumTexturedRectangles * 4) + (NumTexturedTriangles * 3);
        int numNonsharedTris = (NumTexturedRectangles * 3 * 2) + (NumTexturedTriangles * 3);

        Vector3[] nonSharedVertices = new Vector3[numNonsharedVertices];
        Vector2[] nonSharedUVs = new Vector2[numNonsharedVertices];
        int[] nonSharedTris = new int[numNonsharedTris];


        //triangles = new int[leveldata.Rooms[chkRoom].RoomData.NumRectangles * 3 * 2];
        Parser.Tr2Face4[] TexturedRectangles = tr2mesh.TexturedRectangles;
        for (int rectCount = 0; rectCount < NumTexturedRectangles; rectCount++)
        {

            int Idx0 = TexturedRectangles[rectCount].Vertices0;
            int Idx1 = TexturedRectangles[rectCount].Vertices1;
            int Idx2 = TexturedRectangles[rectCount].Vertices2;
            int Idx3 = TexturedRectangles[rectCount].Vertices3;


            int vertOrUVIdx0 = rectCount * 4 + 0;
            int vertOrUVIdx1 = rectCount * 4 + 1;
            int vertOrUVIdx2 = rectCount * 4 + 2;
            int vertOrUVIdx3 = rectCount * 4 + 3;


            nonSharedVertices[vertOrUVIdx0] = sharedVertices[Idx0];
            nonSharedVertices[vertOrUVIdx1] = sharedVertices[Idx1];
            nonSharedVertices[vertOrUVIdx2] = sharedVertices[Idx2];
            nonSharedVertices[vertOrUVIdx3] = sharedVertices[Idx3];

            ushort texObjectIdx = TexturedRectangles[rectCount].Texture;
            if (texObjectIdx >= leveldata.ObjectTextures.Length) continue; //fixed:  outof bound exception for Parser.Tr2Level.ObjectTextures
            Parser.Tr2ObjectTexture texObj = leveldata.ObjectTextures[texObjectIdx];
            ushort texTileIdx = texObj.Tile;  //bind this textile in material?

            //if(texTileIdx != prevTexture)
            //{
            //newMatCount +=1;
            //prevTexture = texTileIdx;
            ////print("newMatCount:"+ newMatCount);
            //}

            SetFaceUVs(nonSharedUVs, vertOrUVIdx0, vertOrUVIdx1, vertOrUVIdx2, vertOrUVIdx3, texObj);

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

        Parser.Tr2Face3[] TexturedTriangles = tr2mesh.TexturedTriangles;
        for (int triCount = 0; triCount < NumTexturedTriangles; triCount++)
        {
            int Idx0 = TexturedTriangles[triCount].Vertices0;
            int Idx1 = TexturedTriangles[triCount].Vertices1;
            int Idx2 = TexturedTriangles[triCount].Vertices2;

            int vertOrUVIdx0 = triCount * 3 + 0;
            int vertOrUVIdx1 = triCount * 3 + 1;
            int vertOrUVIdx2 = triCount * 3 + 2;

            int strideVertIdx = (NumTexturedRectangles * 4);
            int strideTriIdx = (NumTexturedRectangles * 3 * 2);


            nonSharedVertices[strideVertIdx + vertOrUVIdx0] = sharedVertices[Idx0];
            nonSharedVertices[strideVertIdx + vertOrUVIdx1] = sharedVertices[Idx1];
            nonSharedVertices[strideVertIdx + vertOrUVIdx2] = sharedVertices[Idx2];


            ushort texObjectIdx = tr2mesh.TexturedTriangles[triCount].Texture;
            if (texObjectIdx >= leveldata.ObjectTextures.Length) continue; //fixed:  outof bound exception for Parser.Tr2Level.ObjectTextures
            Parser.Tr2ObjectTexture texObj = leveldata.ObjectTextures[texObjectIdx];


            //if(texTileIdx != prevTexture)
            //{
            //newMatCount +=1;
            //prevTexture = texTileIdx;
            ////print("newMatCount:"+ newMatCount);
            //}

            SetFaceUVs(nonSharedUVs, strideVertIdx + vertOrUVIdx0, strideVertIdx + vertOrUVIdx1, strideVertIdx + vertOrUVIdx2, texObj);


            ////print("uv[Idx0]"+ uv[Idx0].x + " " + uv[Idx0].y);
            ////print("uv[Idx1]"+ uv[Idx1].x + " " + uv[Idx1].y);

            //ushort opacity = texObj.TransparencyFlags;  //isItOpacq

            nonSharedTris[strideTriIdx + vertOrUVIdx0] = strideVertIdx + vertOrUVIdx0;
            nonSharedTris[strideTriIdx + vertOrUVIdx1] = strideVertIdx + vertOrUVIdx1;
            nonSharedTris[strideTriIdx + vertOrUVIdx2] = strideVertIdx + vertOrUVIdx2;

        }
        for (int i = 0; i < nonSharedVertices.Length; i++) { nonSharedVertices[i] = nonSharedVertices[i] * Settings.SceneScaling; }
        Mesh mesh = new Mesh();
        mesh.Clear();
        mesh.vertices = nonSharedVertices;

        mesh.uv = nonSharedUVs;
        mesh.triangles = nonSharedTris;
        MeshModifier.VertexWeild(mesh);
#if UNITY_EDITOR
        Vector4[] tangents = new Vector4[mesh.vertices.Length];
        computeTangentsAndBinormals(mesh.vertices, mesh.normals, mesh.uv, mesh.triangles, tangents);
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

    static public Vector4 computeTangentVector(int vidx0, int vidx1, int vidx2, Vector3[] verts, Vector3[] norms, Vector2[] uvs)
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

        Vector3 vProjAB = vAB - (Vector3.Dot(nA, vAB) * nA);
        Vector3 vProjAC = vAC - (Vector3.Dot(nA, vAC) * nA);

        // tu texture coordinate change  along suface cordinate system's x axis
        float duAB = pVtxB_t.x - pVtxA_t.x;
        float duAC = pVtxC_t.x - pVtxA_t.x;

        // tu texture coordinate change  along suface cordinate system's y axis
        float dvAB = pVtxB_t.y - pVtxA_t.y;
        float dvAC = pVtxC_t.y - pVtxA_t.y;


        //calculate handness to determine filp
        if ((duAC * dvAB) > (duAB * dvAC))
        {
            duAC = -duAC;
            duAB = -duAB;
        }

        Vector3 vTangent = (duAC * vProjAB) - (duAB * vProjAC);
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
    static public void computeTangentsAndBinormals(Vector3[] verts, Vector3[] norms, Vector2[] uvs, int[] tris, Vector4[] vTangents)
    {
        int nNumIndices = tris.Length;

        //const int nNumIndices = 6;
        //int indices[nNumIndices] = { 0,1,2,  2,3,1 };  //triangles

        //
        // For every triangle or face, use the indices to find the vertices 
        // that make it up. Then, compute the tangent vector for each one,
        // averaging whenever a vertex happens to be shared amongst triangles.
        //
        for (int i = 0; i < nNumIndices; i += 3)
        {
            int a = tris[i + 0];
            int b = tris[i + 1];
            int c = tris[i + 2];

            // We use += because we want to average the tangent vectors with 
            // neighboring triangles that share vertices.
            vTangents[a] += computeTangentVector(a, b, c, verts, norms, uvs);
            vTangents[b] += computeTangentVector(b, a, c, verts, norms, uvs);
            vTangents[c] += computeTangentVector(c, a, b, verts, norms, uvs);
        }

        //
        // Normalize each tangent vector and create a binormal to pair with it...
        //

        for (int i = 0; i < verts.Length; ++i)
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





        BoxCollider collider = retval.AddComponent<BoxCollider>();
        collider.center = new Vector3(0.5f * 1024, 0, 0.5f * 1024) * Settings.SceneScaling;
        collider.size = new Vector3(1024, 1024, 1024) * Settings.SceneScaling;
        collider.isTrigger = true;
        retval.layer = UnityLayer.Player;
        return retval;
    }

    static void SetFaceUVs(Vector2[] nonSharedUVs, int vertOrUVIdx0, int vertOrUVIdx1, int vertOrUVIdx2, int vertOrUVIdx3, Parser.Tr2ObjectTexture texObj)
    {
        ushort texTileIdx = (ushort)(texObj.Tile & 0x7ff);  //bind this textile in material? bugfix: use ony 15 bit
        Parser.Tr2ObjectTextureVertex[] Vertices = texObj.Vertices;

        nonSharedUVs[vertOrUVIdx0].x = (float)TextureUV.AdjustTextureCoordinateX((byte)Vertices[0].Xpixel, (sbyte)Vertices[0].Xcoordinate, texTileIdx);
        nonSharedUVs[vertOrUVIdx0].y = (float)TextureUV.AdjustTextureCoordinateY((byte)Vertices[0].Ypixel, (sbyte)Vertices[0].Ycoordinate, texTileIdx);

        nonSharedUVs[vertOrUVIdx1].x = (float)TextureUV.AdjustTextureCoordinateX((byte)Vertices[1].Xpixel, (sbyte)Vertices[1].Xcoordinate, texTileIdx);
        nonSharedUVs[vertOrUVIdx1].y = (float)TextureUV.AdjustTextureCoordinateY((byte)Vertices[1].Ypixel, (sbyte)Vertices[1].Ycoordinate, texTileIdx);

        nonSharedUVs[vertOrUVIdx2].x = (float)TextureUV.AdjustTextureCoordinateX((byte)Vertices[2].Xpixel, (sbyte)Vertices[2].Xcoordinate, texTileIdx);
        nonSharedUVs[vertOrUVIdx2].y = (float)TextureUV.AdjustTextureCoordinateY((byte)Vertices[2].Ypixel, (sbyte)Vertices[2].Ycoordinate, texTileIdx);

        nonSharedUVs[vertOrUVIdx3].x = (float)TextureUV.AdjustTextureCoordinateX((byte)Vertices[3].Xpixel, (sbyte)Vertices[3].Xcoordinate, texTileIdx);
        nonSharedUVs[vertOrUVIdx3].y = (float)TextureUV.AdjustTextureCoordinateY((byte)Vertices[3].Ypixel, (sbyte)Vertices[3].Ycoordinate, texTileIdx);
    }

    static void SetFaceUVs(Vector2[] nonSharedUVs, int vertOrUVIdx0, int vertOrUVIdx1, int vertOrUVIdx2, Parser.Tr2ObjectTexture texObj)
    {
        ushort texTileIdx = (ushort)(texObj.Tile & 0x7ff);  //bind this textile in material? 
        Parser.Tr2ObjectTextureVertex[] Vertices = texObj.Vertices;

        nonSharedUVs[vertOrUVIdx0].x = (float)TextureUV.AdjustTextureCoordinateX((byte)Vertices[0].Xpixel, (sbyte)Vertices[0].Xcoordinate, texTileIdx);
        nonSharedUVs[vertOrUVIdx0].y = (float)TextureUV.AdjustTextureCoordinateY((byte)Vertices[0].Ypixel, (sbyte)Vertices[0].Ycoordinate, texTileIdx);

        nonSharedUVs[vertOrUVIdx1].x = (float)TextureUV.AdjustTextureCoordinateX((byte)Vertices[1].Xpixel, (sbyte)Vertices[1].Xcoordinate, texTileIdx);
        nonSharedUVs[vertOrUVIdx1].y = (float)TextureUV.AdjustTextureCoordinateY((byte)Vertices[1].Ypixel, (sbyte)Vertices[1].Ycoordinate, texTileIdx);

        nonSharedUVs[vertOrUVIdx2].x = (float)TextureUV.AdjustTextureCoordinateX((byte)Vertices[2].Xpixel, (sbyte)Vertices[2].Xcoordinate, texTileIdx);
        nonSharedUVs[vertOrUVIdx2].y = (float)TextureUV.AdjustTextureCoordinateY((byte)Vertices[2].Ypixel, (sbyte)Vertices[2].Ycoordinate, texTileIdx);

    }

    static void SetFaceColors(Color[] colors, int vertOrUVIdx0, int vertOrUVIdx1, int vertOrUVIdx2, int vertOrUVIdx3, Parser.Tr2VertexRoom[] Vertices)
    {
        //set lighting effect modifier through color
        colors[vertOrUVIdx0] = new Color(0, 0, 0, 1);
        colors[vertOrUVIdx1] = new Color(0, 0, 0, 1);
        colors[vertOrUVIdx2] = new Color(0, 0, 0, 1);
        colors[vertOrUVIdx3] = new Color(0, 0, 0, 1);
    }

    static void SetFaceColors(Color[] colors, int vertOrUVIdx0, int vertOrUVIdx1, int vertOrUVIdx2, Parser.Tr2ObjectTexture texObj)
    {
        ushort texTileIdx = texObj.Tile;  //bind this textile in material?
        Parser.Tr2ObjectTextureVertex[] Vertices = texObj.Vertices;

        colors[vertOrUVIdx0] = new Color(0, 0, 0, 1);
        colors[vertOrUVIdx1] = new Color(0, 0, 0, 1);
        colors[vertOrUVIdx2] = new Color(0, 0, 0, 1);

    }

    static int GetAnimatedTextureIndex(short[] AnimatedTextures, int NumAnimatedTextures, ushort SearchIndex)
    {
        //AnimatedTextures is variable length record

        int index = 1;
        int headerindex = index;
        short numids = (short)(AnimatedTextures[1] + 1);  //origininal num is encrypted by reducing value 1

        bool animated_texture_found = false;
        int textureframeid = 0;

        //Debug.Log("header  " + numids);

        /*for(int i = 1; i < NumAnimatedTextures; i++ )
        {
            Debug.Log("rec val " + AnimatedTextures[i] + " search index " + SearchIndex);
        }

        Debug.Log("-----------------------");

        return -1;*/

        while (index < NumAnimatedTextures)
        {
            if (index > (headerindex + numids))  //set new header
            {
                headerindex = index;
                numids = (short)(AnimatedTextures[index] + 1);
                textureframeid = 0;
                //Debug.Log("header  " + numids);
            }
            else //record body
            {
                if (SearchIndex == AnimatedTextures[index]) //lookup SearchIndex in AnimatedTextures group
                {
                    animated_texture_found = true;
                    //Debug.Log("found animated texture " + textureframeid + " " + SearchIndex);
                    break;
                }
                textureframeid++;

            }

            index++;
        }

        if (animated_texture_found)
        {
            if ((numids - 1) == 0) { return SearchIndex; } //bug fixed : division by zero
            int next_animated_texture_offset = ((textureframeid + 1) % (numids - 1)) + 1;
            return AnimatedTextures[headerindex + next_animated_texture_offset];
        }

        return SearchIndex;
    }

    static bool IsFaceInWater(Parser.Tr2Level level, ushort SearchIndex)
    {
        if (GetAnimatedTextureIndex(level.AnimatedTextures, level.NumAnimatedTextures, SearchIndex) != SearchIndex)
        {
            return true;
        }

        return false;
    }

    static bool IsFaceInWater(byte[] vertex_atrributes, int id0, int id1, int id2)
    {
        if (vertex_atrributes != null)
        {
            if (vertex_atrributes[id0] == 1 && vertex_atrributes[id1] == 1 && vertex_atrributes[id2] == 1)
            {
                return true;
            }
        }

        return false;
    }

    static bool IsFaceInWater(byte[] vertex_atrributes, int id0, int id1, int id2, int id3)
    {
        if (vertex_atrributes != null)
        {
            if (vertex_atrributes[id0] == 1 && vertex_atrributes[id1] == 1 && vertex_atrributes[id2] == 1 && vertex_atrributes[id3] == 1)
            {
                return true;
            }
        }

        return false;
    }

}
