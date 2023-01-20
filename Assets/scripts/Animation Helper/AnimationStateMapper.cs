using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AnimationStateMapper {

	//TODO: This should go to State Player class 
	public static void BuildMap(List<TRAnimationClip> clips, Animation animation, Parser.Tr2Level leveldata)
	{
		for(int clipid = 0; clipid < clips.Count; clipid++)
		{
			//collect statechange info into TRAnimStateChange class
			int offsetstatechange = (int) leveldata.Animations[clipid].StateChangeOffset;
			int nstatechange = leveldata.Animations[clipid].NumStateChanges;
			for(int statechangeid = 0; statechangeid < nstatechange; statechangeid++)
			{
				//read statechange info from global Parser.Tr2StateChange leveldata.StateChanges
				Parser.Tr2StateChange statechange = leveldata.StateChanges[offsetstatechange + statechangeid];
				int ndispatch = statechange.NumAnimDispatches;
				int offsetdispatch = statechange.AnimDispatch;
				
				//create wrapper statechange object
				TRAnimStateChange unitystatechange = new TRAnimStateChange();
				unitystatechange.stateid = (int)statechange.StateID;
				for(int dispatchid = 0; dispatchid < ndispatch; dispatchid++)
				{
					Parser.Tr2AnimDispatch animdispatch = leveldata.AnimDispatches[offsetdispatch + dispatchid];
					//TRAnimDispatcher unityanimdispacher = new TRAnimDispatcher();
					//unityanimdispacher.NextAnimation = animdispatch.NextAnimation;
					//unitystatechange.dispatchers.Add(unityanimdispacher);
                    unitystatechange.tr2dispatchers.Add(animdispatch);
                }
				clips[clipid].statechanges.Add(unitystatechange);
			}
			
			clips[clipid].state = animation[""+ clipid];
		}
	}
}
