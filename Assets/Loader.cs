
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
    public Material m_SharedMaterial;
	static Level m_Level = null;
	public static byte[] m_RawFileData = null;
	WWW m_www = null;
	
	//used in editor only
	static string m_SharedTexturePath = "/Level Texture/";
	static string m_SharedMaterialPath = "/Resources/room_material.mat";
		
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
				
				// generate shared texture
				Texture2D shared_texture = TextureUV.GenerateTextureTile(leveldata);
				if(!Directory.Exists(Application.dataPath + m_SharedTexturePath))
				{
					Directory.CreateDirectory(Application.dataPath + m_SharedTexturePath);
				}
				//if(!File.Exists(Application.dataPath + "/Level Texture/" + Level.m_LevelName + ".png"))
				//File.WriteAllBytes(Application.dataPath + m_SharedTexturePath + Level.m_LevelName + ".png",shared_texture.EncodeToPNG());
				FileStream fstream = File.Open(Application.dataPath + m_SharedTexturePath + Level.m_LevelName + ".png",FileMode.OpenOrCreate,FileAccess.ReadWrite);
				BinaryWriter bw = new BinaryWriter(fstream);
				bw.Write(shared_texture.EncodeToPNG());
				bw.Close();
				//load shared texture
				//refresh assets before tryy
				AssetDatabase.Refresh();
				TextureImporter teximp = (TextureImporter)TextureImporter.GetAtPath("Assets" + m_SharedTexturePath + Level.m_LevelName + ".png");
				
				if(teximp == null)
				{
					EditorUtility.DisplayDialog("Error", "Assets" + m_SharedTexturePath + Level.m_LevelName + ".png" + " is not found in Assets ", "OK");
					return;

				}
				else
				{
					
					#if (UNITY_5_3_OR_NEWER || UNITY_5_3)
					teximp.alphaSource = TextureImporterAlphaSource.FromInput;
					teximp.filterMode   = FilterMode.Bilinear;
					teximp.wrapMode = TextureWrapMode.Clamp;
					//teximp.sRGBTexture = true; 
					teximp.textureType = TextureImporterType.Default;
					teximp.maxTextureSize = 4096;
					teximp.mipmapEnabled = false;
					teximp.textureCompression = TextureImporterCompression.Uncompressed;
					#else
					
					teximp.filterMode = FilterMode.Bilinear;
            		teximp.grayscaleToAlpha = false;
            		teximp.textureFormat = TextureImporterFormat.ARGB32;
            		teximp.wrapMode = TextureWrapMode.Clamp;
					
					#endif
            		
				}
					
				//load shared material
				Material shared_material = (Material )AssetDatabase.LoadAssetAtPath("Assets" + m_SharedMaterialPath, typeof(Material));
				
				if(shared_material == null)
				{
					EditorUtility.DisplayDialog("Error","Assets" + m_SharedMaterialPath + " is not found in Assets ", "OK");
					return;
				}
				
				shared_material.mainTexture = (Texture) AssetDatabase.LoadAssetAtPath("Assets" + m_SharedTexturePath + Level.m_LevelName + ".png", typeof(Texture));
				Level.m_SharedMaterial = shared_material;
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
				Level.m_LevelName = Path.GetFileNameWithoutExtension(Settings.LevelFileUrl);
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
            if (m_SharedMaterial != null)
            {
				m_SharedMaterial.mainTexture = TextureUV.GenerateTextureTile(leveldata);
                Level.m_SharedMaterial = m_SharedMaterial;
                m_Level = new Level(leveldata);
            }
            else
            {
#if UNITY_EDITOR
                EditorUtility.DisplayDialog("Error", "m_SharedMaterial is not set in GameObject:" + this.name, "OK");
#else
                Debug.LogError("m_SharedMaterial is not set in GameObject" + this.name);
#endif
            }

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
				FileStream fstream = File.Open(path,FileMode.Open,FileAccess.ReadWrite);
				BinaryReader br = new BinaryReader(fstream);
				m_RawFileData =  br.ReadBytes((int)fstream.Length); //File.ReadAllBytes(path); //fixed file read access violation
				br.Close();
			}
				
			leveldata = Parser.Parse(m_RawFileData);
			Level.m_LevelName = Path.GetFileNameWithoutExtension(path);
			Debug.Log("LoadLevelFromFile: " + Level.m_LevelName);
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
            if (m_SharedMaterial != null)
            {
				m_SharedMaterial.mainTexture = TextureUV.GenerateTextureTile(leveldata);
                Level.m_SharedMaterial = m_SharedMaterial;
                m_Level = new Level(leveldata);
            }
            else
            {
         
#if UNITY_EDITOR
                EditorUtility.DisplayDialog("Error", "m_SharedMaterial is not set in GameObject: " + this.name, "OK");
             
#else
                Debug.LogError("m_SharedMaterial is not set in GameObject" + this.name);
#endif

            }

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
