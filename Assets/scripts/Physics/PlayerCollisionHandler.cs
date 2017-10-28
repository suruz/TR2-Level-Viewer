using UnityEngine;
using System.Collections;

public class PlayerCollisionHandler : MonoBehaviour {
	
	static BoxCollider collider;
	// Use this for initialization
	void Start () {
	
		collider = gameObject.AddComponent<BoxCollider>();
		collider.size = new Vector3(256, 512, 256) * Settings.SceneScaling;
		collider.center =  new Vector3(0, 512, 0) * Settings.SceneScaling;
		collider.isTrigger = false;
		
		Rigidbody rb = gameObject.AddComponent<Rigidbody>();
		rb.isKinematic = true;
		rb.useGravity = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	static public void ResizeSwimmCollider()
	{
		if(collider != null)
		{
			collider.size = new Vector3(128, 128, 512) * Settings.SceneScaling;
			collider.center =  new Vector3(0, 0, 0) * Settings.SceneScaling;
		}
	}
	
	static public void ResizeNormalCollider()
	{
		if(collider != null)
		{
			collider.size = new Vector3(256, 512, 256) * Settings.SceneScaling;
		}
	}
}
