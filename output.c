
#include <stdlib.h>
#include <string.h>
#include <stdarg.h>
#include <assert.h>
#include "output.h"

int nMaxCacheNode = 2;
int nCacheNode = 0;
lprof_PREVNODE sPrevNode = { 0,0 };

lprofP_OUTPUT pOutputHead = NULL;
lprofP_OUTPUT pOutputTail = NULL;

void lprofP_addFrame(int id, char* str)
{
	lprof_NODE* pNode = (lprof_NODE*)malloc(sizeof(lprof_NODE));
	memset(pNode, 0x0, sizeof(lprof_NODE));
	pNode->id = id;
	pNode->frame = str;
	pNode->data = NULL;
	pNode->next = NULL;
	if (pOutputTail)
	{
		pOutputTail->next = pNode;
		pOutputTail = pNode;
	}	
	else
	{
		pOutputTail = pNode;
	}
	if (pOutputHead == NULL)
		pOutputHead = pOutputTail;
	nCacheNode++;
	if (nCacheNode >= nMaxCacheNode)
		lprofP_output();
}

void lprofP_addData(char* str)
{
	if (pOutputTail)
	{
		pOutputTail->data = str;
	}
}

void lprofP_output()
{
	if (pOutputHead != NULL)
	{
		lprofP_OUTPUT pOut = pOutputHead;
		if (pOut->data)
		{
			output(pOut->frame);
			output(",");
			output(pOut->data);
			output(",");
			sPrevNode.id = pOut->id;
			sPrevNode.data = 1;
		}
		else
		{
			if (sPrevNode.id != pOut->id && sPrevNode.data != 0)
			{
				output(pOut->frame);
				output(",");
				sPrevNode.id = pOut->id;
				sPrevNode.data = 0;
			}
		}
		pOutputHead = pOut->next;
		if (pOutputTail == pOut)
			pOutputTail = pOutputHead;
		free(pOut);
		nCacheNode--;
	}
}

void lprofP_close()
{
	sPrevNode.id = 0;
	sPrevNode.data = 0;
	lprofP_OUTPUT pOut = pOutputHead;
	while (pOut)
	{
		pOut = pOutputHead->next;
		free(pOutputHead);
	}
	pOutputHead = NULL;
	pOutputTail = NULL;
}