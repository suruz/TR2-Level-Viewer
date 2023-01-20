using UnityEngine;
using System.Collections;

public class RaycastTest : MonoBehaviour {

	public MeshFilter mf;
	Mesh mesh;
	Vector3 hp;

	// Use this for initialization
	void Start () {
		mesh = mf.mesh;
		hp = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
	
		Physic3D.RayCast(mf.transform, mesh.vertices, mesh.triangles, transform.position, transform.forward, 10000, ref hp);
		Debug.DrawLine(transform.position,hp );
	}
}
