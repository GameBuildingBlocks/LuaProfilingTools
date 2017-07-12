
#include <stdlib.h>
#include <string.h>
#include <stdarg.h>
#include <assert.h>
#include "output.h"

int nMaxCacheNode = 2;
int nCacheNode = 0;
lprof_PREVNODE sPrevNode = { 0,0 };

//double dTotalWriteConsuming = 0.0;

lprofP_OUTPUT pOutputHead = NULL;
lprofP_OUTPUT pOutputTail = NULL;
const int nOutputBufferSize = (1024*5);
char* pOutputBuffer = NULL;
int nBufferWrited = 0;

#ifdef __cplusplus
extern "C" {
#endif
	//void UnitySendMessage(const char* obj, const char* method, const char* msg);
#ifdef __cplusplus
}
#endif

//pOutputCallback = NULL;

void sendUnityMessage(const char* pMsg)
{
#ifdef __cplusplus
	if (pUnityMethod && pUnityObject)
		UnitySendMessage(pUnityObject, pUnityMethod, pMsg);
#endif
	if (pOutputCallback)
		pOutputCallback(pMsg);
}

void output(const char *format, ...) {
	//LARGE_INTEGER timestart;

	va_list ap;
	va_start(ap, format);
	vfprintf(outf, format, ap);
	va_end(ap);

	fflush(outf);

}

void lprofP_outputToFile(FILE* file,const char* format,...)
{
	if(file)
	{
		va_list ap;
		va_start(ap, format);
		vfprintf(file, format, ap);
		va_end(ap);
		fflush(file);
	}
}


void lprofP_toBuffer(char* str,int length)
{
	if(nBufferWrited + length >= nOutputBufferSize)
	{
		if(nBufferWrited > 0)
			output(pOutputBuffer);
		
		output(str);
		
		memset(pOutputBuffer,0x0,nOutputBufferSize);
		
		nBufferWrited = 0;
		
	}
	else
	{
		strcat(pOutputBuffer,str);
		nBufferWrited += length;
	}
}

void lprofP_addFrame(int id, char* str)
{
	lprof_NODE* pNode = (lprof_NODE*)malloc(sizeof(lprof_NODE));
	if(pNode)
	{
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
	
}

void lprofP_addData(char* str)
{
	int len;
	char* psz;
	if (pOutputTail)
	{
		if (pOutputTail->data)
		{
			len = (int)strlen(pOutputTail->data) + (int)strlen(str) + 3;
			psz = (char*)malloc(len);
			if(psz)
			{
				memset(psz, 0x0, len);
				strcpy(psz, pOutputTail->data);
				strcat(psz, ",\n");
				strcat(psz, str);
				free(str);
				free(pOutputTail->data);
				pOutputTail->data = psz;
			}
			
		}
		else
		{
			pOutputTail->data = str;
		}
		
	}
}

void lprofP_output()
{
	int nLen;
	char* psz;
	if (pOutputHead != NULL)
	{
		lprofP_OUTPUT pOut = pOutputHead;
		if (pOut->data)
		{
			nLen = (int)strlen(pOut->frame) + 3;
			psz = (char*)malloc(nLen);
			if(psz)
			{
				memset(psz, 0x0, nLen);
				strcpy(psz, pOut->frame);
				strcat(psz, ",\n");
				if (pOutputCallback)
				{
					pOutputCallback(psz);
				}
				lprofP_toBuffer(psz,nLen);
				free(psz);
			}
			nLen = (int)strlen(pOut->data) + 3;
			psz = (char*)malloc(nLen);
			if(psz)
			{
				memset(psz, 0x0, nLen);
				strcpy(psz, pOut->data);
				strcat(psz, ",\n");
				sendUnityMessage(psz);
				lprofP_toBuffer(psz,nLen);
				free(psz);
			}
			sPrevNode.id = pOut->id;
			sPrevNode.data = 1;

			free(pOut->data);			
		}
		else
		{
			if (sPrevNode.id != pOut->id && sPrevNode.data != 0)
			{
				nLen = (int)strlen(pOut->frame) + 3;
				psz = (char*)malloc(nLen);
				if(psz)
				{
					memset(psz, 0x0, nLen);
					strcpy(psz, pOut->frame);
					strcat(psz, ",\n");
					sendUnityMessage(psz);
					lprofP_toBuffer(psz,nLen);
					free(psz);
					sPrevNode.id = pOut->id;
					sPrevNode.data = 0;
				}
			}
		}
		pOutputHead = pOut->next;
		if (pOutputTail == pOut)
			pOutputTail = pOutputHead;

		free(pOut->frame);
		free(pOut);
		nCacheNode--;
	}
}

void lprofP_close()
{
	lprofP_OUTPUT pCurrent = NULL;
	if(pOutputBuffer)
	{
		output(pOutputBuffer);
		free(pOutputBuffer);
		pOutputBuffer = NULL;
		nBufferWrited = 0;
	}
	sPrevNode.id = 0;
	sPrevNode.data = 0;
	pCurrent = pOutputHead;
	while (pCurrent)
	{
		lprofP_OUTPUT pNext = pCurrent->next;
		free(pCurrent);
		pCurrent = pNext;
	}
	pOutputHead = NULL;
	pOutputTail = NULL;
	//dTotalWriteConsuming = 0.0;
	nCacheNode = 0;
}

void lprofP_open()
{
	if(pOutputBuffer)
		free(pOutputBuffer);
	nBufferWrited = 0;
	pOutputBuffer = (char*)malloc(sizeof(char)*nOutputBufferSize);
	memset(pOutputBuffer,0x0,sizeof(char)*nOutputBufferSize);
}

/*
void lprofP_outputToFile(FILE* file, const char* format, ...)
{
	if (file)
	{
		va_list ap;
		va_start(ap, format);
		vfprintf(file, format, ap);
		va_end(ap);
		fflush(file);
	}
}
*/