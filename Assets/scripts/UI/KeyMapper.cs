using UnityEngine;
using System.Collections.Generic;

public delegate  void KeyDownDelegate(int statecode, int otherkey, float time);
public delegate  void KeyIdleDelegate(int statecode, float time);
public delegate  bool GoToDelegate(Vector3 pos);

public enum function
{
	Idle = 0,
	//Direction key
	Run = 1,   		
	Back =2,      		
	Left =3,      		
	Right=4,

    //movement key
    Jump=5,
    Walk=6,			

	//Action key
	Action=7,			
	DrawWeapon=8,		
	Roll=9
   		
}

[System.Serializable]
public class KeyMap
{
	public function func = function.Action;
	public KeyCode key = KeyCode.A;
	public int InterfaceCode = 0;
	
	public KeyMap(function f, KeyCode k)
	{
		func = f;
		key = k;
	}
}

public class KeyMapper : MonoBehaviour {
	
	public static KeyCode[] defaultkeycodes ={
	
		KeyCode.Escape,
		//Direction key
		KeyCode.UpArrow,
		KeyCode.DownArrow,
		KeyCode.LeftArrow,
		KeyCode.RightArrow,
	
		//movement key
        KeyCode.Mouse1,
        KeyCode.Keypad5,
		//Action key
		KeyCode.Mouse0,
		KeyCode.KeypadEnter,
		KeyCode.RightShift
	};
	
	public static KeyMap[] Keys = null;
	public static event  KeyDownDelegate OnKeyDown;
	public static event  KeyIdleDelegate OnKeyIdle;
	public static event  GoToDelegate OnGoTo;
	public static bool usedefaultmap = true;
	public static bool reset = false;
	
	//Generated player state codes from key Codes
	public static int PrimaryAction = 0;
	public static int Search = 0;
	public static int Menu = 0;
	public static int DrawWeapon = 0;
	public static int PickupFlare = 0;
	public static int Roll = 0;
	public static int StepToLeft = 0;
	public static int StepToRight = 0;
	public static int Walk = 0;
	public static int Jump = 0;
	public static int Idle = 0;
	
	public static int Run = 0;
	public static int Down = 0;
	public static int Left = 0;
	public static int Right = 0;
	
	//compound code
	public static int JumpAir = 0;
	public static int JumpAirStanding = 0;
	public static int jumpBack = 0;
	public static int FlipBack = 0;
	public static int JumpRight = 0;
	public static int JumpLeft = 0;

	//AI generated code
	public static int Grab = 0;
	public static int PullUpLow = 0;
	public static int PullUpHigh = 0;
	public static int PullUpAcrobatic = 0;
	public static int WalkUp = 0;
    public static int FreeFall = 0;
    public static int DiveIntoWater = 0;
    public static int DiveInAir = 0;



    int	keypadmovement = 0;
	int	keypadaction = 0;
	int dpad = 0;
	int statecode = 0;

	int keyshift = 0; // determine key order
	int keycount = 0;
	
	public static bool active = false;
	//public List<int> keystack;
	
	// Use this for initialization
	void Start () {
	
		keycount = defaultkeycodes.Length;
		InitKeyCode();
	}
	
	bool GetKeyCode(function f)
	{
        int ikey = (int)f;
        if(ikey >= Keys.Length) //fixed indexing bug
        {
            ikey = 0;
        }
        return Input.GetKey(Keys[ikey].key);
	}
	
	bool GetKeyUpCode(function f)
	{
		return Input.GetKeyUp(Keys[(int)f].key);
	}
	
	void HandleIdleKey(int code)
	{
		if(code == 0)
		{
			if(OnKeyIdle !=null) OnKeyIdle(code, Time.time );
		}
	}
	void HandleKey(int code, int otherkey)
	{
		//Debug.Log("Regular Key");
		if(OnKeyDown !=null) OnKeyDown(code , otherkey, Time.time);
	}
	
	static public void InitKeyCode()
	{
		int nkey = defaultkeycodes.Length;
		Keys = new KeyMap[nkey];
		for(int i = 0; i < nkey; i++)
		{
			 Keys[i] = new KeyMap((function) i, defaultkeycodes[i]);
		}
		// [d][d][d][d][m][m][a][a];
		//2 bit action key
		for(int i = 0 ; i < nkey; i++)
		{
			Keys[i].InterfaceCode = i;
		}
		/*//2 bit movement key
		for(int i = nkey - 6; i < nkey - 4; i++)
		{
			int shift  = (i - nkey + 6) + 2;
			Keys[i].InterfaceCode =  1 << shift;
		}
		//4 bit direction key	
		for(int i = nkey - 4; i < nkey; i++)
		{
			int shift  = (i - nkey + 4) + 4;
			Keys[i].InterfaceCode = 1 << shift;
		}*/

		//generate player state code from key codes
		Idle = (int) function.Idle;
		
		Run =  (int)function.Run;   //code 1
		Down = (int)function.Back;  //code 2
        Left = (int)function.Left;  //code 3
        Right = (int)function.Right;//code 4
        Jump = (int)function.Jump;  //code 5
    
		PrimaryAction =(int)function.Action;
		DrawWeapon = (int)function.DrawWeapon;
		Roll = (int)function.Roll;

        //Search = (int)function.Search;
        //Menu = (int)function.Menu;
        //PickupFlare = (int)function.Flare;
        //StepToLeft = (int)function.StepToLeft;
        //StepToRight = (int)function.StepToRight;

        //compound code
        Walk = (int)function.Walk + (int)function.Walk * Run; //code 12
        JumpAir = Jump + Jump * Run; //code 10
        jumpBack = Jump + Jump * Down; //code 15
        JumpLeft =  Jump + Jump * Left; //code 20
        JumpRight = Jump + Jump * Right; //code 25

        //custom AI code goes after UI codes
        FreeFall = 11;
        DiveIntoWater = 13;
        DiveInAir = 14;
        JumpAirStanding = 16;
        WalkUp = 17;

        Grab = 26;
        PullUpLow = 27;
        PullUpHigh = 28;
        PullUpAcrobatic = 29;
     
      


    Debug.Log("Key Mapper StateCode: Run " + Run);
		Debug.Log("Key Mapper StateCode: Jump " + Jump);
		Debug.Log("Key Mapper StateCode: Idle " + Idle);
		Debug.Log("Key Mapper StateCode: PrimaryAction " + JumpLeft);
		active = true;

	}
	
	void ClearKey()
	{
		if(dpad != 0)
		{
			for(int i = 1; i <= 4; ++i)
			{
				function func = (function) i;
				if(GetKeyUpCode(func) )
				{
					dpad = 0;
					statecode = 0;
					HandleIdleKey(statecode);
					break;
				}
			}
		}
		
		if(keypadmovement != 0)
		{
			for(int i = 5; i <=6; ++i)
			{
				function func = (function) i;
				if(GetKeyUpCode(func) )
				{
					keypadmovement = 0;
					statecode = 0;
					HandleIdleKey(statecode);
					break;
				}
			}
		}
		
		
		if(keypadaction != 0)
		{
			for(int i = 7 ; i <= 9; ++i)
			{
				function func = (function) i;
				if(GetKeyCode(func) )
				{
					keypadaction = 0;
					statecode = 0;
					HandleIdleKey(statecode);
					break;
				}
			}
		}
	}
	
	void HandleKeyOrder()
	{
		//2 bit action key
		//2 bit movement key
		//4 bit direction key	
	    // [d][d][d][d][m][m][a][a]	
	    
		
		if(reset)
		{
			keypadmovement = 0;
			keypadaction = 0;
	 		dpad = 0;
	 		statecode = 0;
			reset = false;
		}
		
		if(dpad == 0) //if not pressed
		{
			for(int i = 1; i <= 4; ++i)
			{
				function func = (function) i;
				if(GetKeyCode(func) )
				{
					dpad = i;
					statecode = dpad;
					HandleKey(statecode,0x0);
					break;
				}
			}
		}
		
		if(keypadmovement == 0)
		{
			for(int i = 5; i <= 6; ++i)
			{
				function func = (function) i;
				if(GetKeyCode(func) )
				{
					keypadmovement = i;
					statecode = keypadmovement + (dpad * keypadmovement);
					HandleKey(statecode,0x0);
					break;
				}
			}
		}
		
		if(keypadaction == 0)
		{
			for(int i = 7 ; i <= 9; ++i)
			{
				function func = (function) i;
				if(GetKeyCode(func) )
				{
					keypadaction = i;
					statecode = keypadaction;
					HandleKey(statecode,0x0);
					break;
				}
			}
		}
		
		IsDoubleClick();

	}

	int clickcount = 0;
	void IsDoubleClick()
	{
		if(Input.GetMouseButtonDown(0))
		{
			if(clickcount == 0)
			{
				Invoke("WaitForClick", 0.5f);
			}
			clickcount++;
		}
	}

	void WaitForClick()
	{
		if(clickcount >= 2)
		{
			int statecode = Keys [defaultkeycodes.Length - 4].InterfaceCode;
			HandleKey(Run,keypadmovement);
		}
		clickcount = 0;
	}

	
	void Update()
	{
        if (Keys == null) return; //bug fix 
		HandleKeyOrder();
		ClearKey();
	}
	
}
