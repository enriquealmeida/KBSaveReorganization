@echo off
cd %~dp0
call setenv GX15
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe" KBSaveReorganization.sln
pause