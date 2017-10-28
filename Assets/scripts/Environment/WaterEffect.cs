using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterEffect : MonoBehaviour {

	// Use this for initialization
    static Material _Material;
	void Start ()
    {
        _Material = GetComponent<MeshRenderer>().sharedMaterial;
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
        return _Material;
    }
}
