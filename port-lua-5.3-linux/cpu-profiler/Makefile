all: X32

X32:  
	gcc -I../../lua-profilers/cpu-profiler/ -I../../lua-packages/lua-5.3.4/src -std=gnu99 -Wextra -DLUA_COMPAT_5_2 -DLUAGETP -DLUA_USE_LINUX -g -shared -fPIC  -m32  -o cpuprofiler.so  ../../lua-profilers/cpu-profiler/cJSON.c  ../../lua-profilers/cpu-profiler/clocks.c ../../lua-profilers/cpu-profiler/core_profiler.c ../../lua-profilers/cpu-profiler/function_meter.c ../../lua-profilers/cpu-profiler/lua50_profiler.c ../../lua-profilers/cpu-profiler/output.c ../../lua-profilers/cpu-profiler/stack.c ../../lua-profilers/cpu-profiler/queue.c 
	gcc  -g  -I../../lua-profilers/cpu-profiler/  -I../../lua-packages/lua-5.3.4/src -m32 -DLUAGETP demo.c ../../lua-packages/lua-5.3.4/src/liblua.a cpuprofiler.so -lm  -ldl -lpthread -o demo

X64:  
	gcc -I../../lua-profilers/cpu-profiler/ -I../../lua-packages/lua-5.3.4/src -g -shared -fPIC  -m64 -DLUAGETP -o cpuprofiler.so  ../../lua-profilers/cpu-profiler/cJSON.c  ../../lua-profilers/cpu-profiler/clocks.c ../../lua-profilers/cpu-profiler/core_profiler.c ../../lua-profilers/cpu-profiler/function_meter.c ../../lua-profilers/cpu-profiler/lua50_profiler.c ../../lua-profilers/cpu-profiler/output.c ../../lua-profilers/cpu-profiler/stack.c ../../lua-profilers/cpu-profiler/queue.c 
	gcc  -g  -I../../lua-profilers/cpu-profiler/  -I../../lua-packages/lua-5.3.4/src -DLUAGETP -m64 demo.c  ../../lua-packages/lua-5.3.4/src/liblua.a cpuprofiler.so -lm  -ldl -lpthread -o demo
	



