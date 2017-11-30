using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundMananger : MonoBehaviour {

   static AudioClip sfx_dive;
   static AudioClip sfx_dive_shallow;
   static AudioClip sfx_pull_out_water;
   static AudioClip sfx_landed_from_jump;
   static AudioClip sfx_start_jump;
   static AudioClip sfx_underwater_ambient;
   static AudioSource _AudioSourceBG;
   static AudioSource _AudioSourceSFX;
   static Vector3 _AudioSourcePosition;

    // Use this for initialization
    void Start () {

        _AudioSourceBG = GetComponent<AudioSource>();
        if(_AudioSourceBG == null)
        {
            _AudioSourceBG = gameObject.AddComponent<AudioSource>();
        }
        _AudioSourceBG.volume = 0.35f;

        if (_AudioSourceSFX == null)
        {
            _AudioSourceSFX = gameObject.AddComponent<AudioSource>();
        }
        _AudioSourceSFX.volume = 0.35f;

        sfx_dive = (AudioClip)Resources.Load("sfx/splash", typeof(AudioClip));
        sfx_dive_shallow = (AudioClip)Resources.Load("sfx/splash", typeof(AudioClip));
        sfx_pull_out_water = (AudioClip)Resources.Load("sfx/splash2", typeof(AudioClip));
        sfx_landed_from_jump = (AudioClip)Resources.Load("sfx/landed-from-jump", typeof(AudioClip));
        sfx_start_jump = (AudioClip)Resources.Load("sfx/start jump", typeof(AudioClip));
        sfx_underwater_ambient = (AudioClip)Resources.Load("sfx/under-water-bg", typeof(AudioClip));

        _AudioSourcePosition = transform.position;

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void PlayUnderWaterAmbient(bool play)
    {
        if (_AudioSourceBG != null)
        {
            if (play && !_AudioSourceBG.isPlaying)
            {
                _AudioSourceBG.clip = sfx_underwater_ambient;
                _AudioSourceBG.loop = true;
                _AudioSourceBG.Play();
            
            }
            else
            {

                _AudioSourceBG.clip = null;

            }

        }
    }

    public static void PlayDiveSFX()
    {
        PlayClip(sfx_dive);
    }

    public static void PlayPullUpSFX(AudioClip clip)
    {
        PlayClip(clip);
    }


    public static void PlayWalkUpSFX(AudioClip clip)
    {
      

        PlayClip(clip);

        Debug.Log("PlayWalkUpSFX");
      
    }

    public static void PlayLandedSFX()
    {
       PlayClip(sfx_landed_from_jump);
        
    }

    public static void PlayStartJumpFX()
    {
        PlayClip(sfx_start_jump);
    }

    public static void PlayMovementSFX(AudioClip clip)
    {
        PlayClip(clip);
    }

    static void PlayClip(AudioClip clip)
    {
        if (_AudioSourceSFX != null)
        {
            if (!_AudioSourceSFX.isPlaying)
            {
                _AudioSourceSFX.clip = clip;
                _AudioSourceSFX.loop = false;
                _AudioSourceSFX.Play();
            }
        }
    }

}
