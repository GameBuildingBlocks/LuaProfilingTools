CUR_PWD=$(cd "$(dirname "$0")";pwd)
`cd ../../lua-packages/lua-5.3.4/src && make clean  && make all MYCFLAGS="-DLUA_USE_LINUX -m64" MYLIBS="-Wl,-E -ldl -lreadline -lhistory -lncurses"`

make X64