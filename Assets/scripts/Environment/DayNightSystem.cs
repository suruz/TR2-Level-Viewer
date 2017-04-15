using UnityEngine;
using System.Collections;


public delegate void DayNightHandlerDelegate(bool isday); 

public class DayNightSystem : MonoBehaviour {

	public Transform LightTarget;
	public Material SkyMat;
	Transform _Transform;
	float OrbitRadius = 0;
	float Rotz = 0;
	float _LightIntensity = Settings.DayLightIntensity;

	bool _IsDay = true;
	bool _PrevState = true;

	public AudioClip _DayTimeAudio;
	public AudioClip _NightTimeAudio;
	AudioSource _AudioSource;

	Color _AmbientLightColor = new Color(88f/255f, 88f/255f, 88f/255f, 1f);
	Color _SkyColor =  Color.gray;
	
	public static event DayNightHandlerDelegate OnDayNightUpdate;
	public static event GUIDayTimeUpdateDelegate OnDayTimeUpdate;
	Light light; //builtin property light has been deprecated since unity5.

	// Use this for initialization
	void Start () 
	{
		_Transform = transform;
		OrbitRadius = (_Transform.position - LightTarget.position).magnitude;
		LightTarget.position = new Vector3(LightTarget.position.x, LightTarget.position.y,_Transform.position.z );
		Vector3 pos = LightTarget.InverseTransformPoint(_Transform.position);
		Rotz = Mathf.Atan2(pos.y, pos.x) * 180.0f/ Mathf.PI;

		_AudioSource = GetComponent<AudioSource>();
		_AudioSource.loop = true;
		light = GetComponent<Light>();
		light.intensity = 0.4f;
		if(!Settings.EnableIndoorShadow)
		{
			//light.shadows = LightShadows.None;  // shadow should be controlled by shadow caster 
		}
		QualitySettings.shadowDistance = 40000 * Settings.SceneScaling;
		
		//_AmbientLightColor = RenderSettings.ambientLight;
		DayNightSettings();
	}
	
	// Update is called once per frame
	void Update () {

		float sunanglef = Rotz + Time.time * 0.6f;
		int sunangle = (int)sunanglef % 360;

		Quaternion rot =  Quaternion.Euler(0,0,sunanglef);
		Vector3 dir = rot * Vector3.right;
		Vector3 pos = LightTarget.position + dir * OrbitRadius;
		_Transform.position = pos;
		//_Transform.rotation = rot;
		_Transform.forward = -dir;
		
		light.intensity = _LightIntensity + Vector3.Dot(Vector3.right, _Transform.forward) * 0.5f;

		//Debug.Log(sunangle);
		if(OnDayTimeUpdate !=null) OnDayTimeUpdate((sunangle * 24.0f / 360.0f));

		if(sunangle > 180 )
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
			RenderSettings.ambientLight = _AmbientLightColor;
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
