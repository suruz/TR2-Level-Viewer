using UnityEngine;
using System.Collections;

public class DefaultStatePlayer : MonoBehaviour {

	Animation anim = null;
	int clipid = -1;

	// Use this for initialization
	void Start () 
	{
		anim = GetComponent<Animation>();
		if(anim != null && anim.GetClipCount() > 0) 
		{
			anim.wrapMode = WrapMode.Loop;
			clipid = anim.GetClipCount() - 1;
			anim.Play("" + clipid);
		}
	}
	
	void Update()
	{
		if(anim != null && clipid !=-1) 
		{
			anim.Play("" + clipid);
		}
	}
}
