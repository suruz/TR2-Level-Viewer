using UnityEngine;
using System.Collections.Generic;

public delegate  void KeyDownDelegate(int statecode, int otherkey, float time);
public delegate  void KeyIdleDelegate(int statecode, float time);
public delegate  bool GoToDelegate(Vector3 pos);

public enum function
{
	//Action key
	Idle = 0,		
	Action,			
	DrawWeapon,		
	Roll,			
	//movement key
	Walk,			
	Jump,	
	//Direction key
	Run ,   		
	Back,      		
	Left,      		
	Right,     		
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
		
		//Action key
		KeyCode.Escape,
		KeyCode.Mouse0,
		KeyCode.KeypadEnter,
		KeyCode.RightShift,
		//movement key
		KeyCode.Keypad5,
		KeyCode.Mouse1,
		//Direction key
		KeyCode.UpArrow,
		KeyCode.DownArrow,
		KeyCode.LeftArrow,
		KeyCode.RightArrow,
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
	
	public static int Run = 16;
	public static int Down = 32;
	public static int Left = 64;
	public static int Right = 128;
	
	//compound code
	public static int JumpAir = 0;
	public static int JumpAirStanding = 0;
	public static int jumpBack = 0;
	public static int FlipBack = 0;
	public static int JumpRight = 0;
	public static int JumpLeft = 0;

	//AI generated code
	public static int Grab = 256 + 2;
	public static int PullUpLow = 256 + 4;
	public static int PullUpHigh = 256 + 8;
	public static int PullUpAcrobatic = 256 + 16;
	public static int WalkUp = 256 + 32;
	

	int[] dpadmap = {16,32,64,128};
	int	keypadmovement = 0;
	int	keypadaction = 0;
	int dpad = 0;
	int statecode = 0;
	int activestate = 0;

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
		return Input.GetKey(Keys[(int)f].key);
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
		for(int i = 0 ; i < 4; i++)
		{
			Keys[i].InterfaceCode = i;
		}
		//2 bit movement key
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
		}

		//generate player state code from key codes
		Idle = (int) function.Idle;
		PrimaryAction =(int)function.Action;
		DrawWeapon = (int)function.DrawWeapon;
		Roll = (int)function.Roll;
		
		Run =  1 <<(((int)function.Run - nkey + 4) + 4);
		Down = 1 <<(((int)function.Back - nkey + 4) + 4);
		Left = 1 <<(((int)function.Left - nkey + 4) + 4);
		Right = 1 <<(((int)function.Right - nkey + 4) + 4);
		
		Walk = Run | (1 << (((int)function.Walk - nkey + 6) + 2));
		Jump = 1 << (((int)function.Jump - nkey + 6) + 2);
		
		//Search = (int)function.Search;
		//Menu = (int)function.Menu;
		//PickupFlare = (int)function.Flare;
		//StepToLeft = (int)function.StepToLeft;
		//StepToRight = (int)function.StepToRight;
		
		//compound code
		JumpAir = Run | Jump;
		JumpAirStanding = JumpAir + 256;
		
		jumpBack = Down;
		FlipBack = Down | Jump;
		
		JumpRight = Right | Jump;
		JumpLeft =  Left | Jump;

		Debug.Log("Key Mapper StateCode: JumpAir" + JumpAir);
		active = true;

	}
	
	void ClearKey()
	{
		if(keypadaction != 0)
		{
			for(int i = 0; i < 4; i++)
			{
				function func = (function) i;
				if(GetKeyUpCode(func) )
				{
					keypadaction = 0;
					statecode = statecode & 0xfc;
					HandleIdleKey(statecode);
					break;
				}
			}
		}
		
		if(keypadmovement != 0)
		{
			for(int i = keycount - 6; i < keycount - 4; i++)
			{
				function func = (function) i;
				if(GetKeyUpCode(func) )
				{
					keypadmovement = 0;
					statecode = statecode & ~Keys[i].InterfaceCode;
					HandleIdleKey(statecode);
					break;
				}
			}
		}
		
		
		if(dpad != 0)
		{
			for(int i = keycount - 4; i < keycount; i++)
			{
				function func = (function) i;
				if(GetKeyUpCode(func) )
				{
					dpad = 0;
					statecode = statecode & ~Keys[i].InterfaceCode;
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
	 		activestate = 0;
			reset = false;
		}
		
		if(keypadaction == 0)
		{
			for(int i = 0 ; i < 4; i++)
			{
				function func = (function) i;
				if(GetKeyCode(func) )
				{
					keypadaction = i;
					statecode |= Keys[i].InterfaceCode;
					HandleKey(statecode,0x0);
					//Debug.Log(func);
					break;
				}
			}
		}
		
		if(keypadmovement == 0)
		{
			for(int i = keycount - 6; i < keycount - 4; i++)
			{
				function func = (function) i;
				if(GetKeyCode(func) )
				{
					keypadmovement = i;
					statecode |= Keys[i].InterfaceCode ;
					HandleKey(statecode,dpad);
					//Debug.Log(func);
					break;
				}
			}
		}
		
		
		if(dpad == 0)
		{
			for(int i = keycount - 4; i < keycount; i++)
			{
				function func = (function) i;
				if(GetKeyCode(func) )
				{
					dpad = i;
					statecode |= Keys[i].InterfaceCode;
					HandleKey(statecode,keypadmovement);
					//Debug.Log(func);
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
			HandleKey(statecode,keypadmovement);
		}
		clickcount = 0;
	}

	
	void Update()
	{
		HandleKeyOrder();
		ClearKey();
	}
	
}
