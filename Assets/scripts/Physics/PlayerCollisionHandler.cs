using UnityEngine;
using System.Collections;

public class PlayerCollisionHandler : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
		BoxCollider collider = gameObject.AddComponent<BoxCollider>();
		collider.size = new Vector3(256, 512, 256);
		collider.center =  new Vector3(0, 512, 0);
		collider.isTrigger = false;
		
		Rigidbody rb = gameObject.AddComponent<Rigidbody>();
		rb.isKinematic = true;
		rb.useGravity = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
