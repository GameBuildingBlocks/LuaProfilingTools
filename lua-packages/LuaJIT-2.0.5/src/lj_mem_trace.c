#define lj_mem_tracer_c

#include "lj_obj.h"

#ifdef LUAJIT_USE_MEM_TRACE

#include "lj_mem_trace.h"
#include "lua.h"

#include <stdio.h>
#include <string.h>
#include <stdlib.h>
#include <execinfo.h>
#include <signal.h>
#include <time.h>
#ifdef WIN32_LEAN_AND_MEAN
#include <windows.h>
#else
#include <sys/time.h>
#endif

static int binitialize_mem_trace = 0;
static int btrace_mem_inprogress = 0;
static FILE *mem_trace_file = NULL;
static int mem_trace_count = 0;

#define MAX_DATE_SIZE 50
#define MAX_STACK_SIZE 200

typedef struct mem_trace_info
{
    char *name;
    char *name_what;
    char *what;
    char *source;
    int current_line;
    int line_defined;
    int last_line_defined;
    size_t mem_size;
    char *log_date;
    long long tv_usec;
    long address;
    void *stack_trace_buffer[MAX_STACK_SIZE];
    char **stack_trace_info;
    size_t stack_trace_size;
} mem_trace_info_t;

#define MAX_TRACE_SIZE 50

static mem_trace_info_t trace_infos[MAX_TRACE_SIZE];
static size_t current_trace_index = 0;

static char *str_new_cpy(const char *src)
{
    size_t sz;
    char *deep_copy;
    deep_copy = NULL;
    if (src)
    {
        sz = strlen(src);
        deep_copy = (char *)malloc(sz + 1);
        deep_copy[sz] = 0;
        if (0 != sz)
        {
            strncpy(deep_copy, src, sz);
        }
    }
    return deep_copy;
}

static void dump_stack_info(mem_trace_info_t *trace_info)
{
#ifdef WIN32_LEAN_AND_MEAN
 // TODO windows with StackWalk
#else
    trace_info->stack_trace_size = backtrace(trace_info->stack_trace_buffer, MAX_STACK_SIZE);
    trace_info->stack_trace_info = backtrace_symbols(trace_info->stack_trace_buffer, trace_info->stack_trace_size);
#endif
}

static void flush_trace_info(mem_trace_info_t *trace_info)
{
#define TRACE_FEILD_SEPERATOR ","

    if (0 == trace_info->mem_size)
    {
        free(trace_info->name);
        free(trace_info->name_what);
        free(trace_info->what);
        free(trace_info->source);
        free(trace_info->log_date);
        if (0 < trace_info->stack_trace_size)
        {
            size_t i;
            for (i = 0; i < trace_info->stack_trace_size; ++i)
            {
                free(trace_info->stack_trace_info[i]);
            }
            free(trace_info->stack_trace_info);
        }
        memset(trace_info, 0, sizeof(mem_trace_info_t));
        return;
    }

    if (trace_info->name)
    {
        fprintf(mem_trace_file, "%s", trace_info->name);
        free(trace_info->name);
    }
    else
    {
        fprintf(mem_trace_file, "%s", "WORLD");
    }
    fprintf(mem_trace_file, "%s", TRACE_FEILD_SEPERATOR);

    if (trace_info->name_what && 0 != strlen(trace_info->name_what))
    {
        fprintf(mem_trace_file, "%s", trace_info->name_what);
        free(trace_info->name_what);
    }
    else
    {
        fprintf(mem_trace_file, "%s", "world");
    }
    fprintf(mem_trace_file, "%s", TRACE_FEILD_SEPERATOR);

    if (trace_info->what)
    {
        fprintf(mem_trace_file, "%s", trace_info->what);
        free(trace_info->what);
    }
    fprintf(mem_trace_file, "%s", TRACE_FEILD_SEPERATOR);

    if (trace_info->source)
    {
        fprintf(mem_trace_file, "%s", trace_info->source);
        free(trace_info->source);
    }
    fprintf(mem_trace_file, "%s", TRACE_FEILD_SEPERATOR);

    fprintf(mem_trace_file, "%d", trace_info->current_line);
    fprintf(mem_trace_file, "%s", TRACE_FEILD_SEPERATOR);

    fprintf(mem_trace_file, "%d", trace_info->line_defined);
    fprintf(mem_trace_file, "%s", TRACE_FEILD_SEPERATOR);

    fprintf(mem_trace_file, "%d", trace_info->last_line_defined);
    fprintf(mem_trace_file, "%s", TRACE_FEILD_SEPERATOR);

#ifdef LJ_64
    fprintf(mem_trace_file, "%lu", trace_info->mem_size);
#else
    fprintf(mem_trace_file, "%u", trace_info->mem_size);
#endif
    fprintf(mem_trace_file, TRACE_FEILD_SEPERATOR);

    fprintf(mem_trace_file, "%s", trace_info->log_date);
    fprintf(mem_trace_file, TRACE_FEILD_SEPERATOR);
    free(trace_info->log_date);

    fprintf(mem_trace_file, "%lld", trace_info->tv_usec);
    fprintf(mem_trace_file, TRACE_FEILD_SEPERATOR);

    fprintf(mem_trace_file, "%lX", trace_info->address);
    fprintf(mem_trace_file, TRACE_FEILD_SEPERATOR);

    if (0 != trace_info->stack_trace_size)
    {
        size_t i;
        for (i = 0; i < trace_info->stack_trace_size; ++i)
        {
            fprintf(mem_trace_file, "%s|", trace_info->stack_trace_info[i]);
        }

        free(trace_info->stack_trace_info);
    }

    fprintf(mem_trace_file, "\r\n");

    memset(trace_info, 0, sizeof(mem_trace_info_t));
}

static void flush_trace_buffer()
{
    size_t i;
    for (i = 0; i < current_trace_index; ++i)
    {
        flush_trace_info(&trace_infos[i]);
    }
    memset(trace_infos, 0, sizeof(trace_infos));
    current_trace_index = 0;
}

static mem_trace_info_t *get_mem_trace_info()
{
    mem_trace_info_t *trace_info;

    if (MAX_TRACE_SIZE <= current_trace_index)
    {
        flush_trace_buffer();
    }

    trace_info = &trace_infos[current_trace_index++];
    memset(trace_info, 0, sizeof(mem_trace_info_t));
    return trace_info;
}

static mem_trace_info_t *current_mem_trace_info()
{
    return &trace_infos[0 == current_trace_index ? current_trace_index :current_trace_index - 1];
}

static long long current_msecond()
{
#ifdef WIN32_LEAN_AND_MEAN
    LARGE_INTEGER pf;
    QueryPerformanceFrequency(&pf)
    LARGE_INTEGER pc;
    QueryPerformanceCounter(&pc)
    return fc.QuadPart / (pf.QuadPart / 1000);
#else
    struct timeval t;
    gettimeofday(&t, NULL);
    return t.tv_sec * 1000 + (t.tv_usec / 1000);
#endif
}

static void generate_mem_trace_info(
    lua_State *L,
    lua_Debug *ar)
{
    mem_trace_info_t *trace_info;
    trace_info = get_mem_trace_info();

    lua_getinfo(L, "nSl", ar);

    trace_info->name = str_new_cpy(ar->name);
    trace_info->name_what = str_new_cpy(ar->namewhat);
    trace_info->what = str_new_cpy(ar->what);
    trace_info->source = str_new_cpy(ar->source);
    trace_info->current_line = ar->currentline;
    trace_info->line_defined = ar->linedefined;
    trace_info->last_line_defined = ar->lastlinedefined;
}

static void stat_mem_trace_info(void *addr, const size_t mb)
{
    mem_trace_info_t *trace_info;
    trace_info = current_mem_trace_info();
    trace_info->mem_size += mb;
    trace_info->tv_usec = current_msecond();
    trace_info->address = (long)addr;

    time_t now_time;
    now_time = time(NULL);
    struct tm *local_time;
    local_time = localtime(&now_time);
    trace_info->log_date = (char *)malloc(MAX_DATE_SIZE);
    memset(trace_info->log_date, 0, MAX_DATE_SIZE);
    sprintf(trace_info->log_date, "%s", asctime(local_time));
    size_t date_size;
    date_size = strlen(trace_info->log_date);
    trace_info->log_date[date_size - 1] = 0;

    dump_stack_info(trace_info);
}

static void lua_function_callIN(lua_State *L, lua_Debug *ar)
{
    generate_mem_trace_info(L, ar);
    btrace_mem_inprogress = 1;
}

static void lua_function_callOUT(lua_State *L, lua_Debug *ar)
{
    btrace_mem_inprogress = 0;
}

static void callback_trace_function(lua_State *L, lua_Debug *ar)
{
    if (ar->event)
    {
        lua_function_callIN(L, ar);
    }
    else
    {
        lua_function_callOUT(L, ar);
    }
}

void lj_start_trace_memory(lua_State *L, const char *file_name)
{
    if (binitialize_mem_trace)
        return;

    char *name = "/mem_trace.mt";

#ifdef WIN32_LEAN_AND_MEAN
    char *home_path = getenv("HOMEPATH");
#else
    char *home_path = getenv("HOME");
    char *root_path = "/root";
    if (0 == strcmp(root_path, home_path))
        home_path = "/home";
#endif

    size_t sz = strlen(home_path) + strlen(name);
    char *trace_file_name = (char*)malloc(sz + 1);
    trace_file_name[sz] = 0;
    strcpy(trace_file_name, home_path);
    strcat(trace_file_name, name);

    mem_trace_file = fopen(trace_file_name, "a+");
    if (NULL == mem_trace_file)
        return;

    lua_sethook(L, callback_trace_function, LUA_MASKLINE, 0);
    memset(trace_infos, 0, sizeof(trace_infos));
    binitialize_mem_trace = 1;
}

void lj_update_trace_memory(void *addr, const size_t mb)
{
    if (binitialize_mem_trace && btrace_mem_inprogress)
        stat_mem_trace_info(addr, mb);
    ++mem_trace_count;
}

void lj_end_trace_memory()
{
    if (binitialize_mem_trace && mem_trace_file) {
        flush_trace_buffer();
        fclose(mem_trace_file);
        binitialize_mem_trace = 0;
        btrace_mem_inprogress = 0;
    }
    printf("CALL-MMAP-COUNT:%d\r\n", mem_trace_count);
}

#else

void lj_start_trace_memory(lua_State *L, const char* file_name) {}
void lj_update_trace_memory(void *addr, const size_t mb) {}
void lj_end_trace_memory() {}

#endif // LUAJIT_USE_MEM_TRACE