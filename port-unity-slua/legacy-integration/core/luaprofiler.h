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

/*
	不需要额外定义DLL_API，直接使用LUA_API即可
	2016-08-12 lennon.c
*/
LUA_API void init_profiler(lua_State *L);
LUA_API int add_profiler(int x,int y);
LUA_API void frame_profiler(int id, int unitytime);
LUA_API void register_callback(void* pcallback);
LUA_API int isregister_callback();
LUA_API void unregister_callback();

