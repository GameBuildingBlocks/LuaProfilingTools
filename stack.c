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
#include "cJSON.h"
#include "output.h"
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

cJSON* treeTojson(lprofT_NODE* p, calltype precalltype, double* pdLuaConsuming,double* pdFunConsuming);
void freeTree(lprofT_NODE* p);

void formats(char *s) {
	int i;
	if (!s)
		return;
	for (i = strlen(s); i >= 0; i--) {
		if ((s[i] == '|') || (s[i] == '\n'))
			s[i] = ' ';
	}
}

void lprofT_addchild(lprofT_NODE* pParent, lprofT_NODE* pChild)
{
	if (pParent)
	{
		if (pParent->nChildCount >= pParent->nMaxChildCount)
		{
			lprofT_NODE* ppTmp = realloc(pParent->pChild, pParent->nMaxChildCount * 2 * sizeof(lprofT_NODE));
			assert(ppTmp);
			if (ppTmp)
			{
				pParent->pChild = ppTmp;
				pParent->pChild[pParent->nChildCount] = *pChild;
				pParent->nMaxChildCount = pParent->nMaxChildCount * 2;
			}
		}
		else
		{
			pParent->pChild[pParent->nChildCount] = *pChild;
		}
		pChild->pParent = pParent;
		pParent->nChildCount++;
		if (pChild && pChild->pNode && pParent->pNode)
			pChild->pNode->interval_time = lprofC_get_seconds2(&pParent->pNode->time_maker_local_time_begin);
	}
}

void lprofT_pop()
{
	if (pTreeNode)
	{
		assert(pTreeNode->pNode);
		pTreeNode->pNode->local_time = lprofC_get_seconds2(&pTreeNode->pNode->time_maker_local_time_begin);
		lprofC_start_timer2(&pTreeNode->pNode->time_maker_local_time_end);
		if (pTreeNode->pNode->stack_level <= 1)
			//dTotalTimeConsuming += pTreeNode->pNode->local_time;
			dTotalTimeConsuming += lprofC_get_interval(&pTreeNode->pNode->time_maker_local_time_begin, &pTreeNode->pNode->time_maker_local_time_end);
		if (pTreeNode->pParent)
		{
			pTreeNode = pTreeNode->pParent;
		}
	}
}

lprofT_NODE* lprofT_createNode(int count)
{
	int i = 0;
	lprofT_NODE* pNode = NULL;
	node_size += count * sizeof(lprofT_NODE);
	pNode = (lprofT_NODE*)malloc(count * sizeof(lprofT_NODE));
	if(pNode)
	{
		memset(pNode, 0x0, count*sizeof(lprofT_NODE));
		for (i = 0; i < count; i++)
		{
			pNode[i].stack_level = 0;
			pNode[i].pNode = NULL;
			pNode[i].pChild = (lprofT_NODE*)malloc(MAX_CHILD_SIZE*sizeof(lprofT_NODE));
			pNode[i].nChildCount = 0;
			pNode[i].nMaxChildCount = MAX_CHILD_SIZE;
			if(pNode[i].pChild)
				memset(pNode[i].pChild, 0x0, MAX_CHILD_SIZE*sizeof(lprofT_NODE));
		}
	}
	
	
	return pNode;
}


void lprofS_push(lprofS_STACK *p, lprofS_STACK_RECORD r) {
lprofS_STACK q;
        q=(lprofS_STACK)malloc(sizeof(lprofS_STACK_RECORD));
		if(q)
		{
			*q=r;
			q->next=*p;
			*p=q;
			lprofT_add(q);
		}
}

lprofS_STACK_RECORD lprofS_pop(lprofS_STACK *p) 
{
	lprofS_STACK_RECORD r;
	lprofS_STACK q;

        r=**p;
        q=*p;
        *p=(*p)->next;
        //free(q);
		lprofT_pop();
        return r;
}

void lprofT_add(lprofS_STACK pChild)
{	
	lprofT_NODE* p = NULL;
	nTotalCall++;
	p = lprofT_createNode(1);
	p->pNode = pChild;
	p->stack_level = pChild->stack_level;
	lprofC_start_timer2(&(pChild->time_maker_local_time_begin));
	if (pChild->stack_level > nMaxStackLevel)
		nMaxStackLevel = pChild->stack_level;
	if (pTreeRoot == NULL)
	{
		pTreeNode = pTreeRoot = p;
	}
	else
	{
		if (pTreeNode->stack_level == pChild->stack_level)
		{
			lprofT_addchild(pTreeNode->pParent, p);
		}
		else
		{
			lprofT_addchild(pTreeNode, p);
		}

		pTreeNode = p;
	}
	
}

void lprofT_free(lprofT_NODE* p)
{
	if (p)
	{
		/*
		if (p->pNode)
		{
			free(p->pNode);
			p->pNode = NULL;
		}
		free(p->pChild);
		*/
		free(p);
		p = NULL;
	}
	
}

void lprofT_tojson()
{
	if (pTreeRoot)
	{
		double dLuaConsuming = 0.0;
		double dFunConsuming = 0.0;
		cJSON* root = treeTojson(pTreeRoot,none,&dLuaConsuming,&dFunConsuming);
		freeTree(pTreeRoot);
		char *jstring = cJSON_Print(root);
		//output(jstring);
		//output(",");
		lprofP_addData(jstring);
		cJSON_Delete(root);
		//free(jstring);
		//nTotalCall = 0;
		dPreFrameLuaConsuming += dLuaConsuming;
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

cJSON* treeTojson(lprofT_NODE* p, calltype precalltype,double* pdLuaConsuming, double* pdFunConsuming)
{
	
	cJSON* root = NULL;
	assert(p);
	if (p && p->pNode)
	{
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
			cJSON_AddItemToArray(pChild, treeTojson(&p->pChild[i], curCalltype,pdLuaConsuming,pdFunConsuming));
		//lprofT_free(p);
	}
	return root;
}

void freeTree(lprofT_NODE* p)
{
	if (p)
	{
		for (int i = 0;i < p->nChildCount;i++)
		{
			freeTree(&p->pChild[i]);
		}
		lprofT_free(p);
	}
}

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


cJSON* frameTojson(int id, int unitytime)
{
	cJSON* root = NULL;
	double frametime = lprofC_get_seconds2(&time_maker_golbal_start);

	root = cJSON_CreateObject();
	cJSON_AddItemToObject(root, "fid", cJSON_CreateNumber(id));
	cJSON_AddItemToObject(root, "ft", cJSON_CreateNumber(frametime));
	cJSON_AddItemToObject(root, "fut", cJSON_CreateNumber(unitytime));
	
	//cJSON_AddItemToObject(root, "writefileTime", cJSON_CreateNumber(dTotalWriteConsuming));
	if (dPreFrameLuaConsuming > 0)
	{
		double dFrameInterval = 0.0;
		cJSON_AddItemToObject(root, "lc", cJSON_CreateNumber(dPreFrameLuaConsuming));
		cJSON_AddItemToObject(root, "fc", cJSON_CreateNumber(dPreFrameFunConsuming));
		dFrameInterval = frametime - dPreFrameTime;
		cJSON_AddItemToObject(root, "fi", cJSON_CreateNumber(dFrameInterval));
		cJSON_AddItemToObject(root, "stFlag", cJSON_CreateFalse());
	}
	else
	{
		cJSON_AddItemToObject(root, "stFlag", cJSON_CreateTrue());
	}
	dPreFrameTime = frametime;
	//cJSON_AddItemToObject(root, "frameInterval", cJSON_CreateNumber(frametime));

	return root;

}

void lprofT_frame(int id, int unitytime)
{
	cJSON* root = frameTojson(id, unitytime);
	if (root)
	{
		char *jstring = cJSON_Print(root);
		lprofP_addFrame(id, jstring);
		//output(jstring);
		//output(",");
		cJSON_Delete(root);
		dPreFrameLuaConsuming = 0.0;
		dPreFrameFunConsuming = 0.0;
	}
}

void lprofT_init()
{
	//lprofC_start_timer2(&time_maker_golbal_init);
	lprofC_start_timer2(&time_maker_golbal_start);
}

void lprofT_start()
{
	//lprofC_start_timer2(&time_maker_golbal_start);
}