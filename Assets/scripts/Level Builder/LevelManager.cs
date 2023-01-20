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

    Player m_Player;
    Transform m_CameraTransform;
    SoundMananger m_SoundManager;

    bool m_bCameraInWater = false;

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
        CheckCameraInWater(); 
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

    public void SetPlayer(Player player)
    {
        m_Player = player;
    }

    public void SetFollowCamera(Transform camera)
    {
        m_CameraTransform = camera;
    }

    public void SetSoundManager(SoundMananger manager)
    {
        m_SoundManager = manager;
    }

    void CheckCameraInWater()
    {
        if (m_Player != null && m_Player.m_Room != null && m_Player.m_SwimState != SwimmingState.None)
        {

            float water_level = m_Player.m_Room.GetCenterPoint().y;
            Bounds b = m_Player.m_Room.GetBound();
            Material room_mat = LevelManager.GetSharedMaterial();

            if (b.Contains(m_CameraTransform.position) && !m_bCameraInWater )
            {
                //Debug.Log("Camera In Water");
                //if(room_mat.color != water_color)
                //{
                //room_mat.color = water_color;
                //}

                DayNightSystem.SetAmbientTint(1);
                //do following shader update on entering water

                room_mat.SetFloat("_InSideWater", 1);
                room_mat.SetFloat("_WaterPlaneY", water_level);

                LevelManager.UpdateWaterHolderMaterials(1);
                m_bCameraInWater = true;
                if (m_SoundManager != null)
                {
                    m_SoundManager.PlayUnderWaterAmbient(m_bCameraInWater);
                }


            }

            if (m_CameraTransform.position.y > water_level && m_bCameraInWater)
            {
                //if(room_mat.color != Color.white)
                //{
                //room_mat.color = Color.white;
                //}

                DayNightSystem.SetAmbientTint(0);

                //do following shader update on exiting water
                room_mat.SetFloat("_InSideWater", 0);
                LevelManager.UpdateWaterHolderMaterials(0);

                m_bCameraInWater = false;
                if (m_SoundManager != null)
                {
                    m_SoundManager.PlayUnderWaterAmbient(m_bCameraInWater);
                }
            }

         
        }
    }

    Color m_WaterColor = new Color(90.0f / 255f, 200f / 255f, 1);
    Color m_NormalColor = new Color(1, 1, 1);

}
