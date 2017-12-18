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
     
        if ((Application.platform == RuntimePlatform.Android) || (Application.platform == RuntimePlatform.IPhonePlayer))
        {
            if (Input.GetMouseButtonDown(0) || initmouse)
            {
                mousepos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
                prevmousepos = mousepos;
                initmouse = false;
            }
            else if (Input.GetMouseButton(0))
            {
                mousepos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            }



            if (prevmousepos != mousepos)
            {
                if (m_OnMouseMove != null) m_OnMouseMove(mousepos.x - prevmousepos.x, mousepos.y - prevmousepos.y);
            }
        }
        else
        {
            // Get the mouse delta. This is not in the range -1...1
#if UNITY_WEBGL
            if(Input.mousePosition.x < 0 || Input.mousePosition.x > Screen.width || Input.mousePosition.y < 0 || Input.mousePosition.y > Screen.height)
            {
                return;
            }
#endif


            float x = Input.GetAxis("Mouse X") * 8;
            float y = Input.GetAxis("Mouse Y");
            if (m_OnMouseMove != null) m_OnMouseMove(x, y);


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
