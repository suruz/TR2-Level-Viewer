using UnityEngine;
using System.Collections;

public class FlashLightStatePlayer : MonoBehaviour {
	
	Light light;
	// Use this for initialization
	void Start () {
		light = GetComponent<Light>();
		DayNightSystem.AddDayNightEventHandler(UpdateDayNight);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void UpdateDayNight(bool isday)
	{
		if(light !=null) light.enabled = !isday;
		//Debug.Log(isday);
	}
}
