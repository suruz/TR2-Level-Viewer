using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public class LevelTR4:Level
{
    public  Material m_SharedMaterialObject;

    public LevelTR4(Parser.Tr2Level leveldata, Material sharedmaterial, Transform roottransform) : base(leveldata, sharedmaterial, roottransform)
    {
        m_SharedMaterialObject = new Material(sharedmaterial);
    }

   override public GameObject CreateObject(Mesh mesh, Vector3 position, Quaternion rotation, string name)
    {
        Debug.Log("LevelTR4: overriding CreateObject");
        GameObject go = new GameObject(name);
        Renderer renderer = go.AddComponent<MeshRenderer>();
        MeshFilter mf = go.AddComponent<MeshFilter>();
        mf.mesh = mesh;

        go.transform.position = position;
        go.transform.rotation = rotation;
        renderer.sharedMaterial = m_SharedMaterialObject;

        //renderer.material.mainTexture = m_LevelTextureTile;
       // renderer.material.color = new Color(1f, 1f, 1f, 1.0f);
        return go;
    }

}
