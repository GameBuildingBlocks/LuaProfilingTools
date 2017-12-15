	#pragma once
#include <time.h>

/*
	跨平台支持
	2016-08-10 lennon.c
*/
#pragma warning (disable:4100)
#ifdef _MSC_VER
	#include <windows.h>
	#define GETPID GetCurrentProcessId
	
	#define SLEEP_SHORT_TIME() Sleep(1)
	#define DllExport __declspec(dllexport)

	#define NONLOCK_QUEUE_SIZE (100000)

#else
	#include <pthread.h>
	#include <unistd.h>
	typedef struct timeval LARGE_INTEGER;
	#define GETPID getpid
	#define SLEEP_SHORT_TIME()  usleep(10)
	#define DllExport  
	#define NONLOCK_QUEUE_SIZE (5000000)
#endif

#if !defined(LUAGETP)
#define lua_getuservalue 
#define lua_rawgetp
#define lua_rawsetp
#endif
