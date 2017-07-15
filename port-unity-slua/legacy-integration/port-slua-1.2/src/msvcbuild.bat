@rem Script to build Lua under "Visual Studio .NET Command Prompt".
@rem It creates lua53.dll, lua53.lib, lua53.exe, and luac53.exe in src.
@rem (contributed by David Manura and Mike Pall)

@setlocal
@set MYCOMPILE=cl /nologo /MT /O2 /W3 /c /D_CRT_SECURE_NO_DEPRECATE
@set MYLINK=link /nologo
@set MYMT=mt /nologo

%MYCOMPILE% /DLUA_BUILD_AS_DLL l*.c slua.c
del lua.obj luac.obj
%MYLINK% /DLL /out:lua53.dll *.obj
if exist lua53.dll.manifest^
  %MYMT% -manifest lua53.dll.manifest -outputresource:lua53.dll;2
  
%MYCOMPILE% /DLUA_BUILD_AS_DLL lua.c
%MYLINK% /out:lua53.exe lua.obj lua53.lib
if exist lua53.exe.manifest^
  %MYMT% -manifest lua.exe.manifest -outputresource:lua53.exe
  
%MYCOMPILE% l*.c
del lua.obj
%MYLINK% /out:luac53.exe *.obj
if exist luac53.exe.manifest^
  %MYMT% -manifest luac.exe.manifest -outputresource:luac53.exe

del *.obj *.manifest
