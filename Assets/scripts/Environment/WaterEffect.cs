using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterEffect : MonoBehaviour {

	// Use this for initialization
    static Material _Material;
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
}
