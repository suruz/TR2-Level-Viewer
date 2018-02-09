using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class KeyFrameData
{
    public ushort[] data;
    public int numshorts;

    public int start_animation_frame_index = 0;
    public int startoffset;   // Key frame start offset (in number of short) for an animation in global key frames array
    public int framesize;     // key frame size in short
    public int numkeyframe;   // number key frame in animation
    public float time_per_frame;   // animation frame rate
    public bool endofanimation = false;
    public bool bplayer = false;
}

//Unity3D animation wrapper classes
[System.Serializable]
public class TRAnimDispatcher
{
    //public Parser.Tr2AnimDispatch info;
    public int NextAnimation;
}
[System.Serializable]
public class TRAnimStateChange
{
    public List<Parser.Tr2AnimDispatch> tr2dispatchers = null;
    public int stateid = -1;
    public TRAnimStateChange()
    {
        tr2dispatchers = new List<Parser.Tr2AnimDispatch>();
    }
}
[System.Serializable]
public class TRAnimationClip
{
    //public AnimationClip animation;
    public AnimationClip clip;
    public int index = -1;
    public int stateid = -1;
    public List<TRAnimStateChange> statechanges = null;
    public AnimationState state = null;
    public float starttime = 0.0f;
    public float endtime = 0.0f;
    public float framerate = 1.0f;
    public float time_per_frame = 0;
    public int start_animation_frame_index = 0;

    public TRAnimationClip(AnimationClip clip, int forsate)
    {
        this.clip = clip;
        this.stateid = forsate;
        statechanges = new List<TRAnimStateChange>();
        //
    }
    //Get time of key frame time
	public float GetAnimationFrameTime(int frame)
	{
		return frame * time_per_frame;
	}
}

public class Animator
{
    //param  transformtree: number of transform used in animation clip
    //param startclipid : first animation clip index into Parser.Tr2Animation array
    //keyframeinfo: holds tr2 keyframes related info for an animation

    public static List<TRAnimationClip> CreateAnimationWithID(Tr2Moveable tr2movable, Transform[] transformtree, Parser.Tr2Level leveldata)
    {
        List<TRAnimationClip> tranimclips = new List<TRAnimationClip>();

        int ntransform = transformtree.Length;
        int trclipoffset = tr2movable.AnimationStartOffset;
        Parser.TR2VersionType enginetype = leveldata.EngineVersion;

        //each tr anim actually  information that reffer a chunk of animation frames  contained in frames[]
        //each frame chunk contain sequential  data [key] for all of the transform of this object
        //Now question is how many animation clips there are ? 

        //KeyFrameData keyframeinfo = CalculateAnimationKeyFrameData(trclipoffset, leveldata);

        /*bool shortanimation = false;
        if (keyframeinfo.numkeyframe < 15)
        {
            shortanimation = true;
        }

        int nclip = 1;
        if (tr2movable.ObjectID == 0)
        {
            nclip = 261;

            Debug.Log(" lara trclipoffset: " + trclipoffset);
        }

        //Debug.Log("ID: " + tr2movable.ObjectID + " trclipoffset: " + trclipoffset);
        */


        for (int clipid = 0; clipid < tr2movable.NumClips; clipid++)
        {
            //if(shortanimation && clipid > 5) break;

            KeyFrameData keyframeinfo = CalculateAnimationKeyFrameData(trclipoffset, leveldata);
            Parser.Tr2Animation tr2animation = leveldata.Animations[trclipoffset];

            AnimationCurve curvRelX = null;
            AnimationCurve curvRelY = null;
            AnimationCurve curvRelZ = null;

            AnimationCurve[] curvRelRotX = new AnimationCurve[ntransform];
            AnimationCurve[] curvRelRotY = new AnimationCurve[ntransform];
            AnimationCurve[] curvRelRotZ = new AnimationCurve[ntransform];
            AnimationCurve[] curvRelRotW = new AnimationCurve[ntransform];

            //prepare curves for animation
            for (int transformId = 0; transformId < ntransform; transformId++)
            {
                //create curves
                curvRelRotX[transformId] = new AnimationCurve(null);
                curvRelRotY[transformId] = new AnimationCurve(null);
                curvRelRotZ[transformId] = new AnimationCurve(null);
                curvRelRotW[transformId] = new AnimationCurve(null);

                if (transformId == 0)
                {
                    curvRelX = new AnimationCurve(null);
                    curvRelY = new AnimationCurve(null);
                    curvRelZ = new AnimationCurve(null);
                }
            }

            int numkeyframe = keyframeinfo.numkeyframe;

            for (int keyFrameCount = 0; keyFrameCount < numkeyframe; ++keyFrameCount)
            {
                int frameoffset = keyframeinfo.startoffset + (keyframeinfo.framesize * keyFrameCount);

                //extract key frme rotation
                int l = 9;   //first angle offset in this Frame
                for (int transformId = 0; transformId < ntransform; transformId++)
                {
                    ushort itmp = keyframeinfo.data[frameoffset + l];
                    ushort itmp2;
                    double angle;
                    float rotx = 0;
                    float roty = 0;
                    float rotz = 0;
                    l = l + 1;
                    if (enginetype == Parser.TR2VersionType.TombRaider_1)
                    {
                        // all angles are three-axis
                        angle = (itmp >> 4) & 0x03ff;
                        angle *= 360.0 / 1024.0;
                        rotx = (float)angle; //keyframe rotx value

                        itmp2 = (ushort)((itmp << 6) & 0x03c0);
                        itmp = keyframeinfo.data[frameoffset + l]; // get Z rotation
                        l = l + 1;

                        itmp2 |= (ushort)((itmp >> 10) & 0x003f);
                        angle = itmp2;
                        angle *= 360.0 / 1024.0;
                        roty = (float)angle; //keyframe roty value

                        angle = itmp & 0x3ff;
                        angle *= 360.0 / 1024.0;
                        rotz = (float)angle; //keyframe rotz value
                    }
                    else if ((itmp & 0xc000) > 0)  // TR2, TR3, TR4 - single axis of rotation
                    {
                        if (enginetype == Parser.TR2VersionType.TombRaider_4)
                        {
                            angle = itmp & 0x0fff;
                            angle /= 4096.0;
                            angle *= 360.0;
                        }
                        else
                        {
                            angle = itmp & 0x3ff;
                            angle /= 1024.0;
                            angle *= 360.0;
                        }

                        switch (itmp & 0xc000)
                        {
                            case 0x4000:
                                rotx = (float)angle;
                                break;
                            case 0x8000:
                                roty = (float)angle;
                                break;
                            case 0xc000:
                                rotz = (float)angle;
                                break;
                        }
                    }
                    else   // TR2, TR3, TR4 - three axes
                    {
                        angle = (itmp >> 4) & 0x03ff;
                        angle *= 360.0 / 1024.0;
                        rotx = (float)angle;

                        itmp2 = (ushort)((itmp << 6) & 0x03c0);
                        itmp = keyframeinfo.data[frameoffset + l]; // get Z rotation
                        l = l + 1;

                        itmp2 |= (ushort)((itmp >> 10) & 0x003f);
                        angle = itmp2;
                        angle *= 360.0 / 1024.0;
                        roty = (float)angle;

                        angle = itmp & 0x3ff;
                        angle *= 360.0 / 1024.0;
                        rotz = (float)angle;
                    }

                    //if(rotx > 180)
                    //{
                    rotx = Mathf.Abs(360 - rotx);
                    //}

                    //if(rotz > 180)
                    //{
                    rotz = Mathf.Abs(360 - rotz); ;
                    //}

                    //if(roty > 180)
                    //{
                    //roty= Mathf.Abs(360 - roty) ;;
                    //}

                    if (transformId == 0)
                    {
                        float ItemAnimX = (short)keyframeinfo.data[frameoffset + 6] * Settings.SceneScaling;
                        float ItemAnimY = (short)keyframeinfo.data[frameoffset + 7] * Settings.SceneScaling;
                        float ItemAnimZ = (short)keyframeinfo.data[frameoffset + 8] * Settings.SceneScaling;

                        if (numkeyframe == 1) //addition key after last key
                        {
                            curvRelX.AddKey(0, ItemAnimX);
                            curvRelY.AddKey(0, -ItemAnimY);
                            curvRelZ.AddKey(0, ItemAnimZ);

                            curvRelX.AddKey(1 * keyframeinfo.time_per_frame, ItemAnimX);
                            curvRelY.AddKey(1 * keyframeinfo.time_per_frame, -ItemAnimY);
                            curvRelZ.AddKey(1 * keyframeinfo.time_per_frame, ItemAnimZ);
                        }
                        else
                        {
                            int keylength = curvRelX.length;
                            if (keylength > 0)
                            {

                                Keyframe kx = new Keyframe(keylength * keyframeinfo.time_per_frame, ItemAnimX, Mathf.Infinity, Mathf.Infinity);
                                Keyframe ky = new Keyframe(keylength * keyframeinfo.time_per_frame, -ItemAnimY, Mathf.Infinity, Mathf.Infinity);
                                Keyframe kz = new Keyframe(keylength * keyframeinfo.time_per_frame, ItemAnimZ, Mathf.Infinity, Mathf.Infinity);
                                curvRelX.AddKey(kx);
                                curvRelY.AddKey(ky);
                                curvRelZ.AddKey(kz);
                            }
                            else
                            {
                                curvRelX.AddKey(0, ItemAnimX);
                                curvRelY.AddKey(0, -ItemAnimY);
                                curvRelZ.AddKey(0, ItemAnimZ);
                            }
                        }
                    }

                    //TODO:
                    //multiply transform with reltive rotation and translation data
                    //relative translation of animation. allready provided?
                    //problem: animation transform works in local space.Thats mean it does not work on root?Am I working in root?

                    Quaternion finalrot =
                        Quaternion.AngleAxis(roty, Vector3.up) *
                            Quaternion.AngleAxis(rotx, Vector3.right) *
                            Quaternion.AngleAxis(rotz, Vector3.forward);

                    if (numkeyframe == 1) //addition key after last key
                    {
                        curvRelRotX[transformId].AddKey(0, finalrot.x);
                        curvRelRotY[transformId].AddKey(0, finalrot.y);
                        curvRelRotZ[transformId].AddKey(0, finalrot.z);
                        curvRelRotW[transformId].AddKey(0, finalrot.w);

                        curvRelRotX[transformId].AddKey(keyframeinfo.time_per_frame, finalrot.x);
                        curvRelRotY[transformId].AddKey(keyframeinfo.time_per_frame, finalrot.y);
                        curvRelRotZ[transformId].AddKey(keyframeinfo.time_per_frame, finalrot.z);
                        curvRelRotW[transformId].AddKey(keyframeinfo.time_per_frame, finalrot.w);
                    }
                    else
                    {
                        int keylength = curvRelRotX[transformId].length;
                        if (keylength > 0)
                        {
                            //FIX: set outTangent and inTangent to Mathf.Infinity
                            Keyframe kfrotx = new Keyframe(keylength * keyframeinfo.time_per_frame, finalrot.x, Mathf.Infinity, Mathf.Infinity);
                            Keyframe kfroty = new Keyframe(keylength * keyframeinfo.time_per_frame, finalrot.y, Mathf.Infinity, Mathf.Infinity);
                            Keyframe kfrotz = new Keyframe(keylength * keyframeinfo.time_per_frame, finalrot.z, Mathf.Infinity, Mathf.Infinity);
                            Keyframe kfrotw = new Keyframe(keylength * keyframeinfo.time_per_frame, finalrot.w, Mathf.Infinity, Mathf.Infinity);

                            curvRelRotX[transformId].AddKey(kfrotx);
                            curvRelRotY[transformId].AddKey(kfroty);
                            curvRelRotZ[transformId].AddKey(kfrotz);
                            curvRelRotW[transformId].AddKey(kfrotw);
                        }
                        else
                        {
                            curvRelRotX[transformId].AddKey(0, finalrot.x);
                            curvRelRotY[transformId].AddKey(0, finalrot.y);
                            curvRelRotZ[transformId].AddKey(0, finalrot.z);
                            curvRelRotW[transformId].AddKey(0, finalrot.w);
                        }
                    }

                }
            }

            AnimationClip animClip = new AnimationClip();
//if animClip is not set to legacy set curve will not workt on vesion 4 or higher
#if UNITY_4_0
			animClip.legacy = true;
#elif UNITY_4_0_1
			animClip.legacy = true;
#elif UNITY_4_1
			animClip.legacy = true;
#elif UNITY_4_2
			animClip.legacy = true;
#elif UNITY_4_3
			animClip.legacy = true;
#elif UNITY_4_5
			animClip.legacy = true;
#elif UNITY_4_6
			animClip.legacy = true;
#elif UNITY_5_0
			animClip.legacy = true;
#endif
#if (UNITY_5_3_OR_NEWER || UNITY_5_3)
            animClip.legacy = true;
#endif

            for (int transformId = 0; transformId < ntransform; transformId++)
            {
                System.String relCurvePath = CalculateCurveRelativePath(transformtree[transformId]);
                //print("relCurvePath:"+relCurvePath);

                if (transformId != 0)
                {
                    animClip.SetCurve(relCurvePath, typeof(Transform), "localRotation.x", curvRelRotX[transformId]);
                    animClip.SetCurve(relCurvePath, typeof(Transform), "localRotation.y", curvRelRotY[transformId]);
                    animClip.SetCurve(relCurvePath, typeof(Transform), "localRotation.z", curvRelRotZ[transformId]);
                    animClip.SetCurve(relCurvePath, typeof(Transform), "localRotation.w", curvRelRotW[transformId]);
                }
                else
                {

                    animClip.SetCurve(relCurvePath, typeof(Transform), "localRotation.x", curvRelRotX[transformId]);
                    animClip.SetCurve(relCurvePath, typeof(Transform), "localRotation.y", curvRelRotY[transformId]);
                    animClip.SetCurve(relCurvePath, typeof(Transform), "localRotation.z", curvRelRotZ[transformId]);
                    animClip.SetCurve(relCurvePath, typeof(Transform), "localRotation.w", curvRelRotW[transformId]);

                    animClip.SetCurve(relCurvePath, typeof(Transform), "localPosition.x", curvRelX);
                    animClip.SetCurve(relCurvePath, typeof(Transform), "localPosition.y", curvRelY);
                    animClip.SetCurve(relCurvePath, typeof(Transform), "localPosition.z", curvRelZ);
                }
            }

            TRAnimationClip tranimclip = new TRAnimationClip(animClip, leveldata.Animations[clipid].StateID);
            tranimclip.time_per_frame = keyframeinfo.time_per_frame;
            tranimclip.starttime = 0.0f;
            tranimclip.endtime = keyframeinfo.numkeyframe * tranimclip.time_per_frame;
            tranimclip.framerate = 7 - tr2animation.FrameRate;// 1f / tranimclip.time_per_frame ;
            tranimclip.index = clipid;
            tranimclip.start_animation_frame_index = keyframeinfo.start_animation_frame_index;
            tranimclips.Add(tranimclip);
            //goto next clip
            trclipoffset++;
        }

        return tranimclips;
    }

    public static List<TRAnimationClip> AttachAnimation(Tr2Moveable tr2movable, Parser.Tr2Level leveldata)
    {
        List<TRAnimationClip> clips = CreateAnimationWithID(tr2movable, tr2movable.TransformsTree, leveldata);
        //tr2movable.UnityObject.AddComponent<AnimationPlayer>();
        //m_DynamicPrefabs[i].AnimClips = new List<TRAnimationClip>();
        //add these clips to Tr2Moveable tr2movable.UnityAnimation
        for (int ci = 0; ci < clips.Count; ci++)
        {
            tr2movable.UnityAnimation.AddClip(clips[ci].clip, "" + ci);
        }
		 tr2movable.AnimClips = clips;
		
        return clips;
    }

    static KeyFrameData CalculateAnimationKeyFrameData(int animid, Parser.Tr2Level leveldata)
    {
        //Note: animid is a index to tranim list. 

        KeyFrameData tr2framedata = new KeyFrameData();
        if (animid == leveldata.NumAnimations)
        {
            //endofanimation
            tr2framedata.endofanimation = true;
            return tr2framedata;
        }

        if (animid == 0)
        {
            tr2framedata.bplayer = true;
        }

        Parser.Tr2Animation tr2animation = leveldata.Animations[animid];
        //create  s16 offset list of frames for this animation;
        //determine short stating offset to frames chunk of this animclip
        //calculate index into tr2frames[] and how large the frame is
        //tr2animation.FrameOffset is byte offset into Frame, make it short offset
        //tr2animation.FrameSize = (byte)((tr2frames[fo + 9] * 2) + 10);

        tr2framedata.data = leveldata.Frames;
        tr2framedata.startoffset = (int)tr2animation.FrameOffset / 2;
        tr2framedata.framesize = (int)tr2animation.FrameSize;  // num shorts of this frame step
        tr2framedata.time_per_frame = (float)tr2animation.FrameRate / 30f;

        if (animid < (int)leveldata.NumAnimations - 1)
        {
            Parser.Tr2Animation nexttr2animation = leveldata.Animations[animid + 1];
            int nextoffset = (int)(nexttr2animation.FrameOffset / 2);
            tr2framedata.numshorts = nextoffset - tr2framedata.startoffset;
        }
        else
        {
            tr2framedata.numshorts = (int)(leveldata.NumFrames - (uint)tr2framedata.startoffset);
        }

        if (tr2framedata.framesize != 0)
        {
            tr2framedata.numkeyframe = tr2framedata.numshorts / tr2framedata.framesize;
        }
        else
        {
            tr2framedata.numkeyframe = 0;
        }

        tr2framedata.start_animation_frame_index = tr2animation.FrameStart;// tr2animation (int)tr2framedata.startoffset / tr2framedata.framesize;

       // tr2framedata.numkeyframe = (tr2animation.FrameEnd - tr2animation.FrameStart) + 1;

        //if(tr2framedata.numkeyframe > 15)
        //Debug.Log("numkeyframe: " + tr2framedata.numkeyframe + " NextAnimation:" + tr2animation.NextAnimation);

        return tr2framedata;
    }

    public static string CalculateCurveRelativePath(Transform obj)
    {
        Transform tempObj = obj;

        string relPath = "";
        System.String[] pathComponent = null;
        int pathComponentCount = 0;
        int i = 0;
        bool bHasAnim = false;

        while (tempObj.parent)
        {
            Animation anim = (Animation)tempObj.gameObject.GetComponent(typeof(Animation));
            if (anim)
            {
                bHasAnim = true;
                break;
            }
            else
            {
                pathComponentCount++;
            }
            tempObj = tempObj.parent;
        }

        if (bHasAnim)
        {
            pathComponent = new System.String[pathComponentCount];
            if (pathComponentCount > 0)
            {
                //relPath = obj.name;
                //get path component
                tempObj = obj;
                i = 0;
                while (tempObj.parent)
                {
                    Animation anim = (Animation)tempObj.gameObject.GetComponent<Animation>();
                    if (anim)
                    {
                        break;
                    }
                    else
                    {
                        pathComponent[i] = tempObj.name;
                        i++;
                    }
                    tempObj = tempObj.parent;
                }

                //build path from component
                for (int j = pathComponentCount - 1; j >= 0; j--)
                {
                    if (j == pathComponentCount - 1)
                    {
                        relPath = pathComponent[j];
                    }
                    else
                    {
                        relPath += "/" + pathComponent[j];
                    }
                }
            }
            else
            {
                //relPath = "";
            }
        }

        return relPath;

    }
}
