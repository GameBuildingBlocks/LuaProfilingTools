#pragma once
#include <time.h>

/*
	跨平台支持
	2016-08-10 lennon.c
*/
#pragma warning (disable:4100)
#ifdef _MSC_VER
	#include <windows.h>
#else
	#include <pthread.h>
	typedef struct timeval LARGE_INTEGER;
#endif