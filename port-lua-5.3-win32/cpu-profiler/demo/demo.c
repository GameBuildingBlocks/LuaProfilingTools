#include "lua.h"
#include "lauxlib.h"
#include "luaprofiler.h"

//#define MEM_STAT 1
#define CPU_STAT 1

int test(lua_State* L){
	lua_newtable(L);

	lua_pushstring(L, "HAHA");
	return 1;
}


int main(int argc, char** argv){

	int status;

#ifdef MEM_STAT
	void* handle = dlopen("luamemstat.so", 1);
	printf("luamemstat.so handle=%p ", handle);
	lua_Alloc alloc_f =  (lua_Alloc)dlsym(handle, "LuaAlloc");
	lua_State *L = lua_newstate(alloc_f, NULL);
	luaL_openlibs(L);	
	lua_CFunction hook_f = dlsym(handle, "StartLuaHook");
	printf("luamemstat.so hook_f=%p ", hook_f);
	lua_register(L, "StartLuaMemoryHook", hook_f);	

	lua_register(L, "test", test);	
#else
	lua_State *L = luaL_newstate();	/* create state */
	luaL_openlibs(L);	

#endif

	
	printf("hello\n");


#ifdef CPU_STAT	
	init_profiler(L);
	luaL_dostring(L, "profiler_start('luaprofiler.txt')");
#endif

	
	
	luaL_dofile(L, argv[1]);

#ifdef CPU_STAT
	int index = 0;
	int i = 0;

	while(1){
		for(i = 0; i < 10; i++){
			lua_getglobal(L, "ff");
			lua_pcall(L, 0,0,0);
		}

		frame_profiler(i++, time(NULL));
	}
#endif
	printf("haha");
	return 0;	
}



