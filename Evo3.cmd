@echo off
cd %~dp0
call setEnv EVO3
call "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe" KBSaveReorganization.sln