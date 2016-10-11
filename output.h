#ifndef _OUTPUT_H
#define _OUTPUT_H

#include <time.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <stdarg.h>
#include <assert.h>
#include "lp.h"

FILE *outf;

typedef void(_stdcall *pfnoutputCallback)(char* info);

pfnoutputCallback pOutputCallback;

double dTotalWriteConsuming;

typedef struct tag_lprof_OUTPUT lprof_OUTPUT;

struct tag_lprof_OUTPUT
{
	int id;
	char* frame;
	char* data;
	lprof_OUTPUT* next;
};

typedef lprof_OUTPUT* lprofP_OUTPUT;
typedef struct tag_lprof_OUTPUT lprof_NODE;

struct tag_lprof_PREV
{
	int id;
	int data;
};

typedef struct tag_lprof_PREV lprof_PREVNODE;

void lprofP_addFrame(int id,char* str);
void lprofP_addData(char* str);
void lprofP_output();
void lprofP_close();



#endif