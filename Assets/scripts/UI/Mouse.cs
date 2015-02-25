using UnityEngine;
using System.Collections;

public delegate  void MouseMoveDelegate(float dx, float dy);

public class Mouse : MonoBehaviour {

	static Vector2 mousepos = Vector2.zero;
	static Vector2 prevmousepos = Vector2.zero;
	static bool initmouse = true;
	static public event MouseMoveDelegate m_OnMouseMove;
	
	// Use this for initialization
	void Start () {
	
	}
	
	public void LateUpdate()
	{
		Reset();
	}
	
	// Update is called once per frame
	public void Update () 
	{
		// Get the mouse delta. This is not in the range -1...1
		//var h = horizontalSpeed * Input.GetAxis ("Mouse X");
		//var v = verticalSpeed * Input.GetAxis ("Mouse Y");
		
		if(Input.GetMouseButtonDown(0) || initmouse)
		{
			mousepos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
			prevmousepos = mousepos;
			initmouse = false;
		}
		else if (Input.GetMouseButton(0))
		{
			mousepos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
		}
		
		if(prevmousepos != mousepos)
		{
			if(m_OnMouseMove!=null) m_OnMouseMove(mousepos.x - prevmousepos.x, mousepos.y - prevmousepos.y);
		}

	}

	static public float GetAxisX ()
	{
		return (mousepos.x - prevmousepos.x);
	}

	static public float GetAxisY ()
	{
		return (mousepos.y - prevmousepos.y);
	}

	static public void Reset()
	{
		prevmousepos = mousepos;
	}
}
