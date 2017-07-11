/*
** LuaProfiler
** Copyright Kepler Project 2005-2007 (http://www.keplerproject.org/luaprofiler)
** $Id: stack.c,v 1.4 2007-08-22 19:23:53 carregal Exp $
*/

/*****************************************************************************
stack.c:
   Simple stack manipulation
*****************************************************************************/

#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdarg.h>
#include <assert.h>
#include "stack.h"
#include "clocks.h"
#include "output.h"
#include "core_profiler.h"
#ifdef _MSC_VER
#include <process.h>
#endif
/*
#if defined (_WIN32)
#include"pthread.h"
#include<Windows.h>
#else
#include <pthread.h>
#endif
*/
int MAX_CHILD_SIZE = 20;
const char* gpsz_luamark = "Lua";

lprofT_NODE* pTreeRoot = NULL;
lprofT_NODE* pTreeNode = NULL;
lprofT_NODE* pTopRoot = NULL;

char* pOutput = NULL;
int   nMaxStackLevel = 0;
int   nTotalCall = 0;
double dTotalTimeConsuming = 0.0;
int   nOutputCount = 0;
long node_size = 0;
int first_flush = 1;
double dPreFrameLuaConsuming = 0.0;
double dPreFrameFunConsuming = 0.0;
double dPreFrameTime = 0.0;

LARGE_INTEGER time_maker_golbal_start;
LARGE_INTEGER time_maker_golbal_stop;

extern lprofP_STATE* g_S;

extern double stat_hook_cost_ts;
extern double stat_frame_cost_ts;
extern int    stat_hook_call_cnt;

void formats(char *s) {
	int i;
	if (!s)
		return;
	for (i = (int)strlen(s); i >= 0; i--) {
		if ((s[i] == '|') || (s[i] == '\n'))
			s[i] = ' ';
	}
}

lprofT_NODE* lprofT_addchild(lprofT_NODE* pParent, lprofT_NODE* pChild)
{
	lprofT_NODE* pResult = NULL;
	if (pParent)
	{
// 		if (pParent->nChildCount >= pParent->nMaxChildCount)
// 		{
// 			lprofT_NODE* ppTmp = (lprofT_NODE*)realloc(pParent->pChild, pParent->nMaxChildCount * 2 * sizeof(lprofT_NODE));
// 			assert(ppTmp);
// 			if (ppTmp)
// 			{
// 				pParent->pChild = ppTmp;
// 				//pParent->pChild[pParent->nChildCount] = *pChild;
// 				pResult = &(pParent->pChild[pParent->nChildCount]);
// 				lprofT_assigningNode(pResult,pChild);
// 				pParent->nMaxChildCount = pParent->nMaxChildCount * 2;
// 			}
// 		}
// 		else
// 		{
// 			//pParent->pChild[pParent->nChildCount] = *pChild;
// 			pResult = &(pParent->pChild[pParent->nChildCount]);
// 			lprofT_assigningNode(pResult,pChild);
// 		}
		if(pParent->nChildCount > 0)
		{
			lprofT_NODE* ppTmp = (lprofT_NODE*)realloc(pParent->pChild, (pParent->nChildCount + 1) * sizeof(lprofT_NODE));
			assert(ppTmp);
			if (ppTmp)
			{
				pParent->pChild = ppTmp;
				pResult = &(pParent->pChild[pParent->nChildCount]);
				lprofT_assigningNode(pResult,pChild);
			}
		}
		else
		{
			pResult = (lprofT_NODE*)malloc(sizeof(lprofT_NODE));
			memset(pResult,0x0,sizeof(lprofT_NODE));
			lprofT_assigningNode(pResult,pChild);
			pParent->pChild = pResult;
		}
		
		pResult->pParent = pParent;
		pParent->nChildCount++;
	}
	return pResult;
}

void lprofT_pop(lprof_DebugInfo* dbg_info)
{
	if (pTreeNode)
	{
		assert(pTreeNode->pNode);
		pTreeNode->pNode->local_time = (float)lprofC_get_interval(&pTreeNode->pNode->time_maker_local_time_begin, &dbg_info->currenttime);
		pTreeNode->pNode->time_maker_local_time_end = dbg_info->currenttime;
		if (pTreeNode->pNode->stack_level <= 1)
			//dTotalTimeConsuming += pTreeNode->pNode->local_time;
			dTotalTimeConsuming += lprofC_get_interval(&pTreeNode->pNode->time_maker_local_time_begin, &pTreeNode->pNode->time_maker_local_time_end);
		if (pTreeNode->pParent)
		{
			pTreeNode = pTreeNode->pParent;
		}
	}
}

lprofT_NODE* lprofT_createNode()
{
	int i = 0;
	lprofT_NODE* pNode = NULL;
	pNode = (lprofT_NODE*)malloc(sizeof(lprofT_NODE));
	if(pNode)
	{
		memset(pNode, 0x0, sizeof(lprofT_NODE));
		//pNode->pChild = (lprofT_NODE*)malloc(MAX_CHILD_SIZE*sizeof(lprofT_NODE));
		pNode->nChildCount = 0;
		//pNode->nMaxChildCount = MAX_CHILD_SIZE;
		//if(pNode->pChild)
			//memset(pNode->pChild, 0x0, MAX_CHILD_SIZE*sizeof(lprofT_NODE));
	}
	return pNode;
}


void lprofS_push(lprofS_STACK *p, lprofS_STACK_RECORD r, lprof_DebugInfo* dbg_info) {
lprofS_STACK q;
        q=(lprofS_STACK)malloc(sizeof(lprofS_STACK_RECORD));
		if(q)
		{
			*q=r;
			q->next=*p;
			*p=q;
			lprofT_add(q, dbg_info);
		}
}

lprofS_STACK_RECORD lprofS_pop(lprofS_STACK *p, lprof_DebugInfo* dbg_info) 
{
	lprofS_STACK_RECORD r;
	lprofS_STACK q;

        r=**p;
        q=*p;
        *p=(*p)->next;

		
		if(q->function_name)
		{
			free(q->function_name);
			 
		}
		if(q->what)
		{
			free(q->what);
		}

		if(q->file_defined){
			free(q->file_defined);
		}

		
		
        free(q);
		lprofT_pop(dbg_info);
        return r;
}

lprofT_NODE* lprofT_assigningNode(lprofT_NODE* pDest,lprofT_NODE* pSource)
{
	if(pDest && pSource)
	{
		pDest->nChildCount = pSource->nChildCount;
		//pDest->nMaxChildCount = pSource->nMaxChildCount;
		pDest->pChild = NULL;
		pDest->pNode = lprofT_assigningStack(pDest->pNode,pSource->pNode);
		pDest->pParent = pSource->pParent;
		pDest->stack_level = pSource->stack_level;
	}
	return pDest;
}

lprofS_STACK lprofT_assigningStack(lprofS_STACK pDest,lprofS_STACK pSource)
{
	if(pSource)
	{
		pDest = (lprofS_STACK)malloc(sizeof(lprofS_STACK_RECORD));
		memset(pDest,0x0,sizeof(lprofS_STACK_RECORD));
		pDest->current_line = pSource->current_line;
		pDest->current_time = pSource->current_time;
		if(pSource->file_defined)
		{
			pDest->file_defined = (char*)malloc(strlen(pSource->file_defined) + 1);
			memset(pDest->file_defined,0x0,strlen(pSource->file_defined) + 1);
			strcpy(pDest->file_defined,pSource->file_defined);
		}
		if(pSource->function_name)
		{
			pDest->function_name = (char*)malloc(strlen(pSource->function_name) + 1);
			memset(pDest->function_name,0x0,strlen(pSource->function_name) + 1);
			strcpy(pDest->function_name,pSource->function_name);
		}
		pDest->interval_time = pSource->interval_time;
		pDest->line_defined = pSource->line_defined;
		pDest->local_time = pSource->local_time;
		pDest->next = pSource->next;
		if(pSource->source_code)
		{
			pDest->source_code = (char*)malloc(strlen(pSource->source_code) + 1);
			memset(pDest->source_code,0x0,strlen(pSource->source_code) + 1);
			strcpy(pDest->source_code,pSource->source_code);
		}
		pDest->stack_level = pSource->stack_level;
		pDest->time_maker_local_time_begin = pSource->time_maker_local_time_begin;
		pDest->time_maker_local_time_end = pSource->time_maker_local_time_end;
		pDest->time_marker_function_local_time = pSource->time_marker_function_local_time;
		pDest->time_marker_function_total_time = pSource->time_marker_function_total_time;
		if(pSource->what)
		{
			pDest->what = (char*)malloc(strlen(pSource->what) + 1);
			memset(pDest->what,0x0,strlen(pSource->what) + 1);
			strcpy(pDest->what,pSource->what);
		}
		
	}
	return pDest;
}

void lprofT_add(lprofS_STACK pChild, lprof_DebugInfo* dbg_info)
{	
	lprofT_NODE* p = NULL;
	nTotalCall++;
	p = lprofT_createNode();
	p->pNode = lprofT_assigningStack(p->pNode,pChild);
	p->stack_level = p->pNode->stack_level;

	p->pNode->time_maker_local_time_begin = dbg_info->currenttime;
	if (p->pNode->stack_level > nMaxStackLevel)
		nMaxStackLevel = p->pNode->stack_level;
	if (pTreeRoot == NULL)
	{
		pTreeNode = pTreeRoot = p;
	}
	else
	{
		if (pTreeNode->stack_level == p->pNode->stack_level)
		{
			pTreeNode = lprofT_addchild(pTreeNode->pParent, p);
		}
		else
		{
			pTreeNode = lprofT_addchild(pTreeNode, p);
		}

		lprofT_free(p);
	}
	
}

void lprofT_free(lprofT_NODE* p)
{
	if (p)
	{
		if(p->nChildCount > 0)
		{
			int i = 0;
			for(i = 0;i < p->nChildCount;i++)
				lprofT_free(&p->pChild[i]);
			//free(p->pChild);
		}
		freeNode(p);
		if(p->pParent == NULL)
		{
			free(p);
			p = NULL;
		}
	}
}

void freeNode(lprofT_NODE* p)
{
	if(p)
	{
		if(p->pNode)
		{
			if(p->pNode->file_defined)
			{
				free(p->pNode->file_defined);
				p->pNode->file_defined = NULL;
			}
			if(p->pNode->source_code)
			{
				free(p->pNode->source_code);
				p->pNode->source_code = NULL;
			}
			if(p->pNode->function_name)
			{
				free(p->pNode->function_name);
				p->pNode->function_name = NULL;
			}
			if(p->pNode->what)
			{
				free(p->pNode->what);
				p->pNode->what = NULL;
			}
			free(p->pNode);
			p->pNode = NULL;
		}
		if(p->pChild)
		{
			free(p->pChild);
			p->pChild = NULL;
		}
	}
}

void lprofT_tojson()
{
	char *jstring = NULL;
	double dLuaConsuming = 0.0;
	double dFunConsuming = 0.0;
	double topFuncs = 0;
	if (pTreeRoot)
	{
		cJSON* root = treeTojson(pTreeRoot,none,&dLuaConsuming,&dFunConsuming, &topFuncs);
		lprofT_free(pTreeRoot);
		jstring = cJSON_Print(root);
		lprofP_addData(jstring);
		cJSON_Delete(root);
		dPreFrameLuaConsuming += topFuncs;
		dPreFrameFunConsuming += dFunConsuming;
		pTreeRoot = NULL;
	}

}

void lprofT_close()
{
	lprofP_close();
	nTotalCall = 0;
	dTotalTimeConsuming = 0.0;
	pTreeRoot = NULL;
	dPreFrameTime = 0.0;

}

cJSON* treeTojson(lprofT_NODE* p, calltype precalltype,double* pdLuaConsuming, double* pdFunConsuming, double* topFunCs)
{
	
	cJSON* root = NULL;
	assert(p);
	if (p && p->pNode)
	{
		/*
		if(p->pNode->function_name && filter_lua_api(p->pNode->function_name)){
			return NULL;
		}*/

	
		cJSON* pChild = NULL;
		calltype curCalltype;
		char* source = NULL;
		char* name = NULL;
		int i = 0;
		double beginTime = lprofC_get_interval(&time_maker_golbal_start,&p->pNode->time_maker_local_time_begin);
		double endTime = lprofC_get_interval(&time_maker_golbal_start,&p->pNode->time_maker_local_time_end);
		double consumingTimer = lprofC_get_interval(&p->pNode->time_maker_local_time_begin, &p->pNode->time_maker_local_time_end);
		root = cJSON_CreateObject();
		cJSON_AddItemToObject(root, "ln", cJSON_CreateNumber(p->pNode->current_line));
		//cJSON_AddItemToObject(root, "lineDefined", cJSON_CreateNumber(p->pNode->line_defined));
		cJSON_AddItemToObject(root, "cs", cJSON_CreateNumber(consumingTimer));
		//cJSON_AddItemToObject(root, "timeConsuming", cJSON_CreateNumber(p->pNode->local_time));
		cJSON_AddItemToObject(root, "lv", cJSON_CreateNumber(p->pNode->stack_level));
		//cJSON_AddItemToObject(root, "interval", cJSON_CreateNumber(p->pNode->interval_time));
		cJSON_AddItemToObject(root, "info", cJSON_CreateString(p->pNode->what));
		cJSON_AddItemToObject(root, "bt", cJSON_CreateNumber(beginTime));
		cJSON_AddItemToObject(root, "et", cJSON_CreateNumber(endTime));
 		source = p->pNode->file_defined;
		if (source == NULL || strcmp(source,"") == 0) {
			source = "(string)";
		}

		if(topFunCs != NULL)
			*topFunCs += consumingTimer;

		cJSON_AddItemToObject(root, "mod", cJSON_CreateString(source));
		//free(source_out);
		name = p->pNode->function_name;
		formats(name);
		cJSON_AddItemToObject(root, "fn", cJSON_CreateString(name));

		if (lua == precalltype)
		{
			if (*pdLuaConsuming <= 0.0 && strcmp(p->pNode->what, gpsz_luamark) == 0)
			{
				*pdLuaConsuming = consumingTimer;
			}
			else if (*pdLuaConsuming >= 0.0 && strcmp(p->pNode->what, gpsz_luamark) != 0)
			{
				*pdLuaConsuming -= consumingTimer;
			}
		}
		else if (nolua == precalltype)
		{
			if (strcmp(p->pNode->what, gpsz_luamark) == 0)
				*pdLuaConsuming += consumingTimer;
		}
		else if (none == precalltype)
		{
			if (strcmp(p->pNode->what, gpsz_luamark) == 0)
				*pdLuaConsuming = consumingTimer;
			*pdFunConsuming = consumingTimer;
		}

		if (strcmp(p->pNode->what, gpsz_luamark) == 0)
			curCalltype = lua;
		else
			curCalltype = nolua;

		pChild = cJSON_CreateArray();
		if (pChild)
			cJSON_AddItemToObject(root, "sub", pChild);
		
		for (i = 0; i < p->nChildCount; i++)
			cJSON_AddItemToArray(pChild, treeTojson(&p->pChild[i], curCalltype,pdLuaConsuming,pdFunConsuming, NULL));
		//lprofT_free(p);
	}
	return root;
}

// void freeTree(lprofT_NODE* p)
// {
// 	int i = 0;
// 	if (p)
// 	{
// 		for (i = 0;i < p->nChildCount;i++)
// 		{
// 			freeTree(&p->pChild[i]);
// 		}
// 		lprofT_free(p);
// 	}
// }

/*
	写文件多线程版本，暂时不使用
	2016-08-10 lennon.c
*/
void thread_func()
{
	lprofT_tojson();

	if (outf)
		fclose(outf);
}

#ifdef _MSC_VER
unsigned int __stdcall thread_func_win(void* m)
#else
unsigned int thread_func_win(void* m)
#endif
{
	thread_func();
	return 0;
}

void* thread_func_linux(void* m)
{
	thread_func();
#ifndef _MSC_VER
	pthread_exit(NULL);
#endif
	return NULL;
}

void lprofT_tojson_thread()
{
#ifdef _MSC_VER
	_beginthreadex(NULL, 0, thread_func_win, NULL, 0, NULL);
#else
	pthread_t thr;
	pthread_create(&thr, NULL, thread_func_linux, NULL);
#endif
}


cJSON* frameTojson(int id, int unitytime, double frame_cs)
{
	cJSON* root = NULL;
	

	root = cJSON_CreateObject();
	cJSON_AddItemToObject(root, "fid", cJSON_CreateNumber(id));
	cJSON_AddItemToObject(root, "ft", cJSON_CreateNumber(0));
	cJSON_AddItemToObject(root, "fut", cJSON_CreateNumber(unitytime));
	
	//cJSON_AddItemToObject(root, "writefileTime", cJSON_CreateNumber(dTotalWriteConsuming));
	if (dPreFrameLuaConsuming > 0)
	{
		double dFrameInterval = 0.0;
		cJSON_AddItemToObject(root, "lc", cJSON_CreateNumber(dPreFrameLuaConsuming));
		cJSON_AddItemToObject(root, "fc", cJSON_CreateNumber(frame_cs));
	 
		cJSON_AddItemToObject(root, "fi", cJSON_CreateNumber(dFrameInterval));
		cJSON_AddItemToObject(root, "stFlag", cJSON_CreateFalse());
	}
	else
	{
		cJSON_AddItemToObject(root, "stFlag", cJSON_CreateTrue());
	}
	 
	 

	return root;

}

void lprofT_frame(int id, int unitytime, double framecs,double hook_cost_cs, int hook_cnt)
{



	static FILE* s_error_file = NULL;

	cJSON* root = frameTojson(id, unitytime, framecs);
	if (root)
	{
		cJSON_AddItemToObject(root, "hook_ts", cJSON_CreateNumber(hook_cost_cs));
		cJSON_AddItemToObject(root, "hook_cnt", cJSON_CreateNumber(hook_cnt));
		cJSON_AddItemToObject(root, "frame_cs", cJSON_CreateNumber(framecs));		
	
		char *jstring = cJSON_Print(root);
		lprofP_addFrame(id, jstring);
		//output(jstring);
		//output(",");
		cJSON_Delete(root);
		dPreFrameLuaConsuming = 0.0;
		dPreFrameFunConsuming = 0.0;
	}

	stat_hook_cost_ts = 0;
	stat_hook_call_cnt = 0;
	if(g_S->stack_level != 0 || g_S->stack_top != NULL || pTreeRoot != NULL){

		if(s_error_file == NULL){
			char tmpbuf[256];

			sprintf(tmpbuf, "luaprofiler.error.%d", (int)(GETPID()));

			s_error_file = fopen(tmpbuf, "w");
		}


		fprintf(s_error_file, "\r\n\r\n\r\nerror occurs fid:%d g_S->stack_level=%d g_S->stack_top=%p pTreeRoot=%p\n", id, g_S->stack_level, g_S->stack_top, pTreeRoot);

		while(g_S->stack_top){
			
			fprintf(s_error_file, "%s %s %d\n",g_S->stack_top->file_defined,  g_S->stack_top->function_name,  g_S->stack_top->current_line);

			lprof_DebugInfo dbg_info;
			
			lprofS_pop(&(g_S->stack_top), &dbg_info);
			
		}

		if(pTreeRoot != NULL){
		 	lprofT_free(pTreeRoot);
			pTreeRoot = NULL;
		}
		g_S->stack_level = 0;

		fflush(s_error_file);
	}

	
}

void lprofT_init()
{
	lprofC_start_timer2(&time_maker_golbal_start);
}

void lprofT_start()
{
	//lprofC_start_timer2(&time_maker_golbal_start);
}

