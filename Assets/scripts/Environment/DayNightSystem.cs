using UnityEngine;
using System.Collections;


public delegate void DayNightHandlerDelegate(bool isday); 

public class DayNightSystem : MonoBehaviour {

	public Transform LightTarget;
	public Material SkyMat;
	Transform _Transform;
	float OrbitRadius = 0;
	float Rotz = 60.0f;
	float _LightIntensity = Settings.DayLightIntensity;
	float _StartTime = 0;
	bool _IsDay = true;
	bool _PrevState = false;

	public AudioClip _DayTimeAudio;
	public AudioClip _NightTimeAudio;
	AudioSource _AudioSource;

	Color _AmbientLightColor = new Color(88f/255f, 88f/255f, 88f/255f, 1f);
	Color _SkyColor =  Color.gray;
	
	public static event DayNightHandlerDelegate OnDayNightUpdate;
	public static event GUIDayTimeUpdateDelegate OnDayTimeUpdate;
	Light light; //builtin property light has been deprecated since unity5.
	public AnimationCurve _IntensitySampler = AnimationCurve.Linear(0,0,1,1);
	// Use this for initialization
	void Start () 
	{
		_Transform = transform;
		OrbitRadius = (_Transform.position - LightTarget.position).magnitude;
		LightTarget.position = new Vector3(LightTarget.position.x, LightTarget.position.y,_Transform.position.z );
		Vector3 pos = LightTarget.InverseTransformPoint(_Transform.position);
		Rotz = 60.0f;//Mathf.Atan2(pos.y, pos.x) * 180.0f/ Mathf.PI;
		_StartTime = Time.time;
		_AudioSource = GetComponent<AudioSource>();
		_AudioSource.loop = true;
		light = GetComponent<Light>();
		light.intensity = _LightIntensity;
		if(!Settings.EnableIndoorShadow)
		{
			//light.shadows = LightShadows.None;  // shadow should be controlled by shadow caster 
		}
		QualitySettings.shadowDistance = 40000 * Settings.SceneScaling;
		
		_AmbientLightColor = RenderSettings.ambientLight;
		DayNightSettings();
	}
	
	// Update is called once per frame
	void Update () {

		float sunanglef = Rotz + (Time.time - _StartTime) * 0.6f;
		//int sunangle = (int)sunanglef % 360;

		Quaternion rot =  Quaternion.Euler(0,0,sunanglef);
		Vector3 dir = rot * Vector3.right;
		//Vector3 pos = LightTarget.position + dir * OrbitRadius;
		//_Transform.position = pos;
		//_Transform.rotation = rot;
		_Transform.forward = -dir;
		
		 _LightIntensity = Vector3.Dot(-Vector3.up, _Transform.forward);
		//light.intensity = _LightIntensity * 1.26f;

		//Debug.Log(sunangle);
		if(OnDayTimeUpdate !=null) OnDayTimeUpdate((sunanglef * 24.0f / 360.0f));

		if(sunanglef > 180 )
		{
			_IsDay = false;

		}
		else
		{
			_IsDay = true;
		}

		if(_IsDay != _PrevState)
		{
			DayNightSettings();
			_PrevState = _IsDay;
		}
		
		
		
		if(_IsDay)
		{
			float intensity = _IntensitySampler.Evaluate(_LightIntensity);
			//light.intensity = intensity * 1.2f;
			if(intensity > 0.75f)
			{
				intensity = 0.75f;
			}
			RenderSettings.ambientLight =  Color.white * intensity;
			
		}

		//Debug.DrawLine(LightTarget.position,_Transform.position );
	}


	void DayNightSettings()
	{
		RenderSettings.fogColor = new Color(18f/255f,30f/255f,44f/255f,1f);
		RenderSettings.fogMode = FogMode.ExponentialSquared;
		RenderSettings.fogDensity = 0.00025f; 
		RenderSettings.fog = !_IsDay;
		if(light!=null) light.enabled = _IsDay;

		if(_IsDay)
		{
			_AudioSource.clip = _DayTimeAudio;
			//RenderSettings.ambientLight =  Color.white * light.intensity * 0.75f;
			RenderSettings.skybox = SkyMat;
		}
		else
		{
			_AudioSource.clip =_NightTimeAudio;
			RenderSettings.ambientLight = Color.white * 0.1f;
			RenderSettings.skybox = null;
			
		}
		if(OnDayNightUpdate!=null) OnDayNightUpdate(_IsDay);
		_AudioSource.Play();
	}

	static public void AddDayNightEventHandler(DayNightHandlerDelegate handler)
	{
		OnDayNightUpdate+=handler;
	}
	static public void AddDayTimeEventHandler(GUIDayTimeUpdateDelegate handler)
	{
		OnDayTimeUpdate+=handler;
	}

}
