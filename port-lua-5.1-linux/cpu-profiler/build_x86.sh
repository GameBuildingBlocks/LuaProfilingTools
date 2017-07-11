CUR_PWD=$(cd "$(dirname "$0")";pwd)
`cd ../../lua-packages/lua-5.1.5/src && make clean  && make all MYCFLAGS="-DLUA_USE_LINUX -m32" MYLIBS="-Wl,-E -ldl -lreadline -lhistory -lncurses"`

make X32