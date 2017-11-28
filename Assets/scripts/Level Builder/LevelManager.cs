using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour {

    public Material SharedMaterial;
    public Material  SharedMaterialWater;
    public List<Material> InstancedMaterialWaterHolders;


    static  Material m_SharedMaterial;
    public static Material m_SharedMaterialWater;
    public static List<Material> m_InstancedMaterialWaterHolders;

    // Use this for initialization
    void Start()
    {
        m_SharedMaterial = SharedMaterial;
        m_SharedMaterialWater = SharedMaterialWater;
        m_InstancedMaterialWaterHolders = InstancedMaterialWaterHolders;
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public static Material GetSharedMaterial()
    {
        return m_SharedMaterial;
    }


    public static void UpdateWaterHolderMaterials(int inside)
    {
        foreach (Material mat in m_InstancedMaterialWaterHolders)
        {
            mat.SetFloat("_InSideWater", inside);
        }
    }
}
