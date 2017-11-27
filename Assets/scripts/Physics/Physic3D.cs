using UnityEngine;
using System.Collections;



public class Physic3D {
	public float g = 9.8f; //gravitational acceleration
	float Tymax = 0.0f; //Time will be taken for maximum vertical displacement
	float Txmax = 0.0f; //Time will be taken for maximum horizontal displacement
	float Hv = 0.0f; //Initial horizontal Velocity
	float Vv = 0.0f; //Initial vertical Velocity
	float Vd = 0.0f; //Maximum vertical displacement
	float Hd = 0.0f; //Maximum horizontal displacement
	Quaternion rotation ; //Current world space rotation of parent transform
	Vector3 Start = Vector3.zero; //Jump start position
	bool HasValidCurve = false;
	float FreefallSpeed = 1f;
    public delegate void DidReachedMaxHeight();
    public event DidReachedMaxHeight OnReachedMaximumHeight;
    bool ReachedMaximumHeight = false;
    public Physic3D(Vector3 startpos)
	{
		Start = startpos;
	}

	/*UpdateJump returns world space jump position.Call it in Unity Update()
	input:
	deltatime: difference between current time ( actually, Time.time in Unity) and jump start time
	*/

	public Vector3 UpdateJump(float deltatime)
	{
		if(!HasValidCurve) return Start ;

		float dx = deltatime * Hv ;
		float dy = Vv * deltatime - (0.5f * g * deltatime * deltatime);
		Vector3 posvec = new Vector3(0.0f,dy,dx);

        if(dy > (Vd * 0.8f))
        {
          
            if(OnReachedMaximumHeight != null && !ReachedMaximumHeight)
            {
                OnReachedMaximumHeight();
                ReachedMaximumHeight = true;
            }
        }
		//Debug.Log("Physic3D: Jump ");
		return Start + rotation * posvec;
  	}		
	

	/*CalculateCurve Calculates initial vertical velocity Vv and horizontal velocity Hv. Call this when starting to
	jump.
	Inputs:
	From: Jump start position
	To: Direction vector of jump
	rot: Current rotation of transform
	*/

	public bool CalculateCurve(Vector3 From, Vector3 To, Quaternion rot , float sign)
	{
		/*if(rot == null)
		{
			HasValidCurve = false;
			return false;
		}*/
		rotation = rot;
		Vector3 Forward = To ;
		float distance = new Vector3(Forward.x,0.0f, Forward.z).magnitude;
		Hd = sign * distance;
		Vd = Mathf.Abs(Forward.y);

		//I want maximum initial velocity Vv, that will continue to dominate at maximum height Vd
		Vv = 2 * g * Vd;
		Vv = Mathf.Sqrt(Vv);

		if(Vv !=0.0f)
		{
			Tymax = Vv / g;
			Txmax = 2.0f * Tymax; //maximum horizontal displacement time is twice of Tymax
			Hv = Hd / Txmax ;
			Start = From;
			HasValidCurve = true;
		}
		else	
		{		
			HasValidCurve = false;
			
		}

        ReachedMaximumHeight = false;


        return HasValidCurve;
		
	}
	
	public static bool RayHit( Vector3 l1, Vector3 l2, Vector3 pv1, Vector3 pv2, Vector3 pv3, ref Vector3 hitp )
	{
		Vector3 vintersect = Vector3.zero;
		Vector3 vnorm = Vector3.Cross(( pv2 - pv1 ),( pv3 - pv1 ));
		vnorm.Normalize();
		float fDst1 = Vector3.Dot(l1-pv1,vnorm );
		float fDst2 = Vector3.Dot(l2-pv1,vnorm );
		
		if ( (fDst1 * fDst2) >= 0.0f) return false; 
		if ( fDst1 == fDst2) 
		{
			return false;
		} 
		vintersect = l1 + (l2-l1) * ( -fDst1/(fDst2-fDst1) );
		Vector3 vtest = Vector3.Cross( vnorm,( pv2-pv1 ));
		if (Vector3.Dot(vtest, vintersect-pv1 ) < 0.0f ) return false;
		
		vtest = Vector3.Cross( vnorm,( pv3-pv2 ));
		if (Vector3.Dot(vtest, vintersect-pv2 ) < 0.0f ) return false;
		
		vtest = Vector3.Cross( vnorm,( pv1-pv3 ));
		if (Vector3.Dot(vtest, vintersect-pv1 ) < 0.0f ) return false;
		hitp = vintersect;
		return true;
	}


	public static  bool RayCast(Mesh mesh , Vector3 raystart, Vector3 raydir, float raylength, ref Vector3 hitpoint)
	{
		int[] tris = mesh.triangles;
		Vector3[] vertices =  mesh.vertices;
		int numtris = tris.Length / 3;
		
		//ector3.forward,MaxExtent
		Vector3 rayend = raystart + raydir * raylength;
		float hitdistance = Mathf.Infinity;
		int faceid = -1;
		//Vector3 hitpoint = Vector3.zero;
		
		for(int i = 0; i < numtris; i++)
		{
			Vector3 v0 = vertices[tris[i * 3 + 0]];
			Vector3 v1 = vertices[tris[i * 3 + 1]];
			Vector3 v2 = vertices[tris[i * 3 + 2]];
			Vector3 hit = Vector3.zero;
			if(RayHit(raystart,rayend,v0,v1,v2,ref hit))
			{
				float dist = (hit - raystart).sqrMagnitude;
				if(dist < hitdistance)
				{
					hitdistance = dist;
					faceid = i;
					hitpoint = hit;
				}
			}
				
		}
		
		if(faceid != -1)
		{
			return true;
		}
		else
		{
			return false;
		}
		
	}

	public static  int RayCast(Transform target, Vector3[] vertices, int[] tris , Vector3 raystart, Vector3 raydir, float raylength, ref Vector3 hitpoint)
	{
		int numtris = tris.Length / 3;
		//ector3.forward,MaxExtent
		Vector3 rayend = raystart + raydir * raylength;
		float hitdistance = Mathf.Infinity;
		int faceid = -1;
		//Vector3 hitpoint = Vector3.zero;
		
		for(int i = 0; i < numtris; i++)
		{
			Vector3 v0 = target.TransformPoint( vertices[tris[i * 3 + 0]]);
			Vector3 v1 = target.TransformPoint( vertices[tris[i * 3 + 1]]);
			Vector3 v2 = target.TransformPoint( vertices[tris[i * 3 + 2]]);
			Vector3 hit = Vector3.zero;
			if(RayHit(raystart,rayend,v0,v1,v2,ref hit))
			{
				float dist = (hit - raystart).sqrMagnitude;
				if(dist < hitdistance)
				{
					hitdistance = dist;
					faceid = i;
					hitpoint = hit;
				}
			}
			
		}
		
		return faceid;

	}


	public void StartFreeFall(Vector3 position)
	{
		Start = position;
	}

	public Vector3 UpdateFreeFall(float deltatime)
	{
		float dy = - (0.5f* g * deltatime * deltatime) * FreefallSpeed;
		Vector3 posvec = new Vector3(0.0f,dy,0);
		return Start + posvec;
	}

	static void AnalyzeSurface(Mesh mesh)
	{
		//SurfaceAnalyzer.Analyze(mesh.vertices,mesh.triangles,null);
	}

    public void SetFreeFallSpeed(float speed)
    {
        FreefallSpeed = speed;
    }
}
















