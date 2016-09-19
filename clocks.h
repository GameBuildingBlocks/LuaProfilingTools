/*
** LuaProfiler
** Copyright Kepler Project 2005-2007 (http://www.keplerproject.org/luaprofiler)
** $Id: clocks.h,v 1.4 2007-08-22 19:23:53 carregal Exp $
*/

/*****************************************************************************
clocks.h:
   Module to register the time (seconds) between two events

Design:
   'lprofC_start_timer()' marks the first event
   'lprofC_get_seconds()' gives you the seconds elapsed since the timer
                          was started
*****************************************************************************/

#include <time.h>
#include "lp.h"

void lprofC_start_timer(clock_t *time_marker);
float lprofC_get_seconds(clock_t time_marker);
time_t lprofC_get_current();

void lprofC_start_timer2(LARGE_INTEGER *nBeginTime);
double lprofC_get_seconds2(LARGE_INTEGER *nBeginTime);
double lprofC_get_millisecond(LARGE_INTEGER *nTime);
double lprofC_get_interval(LARGE_INTEGER *nBeginTime, LARGE_INTEGER *nEndTime);