using UnityEngine;
using System.Collections;

public class ObjectExt: MonoBehaviour  {

	public Parser.Tr2Item m_Tr2Item;
	public RoomEx m_Room = null;
	
	protected Vector3 m_PrevPlayPos;
	protected Transform m_Transform;
	
	public SwimmingState m_SwimState = SwimmingState.None;

}
