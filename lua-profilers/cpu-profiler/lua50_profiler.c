/*
** LuaProfiler
** Copyright Kepler Project 2005-2007 (http://www.keplerproject.org/luaprofiler)
** $Id: lua50_profiler.c,v 1.16 2008-05-20 21:16:36 mascarenhas Exp $
*/

/*****************************************************************************
lua50_profiler.c:
   Lua version dependent profiler interface
*****************************************************************************/
/*
	解决跨平台编译时宏控制import/export的问题 lennon.c
	2016-08-11 lennon.c
*/ 
#define LUA_CORE
 
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include "lp.h"
#if defined(linux)
#define __USE_GNU 1
#include <dlfcn.h>
#endif

#include "clocks.h"
#include "core_profiler.h"
#include "function_meter.h"

#include "lua.h"
#include "lauxlib.h"
#include "luaprofiler.h"
#include "queue.h"


/* Indices for the main profiler stack and for the original exit function */
static int exit_id = 0;
static int profstate_id = 0;
static int is_pause = 0;
static int is_start = 0;

static int lua_nTableCount = 0;
static int lua_nFuncCount = 0;
static int lua_nThreadCount = 0;
static int lua_nUserDataCount = 0;


double stat_hook_cost_ts = 0;
double stat_frame_cost_ts = 0;
int    stat_hook_call_cnt = 0;

static int g_multithread = 1;
lprofP_STATE* g_S = NULL;
// �������У�һ��һ��������
QUEUE g_nolock_queue;

//g_fHookAllocFile = NULL;
//g_iHookFunc = 0;
//g_nHookSize = 0;

#define TABLE 1
#define FUNCTION 2
#define SOURCE 3
#define THREAD 4
#define USERDATA 5
#define MARK 6

#define mark_function_env(L,dL,t)
//static int profresult_id;

/* Forward declaration */
static int profiler_stop(lua_State *L);
static int profiler_frame(lua_State *L);
static void mark_object(lua_State *L, lua_State *dL, const void * parent, const char * desc);

#if defined(linux)
	#define DWORD int
	#define WINAPI
#endif


DWORD WINAPI thread_run(void* data);

static FILE* g_fMemorySnapshot = NULL;
//static int profiler_clear(lua_State *L);

void handle_dbg_info(lprof_DebugInfo* dbg_info) {

	if (dbg_info->type == FUNCTION_HOOK) {
		if (!dbg_info->event) {
			/* entering a function */
			lprofP_callhookIN(g_S, (char *)dbg_info->p_name,
				(char *)dbg_info->p_source, dbg_info->linedefined,
				dbg_info->currentline, (char *)dbg_info->p_what, dbg_info->ccallname, dbg_info);
		}
		else { /* ar->event == "return" */
			lprofP_callhookOUT(g_S, dbg_info);
		}

	}
	else if (dbg_info->type == FRAME) {
		lprofT_frame(dbg_info->frameid, dbg_info->unitytime, dbg_info->framecs, dbg_info->hook_cost_cs, dbg_info->hook_call_cnt);
	}
	else {

		printf("Error type=%d!", dbg_info->type);
	}

	free(dbg_info);
}

#if defined(linux)
static const char* get_base_name(const char* fullpath){

	if (NULL == fullpath)
		return "";
	
	int len = strlen(fullpath);

	if(len == 0)
		return fullpath;

	int i = 0;
	
	for(i = len - 1; i > 0; i--){

		if(fullpath[i] == '/'){
			if(i < len - 1){
				return fullpath + i + 1;
			}
		}
	}

	return fullpath;
	
}
#endif


void dispatch_dbg_info(lprof_DebugInfo* dbg_info) {

	if (g_multithread == 1) {
		queue_push_without_alloc(&g_nolock_queue, dbg_info);
	}
	else {
		handle_dbg_info(dbg_info);
	}
}


/* called by Lua (via the callhook mechanism) */
static void callhook(lua_State *L, lua_Debug *ar) {

  LARGE_INTEGER nBeginTime ;

  lprofC_start_timer2(&nBeginTime);

  int currentline;
  lua_Debug previous_ar;

  int stackIndex = -1;


  if (lua_getstack(L, 1, &previous_ar) == 0) {
    currentline = -1;
  } else {
    lua_getinfo(L, "l", &previous_ar);
    currentline = previous_ar.currentline;
  }
      
  lua_getinfo(L, "nSf", ar);

  

  lprof_DebugInfo* dbg_info = (lprof_DebugInfo*)malloc(sizeof(lprof_DebugInfo));
  dbg_info->type = FUNCTION_HOOK;
  LPROF_COPY_LUA_DEBUG_INFO(dbg_info, ar, name, LP_MAX_NAME_LEN);
  LPROF_COPY_LUA_DEBUG_INFO(dbg_info, ar, source, LP_MAX_SOURCE_LEN);
  LPROF_COPY_LUA_DEBUG_INFO(dbg_info, ar, what, LP_MAX_WHAT_LEN);  
  dbg_info->event = ar->event;
  dbg_info->currentline = currentline;
  dbg_info->ccallname[0] = '\0';
  dbg_info->linedefined = ar->linedefined;
  
  lprofC_start_timer2(&dbg_info->currenttime);

#if defined(linux)
  if(ar->source &&  strcmp(ar->source, "=[C]") == 0)
  { 
  		dbg_info->ccallname[99] = '\0'; 
     	void* cfun = lua_tocfunction(L, -1);
		Dl_info info;
		dladdr(cfun, &info);        
    	int xx =  (int)((unsigned  int)cfun - (unsigned int )info.dli_fbase);
        snprintf(dbg_info->ccallname, 98, "(CFUN(%s 0x%x %p))\0",get_base_name( info.dli_fname), xx,  cfun );
  }
#endif

  lua_pop(L, 1);

  //stackIndex = lua_gettop(L);

  dispatch_dbg_info(dbg_info);

  stat_hook_cost_ts += lprofC_get_seconds2(&nBeginTime);
  stat_hook_call_cnt += 1;

}


/* Lua function to exit politely the profiler                               */
/* redefines the lua exit() function to not break the log file integrity    */
/* The log file is assumed to be valid if the last entry has a stack level  */
/* of 1 (meaning that the function 'main' has been exited)                  */
static void exit_profiler(lua_State *L) {
  lprofP_STATE* S;
  lua_pushlightuserdata(L, &profstate_id);
  lua_gettable(L, LUA_REGISTRYINDEX);
  S = (lprofP_STATE*)lua_touserdata(L, -1);
  /* leave all functions under execution */
 // while (lprofP_callhookOUT(S)) ;
  /* call the original Lua 'exit' function */
  lua_pushlightuserdata(L, &exit_id);
  lua_gettable(L, LUA_REGISTRYINDEX);
  lua_call(L, 0, 0);
}

/* Our new coroutine.create function  */
/* Creates a new profile state for the coroutine */
#if 0
static int coroutine_create(lua_State *L) {
  lprofP_STATE* S;
  lua_State *NL = lua_newthread(L);
  luaL_argcheck(L, lua_isfunction(L, 1) && !lua_iscfunction(L, 1), 1,
		"Lua function expected");
  lua_pushvalue(L, 1);  /* move function to top */
  lua_xmove(L, NL, 1);  /* move function from L to NL */
  /* Inits profiler and sets profiler hook for this coroutine */
  S = lprofM_init();
  lua_pushlightuserdata(L, NL);
  lua_pushlightuserdata(L, S);
  lua_settable(L, LUA_REGISTRYINDEX);
  lua_sethook(NL, (lua_Hook)callhook, LUA_MASKCALL | LUA_MASKRET, 0);
  return 1;	
}
#endif

static int profiler_pause(lua_State *L) {
  lprofP_STATE* S;
  lua_pushlightuserdata(L, &profstate_id);
  lua_gettable(L, LUA_REGISTRYINDEX);
  S = (lprofP_STATE*)lua_touserdata(L, -1);
  lprofM_pause_function(S);
  is_pause = 1;
  return 0;
}

static int profiler_resume(lua_State *L) {
  lprofP_STATE* S;
  lua_pushlightuserdata(L, &profstate_id);
  lua_gettable(L, LUA_REGISTRYINDEX);
  S = (lprofP_STATE*)lua_touserdata(L, -1);
  lprofM_pause_function(S);
  is_pause = 0;
  return 0;
}





static int profiler_start(lua_State *L) {

	if(is_start){
		return 0;
	}

	
	lprofP_STATE* S;
	const char* outfile;

	lua_pushlightuserdata(L, &profstate_id);
	lua_gettable(L, LUA_REGISTRYINDEX);
	if(!lua_isnil(L, -1)) {
	profiler_stop(L);
	}
	lua_pop(L, 1);

	outfile = NULL;
	if(lua_gettop(L) >= 1)
		outfile = luaL_checkstring(L, 1);

	if (lua_gettop(L) >= 2)
		g_multithread = lua_tointeger(L, 2);

	printf("\n[profiler_start] outfile=%s,g_multithread=%d\n", outfile, g_multithread);

#if defined(linux)	
    #include <sys/types.h>
    #include <unistd.h>
	char tmpbuff[256];

	static int s_index = 0;

	sprintf(tmpbuff, "%s.%d.%d", outfile, (int)GETPID(), ++s_index);

	outfile = tmpbuff;
#endif

	/* init with default file name and printing a header line */
	if (!(S=lprofP_init_core_profiler(outfile, 1, 0))) {
	return luaL_error(L,"LuaProfiler error: output file could not be opened!");
	}

	g_S = S;

	lua_sethook(L, (lua_Hook)callhook, LUA_MASKCALL | LUA_MASKRET, 0);

	lua_pushlightuserdata(L, &profstate_id);
	lua_pushlightuserdata(L, S);
	lua_settable(L, LUA_REGISTRYINDEX);
	
	/* use our own exit function instead */
	lua_getglobal(L, "os");
	lua_pushlightuserdata(L, &exit_id);
	lua_pushstring(L, "exit");
	lua_gettable(L, -3);
	lua_settable(L, LUA_REGISTRYINDEX);
	lua_pushstring(L, "exit");
	lua_pushcfunction(L, (lua_CFunction)exit_profiler);
	lua_settable(L, -3);

	/* the following statement is to simulate how the execution stack is */
	/* supposed to be by the time the profiler is activated when loaded  */
	/* as a library.                                                     */

	if (g_multithread == 1) {

		queue_init(&g_nolock_queue, 0);
		queue_reserved(&g_nolock_queue, NONLOCK_QUEUE_SIZE);

	#if defined(linux)
		pthread_t newthread; 
		pthread_create(&newthread, 0, (void *(*)(void *))thread_run, 0);
	#else
		CreateThread(NULL, 0, thread_run, NULL, 0, NULL);
	#endif

	}


	lprof_DebugInfo* dbg_info = (lprof_DebugInfo*)malloc(sizeof(lprof_DebugInfo));

	dbg_info->type = FUNCTION_HOOK;
	
	strcpy(dbg_info->name, "profiler_start");
	dbg_info->p_name = dbg_info->name;
	
	strcpy(dbg_info->what,"C");
	dbg_info->p_what = dbg_info->what;
	
	strcpy(dbg_info->source,"(C)");
	dbg_info->p_source= dbg_info->source; 
	dbg_info->event = 0;
	dbg_info->currentline = -1;
	dbg_info->ccallname[0] = '\0';
 
	dispatch_dbg_info(dbg_info);

	//lprofP_callhookIN(S, "profiler_start", "(C)", -1, -1,"C", "\0");
	is_start = 1;
	lua_pushboolean(L, 1);
	return 1;
}


static int is_profiler_pause(lua_State *L)
{
	lua_pushboolean(L, is_pause);
	return 1;
}

static int profiler_stop(lua_State *L) {
	lprofP_STATE* S;
	lua_sethook(L, (lua_Hook)callhook, 0, 0);
	lua_pushlightuserdata(L, &profstate_id);
	lua_gettable(L, LUA_REGISTRYINDEX);
	if(!lua_isnil(L, -1)) 
	{
		S = (lprofP_STATE*)lua_touserdata(L, -1);
		/* leave all functions under execution */
		//while (lprofP_callhookOUT(S))
			;
		lprofP_close_core_profiler(S);
		lua_pushlightuserdata(L, &profstate_id);
		lua_pushnil(L);
		lua_settable(L, LUA_REGISTRYINDEX);
		lua_pushboolean(L, 1);
	} 
	else 
	{ 
		lua_pushboolean(L, 0); 
	}
	is_start = 0;
  return 1;
}

static int profiler_frame(lua_State *L)
{
	int frameid = 0;
	int frametime = 0;
	if(is_start == 1)
	{
		lua_pushlightuserdata(L, &profstate_id);
		lua_gettable(L, LUA_REGISTRYINDEX);
		frameid = (int)luaL_checkinteger(L,-2);
		frametime = (int)luaL_checkinteger(L,-1);

		lprof_DebugInfo* dbg_info = (lprof_DebugInfo*)malloc(sizeof(lprof_DebugInfo));
		dbg_info->type = FRAME;
		dbg_info->frameid = frameid;
		dbg_info->unitytime = frametime;	
		dbg_info->hook_cost_cs = stat_hook_cost_ts;
		dbg_info->hook_call_cnt = stat_hook_call_cnt;
		stat_hook_cost_ts = 0;
		stat_hook_call_cnt = 0;
		dispatch_dbg_info(dbg_info);
		//lprofT_frame(frameid,frametime);
	}
	return 0;
}

/************************************************************************/
/* Lua memory snapshot                                                  */
/************************************************************************/
static int ismarked(lua_State *dL, const void *p) {
	lua_rawgetp(dL, MARK, p);
	if (lua_isnil(dL,-1)) {
		lua_pop(dL,1);
		lua_pushboolean(dL,1);
		lua_rawsetp(dL, MARK, p);
		return 0;
	}
	lua_pop(dL,1);
	return 1;
}

static const void *readobject(lua_State *L, lua_State *dL, const void *parent, const char *desc) {
	int t = lua_type(L, -1);
	const void * p = lua_topointer(L, -1);
	int tidx = 0;
	switch (t) {
	case LUA_TTABLE:
		tidx = TABLE;
		break;
	case LUA_TFUNCTION:
		tidx = FUNCTION;
		break;
	case LUA_TTHREAD:
		tidx = THREAD;
		break;
	case LUA_TUSERDATA:
		tidx = USERDATA;
		break;
	default:
		return NULL;
	}

	if (ismarked(dL, p)) {
		lua_rawgetp(dL, tidx, p);
		if (!lua_isnil(dL,-1)) {
			lua_pushstring(dL,desc);
			lua_rawsetp(dL, -2, parent);
		}
		lua_pop(dL,1);
		lua_pop(L,1);
		return NULL;
	}

	lua_newtable(dL);
	lua_pushstring(dL,desc);
	lua_rawsetp(dL, -2, parent);
	lua_rawsetp(dL, tidx, p);

	return p;
}

static const char *keystring(lua_State *L, int index, char * buffer) {
	int t = lua_type(L,index);
	switch (t) {
	case LUA_TSTRING:
		return lua_tostring(L,index);
	case LUA_TNUMBER:
		sprintf(buffer,"[%lg]",lua_tonumber(L,index));
		break;
	case LUA_TBOOLEAN:
		sprintf(buffer,"[%s]",lua_toboolean(L,index) ? "true" : "false");
		break;
	case LUA_TNIL:
		sprintf(buffer,"[nil]");
		break;
	default:
		sprintf(buffer,"[%s:%p]",lua_typename(L,t),lua_topointer(L,index));
		break;
	}
	return buffer;
}

static void mark_table(lua_State *L, lua_State *dL, const void * parent, const char * desc) {
	int type = lua_type(L, -1);
	const void * t = readobject(L, dL, parent, desc);
	int weakk = 0;
	int weakv = 0;
	const char *mode = NULL;
	const char *key = NULL;
	char* name = NULL;
	char* value = NULL;
	char addr[32];
	if (t == NULL)
		return;
	sprintf(addr, "%p",t );
	//output("%p:%s\n", t, desc);
	name = (char*)malloc(sizeof(char)*(strlen(desc) + strlen(addr) + 4));
	sprintf(name, "%p:%s", t, desc);
	if (lua_getmetatable(L, -1)) {
		lua_pushliteral(L, "__mode");
		lua_rawget(L, -2);
		if (lua_isstring(L,-1)) {
			mode = lua_tostring(L, -1);
			if (strchr(mode, 'k')) {
				weakk = 1;
			}
			if (strchr(mode, 'v')) {
				weakv = 1;
			}
		}
		lua_pop(L,1);

		luaL_checkstack(L, LUA_MINSTACK, NULL);
		mark_table(L, dL, t, "[metatable]");
	}
	lua_pushnil(L);
	while (lua_next(L, -2) != 0) {
		if (weakv) {
			lua_pop(L,1);
		} else {
			char temp[32];
			key = keystring(L, -2, temp);
			if (!value)
			{
				int count = (int)sizeof(char) * (int)(strlen(key) + 2);
				value = (char*)malloc(count);
				memset(value, 0, count);
				strcpy(value, key);
			}
			else
			{
				value = (char*)realloc(value, sizeof(char)*(strlen(value) + strlen(key) + 2));
				strcat(value, key);
			}
			strcat(value, "\n");
			//output("      ----- %s %d\n", key,type);
			mark_object(L, dL, t , key);
		}
		if (!weakk) {
			lua_pushvalue(L,-1);
			mark_object(L, dL, t , "[key]");
		}
	}
	if (name)
	{
		lprofP_outputToFile(g_fMemorySnapshot,"%s\n", name);
		free(name);
	}
	if (value)
	{
		lprofP_outputToFile(g_fMemorySnapshot,"%s\n", value);
		free(value);
	}
	lua_pop(L,1);
}

static void mark_userdata(lua_State *L, lua_State *dL, const void * parent, const char *desc) {
	const void * t = readobject(L, dL, parent, desc);
	int type = lua_type(L, -1);
	if (t == NULL)
		return;
	//output("--------------addr %p   %d    %s\n", t, type, desc);
	if (lua_getmetatable(L, -1)) {
		mark_table(L, dL, t, "[metatable]");
	}

	lua_getuservalue(L,-1);
	if (lua_isnil(L,-1)) {
		lua_pop(L,2);
	} else {
		mark_table(L, dL, t, "[uservalue]");
		lua_pop(L,1);
	}
}

static void mark_function(lua_State *L, lua_State *dL, const void * parent, const char *desc) {
	const void * t = readobject(L, dL, parent, desc);
	int type = lua_type(L, -1);
	
	const char *name = NULL;
	char tmp[16];
	int i;
	lua_Debug ar;
	luaL_Buffer b;
	if (t == NULL)
		return;
	//output("--------------addr %p   %d   %s\n", t, type, desc);
	mark_function_env(L,dL,t);

	for (i=1;;i++) {
		name = lua_getupvalue(L,-1,i);
		if (name == NULL)
			break;
		mark_object(L, dL, t, name[0] ? name : "[upvalue]");
	}
	if (lua_iscfunction(L,-1)) {
		if (i==1) {
			// light c function
			lua_pushnil(dL);
			lua_rawsetp(dL, FUNCTION, t);
		}
		lua_pop(L,1);
	} else {

		lua_getinfo(L, ">S", &ar);

		luaL_buffinit(dL, &b);
		luaL_addstring(&b, ar.short_src);

		sprintf(tmp,":%d",ar.linedefined);
		luaL_addstring(&b, tmp);
		luaL_pushresult(&b);
		//output("      ----- %s%s %d\n", ar.short_src,tmp, type);
		lua_rawsetp(dL, SOURCE, t);
	}
}

static void mark_thread(lua_State *L, lua_State *dL, const void * parent, const char *desc) {
	const void * t = readobject(L, dL, parent, desc);
	int type = lua_type(L, -1);
	
	int top = 0;
	int level = 0;
	lua_State *cL = NULL;
	char tmp[128];
	lua_Debug ar;
	luaL_Buffer b;
	const char * name = NULL;
	int i,j;
	if (t == NULL)
		return;
	//output("--------------addr %p   %d   %s\n", t, type, desc);
	cL = lua_tothread(L,-1);
	if (cL == L) {
		level = 1;
	} else {
		// mark stack
		top = lua_gettop(cL);
		luaL_checkstack(cL, 1, NULL);

		for (i=0;i<top;i++) {
			lua_pushvalue(cL, i+1);
			sprintf(tmp, "[%d]", i+1);
			mark_object(cL, dL, cL, tmp);
		}
	}

	luaL_buffinit(dL, &b);
	while (lua_getstack(cL, level, &ar)) {
		lua_getinfo(cL, "Sl", &ar);
		luaL_addstring(&b, ar.short_src);
		if (ar.currentline >=0) {
			memset(tmp,0x0,128);
			sprintf(tmp,":%d ",ar.currentline);
			//output("      ----- %s%s %d\n", ar.short_src, tmp, type);
			luaL_addstring(&b, tmp);
		}


		for (j=1;j>-1;j-=2) {
			for (i=j;;i+=j) {
				name = lua_getlocal(cL, &ar, i);
				if (name == NULL)
					break;
				//snprintf(tmp, sizeof(tmp), "%s : %s:%d",name,ar.short_src,ar.currentline);
				memset(tmp,0x0,128);
				sprintf(tmp, "%s : %s:%d",name,ar.short_src,ar.currentline);
				mark_object(cL, dL, t, tmp);
			}
		}

		++level;
	}
	luaL_pushresult(&b);
	lua_rawsetp(dL, SOURCE, t);
	lua_pop(L,1);
}

static void mark_object(lua_State *L, lua_State *dL, const void * parent, const char *desc) {
	int t;
	luaL_checkstack(L, LUA_MINSTACK, NULL);
	t = lua_type(L, -1);
	switch (t) {
	case LUA_TTABLE:
		mark_table(L, dL, parent, desc);
		break;
	case LUA_TUSERDATA:
		mark_userdata(L, dL, parent, desc);
		break;
	case LUA_TFUNCTION:
		mark_function(L, dL, parent, desc);
		break;
	case LUA_TTHREAD:
		mark_thread(L, dL, parent, desc);
		break;
	default:
		lua_pop(L,1);
		break;
	}
}

static int count_table(lua_State *L, int idx) {
	int n = 0;
	lua_pushnil(L);
	while (lua_next(L, idx) != 0) {
		++n;
		lua_pop(L,1);
	}
	return n;
}

static void gen_table_desc(lua_State *dL, luaL_Buffer *b, const void * parent, const char *desc) {
	char tmp[32];
	size_t l = sprintf(tmp,"%p : ",parent);
	luaL_addlstring(b, tmp, l);
	luaL_addstring(b, desc);
	luaL_addchar(b, '\n');
}

static void pdesc(lua_State *L, lua_State *dL, int idx, const char * type_name) {
	size_t l = 0;
	const char* s = NULL;
	const void * parent = NULL;
	const char * desc = NULL;
	const void * key = NULL;
	lua_pushnil(dL);
	while (lua_next(dL, idx) != 0) {
		luaL_Buffer b;
		luaL_buffinit(L, &b);
		key = lua_touserdata(dL, -2);
		if (idx == FUNCTION) {
			lua_rawgetp(dL, SOURCE, key);
			if (lua_isnil(dL, -1)) {
				luaL_addstring(&b,"cfunction\n");
			} else {
				l = 0;
				s = lua_tolstring(dL, -1, &l);
				luaL_addstring(&b, "function\n");
				luaL_addlstring(&b,s,l);
				luaL_addchar(&b,'\n');
			}
			lua_pop(dL, 1);
		} else if (idx == THREAD) {
			lua_rawgetp(dL, SOURCE, key);
			l = 0;
			s = lua_tolstring(dL, -1, &l);
			luaL_addstring(&b, "thread\n");
			luaL_addlstring(&b,s,l);
			luaL_addchar(&b,'\n');
			lua_pop(dL, 1);
		} else {
			luaL_addstring(&b, type_name);
			luaL_addchar(&b,'\n');
		}
		lua_pushnil(dL);
		while (lua_next(dL, -2) != 0) {
			parent = lua_touserdata(dL,-2);
			desc = luaL_checkstring(dL,-1);
			gen_table_desc(dL, &b, parent, desc);
			//output("--------------------------------------------------    %s\n", desc);
			lua_pop(dL,1);
		}
		luaL_pushresult(&b);
		lua_rawsetp(L, -2, key);
		lua_pop(dL,1);
	}
}

static void gen_result(lua_State *L, lua_State *dL) {
	int count = 0;
	count += count_table(dL, TABLE);
	count += count_table(dL, FUNCTION);
	count += count_table(dL, USERDATA);
	count += count_table(dL, THREAD);
	lua_createtable(L, 0, count);
	pdesc(L, dL, TABLE, "table");
	pdesc(L, dL, USERDATA, "userdata");
	pdesc(L, dL, FUNCTION, "function");
	pdesc(L, dL, THREAD, "thread");
}

static void count_result(lua_State* L,lua_State* dL)
{
	lua_nTableCount = count_table(dL,TABLE);
	lua_nFuncCount = count_table(dL,FUNCTION);
	lua_nUserDataCount = count_table(dL,USERDATA);
	lua_nThreadCount = count_table(dL,THREAD);
}

/*static int memory_snapshot(lua_State *L)
{
	traverse_table(L, -1);
	return 0;
}
*/


static int memory_snapshot(lua_State *L) {
	int i;
	const char* file = NULL;
	//char sz[160];
	lua_State *dL = NULL;
	if(lua_gettop(L) >= 1)
		file = luaL_checkstring(L, 1);
	if(file)
	{
		g_fMemorySnapshot = fopen(file,"w");
		if(g_fMemorySnapshot)
		{
			dL = luaL_newstate();
			for (i=0;i<MARK;i++) {
				lua_newtable(dL);
			}
			lua_pushvalue(L, LUA_REGISTRYINDEX);
			mark_table(L, dL, NULL, "[registry]");
			fclose(g_fMemorySnapshot);
			lua_close(dL);
			g_fMemorySnapshot = NULL;
		}
	}
	return 0;
}


/************************************************************************/
/*         Lua Register Function                                        */
/************************************************************************/
int profiler_open(lua_State *L)
{
	is_pause = 0;
	is_start = 0;
	lua_register(L, "profiler_start", profiler_start);
	lua_register(L, "profiler_pause", profiler_pause);
	lua_register(L, "profiler_resume", profiler_resume);
	lua_register(L, "profiler_stop", profiler_stop);
	lua_register(L, "profiler_frame",profiler_frame);
	lua_register(L, "profiler_snapshot",memory_snapshot);
	// 增加一个判断是否暂停的函数 2016-08-10 lennon.c
	lua_register(L, "is_profiler_pause", is_profiler_pause);

	return 1;
}


LUA_API void init_profiler(lua_State *L)
{
	profiler_open(L);
	pOutputCallback = NULL;
	pUnityObject = NULL;
	pUnityMethod = NULL;
	lprofT_init();
}

LUA_API void frame_profiler(int id, int unitytime)
{
	

	static LARGE_INTEGER s_last_ts;

	static int s_has_set_ts = 0;



	if(is_start)
	{
	    if(!s_has_set_ts){
			lprofC_start_timer2(&s_last_ts);
			s_has_set_ts = 1;
	    }		

		
		
		lprof_DebugInfo* dbg_info = (lprof_DebugInfo*)malloc(sizeof(lprof_DebugInfo));
		dbg_info->type = FRAME;
		dbg_info->frameid = id;
		dbg_info->unitytime = unitytime;
		dbg_info->framecs = lprofC_get_seconds2(&s_last_ts);
		dbg_info->hook_cost_cs = stat_hook_cost_ts;
		dbg_info->hook_call_cnt = stat_hook_call_cnt;
		stat_hook_cost_ts = 0;
		stat_hook_call_cnt = 0;


		dispatch_dbg_info(dbg_info);

		lprofC_start_timer2(&s_last_ts);
		//lprofT_frame(id, unitytime);
	}
}

LUA_API void register_callback(void* pcallback)
{
	if (pcallback)
	{
		pOutputCallback = (pfnoutputCallback)pcallback;
	}
}

LUA_API int isregister_callback()
{
	if (pOutputCallback)
		return 1;
	else
		return 0;
}

LUA_API void unregister_callback()
{
	pOutputCallback = NULL;
	if (pUnityObject)
		free(pUnityObject);
	if (pUnityMethod)
		free(pUnityMethod);
	pUnityObject = pUnityMethod = NULL;

}


DWORD WINAPI thread_run(void* data)
{

	while(1){

		lprof_DebugInfo* dbg_info = NULL;

		queue_pop_without_dealloc(&g_nolock_queue, (void**)&dbg_info);

		if(NULL != dbg_info){
			handle_dbg_info(dbg_info);
			continue;
		}

		SLEEP_SHORT_TIME();
	}

	return 0;
}





