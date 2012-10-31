
using UnityEngine;

using System;
using System.Collections;
using System.Runtime.InteropServices;

public class luaTest : MonoBehaviour {

	[DllImport ("lua521")] public static extern IntPtr luaL_newstate();
	[DllImport ("lua521")] public static extern void luaL_openlibs(IntPtr lua_State);
	[DllImport ("lua521")] public static extern void lua_close(IntPtr lua_State);
	[DllImport ("lua521")] public static extern void lua_pushcclosure(IntPtr lua_State, LuaFunction func, int n);
	[DllImport ("lua521")] public static extern void lua_setglobal(IntPtr lua_State, string s);
	[DllImport ("lua521")] public static extern int lua_pcallk(IntPtr lua_State, int nargs, int nresults, int errfunc, int ctx, LuaFunction func);
	[DllImport ("lua521")] public static extern int luaL_loadfilex(IntPtr lua_State, string s, string mode);
	[DllImport ("lua521")] public static extern int luaL_loadstring(IntPtr lua_State, string s);
	[DllImport ("lua521")] public static extern IntPtr luaL_checklstring(IntPtr lua_State, int idx, IntPtr len);

	[DllImport ("lua521")] public static extern int lua_rawgeti(IntPtr lua_State, int idx0, int idx1);
	[DllImport ("lua521")] public static extern int luaL_ref(IntPtr lua_State, int idx);
	[DllImport ("lua521")] public static extern int luaL_unref(IntPtr lua_State, int idx0, int idx1);
	[DllImport ("lua521")] public static extern int lua_pushnumber(IntPtr lua_State, double num);

	//------------------------------------
	// Lua
	//

	//註冊 C# Function 讓 Lua 可以呼叫執行
	public delegate int LuaFunction(IntPtr pLuaState);
	public static void lua_register(IntPtr pLuaState, string strFuncName, LuaFunction pFunc)
	{
	    lua_pushcclosure(pLuaState, pFunc, 0);
	    lua_setglobal(pLuaState, strFuncName);
	}

	//執行 Lua 檔案
	public static int luaL_dofile(IntPtr lua_State, string s)
	{
	    if (luaL_loadfilex(lua_State, s, null) != 0)
	        return 1;
	 
	    return lua_pcallk(lua_State, 0, -1, 0, 0, null);
	}

	//執行 Lua Callback Function
	public static int luaL_doCallBackFromCBIdx(IntPtr lua_State, int idx, double num)
	{
		lua_rawgeti(lua_State, -1000000-1000, idx);
		lua_pushnumber(lua_State, num);
		int ret =lua_pcallk(lua_State, 1, 0, 0, 0, null);
		if (ret !=0)
		{
			//Error
			//...
		}
		luaL_unref(lua_State, -1000000-1000, idx);

		return ret;
	}


	//------------------------------------
	// Application
	//
	public static int nLuaCBFuncIdx =-1;
	public static IntPtr m_luaState = IntPtr.Zero;

	public static int tellMeNum(IntPtr pLuaState)
	{
		//取得回呼函數堆疊上的 index
		nLuaCBFuncIdx =luaL_ref(pLuaState, -1000000-1000);
	    return 0;
	}

	public static int msg(IntPtr pLuaState)
	{
		IntPtr retPtr =luaL_checklstring(pLuaState, 1, IntPtr.Zero);
		string retStr = System.Runtime.InteropServices.Marshal.PtrToStringAnsi(retPtr);
		
    	Debug.Log(retStr);
    	return 0;
	}

	public static string GetAppPath()
	{
	    return Application.dataPath.Substring(0, Application.dataPath.Length-6);
	}	

	// Use this for initialization
	void Start () {
		
		//初始化 Lua
	    m_luaState = luaL_newstate();
	    luaL_openlibs(m_luaState);

	    //註冊 C# Function
	    lua_register(m_luaState, "tellMeNum", tellMeNum);
	    lua_register(m_luaState, "msg", msg);

	    //執行 Lua 檔案
	    string strPath =GetAppPath() + "LuaScript/lua.txt";
	    luaL_dofile(m_luaState, strPath);
	}
	
	// Update is called once per frame
	void Update () {

		if (nLuaCBFuncIdx !=-1)
		{
			luaL_doCallBackFromCBIdx(m_luaState, nLuaCBFuncIdx, 12345.678);
			nLuaCBFuncIdx =-1;
		}
	
	}

	void OnApplicationQuit()
	{
		lua_close(m_luaState);	
	}

}
