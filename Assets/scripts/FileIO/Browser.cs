using UnityEngine;
using System.IO;
public class Browser : MonoBehaviour {
 
	//public static string m_textPath = "";
	public static string m_currentDir;
	public Texture2D m_TitleTex;
	public GUIStyle m_TextStyle;
	public GUIStyle m_TextStyle2;
 
	protected FileBrowser m_fileBrowser;
	bool m_showingUrlWindow = false;
	bool m_showingBrowserWindow = false;
	bool m_showLevelLoaingMessage = false;
	bool m_LoadLevel = false;
	Rect m_windowRect;
	
	WWW m_www = null;
	float m_DownloadProgress = 0;
	
 
	[SerializeField]
	protected Texture2D	m_directoryImage,
						m_fileImage;
	
	void Start()
	{
		string url = PlayerPrefs.GetString("LevelFileUrl");
		if(url =="")
		{
			//do nothing
		}
		else
		{
			Settings.LevelFileUrl = url;
		}
		
		m_windowRect = new Rect(Screen.width * 0.1f, Screen.height * 0.1f,Screen.width - Screen.width * 0.3f, 300);


#if !UNITY_EDITOR && UNITY_WEBGL
    WebGLInput.captureAllKeyboardInput = true;  //
#endif

    }

    protected void OnGUI () {
		GUI.DrawTexture(new Rect(0,0, Screen.width, Screen.height), m_TitleTex);
		OnGUIMain();
		if (m_fileBrowser != null) {
			m_fileBrowser.OnGUI();
		} 
		
	}
 
	protected void OnGUIMain() 
	{
		GUI.enabled = !(m_showingUrlWindow || m_showingBrowserWindow);
		GUI.Label(new Rect(10,Screen.height * 0.06f,Screen.width,50),"Select Tomb Raider II level file (*.tr2)" ,  m_TextStyle);
	
		
		if (GUI.Button(new Rect(10,Screen.height - 80, 150, 35),  "Load Level From Web")) 
		{
            Settings.LoadDemoLevel = false; //fixed unwated demo level load
            m_showingUrlWindow = true;
		}

#if UNITY_WEBGL

        //do nothing
#else
        else if (GUI.Button(new Rect(170,Screen.height - 80, 150, 35),  "Browse Level")) 
		{

            Settings.LoadDemoLevel = false; //fixed unwated demo level load
            Settings.LoadLevelFileFromUrl = false;

            string m_currentDir = PlayerPrefs.GetString("Level_Path");
			if(m_currentDir == null )
			{
				m_currentDir = Directory.GetCurrentDirectory();
			}
			else if(!Directory.Exists(m_currentDir))
			{
				m_currentDir = Directory.GetCurrentDirectory();
			}
			m_fileBrowser = new FileBrowser(new Rect(Screen.width * 0.1f, 0, Screen.width - Screen.width * 0.5f, Screen.height - 10),"Select level file (*.tr2) ",m_currentDir, FileSelectedCallback);
			m_fileBrowser.SelectionPattern = "*." + Settings.DefaultTR2FileExtension.Trim(new char[]{' '});
			m_fileBrowser.DirectoryImage = m_directoryImage;
			m_fileBrowser.FileImage = m_fileImage;
			m_showingBrowserWindow = true;

        }
#endif
		else if(GUI.Button(new Rect(330,Screen.height - 80, 150, 35),  "Load Demo")) 
		{
			Settings.LevelFileLocalPath = Application.persistentDataPath + "/Resources/HILTOP.TR2";
#if UNITY_EDITOR
			Settings.LevelFileLocalPath = Application.dataPath + "/Resources/HILTOP.TR2";
#endif
			Settings.LoadDemoLevel = true;
            Settings.LoadLevelFileFromUrl = false;
            m_showLevelLoaingMessage = true;
			
		}
		
		if(GUI.Button(new Rect(Screen.width - 110,Screen.height - 35, 100, 30), "Exit"))
		{
			Application.Quit();
		}
			
		if(m_showingUrlWindow)
		{
			GUI.enabled = m_showingUrlWindow;
			GUI.Window(0,m_windowRect, Windowf, "Load Level from url"); 
		}
		
		if(m_showLevelLoaingMessage)
		{
			GUI.Label(new Rect(Screen.width - 150,50, 100, 50), "Loading...", m_TextStyle2);
			m_LoadLevel = true;
		}
	}
 
	protected void FileSelectedCallback(string path) {
		
		m_showingBrowserWindow = false;
		m_fileBrowser = null;
		if(path != null)
		{
			m_currentDir = Directory.GetParent(path).FullName;
			if(Directory.Exists(m_currentDir))
			{
				Settings.LevelFileLocalPath = path;
				Debug.Log("m_currentDir " + m_currentDir);
				PlayerPrefs.SetString("Level_Path",m_currentDir);
				m_showLevelLoaingMessage = true;
			}
		}
	}
	
	protected void Windowf(int id)
	{
		GUI.Label( new Rect(10,35, Screen.width,50), "Specify TR2 data file link from where file will be loaded.For example\n " +
			"http://example.com/mylevel.TR2");
		GUI.Label( new Rect(10,100, 100,50), "URL:");	
		//if(m_www != null)
		GUI.Label( new Rect(10,m_windowRect.height - 60 , Screen.width,50), "Downloading: " + m_DownloadProgress.ToString("f2") + "%" );

		
		if(GUI.Button( new Rect(50,200, 100,35), "Cancel"))
		{
			Settings.LoadLevelFileFromUrl = false;
			m_www = null;
			m_showingUrlWindow = false;
		}
		
		GUI.enabled = (m_www==null);
		Settings.LevelFileUrl = GUI.TextField(new Rect(50,100, m_windowRect.width - 75 ,35) , Settings.LevelFileUrl);

		if(GUI.Button( new Rect(200,200, 100,35), "OK"))
		{
			m_www = new WWW(Settings.LevelFileUrl);
		}
					
	}
	
	
	// Update is called once per frame
	void Update () 
	{
        // show status of data download
        //if(Loader.m_RawFileData == null)  // force download by disabling existance check of Loader.m_RawFileData
        //{
        if (m_www!=null)
			{
				m_DownloadProgress = m_www.progress * 100f;
			}
			
			if(m_www!=null && m_www.error == null && m_www.isDone)
			{
				Settings.LoadLevelFileFromUrl = true;
				//m_showingUrlWindow = false;
				PlayerPrefs.SetString("LevelFileUrl",Settings.LevelFileUrl);
				Loader.m_RawFileData = m_www.bytes;
				m_www = null;
				m_showLevelLoaingMessage = true;
			}
			
			if(m_www!=null && m_www.error != null)
			{
				m_www = null;
			}
			
		//}
	}
	
	void LateUpdate()
	{
		if(m_LoadLevel)
		{
			Application.LoadLevel("level");
			m_LoadLevel = false;
		}
		
	}
	
}