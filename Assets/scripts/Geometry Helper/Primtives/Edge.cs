using UnityEngine;
using System.Collections;

public class Edge  {

	public Vector3 PointA;
	public Vector3 PointB;
	public Vector3 AtoB;

	public Edge(Vector3 _PointA, Vector3 _PointB)
	{
		PointA = _PointA;
		PointB = _PointB;
	}
}
