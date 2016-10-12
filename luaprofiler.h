/*
** LuaProfiler
** Copyright Kepler Project 2005-2007 (http://www.keplerproject.org/luaprofiler)
** $Id: luaprofiler.h,v 1.4 2007-08-22 19:23:53 carregal Exp $
*/

/*****************************************************************************
luaprofiler.h:
    Must be included by your main module, in order to profile Lua programs
*****************************************************************************/
#include "lua.h"
#include "lauxlib.h"
#define DLL_API __declspec(dllexport) 

DLL_API void init_profiler(lua_State *L);

DLL_API void frame_profiler(int id, int unitytime);

DLL_API void register_callback(void* pcallback);

DLL_API int isregister_callback();

DLL_API void unregister_callback();

DLL_API int add_profiler(int x,int y);

