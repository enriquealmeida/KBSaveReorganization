ECHO -- BUILD DE  %1
call SetEnv.cmd %1
%NETFRAMEWORK_DIR%\msbuild SaveReorganization.sln  /t:Clean;SaveReorganization
call Pack.cmd %1