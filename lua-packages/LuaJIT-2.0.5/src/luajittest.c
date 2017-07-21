#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <malloc.h>
#include <assert.h>
#include <time.h>

#define luajittest_c

#include "lua.h"
#include "lualib.h"
#include "lauxlib.h"

#define max(x, y) ((x) > (y) ? (x) : (y));

  // luajit max stack 65500
const int max_stack_size = 60000;
const char *lua_code = "string.len(\"lua block memory test\")";

void try_lua_call(lua_State *L)
{
   luaL_loadstring(L, lua_code);
  if (lua_pcall(L, 0, 0, 0)) {
    printf("%s\r\n", lua_tostring(L, -1));
    lua_pop(L, 1);
  } 
}

void align_block_test(lua_State *L, int max_size)
{
  int i = 0;
  int istack = 0;

  printf("%s\r\n", "8b align!");
  {
    const int align_size_8b = 8;
    const int count_8b = max_size / align_size_8b;
    char *str_8b = (char*)malloc(align_size_8b);
    str_8b[align_size_8b - 1] = 0;
    for (i =0, istack = 0; i < count_8b; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      memset(str_8b, i, align_size_8b - 1);
      lua_pushstring(L, str_8b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_8b);
  }

  printf("%s\r\n", "16b align!");
  {
    const int align_size_16b = 16;
    const int count_16b = max_size / align_size_16b;
    char *str_16b = (char*)malloc(align_size_16b);
    str_16b[align_size_16b - 1] = 0;
    for (i =0, istack = 0; i < count_16b; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      memset(str_16b, i, align_size_16b - 1);
      lua_pushstring(L, str_16b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_16b);
  }

  printf("%s\r\n", "32b align!");
  {
    const int align_size_32b = 32;
    const int count_16b = max_size / align_size_32b;
    char *str_16b = (char*)malloc(align_size_32b);
    str_16b[align_size_32b - 1] = 0;
    for (i =0, istack = 0; i < count_16b; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      memset(str_16b, i, align_size_32b - 1);
      lua_pushstring(L, str_16b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_16b);
  }

  printf("%s\r\n", "64b align!");
  {
    const int align_size_64b = 64;
    const int count_64b = max_size / align_size_64b;
    char *str_64b = (char*)malloc(align_size_64b);
    str_64b[align_size_64b - 1] = 0;
    for (i =0, istack = 0; i < count_64b; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      memset(str_64b, i, align_size_64b - 1);
      lua_pushstring(L, str_64b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_64b);
  }

  printf("%s\r\n", "128b align!");
  {
    const int align_size_128b = 256;
    const int count_128b = max_size / align_size_128b;
    char *str_128b = (char*)malloc(align_size_128b);
    str_128b[align_size_128b - 1] = 0;
    for (i =0, istack = 0; i < count_128b; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      memset(str_128b, i, align_size_128b - 1);
      lua_pushstring(L, str_128b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_128b);
  }

  printf("%s\r\n", "256b align!");
  {
    const int align_size_256b = 256;
    const int count_256b = max_size / align_size_256b;
    char *str_256b = (char*)malloc(align_size_256b);
    str_256b[align_size_256b - 1] = 0;
    for (i =0, istack = 0; i < count_256b; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      memset(str_256b, i, align_size_256b - 1);
      lua_pushstring(L, str_256b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_256b);
  }

  printf("%s\r\n", "512b align!");
  {
    const int align_size_521b = 512;
    const int count_512b = max_size / align_size_521b;
    char *str_215b = (char*)malloc(align_size_521b);
    str_215b[align_size_521b - 1] = 0;
    for (i =0, istack = 0; i < count_512b; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      memset(str_215b, i, align_size_521b - 1);
      lua_pushstring(L, str_215b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_215b);
  }

  printf("%s\r\n", "1k align!");
  {
    const int align_size_1k = 1 * 1024;
    const int count_1k = max_size / align_size_1k;
    char *str_1k = (char*)malloc(align_size_1k);
    str_1k[align_size_1k - 1] = 0;
    for (i =0, istack = 0; i < count_1k; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      memset(str_1k, i, align_size_1k - 1);
      lua_pushstring(L, str_1k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_1k);
  }

  printf("%s\r\n", "2k align!");
    {
      const int align_size_2k = 2 * 1024;
      const int count_2k = max_size / align_size_2k;
      char *str_2k = (char*)malloc(align_size_2k);
      str_2k[align_size_2k - 1] = 0;
      for (i =0, istack = 0; i < count_2k; ++i, istack += 2) {
        try_lua_call(L);
        lua_pushinteger(L, i);
        memset(str_2k, i, align_size_2k - 1);
        lua_pushstring(L, str_2k);
        if (max_stack_size < istack) {
            lua_settop(L, 0);
            istack = 0;
        }
      }
      istack = 0;
      lua_settop(L, 0);
      free(str_2k);
    }

  printf("%s\r\n", "4k align!");
  {
    const int align_size_4k = 4 * 1024;
    const int count_4k = max_size / align_size_4k;
    char *str_4k = (char*)malloc(align_size_4k);
    str_4k[align_size_4k - 1] = 0;
    for (i =0, istack = 0; i < count_4k; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      memset(str_4k, i, align_size_4k - 1);
      lua_pushstring(L, str_4k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_4k);
  }

  printf("%s\r\n", "8k align!");
  {
    const int align_size_8k = 8 * 1024;
    const int count_8k = max_size / align_size_8k;
    char *str_8k = (char*)malloc(align_size_8k);
    str_8k[align_size_8k - 1] = 0;
    for (i =0, istack = 0; i < count_8k; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      memset(str_8k, i, align_size_8k - 1);
      lua_pushstring(L, str_8k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_8k);
  }

  printf("%s\r\n", "16k align!");
  {
    const int align_size_16k = 16 * 1024;
    const int count_16k = max_size / align_size_16k;
    char *str_16k = (char*)malloc(align_size_16k);
    str_16k[align_size_16k - 1] = 0;
    for (i =0, istack = 0; i < count_16k; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      memset(str_16k, i, align_size_16k - 1);
      lua_pushstring(L, str_16k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_16k);
  }

  printf("%s\r\n", "32k align!");
  {
    const int align_size_32k = 32 * 1024;
    const int count_32k = max_size / align_size_32k;
    char *str_32k = (char*)malloc(align_size_32k);
    str_32k[align_size_32k - 1] = 0;
    for (i =0, istack = 0; i < count_32k; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      memset(str_32k, i, align_size_32k - 1);
      lua_pushstring(L, str_32k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_32k);
  }

  printf("%s\r\n", "64k align!");
  {
    const int align_size_64k = 64 * 1024;
    const int count_64k = max_size / align_size_64k;
    char *str_64k = (char*)malloc(align_size_64k);
    str_64k[align_size_64k - 1] = 0;
    for (i =0, istack = 0; i < count_64k; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      memset(str_64k, i, align_size_64k - 1);
      lua_pushstring(L, str_64k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_64k);
  }

  printf("%s\r\n", "128k align!");
  {
    const int align_size_128k = 128 * 1024;
    const int count_128k = max_size / align_size_128k;
    char *str_128k = (char*)malloc(align_size_128k);
    str_128k[align_size_128k - 1] = 0;
    for (i =0, istack = 0; i < count_128k; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      memset(str_128k, i, align_size_128k - 1);
      lua_pushstring(L, str_128k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_128k);
  }

  printf("%s\r\n", "256k align!");
  {
    const int align_size_256k = 128 * 1024;
    const int count_256k = max_size / align_size_256k;
    char *str_256k = (char*)malloc(align_size_256k);
    str_256k[align_size_256k - 1] = 0;
    for (i =0, istack = 0; i < count_256k; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      memset(str_256k, i, align_size_256k - 1);
      lua_pushstring(L, str_256k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_256k);
  }

  printf("%s\r\n", "512k align!");
  {
    const int align_size_512k = 512 * 1024;
    const int count_512k = max_size / align_size_512k;
    char *str_512k = (char*)malloc(align_size_512k);
    str_512k[align_size_512k - 1] = 0;
    for (i =0, istack = 0; i < count_512k; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      memset(str_512k, i, align_size_512k - 1);
      lua_pushstring(L, str_512k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_512k);
  }

  printf("%s\r\n", "1024k align!");
  {
    const int align_size_1024k = 1024 * 1024;
    const int count_1024k = max_size / align_size_1024k;
    char *str_1024k = (char*)malloc(align_size_1024k);
    str_1024k[align_size_1024k - 1] = 0;
    for (i =0, istack = 0; i < count_1024k; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      memset(str_1024k, i, count_1024k - 1);
      lua_pushstring(L, str_1024k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_1024k);
  }
}

void noalign_block_test(lua_State *L, int max_size)
{
  int i = 0;
  int istack = 0;
  int min_size = 1;
  // luajit max stack 65500

  srand(time(0));

  printf("%s\r\n", "8b noalign!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_8b = max(rand() % (8), min_size);
      i += align_size_8b;
      char *str_8b = (char*)malloc(align_size_8b);
      memset(str_8b, i, align_size_8b);
      str_8b[align_size_8b - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_8b);
      free(str_8b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "16b noalign!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_16b = max(rand() % (16), min_size);
      i += align_size_16b;
      char *str_16b = (char*)malloc(align_size_16b);
      memset(str_16b, i, align_size_16b);
      str_16b[align_size_16b - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_16b);
      free(str_16b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "32b noalign!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_32b = max(rand() % (32), min_size);
      i += align_size_32b;
      char *str_32b = (char*)malloc(align_size_32b);
      memset(str_32b, i, align_size_32b);
      str_32b[align_size_32b - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_32b);
      free(str_32b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "64b noalign!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_64b = max(rand() % (64), min_size);
      i += align_size_64b;
      char *str_64b = (char*)malloc(align_size_64b);
      memset(str_64b, i, align_size_64b);
      str_64b[align_size_64b - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_64b);
      free(str_64b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "128b noalign!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_128b = max(rand() % (128), min_size);
      i += align_size_128b;
      char *str_128b = (char*)malloc(align_size_128b);
      memset(str_128b, i, align_size_128b);
      str_128b[align_size_128b - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_128b);
      free(str_128b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "256b noalign!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_256b = max(rand() % (256), min_size);
      i += align_size_256b;
      char *str_256b = (char*)malloc(align_size_256b);
      memset(str_256b, i, align_size_256b);
      str_256b[align_size_256b - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_256b);
      free(str_256b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "512b noalign!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_512b = max(rand() % (512), min_size);
      i += align_size_512b;
      char *str_512b = (char*)malloc(align_size_512b);
      memset(str_512b, i, align_size_512b);
      str_512b[align_size_512b - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_512b);
      free(str_512b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "1k noalign!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_1k = max(rand() % (1 * 1024), min_size);
      i += align_size_1k;
      char *str_1k = (char*)malloc(align_size_1k);
      memset(str_1k, i, align_size_1k);
      str_1k[align_size_1k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_1k);
      free(str_1k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "2k noalign!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_2k = max(rand() % (2 * 1024), min_size);
      i += align_size_2k;
      char *str_2k = (char*)malloc(align_size_2k);
      memset(str_2k, i, align_size_2k);
      str_2k[align_size_2k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_2k);
      free(str_2k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "4k noalign!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_4k = max(rand() % (4 * 1024), min_size);
      i += align_size_4k;
      char *str_4k = (char*)malloc(align_size_4k);
      memset(str_4k, i, align_size_4k);
      str_4k[align_size_4k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_4k);
      free(str_4k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "8k noalign!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_8k = max(rand() % (8 * 1024), min_size);
      i += align_size_8k;
      char *str_8k = (char*)malloc(align_size_8k);
      memset(str_8k, i, align_size_8k);
      str_8k[align_size_8k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_8k);
      free(str_8k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "16k noalign!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_16k = max(rand() % (16 * 1024), min_size);
      i += align_size_16k;
      char *str_16k = (char*)malloc(align_size_16k);
      memset(str_16k, i, align_size_16k);
      str_16k[align_size_16k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_16k);
      free(str_16k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "32k noalign!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_32k = max(rand() % (32 * 1024), min_size);
      i += align_size_32k;
      char *str_32k = (char*)malloc(align_size_32k);
      memset(str_32k, i, align_size_32k);
      str_32k[align_size_32k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_32k);
      free(str_32k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "64k noalign!");
  {
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_64k = max(rand() % (64 * 1024), min_size);
      i += align_size_64k;
      char *str_64k = (char*)malloc(align_size_64k);
      memset(str_64k, i, align_size_64k);
      str_64k[align_size_64k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_64k);
      free(str_64k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "128k noalign!");
  {
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_128k = max(rand() % (128 * 1024), min_size);
      i += align_size_128k;
      char *str_128k = (char*)malloc(align_size_128k);
      memset(str_128k, i, align_size_128k);
      str_128k[align_size_128k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_128k);
      free(str_128k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "256k noalign!");
  {
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_256k = max(rand() % (256 * 1024), min_size);
      i += align_size_256k;
      char *str_256k = (char*)malloc(align_size_256k);
      memset(str_256k, i, align_size_256k);
      str_256k[align_size_256k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_256k);
      free(str_256k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "512k noalign!");
  {
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_512k = max(rand() % (256 * 1024), min_size);
      i += align_size_512k;
      char *str_512k = (char*)malloc(align_size_512k);
      memset(str_512k, i, align_size_512k);
      str_512k[align_size_512k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_512k);
      free(str_512k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "1024k noalign!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_1024k = max(rand() % (512 * 1024), min_size);
      i += align_size_1024k;
      char *str_1024k = (char*)malloc(align_size_1024k);
      memset(str_1024k, i, align_size_1024k);
      str_1024k[align_size_1024k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_1024k);
      free(str_1024k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }
}

void align_block_test_shared(lua_State *L, int max_size)
{
  int i = 0;
  int istack = 0;

  printf("%s\r\n", "8b align shared!");
  {
    const int align_size_8b = 8;
    const int count_8b = max_size / align_size_8b;
    char *str_8b = (char*)malloc(align_size_8b);
    memset(str_8b, 1, align_size_8b - 1);
    str_8b[align_size_8b - 1] = 0;
    for (i =0, istack = 0; i < count_8b; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      lua_pushstring(L, str_8b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_8b);
  }

  printf("%s\r\n", "16b align shared!");
  {
    const int align_size_16b = 16;
    const int count_16b = max_size / align_size_16b;
    char *str_16b = (char*)malloc(align_size_16b);
    memset(str_16b, 1, align_size_16b - 1);
    str_16b[align_size_16b - 1] = 0;
    for (i =0, istack = 0; i < count_16b; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      lua_pushstring(L, str_16b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_16b);
  }

  printf("%s\r\n", "32b align shared!");
  {
    const int align_size_32b = 32;
    const int count_16b = max_size / align_size_32b;
    char *str_32b = (char*)malloc(align_size_32b);
    memset(str_32b, 1, align_size_32b - 1);
    str_32b[align_size_32b - 1] = 0;
    for (i =0, istack = 0; i < count_16b; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      lua_pushstring(L, str_32b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_32b);
  }

  printf("%s\r\n", "64b align shared!");
  {
    const int align_size_64b = 64;
    const int count_64b = max_size / align_size_64b;
    char *str_64b = (char*)malloc(align_size_64b);
    memset(str_64b, 1, align_size_64b - 1);
    str_64b[align_size_64b - 1] = 0;
    for (i =0, istack = 0; i < count_64b; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      lua_pushstring(L, str_64b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_64b);
  }

  printf("%s\r\n", "128b align shared!");
  {
    const int align_size_128b = 256;
    const int count_128b = max_size / align_size_128b;
    char *str_128b = (char*)malloc(align_size_128b);
    memset(str_128b, 1, align_size_128b - 1);
    str_128b[align_size_128b - 1] = 0;
    for (i =0, istack = 0; i < count_128b; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      lua_pushstring(L, str_128b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_128b);
  }

  printf("%s\r\n", "256b align shared!");
  {
    const int align_size_256b = 256;
    const int count_256b = max_size / align_size_256b;
    char *str_256b = (char*)malloc(align_size_256b);
    memset(str_256b, 1, align_size_256b - 1);
    str_256b[align_size_256b - 1] = 0;
    for (i =0, istack = 0; i < count_256b; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      lua_pushstring(L, str_256b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_256b);
  }

  printf("%s\r\n", "512b align shared!");
  {
    const int align_size_521b = 512;
    const int count_512b = max_size / align_size_521b;
    char *str_512b = (char*)malloc(align_size_521b);
    memset(str_512b, 1, align_size_521b - 1);
    str_512b[align_size_521b - 1] = 0;
    for (i =0, istack = 0; i < count_512b; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      lua_pushstring(L, str_512b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_512b);
  }

  printf("%s\r\n", "1k align shared!");
  {
    const int align_size_1k = 1 * 1024;
    const int count_1k = max_size / align_size_1k;
    char *str_1k = (char*)malloc(align_size_1k);
    memset(str_1k, 1, align_size_1k - 1);
    str_1k[align_size_1k - 1] = 0;
    for (i =0, istack = 0; i < count_1k; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      lua_pushstring(L, str_1k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_1k);
  }

  printf("%s\r\n", "2k align shared!");
    {
      const int align_size_2k = 2 * 1024;
      const int count_2k = max_size / align_size_2k;
      char *str_2k = (char*)malloc(align_size_2k);
      memset(str_2k, 1, align_size_2k - 1);
      str_2k[align_size_2k - 1] = 0;
      for (i =0, istack = 0; i < count_2k; ++i, istack += 2) {
        try_lua_call(L);
        lua_pushinteger(L, i);
        lua_pushstring(L, str_2k);
        if (max_stack_size < istack) {
            lua_settop(L, 0);
            istack = 0;
        }
      }
      istack = 0;
      lua_settop(L, 0);
      free(str_2k);
    }

  printf("%s\r\n", "4k align shared!");
  {
    const int align_size_4k = 4 * 1024;
    const int count_4k = max_size / align_size_4k;
    char *str_4k = (char*)malloc(align_size_4k);
    memset(str_4k, 1, align_size_4k - 1);
    str_4k[align_size_4k - 1] = 0;
    for (i =0, istack = 0; i < count_4k; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      lua_pushstring(L, str_4k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_4k);
  }

  printf("%s\r\n", "8k align shared!");
  {
    const int align_size_8k = 8 * 1024;
    const int count_8k = max_size / align_size_8k;
    char *str_8k = (char*)malloc(align_size_8k);
    memset(str_8k, 1, align_size_8k - 1);
    str_8k[align_size_8k - 1] = 0;
    for (i =0, istack = 0; i < count_8k; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      lua_pushstring(L, str_8k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_8k);
  }

  printf("%s\r\n", "16k align shared!");
  {
    const int align_size_16k = 16 * 1024;
    const int count_16k = max_size / align_size_16k;
    char *str_16k = (char*)malloc(align_size_16k);
    memset(str_16k, 1, align_size_16k - 1);
    str_16k[align_size_16k - 1] = 0;
    for (i =0, istack = 0; i < count_16k; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      lua_pushstring(L, str_16k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_16k);
  }

  printf("%s\r\n", "32k align shared!");
  {
    const int align_size_32k = 32 * 1024;
    const int count_32k = max_size / align_size_32k;
    char *str_32k = (char*)malloc(align_size_32k);
    memset(str_32k, 1, align_size_32k - 1);
    str_32k[align_size_32k - 1] = 0;
    for (i =0, istack = 0; i < count_32k; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      lua_pushstring(L, str_32k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_32k);
  }

  printf("%s\r\n", "64k align shared!");
  {
    const int align_size_64k = 64 * 1024;
    const int count_64k = max_size / align_size_64k;
    char *str_64k = (char*)malloc(align_size_64k);
    memset(str_64k, 1, align_size_64k - 1);
    str_64k[align_size_64k - 1] = 0;
    for (i =0, istack = 0; i < count_64k; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      lua_pushstring(L, str_64k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_64k);
  }

  printf("%s\r\n", "128k align shared!");
  {
    const int align_size_128k = 128 * 1024;
    const int count_128k = max_size / align_size_128k;
    char *str_128k = (char*)malloc(align_size_128k);
    memset(str_128k, 1, align_size_128k - 1);
    str_128k[align_size_128k - 1] = 0;
    for (i =0, istack = 0; i < count_128k; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      lua_pushstring(L, str_128k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_128k);
  }

  printf("%s\r\n", "256k align shared!");
  {
    const int align_size_256k = 128 * 1024;
    const int count_256k = max_size / align_size_256k;
    char *str_256k = (char*)malloc(align_size_256k);
    memset(str_256k, 1, align_size_256k - 1);
    str_256k[align_size_256k - 1] = 0;
    for (i =0, istack = 0; i < count_256k; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      lua_pushstring(L, str_256k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_256k);
  }

  printf("%s\r\n", "512k align shared!");
  {
    const int align_size_512k = 512 * 1024;
    const int count_512k = max_size / align_size_512k;
    char *str_512k = (char*)malloc(align_size_512k);
    memset(str_512k, 1, align_size_512k - 1);
    str_512k[align_size_512k - 1] = 0;
    for (i =0, istack = 0; i < count_512k; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      lua_pushstring(L, str_512k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_512k);
  }

  printf("%s\r\n", "1024k align shared!");
  {
    const int align_size_1024k = 1024 * 1024;
    const int count_1024k = max_size / align_size_1024k;
    char *str_1024k = (char*)malloc(align_size_1024k);
    memset(str_1024k, 1, count_1024k - 1);
    str_1024k[align_size_1024k - 1] = 0;
    for (i =0, istack = 0; i < count_1024k; ++i, istack += 2) {
      try_lua_call(L);
      lua_pushinteger(L, i);
      lua_pushstring(L, str_1024k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
    free(str_1024k);
  }
}

void noalign_block_test_shared(lua_State *L, int max_size)
{
  int i = 0;
  int istack = 0;
  int min_size = 1;
  // luajit max stack 65500

  srand(time(0));

  printf("%s\r\n", "8b noalign shared!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_8b = max(rand() % (8), min_size);
      i += align_size_8b;
      char *str_1k = (char*)malloc(align_size_8b);
      memset(str_1k, 1, align_size_8b);
      str_1k[align_size_8b - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_1k);
      free(str_1k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "16b noalign shared!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_16b = max(rand() % (16), min_size);
      i += align_size_16b;
      char *str_16b = (char*)malloc(align_size_16b);
      memset(str_16b, 1, align_size_16b);
      str_16b[align_size_16b - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_16b);
      free(str_16b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "32b noalign shared!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_32b = max(rand() % (32), min_size);
      i += align_size_32b;
      char *str_32b = (char*)malloc(align_size_32b);
      memset(str_32b, 1, align_size_32b);
      str_32b[align_size_32b - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_32b);
      free(str_32b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "64b noalign shared!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_64b = max(rand() % (64), min_size);
      i += align_size_64b;
      char *str_64b = (char*)malloc(align_size_64b);
      memset(str_64b,1, align_size_64b);
      str_64b[align_size_64b - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_64b);
      free(str_64b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "128b noalign shared!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_128b = max(rand() % (128), min_size);
      i += align_size_128b;
      char *str_128b = (char*)malloc(align_size_128b);
      memset(str_128b, 1, align_size_128b);
      str_128b[align_size_128b - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_128b);
      free(str_128b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "256b noalign shared!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_256b = max(rand() % (256), min_size);
      i += align_size_256b;
      char *str_256b = (char*)malloc(align_size_256b);
      memset(str_256b, 1, align_size_256b);
      str_256b[align_size_256b - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_256b);
      free(str_256b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "512b noalign shared!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_512b = max(rand() % (512), min_size);
      i += align_size_512b;
      char *str_512b = (char*)malloc(align_size_512b);
      memset(str_512b, 1, align_size_512b);
      str_512b[align_size_512b - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_512b);
      free(str_512b);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "1k noalign shared!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_1k = max(rand() % (1 * 1024), min_size);
      i += align_size_1k;
      char *str_1k = (char*)malloc(align_size_1k);
      memset(str_1k, 1, align_size_1k);
      str_1k[align_size_1k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_1k);
      free(str_1k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "2k noalign shared!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_2k = max(rand() % (2 * 1024), min_size);
      i += align_size_2k;
      char *str_2k = (char*)malloc(align_size_2k);
      memset(str_2k, 1, align_size_2k);
      str_2k[align_size_2k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_2k);
      free(str_2k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "4k noalign shared!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_4k = max(rand() % (4 * 1024), min_size);
      i += align_size_4k;
      char *str_4k = (char*)malloc(align_size_4k);
      memset(str_4k, 1, align_size_4k);
      str_4k[align_size_4k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_4k);
      free(str_4k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "8k noalign shared!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_8k = max(rand() % (8 * 1024), min_size);
      i += align_size_8k;
      char *str_8k = (char*)malloc(align_size_8k);
      memset(str_8k, 1, align_size_8k);
      str_8k[align_size_8k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_8k);
      free(str_8k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "16k noalign shared!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_16k = max(rand() % (16 * 1024), min_size);
      i += align_size_16k;
      char *str_16k = (char*)malloc(align_size_16k);
      memset(str_16k, 1, align_size_16k);
      str_16k[align_size_16k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_16k);
      free(str_16k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "32k noalign shared!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_32k = max(rand() % (32 * 1024), min_size);
      i += align_size_32k;
      char *str_32k = (char*)malloc(align_size_32k);
      memset(str_32k, 1, align_size_32k);
      str_32k[align_size_32k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_32k);
      free(str_32k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "64k noalign shared!");
  {
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_64k = max(rand() % (64 * 1024), min_size);
      i += align_size_64k;
      char *str_64k = (char*)malloc(align_size_64k);
      memset(str_64k, 1, align_size_64k);
      str_64k[align_size_64k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_64k);
      free(str_64k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "128k noalign shared!");
  {
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_128k = max(rand() % (128 * 1024), min_size);
      i += align_size_128k;
      char *str_128k = (char*)malloc(align_size_128k);
      memset(str_128k, 1, align_size_128k);
      str_128k[align_size_128k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_128k);
      free(str_128k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "256k noalign shared!");
  {
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_256k = max(rand() % (256 * 1024), min_size);
      i += align_size_256k;
      char *str_256k = (char*)malloc(align_size_256k);
      memset(str_256k, 1, align_size_256k);
      str_256k[align_size_256k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_256k);
      free(str_256k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "512k noalign shared!");
  {
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_512k = max(rand() % (256 * 1024), min_size);
      i += align_size_512k;
      char *str_512k = (char*)malloc(align_size_512k);
      memset(str_512k, 1, align_size_512k);
      str_512k[align_size_512k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_512k);
      free(str_512k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }

  printf("%s\r\n", "1024k noalign shared!");
  {
    min_size = 1;
    for (i =0, istack = 0; i < max_size; istack += 2) {
      try_lua_call(L);
      const int align_size_1024k = max(rand() % (512 * 1024), min_size);
      i += align_size_1024k;
      char *str_1024k = (char*)malloc(align_size_1024k);
      memset(str_1024k, 1, align_size_1024k);
      str_1024k[align_size_1024k - 1] = 0;
      lua_pushinteger(L, i);
      lua_pushstring(L, str_1024k);
      free(str_1024k);
      if (max_stack_size < istack) {
          lua_settop(L, 0);
          istack = 0;
      }
    }
    istack = 0;
    lua_settop(L, 0);
  }
}

int main(int argc, char **argv)
{
  lua_State *L = lua_open();
  luaL_openlibs(L);
  lua_newtable(L); 
  lua_setglobal(L, "blocks");

  int unit_mb = 1024 * 1024;
  int max_mb = 900;
  int i = 0;
  // size_t max_test_count = (size_t)(-1) - 1;
  // size_t test_count = 0;
  int start = 1;
  int step = 1;

/*    while (test_count < max_test_count) { */
    for (i = start; i <= max_mb; i += step) {
      printf("<----------align shared %d m----------->\r\n", i);
      align_block_test_shared(L, i * unit_mb);
      printf("<----------noalign shared %d m----------->\r\n", i);
      noalign_block_test_shared(L, i * unit_mb);
      printf("<----------align %d m----------->\r\n", i);
      align_block_test(L, i * unit_mb);
      printf("<----------noalign %d m----------->\r\n", i);
      noalign_block_test(L, i * unit_mb);
    } 
/*   }  */

  lua_close(L);
  return 0;
}