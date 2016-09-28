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

#define MAX_FUNCTION_NAME_LENGTH 200
#define MAX_OUTPUT_MESSAGE_LENGTH 100

extern FILE *outf;

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
	float local_time;
	float interval_time;
	float current_time;
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
	lprofT_NODE** ppChild;
	int nChildCount;
	int nMaxChildCount;
};

struct lprofP_sSTATE {
   int stack_level;
   lprofS_STACK stack_top;
};

void lprofS_push(lprofS_STACK *p, lprofS_STACK_RECORD r);
lprofS_STACK_RECORD lprofS_pop(lprofS_STACK *p);

void lprofT_init();
void lprofT_start();
void lprofT_add(lprofS_STACK pChild);
void lprofT_pop();
void lprofT_output(lprofT_NODE* p);
void lprofT_print();
void lprofT_frame(int id,int unitytime);
lprofT_NODE* lprofT_createNode(int count);
void lprofT_addchild(lprofT_NODE* pParent, lprofT_NODE* pChild);
void lprofT_free(lprofT_NODE* p);
void output(const char *format, ...);
void lprofT_tojson();
//void lprofT_tojson2(lprofT_NODE* p);
//void lprofT_tojson(lprofT_NODE* p);
//void lprofT_tojson_thread();
void lprofT_close();

#endif
