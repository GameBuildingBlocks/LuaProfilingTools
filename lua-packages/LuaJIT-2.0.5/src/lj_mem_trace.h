#ifndef _LJ_LUAJIT_USE_MEM_TRACE_H_
#define _LJ_LUAJIT_USE_MEM_TRACE_H_

#include "lj_def.h"

LJ_FUNC void lj_start_trace_memory(lua_State *L, const char* file_name);
LJ_FUNC void lj_update_trace_memory(void *addr, const size_t mb);
LJ_FUNC void lj_end_trace_memory();

#endif // _LJ_LUAJIT_USE_MEM_TRACE_H_
