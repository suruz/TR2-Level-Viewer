
#if UNITY_EDITOR
	using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.IO;

public class Loader :MonoBehaviour {
	
	static TextAsset m_DemoData;
	//public ThirdPersonCam m_Camera = null;
	//public TextMesh m_Text3D = null;
	static Level m_Level = null;
	public static byte[] m_RawFileData = null;
	WWW m_www = null;
	// Use this for initialization
	
#if UNITY_EDITOR
	[MenuItem("TR2 Editor/Create Level")]
	public static void Create () 
	{
		string path = EditorUtility.OpenFilePanel("Open Tomb Raider II Level File (*.TR2)", Application.dataPath, "*.tr2; *.TR2");
		if(path != null)
		{
			Settings.LevelFileLocalPath = path;
			Parser.Tr2Level leveldata = LoadLevelFromFile(Settings.LevelFileLocalPath);
			if(leveldata!= null)
			{
				leveldata.Camera = null;
				leveldata.Text3DPrefav = null;
				m_Level = new Level(leveldata);
			}
		}
	}
#endif

	void Start () 
	{
		if(!Settings.LoadLevelFileFromUrl) //Load level from local data file
		{
			LoadLevel();
		}
		else                         //We have to download data from url
		{
			if(m_RawFileData!= null) //We have downloaded data!
			{
				LoadLevelFromUrl(m_RawFileData);
			}
			else                    //download data
			{
				Debug.Log("Init load level from url: " + Settings.LevelFileUrl);
				m_www = new WWW(Settings.LevelFileUrl);
			}
		}
	}

	void LoadLevel()
	{
		Parser.Tr2Level leveldata = LoadLevelFromFile(Settings.LevelFileLocalPath);
		if(leveldata!= null)
		{
			//leveldata.Camera = m_Camera;
			//leveldata.Text3DPrefav = m_Text3D;
			m_Level = new Level(leveldata);
		}
		else
		{
			//Selected file is not tr2 type! 
			Application.LoadLevel("Browser");
		}
	}

	static Parser.Tr2Level LoadLevelFromFile(string path)
	{
		Parser.Tr2Level leveldata = null;
		if(path != null)
		{
			#if UNITY_WEBPLAYER
				if(Settings.LoadDemoLevel)
				{
					m_DemoData = (TextAsset)Resources.Load("Demo Level", typeof(TextAsset));
					m_RawFileData = m_DemoData.bytes;
					leveldata = Parser.Parse(m_RawFileData);
					Level.m_LevelName = Path.GetFileNameWithoutExtension(path);
				}
			#else
			if(Settings.LoadDemoLevel)
			{
				m_DemoData = (TextAsset)Resources.Load("Demo Level", typeof(TextAsset));
				m_RawFileData = m_DemoData.bytes;
			}
			else
			{
				m_RawFileData =File.ReadAllBytes(path); 
			}
				
			leveldata = Parser.Parse(m_RawFileData);
			Level.m_LevelName = Path.GetFileNameWithoutExtension(path);
			#endif
			
		}
		return leveldata;
	}
	
	void LoadLevelFromUrl(byte[] data)
	{
		if(data == null) return;
		Parser.Tr2Level leveldata = Parser.Parse(data);
		if(leveldata!= null)
		{
			//leveldata.Camera = m_Camera;
			//leveldata.Text3DPrefav = m_Text3D;
			m_Level = new Level(leveldata);
		}
	}

	// Update is called once per frame
	void Update () 
	{
		// show status of data download
		if(m_www!=null && m_RawFileData == null)
		{
			if(m_www.isDone && m_www.error == null)
			{
				LoadLevelFromUrl(m_RawFileData);
				m_www = null;
			}
		}
	}
	
}
