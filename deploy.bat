
@echo off

rem H is the destination game folder
rem GAMEDIR is the name of the mod folder (usually the mod name)
rem GAMEDATA is the name of the local GameData
rem VERSIONFILE is the name of the version file, usually the same as GAMEDATA,
rem    but not always

set H=%KSPDIR%
set GAMEDATA="GameData"
rem set VERSIONFILE=%GAMEDIR%.version

copy /Y "%1%2" "%GAMEDATA%\%3\Plugins"
IF EXIST "%4Lang" xcopy /y /s /I "%4Lang" "%GAMEDATA%\%3\Lang"
copy /y  %3.version  %GAMEDATA%\%3
rem copy /y  README.md %GAMEDATA%\%3
rem copy /y  COPYING %GAMEDATA%\%3

xcopy /y /s /I %GAMEDATA%\%3 "%H%\GameData\%3"
