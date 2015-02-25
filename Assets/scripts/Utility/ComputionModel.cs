using UnityEngine;
using System.Collections;

public class ComputionModel  {

	static int[]  STACK = new int[256];
	static int SP = 0;
	
	public static int Push(int data)
	{
		if(SP < 255)
		{
			SP = SP + 1;
		}
		STACK[SP] = data;
		return STACK[SP] ;
	}
	
	public static int Pop()
	{
		int retval = STACK[SP];
		STACK[SP]  = 0;
		if(SP > 0)
		{
			SP = SP - 1;
		}
		return retval;
	}

	public static void StackInit()
	{
		SP = 0;
		for(int i = 0; i < 256; i++)
		{
			STACK[i] = 0;
		}
	}
}
