using UnityEngine;
using System.Collections;

public class Settings  {
	
	public static bool ShowObjectID = false; 		// Marks all movable tr2 object with ID in scene
	// These IDs are useful for detecting
	// individual tr2 movable item.
	
	public static bool ForceOpenAllDoors = true;    //Force open all doors in level
	public static bool ForceDisableAllBoulder = false;  //Force disable all boulder in level
	
	public static bool EnableIndoorShadow = false; 	//If enabled indoor objects will cast shadow
	public static bool PlatformUnityPro = true;   	//If enabled Unity Pro features will be used,
													//texture transparancy will be eabled.
													//Otherwise Unity Free features will be used.
													//If you dont have unity pro set it to false
	
	public static float DayNightTimeSpeed = 0.1f;   //Must be >=0
													//When 0 Day Night changes will not happen
	public static float DayLightIntensity = 0.65f;  	//Controls amount of day light
	
	public static bool LoadLevelFileFromUrl = false; //If enabled system will load level from url
													 //specified by LevelFileUrl. Url could be either
													 //file: url or http:
													 //otherwise system will load level from
													 //LevelFileLocalPath
	public static string LevelFileUrl = "https://raw.githubusercontent.com/andrewsyc/Tomb-Raider-1-2-3-4-5-Map-viewer-and-levels/master/Tomb-Raider-2-The-Golden-Mask/";
	public static string LevelFileLocalPath = "Custom Demo Files/HILTOP.TR2";	//LevelFileLocalPath is default file path 
																				//that system tries lo load if
																				//if not specified otherwise

	public static string LevelDownloadCSV =         //Levels to download from specified LevelFileUrl,
													//Feel free to change your LevelFileUrl, example http://level_server.com/
													//Level files must be in folder specified by LevelFileUrl, example example http://level_server.com/mylevel.tr2
													//Also, feel free to make you own comma separated  custom level list, that could be found in LevelFileUrl 
		@"01-The-Cold-War.tr2,
		02-Fools-Gold.tr2,
		03-Furnace-of-the-Gods.tr2,
		04-Kingdom.tr2,
		05-Nightmare-in-Vegas.tr2";


	public static string DefaultTR2FileExtension = "tr2";
													//This is default Tomb Raider 2 file extension
													//that file browser will look up.
	public static bool LoadDemoLevel = false;
    public static float SceneScaling = 0.0009765625f; //Scaling support for GI and Physics in Unity 5 and higher


	public static bool bUseFirstPersonCamera = false; // Set it true for first person VR view, otherwise set it false for third person view.


}
