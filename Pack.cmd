if %1%a==a goto nada
    set targetDir=..\_Pack\%1%
	rmdir  /s /q %targetDir%
	md %targetDir%
	xcopy bin\debug\KBSaveReorganization.dll %targetDir%

:nada
pushd %targetDir%
"C:\Program Files\7-Zip\7z.exe" a KBSaveReorganization_%1%.zip  KBSaveReorganization.dll
move KBSaveReorganization_%1%.zip ..\..\_Pack\
popd 
