timeout /T 2
set TargetDir=%1%
echo %TargetDir%
cd %TargetDir%
echo %cd%
echo Release
del *.config
%USERPROFILE%\.nuget\packages\libz.bootstrap\1.2.0\tools\libz inject-dll --assembly=WallHaven.exe --include=*.dll --move=*.dll
move WallHaven.exe ..\