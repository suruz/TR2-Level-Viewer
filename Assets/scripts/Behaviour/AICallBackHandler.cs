using UnityEngine;
using System.Collections;

public class AICallBackHandler : MonoBehaviour {
	

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public static bool OnAttachingBehaviourToObject(GameObject AI, int ObjectID, GameObject Player, Parser.Tr2Item tr2item)
	{

		bool retval = false;
        //TEST: if(ObjectID == 32 || ObjectID == 15 ) return false;
 

        if (ObjectID == 15)
		{
			DogStatePlayer dog = AI.AddComponent<DogStatePlayer>();
			dog.m_FollowTransform = Player.transform;
			dog.m_Tr2Item = tr2item;
            AICondition.SetActive(AI, true);
			retval = true;
		}
		else if (ObjectID == 16 )
		{
			GoonWithMaskStatePlayer goon = AI.AddComponent<GoonWithMaskStatePlayer>();
			goon.m_FollowTransform = Player.transform;
			goon.m_Tr2Item = tr2item;
            AICondition.SetActive(AI, true);
            retval = true;

		}
		else if (ObjectID == 20 )
		{
			BartoliStatePlayer goon = AI.AddComponent<BartoliStatePlayer>();
			goon.m_FollowTransform = Player.transform;
			goon.m_Tr2Item = tr2item;
            AICondition.SetActive(AI, true);
            retval = true;

		}

		else if ( ObjectID == 31 || ObjectID == 32 )
		{
			GoonWithRoolerStatePlayer goon = AI.AddComponent<GoonWithRoolerStatePlayer>();
			goon.m_FollowTransform = Player.transform;
			goon.m_Tr2Item = tr2item;
            AICondition.SetActive(AI, true);
            retval = true;
		}
		else if( ObjectID == 34 ||  ObjectID == 48 ||  ObjectID == 49 || ObjectID == 52 )
		{
			GoonWithArmsStatePlayer goon = AI.AddComponent<GoonWithArmsStatePlayer>();
			goon.m_FollowTransform = Player.transform;
			goon.m_Tr2Item = tr2item;
            AICondition.SetActive(AI, true);
            retval = true;
		}
		else if (ObjectID == 38 || ObjectID == 37)
		{
			CrowStatePlayer crow = AI.AddComponent<CrowStatePlayer>();
			crow.m_FollowTransform = Player.transform;
			crow.m_Tr2Item = tr2item;
            AICondition.SetActive(AI, true);
            retval = true;
		}
		else if (ObjectID == 39)
		{
			TigerStatePlayer tiger = AI.AddComponent<TigerStatePlayer>();
			tiger.m_FollowTransform = Player.transform;
			tiger.m_Tr2Item = tr2item;
            AICondition.SetActive(AI, true);
            retval = true;
		}
		else if (ObjectID == 47)
		{
			EgleStatePlayer egle = AI.AddComponent<EgleStatePlayer>();
			egle.m_FollowTransform = Player.transform;
			egle.m_Tr2Item = tr2item;
            AICondition.SetActive(AI, true);
            retval = true;
		}
		else if (ObjectID == 67)
		{
			BoulderStatePlayer boulder = AI.AddComponent<BoulderStatePlayer>();
			boulder.m_FollowTransform = Player.transform;
			boulder.m_Tr2Item = tr2item;
            AICondition.SetActive(AI, !Settings.ForceDisableAllBoulder); 
			retval = true;
		}
		else if(ObjectID == 103 || ObjectID == 104 || ObjectID == 93)
		{
			SwitchStatePlayer _switch = AI.AddComponent<SwitchStatePlayer>();
			_switch.m_FollowTransform = Player.transform;
			_switch.m_Tr2Item = tr2item;
            AICondition.SetActive(AI, true);
            retval = true;
		}
		else if(ObjectID == 106 || ObjectID == 110)
		{
			DoorStatePlayer door = AI.AddComponent<DoorStatePlayer>();
			door.m_FollowTransform = Player.transform;
			door.m_Tr2Item = tr2item;
            AICondition.SetActive(AI, !Settings.ForceOpenAllDoors);
			
			retval = true;
		}
		else if(ObjectID == 107 || ObjectID == 109 ||  ObjectID == 112 || ObjectID == 114  )
		{
			DoorStatePlayer door = AI.AddComponent<DoorStatePlayer>();
			door.m_FollowTransform = Player.transform;
			door.m_Tr2Item = tr2item;
            AICondition.SetActive(AI, !Settings.ForceOpenAllDoors);
			retval = true;
		}
		else if(ObjectID == 108)
		{
			DoorStatePlayer door = AI.AddComponent<DoorStatePlayer>();
			door.m_FollowTransform = Player.transform;
			door.m_Tr2Item = tr2item;
            AICondition.SetActive(AI,!Settings.ForceOpenAllDoors);
			retval = true;
		}
		else if(ObjectID == 214)
		{
			TRexStatePlayer trex = AI.AddComponent<TRexStatePlayer>();
			trex.m_FollowTransform = Player.transform;
			trex.m_Tr2Item = tr2item;
            AICondition.SetActive(AI, true);
            retval = true;
		}
		else if(ObjectID == 260)
		{
			//ButtlerStatePlayer
			ButtlerStatePlayer buttler = AI.AddComponent<ButtlerStatePlayer>();
			buttler.m_FollowTransform = Player.transform;
			buttler.m_Tr2Item = tr2item;
            AICondition.SetActive(AI, true);
            retval = true;
		}
		else if(ObjectID == 28 || ObjectID == 25)
		{
			
			SharkGoldenStatePlayer shark_golden = AI.AddComponent<SharkGoldenStatePlayer>();
			shark_golden.m_FollowTransform = Player.transform;
			shark_golden.m_Tr2Item = tr2item;
            AICondition.SetActive(AI, true);
            retval = true;
		}
		else if (ObjectID == 21 || ObjectID == 36)
		{
			RatStatePlayer rat = AI.AddComponent<RatStatePlayer>();
			rat.m_FollowTransform = Player.transform;
			rat.m_Tr2Item = tr2item;
            AICondition.SetActive(AI, true);
            retval = true;
		}
		
		
		return retval;  // if false default behabiour will be used
	}
}
