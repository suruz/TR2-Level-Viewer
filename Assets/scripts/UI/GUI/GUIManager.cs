using UnityEngine;
using System.Collections;

public delegate  void GUIUpdateDelegate(Rect displayrect, string info);
public delegate  void GUIPlayerHealthDelegate(Rect displayrect, string info);
public delegate  void GUIDayTimeUpdateDelegate(float time);

public class GUIManager : MonoBehaviour {

	static Rect m_DisplayRect;
	static string m_DisplayInfo;
	static string m_HtmlFormat;

	static Rect m_HealthDisplayRect;
	static Rect m_HealthHtmlFormatRect;
	static string m_HealthDisplayInfo;
	static string m_HealthHtmlFormat;

	static Rect m_TimeDisplayRect;
	static Rect m_TimeHtmlFormatRect;
	static float m_DayTime = 0.0f;
	static string m_TimeDisplayFormat;

	public Font m_GUIFont = null;
	GUIStyle m_GUIStyle;
	GUIStyle m_DisplayFormatStyle;
	
	string m_Helptext = "";
	bool m_bDisplayHelp = false;
	
	public static void HandleGUIUpdate(Rect displayrect, string info)
	{
		//m_DisplayRect = displayrect;
		m_DisplayInfo = info;
	}

	public static void HandleGUIPlayerHealth(Rect displayrect, string info)
	{
		//m_HealthDisplayRect = displayrect;
		m_HealthDisplayInfo = info;
	}

	public static void HandleGUIDayTime( float time)
	{
		//m_HealthDisplayRect = displayrect;
		m_DayTime = time ;
	}
	
	// Use this for initialization
	void Start () {

		m_DisplayRect = new Rect(0,Screen.height - 25,Screen.width,100);
		m_HtmlFormat = "<size=30> Temple Of <color=yellow>Gold</color> </size>";

	
		m_HealthHtmlFormatRect = new Rect(50, 25,100,100);
		m_HealthDisplayRect = new Rect(m_HealthHtmlFormatRect.xMax, m_HealthHtmlFormatRect.yMin, 100, 100);
		m_HealthHtmlFormat = "Health ";//"<size=30> <color=yellow>Health</color> </size>";;
		m_HealthDisplayInfo = "100.00";
		
		m_TimeDisplayRect = new Rect(Screen.width - 200, 25,Screen.width,100);
		m_TimeHtmlFormatRect = new Rect(m_TimeDisplayRect.xMin - 70, m_TimeDisplayRect.yMin,100,100);
		m_TimeDisplayFormat = "Time ";//"<size=30> <color=yellow>Time</color> </size>";
		m_DayTime = 0;

		DayNightSystem.AddDayTimeEventHandler(HandleGUIDayTime);

		
		m_GUIStyle = new GUIStyle();
		m_GUIStyle.font = m_GUIFont;
		m_GUIStyle.normal.textColor = Color.red;
		m_GUIStyle.imagePosition = ImagePosition.TextOnly;
		//m_GUIStyle.richText = true;
		
		m_DisplayFormatStyle = new GUIStyle();
		m_DisplayFormatStyle.normal.textColor = Color.yellow;
		m_DisplayFormatStyle.font = m_GUIFont;
		m_DisplayFormatStyle.imagePosition = ImagePosition.TextOnly;
		
		if(Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
		{
			m_Helptext =
			"Run – Double tap screen and hold.\n" +
			"Look around – Touch screen and move your fingers.\n" +
			"Pull up -   Get close to a platform double tap screen.";
		}
		else
		{
			m_Helptext =
			"Run/Swim - Up arrow\n" +
			"Walk - Num 5\n" +
			"Jump/Dive - Right mouse button\n" +
			"Look around - Move mouse while pressing left mouse button.\n" +
			"Long Jump - Press jump while you are running.\n" +
			"Short Jump - Press up arrow + jump same time.\n" +
			"Pull up -   Get close to a platform and press up arrow + left mouse button.\n";
		}
		

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnGUI()
	{
		//GUI.Box(new Rect(0,Screen.height - 25,Screen.width,50),"" );
		//GUI.Label(m_DisplayRect,m_HtmlFormat + m_DisplayInfo, m_GUIStyle);
		if(GUI.Button(new Rect(Screen.width - 110,Screen.height - 35, 100, 30), "Back"))
		{
            Application.LoadLevel("Browser"); //this will destroy all game object, refferences, event handlers in current scene
        }
		
		
		if(GUI.Button(new Rect(10,Screen.height - 35, 100, 30), "Help"))
		{
			m_bDisplayHelp = !m_bDisplayHelp;
		}
	
		if(m_bDisplayHelp)
		GUI.Label(new Rect(10, 140,Screen.width, Screen.height), m_Helptext);
		
		
		GUI.Label(m_HealthHtmlFormatRect,m_HealthHtmlFormat , m_DisplayFormatStyle);
		GUI.Label(m_HealthDisplayRect,m_HealthDisplayInfo + "%", m_GUIStyle);

		int localtime24 = (int)(m_DayTime + 6) % 24;
		int localtime12 = localtime24 % 12;

		if(localtime12 == 0) localtime12 = 12;
		
		GUI.Label(m_TimeHtmlFormatRect,m_TimeDisplayFormat, m_DisplayFormatStyle);
		if(localtime24 < 12)
		{
			GUI.Label(m_TimeDisplayRect,localtime12 + ":00 AM", m_GUIStyle);
		}
		else
		{
			GUI.Label(m_TimeDisplayRect,localtime12 + ":00 PM", m_GUIStyle);
		}

	}
		

}
