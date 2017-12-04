using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterEffect : MonoBehaviour {

	// Use this for initialization
    static Material _Material;
    bool s_InsideWater = false;
    public int textureSize = 256;
    private RenderTexture m_RefractionTexture;
    private int m_OldRefractionTextureSize;
    private Camera m_RefractionCamera;
    private GameObject m_RefracionRender;

    public float clipPlaneOffset = 0.07f;


    void Start ()
    {
		GetWaterMaterial(); //load water material only once!
    }
	
	// Update is called once per frame
	void Update ()
    {
        int uvswitch = (int)Mathf.PingPong(Time.time, 1.99f);
        if(_Material!=null)
        {
            _Material.SetFloat("_UVToggle", uvswitch);
        }
 
	}

    public static Material GetWaterMaterial()
    {
		if(_Material==null)
		{
			_Material = Resources.Load("water", typeof(Material)) as Material;
		}
		
        return _Material;
    }

    // This is called when it's known that the object will be rendered by some
    // camera. We render reflections / refractions and do other updates here.
    // Because the script executes in edit mode, reflections for the scene view
    // camera will just work!
    public void _OnWillRenderObject()
    {

        return;// This is experimental effect
        
        
        /* if (!enabled || !GetComponent<Renderer>() || !GetComponent<Renderer>().sharedMaterial || !GetComponent<Renderer>().enabled)
        {
            return;
        }*/

        Camera cam = Camera.current;
        if (cam == null)
        {
            Debug.Log("Current Camera Null");
            return;
        }
  

        if(m_RefractionCamera == null)
        {
            Debug.Log("OnWill Render Refraction");

            m_RefracionRender =  new GameObject("Water Refr Camera id" + GetInstanceID() + " for " + cam.GetInstanceID());
            //m_RefracionRender.hideFlags = HideFlags.HideAndDontSave;
            m_RefracionRender.transform.parent = transform;


            m_RefractionCamera = m_RefracionRender.AddComponent<Camera>();
            m_RefractionCamera.CopyFrom(cam);
            m_RefractionCamera.enabled = false;
        }

        // Safeguard from recursive water reflections.
        if (s_InsideWater)
        {
            return;
        }
        s_InsideWater = true;

        // find out the reflection plane: position and normal in world space
        Vector3 pos = transform.position;
        Vector3 normal = transform.up;

        CreateWaterObjects(cam);


        Vector4 clipPlane = CameraSpacePlane(m_RefractionCamera, pos, normal, -1.0f);
		
#if UNITY_5_3_OR_NEWER
        m_RefractionCamera.projectionMatrix = cam.CalculateObliqueMatrix(clipPlane);
		 // Set custom culling matrix from the current camera
        m_RefractionCamera.cullingMatrix = cam.projectionMatrix * cam.worldToCameraMatrix;

#else
	//TODO: Calculate Oblique Matrix
#endif
		
       

        m_RefractionCamera.targetTexture = m_RefractionTexture;
        m_RefractionCamera.Render();
        _Material.SetTexture("_MainTex2", m_RefractionTexture);

        s_InsideWater = false;
    }

    // On-demand create any objects we need for water
    void CreateWaterObjects(Camera currentCamera)
    {
        // Refraction render texture
        if (!m_RefractionTexture || m_OldRefractionTextureSize != textureSize)
        {
            if (m_RefractionTexture)
            {
                DestroyImmediate(m_RefractionTexture);
            }
            m_RefractionTexture = new RenderTexture(textureSize, textureSize, 16);
            m_RefractionTexture.name = "__WaterRefraction" + GetInstanceID();
            m_RefractionTexture.isPowerOfTwo = true;
            m_RefractionTexture.hideFlags = HideFlags.DontSave;
            m_OldRefractionTextureSize = textureSize;
        }

        // Camera for refraction
        if (!m_RefractionCamera) // catch both not-in-dictionary and in-dictionary-but-deleted-GO
        {
            m_RefractionCamera.enabled = false;
            m_RefractionCamera.transform.position = transform.position;
            m_RefractionCamera.transform.rotation = transform.rotation;
        }

    }

    // Given position/normal of the plane, calculates plane in camera space.
    Vector4 CameraSpacePlane(Camera cam, Vector3 pos, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = pos + normal * clipPlaneOffset;
        Matrix4x4 m = cam.worldToCameraMatrix;
        Vector3 cpos = m.MultiplyPoint(offsetPos);
        Vector3 cnormal = m.MultiplyVector(normal).normalized * sideSign;
        return new Vector4(cnormal.x, cnormal.y, cnormal.z, -Vector3.Dot(cpos, cnormal));
    }

}
