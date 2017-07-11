/*
** LuaProfiler
** Copyright Kepler Project 2005-2007 (http://www.keplerproject.org/luaprofiler)
** $Id: stack.h,v 1.5 2007-08-22 19:23:53 carregal Exp $
*/

/*****************************************************************************
stack.h:
   Simple stack manipulation
*****************************************************************************/

#ifndef _STACK_H
#define _STACK_H

#include <time.h>
#include "lp.h"
#include "cJSON.h"

#define LP_MAX_NAME_LEN (256)
#define LP_MAX_SOURCE_LEN (256)
#define LP_MAX_WHAT_LEN (16)


typedef enum _eDebugInfoType{
	FUNCTION_HOOK = 1,
	FRAME = 2,
}eDebugInfoType;

typedef struct _lprof_DebugInfo {
  eDebugInfoType type;
  int event;
  char name[LP_MAX_NAME_LEN];	/* (n) */

  char what[LP_MAX_WHAT_LEN];	/* (S) `Lua', `C', `main', `tail' */
  char source[LP_MAX_SOURCE_LEN];	/* (S) */

  char ccallname[100];

  char* p_name;
  char* p_what;
  char* p_source;
  char* p_namewhat;
  int currentline;	/* (l) */
  int linedefined;	/* (S) */
  int lastlinedefined;	/* (S) */

  
  LARGE_INTEGER currenttime;

  // 应该放两个结构体中，简单起见，现在共用，会浪费内存
  int frameid;
  int unitytime;

  double framecs;
  double hook_cost_cs;
  int    hook_call_cnt;
}lprof_DebugInfo;


#define MAX_FUNCTION_NAME_LENGTH 200
#define MAX_OUTPUT_MESSAGE_LENGTH 100

//extern FILE *outf;

typedef enum {none,lua,nolua}calltype;


typedef struct lprofS_sSTACK_RECORD lprofS_STACK_RECORD;

struct lprofS_sSTACK_RECORD {
	clock_t time_marker_function_local_time;
	clock_t time_marker_function_total_time;
	char *file_defined;
	char *function_name;
	char *source_code;
	char *what;
	long line_defined;
	long current_line;
	double local_time;
	double interval_time;
	time_t current_time;
	int stack_level;
	LARGE_INTEGER time_maker_local_time_begin;
	LARGE_INTEGER time_maker_local_time_end;
	lprofS_STACK_RECORD *next;
};

typedef lprofS_STACK_RECORD *lprofS_STACK;

typedef struct lprofP_sSTATE lprofP_STATE;

typedef struct lprofS_TREE_RECORD lprofT_NODE;

struct lprofS_TREE_RECORD
{
	int stack_level;
	lprofS_STACK pNode;
	lprofT_NODE* pParent;
	lprofT_NODE* pChild;
	int nChildCount;
	//int nMaxChildCount;
};

struct lprofP_sSTATE {
   int stack_level;
   lprofS_STACK stack_top;
};

void lprofS_push(lprofS_STACK *p, lprofS_STACK_RECORD r, lprof_DebugInfo* dbg_info);
lprofS_STACK_RECORD lprofS_pop(lprofS_STACK *p, lprof_DebugInfo* dbg_info);

void lprofT_init();
void lprofT_start();
void lprofT_add(lprofS_STACK pChild, lprof_DebugInfo* dbg_info);
void lprofT_pop();
//void lprofT_output(lprofT_NODE* p);
//void lprofT_print();
void lprofT_frame(int id,int unitytime, double frame_cs,double hook_cost_cs, int hook_cnt);
lprofT_NODE* lprofT_createNode();
lprofS_STACK lprofT_assigningStack(lprofS_STACK pDest,lprofS_STACK pSource);
lprofT_NODE* lprofT_assigningNode(lprofT_NODE* pDest,lprofT_NODE* pSource);
lprofT_NODE* lprofT_addchild(lprofT_NODE* pParent, lprofT_NODE* pChild);
void lprofT_free(lprofT_NODE* p);
void freeNode(lprofT_NODE* p);
void output(const char *format, ...);
void lprofT_tojson();
//void lprofT_tojson2(lprofT_NODE* p);
//void lprofT_tojson(lprofT_NODE* p);
//void lprofT_tojson_thread();
void lprofT_close();
cJSON* treeTojson(lprofT_NODE* p, calltype precalltype, double* pdLuaConsuming, double* pdFunConsuming, double* topFunCs);
//void freeTree(lprofT_NODE* p);
#endif
