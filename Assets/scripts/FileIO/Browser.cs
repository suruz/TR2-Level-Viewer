using UnityEngine;
using System.IO;
using UnityEngine.UI;
using TMPro;

#if UNITY_ANDROID
using NativeFilePickerNamespace;
#endif

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
	
	[SerializeField] protected GameObject fileBrowserPanel;
	[SerializeField] protected Texture2D m_directoryImage, m_fileImage;
	[SerializeField] protected FileListItem itemprefab;
	[SerializeField] protected FileListItem itemPrefabFolder;
	[SerializeField] protected VerticalLayoutGroup layout;
	[SerializeField] protected HorizontalLayoutGroup layoutAddress;
	[SerializeField] protected AddressItem addressItemPrefav;

	[Header("Download Managment")]
	[SerializeField] protected TMP_Dropdown m_dropdownloadUrls;
	[SerializeField] protected TMP_InputField m_TextDownloadUrl;
	[SerializeField] protected TMP_Text m_TextDownloadProgress;
	[SerializeField] protected Button m_ButtonWebBrowseOK;
	[SerializeField] protected Button m_ButtonWebBrowseCancel;

	[Header("Web Browser Panel")]
	[SerializeField] protected CanvasRenderer m_WebBrowserPanel;

	string m_BaseUrl = "https://raw.githubusercontent.com/andrewsyc/Tomb-Raider-1-2-3-4-5-Map-viewer-and-levels/master/Tomb-Raider-2/";
	string[] m_TR2Levels;
	void Start()
	{
		string url = PlayerPrefs.GetString("LevelFileUrl");
		if(url =="")
		{
			m_TextDownloadUrl.text = Settings.LevelFileUrl;
		}
		else
		{
			Settings.LevelFileUrl = url;
			m_TextDownloadUrl.text = Settings.LevelFileUrl;
		}
		
		m_windowRect = new Rect(Screen.width * 0.1f, Screen.height * 0.1f,Screen.width - Screen.width * 0.3f, 300);
		ResetDownload();

		m_dropdownloadUrls.options = new System.Collections.Generic.List<TMP_Dropdown.OptionData>();
		m_TR2Levels = SanitiesCSVInput(Settings.LevelDownloadCSV).Split(',');   
		foreach (string level in m_TR2Levels)
        {
			m_dropdownloadUrls.options.Add(new TMP_Dropdown.OptionData() {text = level.Trim() });
		}
		m_dropdownloadUrls.captionText.text = "Select TR level from this drop list...";

		if (m_TR2Levels.Length > 1)
		{
			m_dropdownloadUrls.value = 1;
			m_dropdownloadUrls.value = 0;
		}

#if !UNITY_EDITOR && UNITY_WEBGL
    WebGLInput.captureAllKeyboardInput = true;  //
#endif

	}

	private string SanitiesCSVInput(string text)
    {
		return text.Replace("\n", "").Replace("\t", "").Replace("\r", "");
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
			}
		}
		fileBrowserPanel.SetActive(false);
	}
	
	/*protected void Windowf(int id)
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
					
	}*/
	
	public void OnClickWebBrowse_OK()
    {
		ResetDownload();
		m_www = new WWW(Settings.LevelFileUrl + m_TR2Levels[m_dropdownloadUrls.value]);
		m_ButtonWebBrowseOK.interactable = false;
	}

	public void OnClickWebBrowse_Cancel()
    {
		ResetDownload();
		m_www = null;
		m_showingUrlWindow = false;

		m_ButtonWebBrowseOK.interactable = true;
		m_WebBrowserPanel.gameObject.SetActive(false);
	}

	public void OnUrlChanged(TMP_InputField urlField)
    {
		Settings.LevelFileUrl = urlField.text;
	}

    private void ResetDownload()
    {
		m_DownloadProgress = 0;
		m_TextDownloadProgress.text = m_DownloadProgress.ToString("f2") + "%";
		Settings.LoadLevelFileFromUrl = false;
		m_showLevelLoaingMessage = false;
		m_LoadLevel = false;
	}

	private void OnDownloadComplete()
    {
		Settings.LoadLevelFileFromUrl = true;
		m_TextDownloadProgress.text = "Loading downloaded level...";
		StartLevelLoader();
	}

	private void StartLevelLoader()
    {
		m_showLevelLoaingMessage = true;
		m_LoadLevel = true;
	}

    // Update is called once per frame
    void Update () 
	{
		// show status of data download
		//if(Loader.m_RawFileData == null)  // force download by disabling existance check of Loader.m_RawFileData
		//{
		if (m_www != null)
		{
			m_DownloadProgress = m_www.progress * 100f;
			m_TextDownloadProgress.text = m_DownloadProgress.ToString("f2") + "%";
		}

		if (m_www != null && m_www.error == null && m_www.isDone)
		{
			//m_showingUrlWindow = false;
			//PlayerPrefs.SetString("LevelFileUrl", Settings.LevelFileUrl);
			Loader.m_RawFileData = m_www.bytes;
			m_www = null;
			OnDownloadComplete();
		}

		if (m_www != null && m_www.error != null)
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

	public void OnBrowseLevel()
    {
		Settings.LoadDemoLevel = false; //fixed unwated demo level load
		Settings.LoadLevelFileFromUrl = false;

		string m_currentDir = PlayerPrefs.GetString("Level_Path");
		if (m_currentDir == null)
		{
			m_currentDir = Directory.GetCurrentDirectory();
			if(Application.platform == RuntimePlatform.Android)
            {
				Directory.SetCurrentDirectory("/sdcard/");
				m_currentDir = "/sdcard/";
			}
		}
		else if (!Directory.Exists(m_currentDir))
		{
			m_currentDir = Directory.GetCurrentDirectory();
			if (Application.platform == RuntimePlatform.Android)
			{
				Directory.SetCurrentDirectory("/sdcard/");
				m_currentDir = "/sdcard/";
			}
		}


		if (Application.platform == RuntimePlatform.Android)
		{
#if UNITY_ANDROID
			// Use MIMEs on Android
			string[] fileTypes = new string[] {"application/octet-stream" };
#else
			// Use UTIs on iOS
			string[] fileTypes = new string[] { "public.image", "public.movie" };
#endif


#if UNITY_ANDROID

			if (!NativeFilePicker.IsFilePickerBusy())
			{
				NativeFilePicker.Permission permission = NativeFilePicker.PickMultipleFiles((paths) =>
				{
					if (paths == null)
						Debug.Log("Operation cancelled");
					else
					{
						//for (int i = 0; i < paths.Length; i++)
						//Debug.Log("Picked file: " + paths[i]);

						OnSelectedFile(paths[0]);
					}
				}, fileTypes);
			}
#endif
		}
		else
		{

			if (m_fileBrowser != null)
			{
				m_fileBrowser.Refresh();
			}

			Debug.Log(m_currentDir);

			//Create a new file browser with, list view + vertical layout group + prefab
			fileBrowserPanel.SetActive(true);

			string selectionPattern = "*." + Settings.DefaultTR2FileExtension.Trim(new char[] { ' ' });
			m_fileBrowser = new FileBrowser(new Rect(Screen.width * 0.1f, 0, Screen.width - Screen.width * 0.5f, Screen.height - 10), "Select level file (*.tr2) ", m_currentDir, FileSelectedCallback,
				itemprefab,
				itemPrefabFolder,
				addressItemPrefav,
				layout,
				layoutAddress,
				selectionPattern,
				OnItemClicked,
				OnItemAddressClicked);

			//m_fileBrowser.DirectoryImage = m_directoryImage;
			//m_fileBrowser.FileImage = m_fileImage;
			m_showingBrowserWindow = true;

		}


	}


	protected void OnSelectedFile(string file)
    {
		FileSelectedCallback(file);
		StartLevelLoader();
	}


	void OnItemClicked(FileListItem Item)
    {
		//m_currentDir = Directory.GetCurrentDirectory()
		//string filepath = Path.Combine(m_currentDir

		if (Item.Type == FileListItem.ItemType.File)
		{
			Debug.Log("Clicked File Item " + Item.Text);
			OnSelectedFile(Item.Text);
		}
		else
        {
			Debug.Log("Clicked Folder Item " + Item.Text);

			if (m_fileBrowser != null)
			{
				m_fileBrowser.Refresh();
			}

			GotoDirectory(Path.GetDirectoryName(Item.Text));
		}

    }

	void OnItemAddressClicked(AddressItem Item)
    {

		if (Item.Text != string.Empty)
		{
			Debug.Log("On Address Item Clicked" + Item.Text);
			GotoDirectory(Item.Text);
		}
	}

	public void GotoDirectory(string path)
    {
		Debug.Log("Clicked Folder Item " + path);

		if (m_fileBrowser != null)
		{
			m_fileBrowser.Refresh();
		}

		//Create a new file browser with, list view + vertical layout group + prefab
		fileBrowserPanel.SetActive(true);
		m_currentDir = path;

		string selectionPattern = "*." + Settings.DefaultTR2FileExtension.Trim(new char[] { ' ' });
		m_fileBrowser = new FileBrowser(new Rect(Screen.width * 0.1f, 0, Screen.width - Screen.width * 0.5f, Screen.height - 10), "Select level file (*.tr2) ", m_currentDir, FileSelectedCallback,
			itemprefab,
			itemPrefabFolder,
			addressItemPrefav,
			layout,
			layoutAddress,
			selectionPattern,
			OnItemClicked,
			OnItemAddressClicked);
	}

	public void OnClickCancel()
    {
		fileBrowserPanel.SetActive(false);
	}

	public void OnClickOK()
	{
		fileBrowserPanel.SetActive(false);
	}

	public void LoadDemo()
	{

#if UNITY_EDITOR
		Settings.LevelFileLocalPath = Application.dataPath + "/Resources/HILTOP.TR2";
#endif
		Settings.LoadDemoLevel = true;
		Settings.LoadLevelFileFromUrl = false;
		StartLevelLoader();


	}


}