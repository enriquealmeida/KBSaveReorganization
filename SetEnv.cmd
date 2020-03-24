CALL :CASE_%1
IF ERRORLEVEL 1 CALL :DEFAULT_CASE 

EXIT /B

:CASE_EVO3
SET GX_PROGRAM_DIR=C:\Program Files (x86)\Artech\GeneXus\GeneXusXEv3
SET GX_SDK_DIR=C:\Program Files (x86)\Artech\GeneXus\GeneXusXEv3PlatformSDK
SET NETFRAMEWORK_DIR="C:\Program Files (x86)\MSBuild\14.0\bin\amd64"
SET TargetFrameworkVersion=v3.5
REM ctt.exe source:KBSaveReorganization.csproj.user transform:TransformacionEvo3.xml destination:SaveReorganization.csproj.user i
GOTO END_CASE

:CASE_GX15
SET GX_PROGRAM_DIR=C:\Program Files (x86)\GeneXus\GeneXus15
SET GX_SDK_DIR=C:\Program Files (x86)\GeneXus\GeneXus15PlatformSDK
SET NETFRAMEWORK_DIR="C:\Program Files (x86)\MSBuild\14.0\bin\amd64"
SET TargetFrameworkVersion=v4.6
ctt.exe source:LBSaveReorganization.csproj.user transform:TransformacionGX15.xml destination:SaveReorganization.csproj.user i
GOTO END_CASE

:CASE_GX16
SET GX_PROGRAM_DIR=d:\GeneXus\GeneXus16
SET GX_SDK_DIR=d:\GeneXus\GeneXus16SDK
SET NETFRAMEWORK_DIR="C:\Program Files (x86)\MSBuild\14.0\bin\amd64"
SET TargetFrameworkVersion=v4.7.1
ctt.exe source:KBSaveReorganization.csproj.user transform:TransformacionGX16.xml destination:SaveReorganization.csproj.user i
GOTO END_CASE

:DEFAULT_CASE
ECHO SetEnv parametro invalido %1 : Debe ser EVO3, GX15 O GX16
PAUSE

:END_CASE
 GOTO :EOF 

