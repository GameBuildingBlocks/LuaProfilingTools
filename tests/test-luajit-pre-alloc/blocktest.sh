#/usr/bin/sh

cd ../../lua-packages/LuaJIT-2.0.5
make
cd ../../tests/test-luajit-pre-alloc
cp -f ../../lua-packages/LuaJIT-2.0.5/src/luajit .
cp -f ../../lua-packages/LuaJIT-2.0.5/src/luajittest .
touch pre-alloc

echo "Lua Block Test with script"
./luajit blocktest.lua
echo "Lua Block Test with C API"
./luajittest