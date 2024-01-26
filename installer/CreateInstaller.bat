@ECHO OFF

rem get the drive path for Inno Setup
set "driveInno=C"
set /p driveInno=Drive for Inno?: 
if /i "%driveInno%" == "" goto :eof

rem get the version number
set /p versionNumber=Dashboard Version Number?: 
if /i "%versionNumber%" == "" goto :eof

rem get the local directory path
set CURRENTPATH=%cd%

rem get the code signing key password from the file
set /p PASSWORD=<"..\code_signing_key\password.txt"
rem get the absolute path to the relative key file
CALL :NORMALIZEPATH "..\code_signing_key\ClearBible.pfx"


::========== BUILD the 64-Bit Version of ClearDashboard ==============
cd ..\src

@echo Deleting all BIN and OBJ folders...
for /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s/q "%%d"
@echo BIN and OBJ folders successfully deleted
echo
 
rem dotnet clean --configuration Release

dotnet build  --configuration Release

pause

::========== Copy and run SetVersion Program ===================
cd ..\installer

copy "%CURRENTPATH%\..\tools\SetVersionInfo\bin\Release\net7.0\SetVersionInfo.*" .
copy "%CURRENTPATH%\..\tools\SetVersionInfo\bin\Release\net7.0\System.CommandLine.*" .

SetVersionInfo.exe --input-version-number %versionNumber%

pause

::========== PUBLISH and SIGN ClearDashboard.Wpf.Application ==============
cd ..\src\ClearDashboard.Wpf.Application

dotnet publish ClearDashboard.Wpf.APplication.csproj -p:PublishProfile=FolderProfile

echo code sign the WPF exe
 ..\code_signing_key\signing_tool\signtool.exe ^
 	sign /v /f %RETVAL% ^
 	/p "%PASSWORD%" ^
 	/t http://timestamp.comodoca.com/authenticode ^
"%CURRENTPATH%\..\src\ClearDashboard.Wpf.Application\bin\Release\net7.0-windows\publish\win-x64\ClearDashboard.Wpf.Application.dll"


::========== PUBLISH and SIGN the PluginManager ==============
cd ../../tools/PluginManager
dotnet publish PluginManager.csproj -p:PublishProfile=FolderProfile

echo code sign the PluginManager.dll
 ..\code_signing_key\signing_tool\signtool.exe ^
 	sign /v /f %RETVAL% ^
 	/p "%PASSWORD%" ^
 	/t http://timestamp.comodoca.com/authenticode ^
"%CURRENTPATH%\..\tools\PluginManager\bin\Release\net7.0-windows\publish\win-x64\PluginManager.dll"


::========== PUBLISH ResetCurrentUser ==============
cd ../ResetCurrentUser
dotnet publish ResetCurrentUser.csproj -p:PublishProfile=FolderProfile



::========== PUBLISH AQuA Files <Check if there is any aqua stuff in the right places>==============
cd ..\..\installer
robocopy %CURRENTPATH%\..\src\ClearDashboard.Aqua.Module\bin\Release\net7.0-windows\ %CURRENTPATH%\..\src\ClearDashboard.Wpf.Application\bin\Release\net7.0-windows\publish\win-x64\Aqua ClearDashboard.Aqua.Module.* /IS /IT ;
robocopy %CURRENTPATH%\..\src\ClearDashboard.Aqua.Module\bin\Release\net7.0-windows\en %CURRENTPATH%\..\src\ClearDashboard.Wpf.Application\bin\Release\net7.0-windows\publish\win-x64\Aqua\en ClearDashboard.Aqua.Module.resources.dll /IS /IT ;
robocopy %CURRENTPATH%\..\src\ClearDashboard.Aqua.Module\bin\Release\net7.0-windows\Services %CURRENTPATH%\..\src\ClearDashboard.Wpf.Application\bin\Release\net7.0-windows\publish\win-x64\Aqua\Services vref.txt /IS /IT ;

rem echo CompressXMLs in Resources folder
rem ..\tools\CompressXML\CompressXML\bin\Release\net5.0\CompressTreeXML.exe


::===================COMPILE the ClearDashboard Installer=====================
echo run the Inno Setup Compliler on Dashboard
"%driveInno%:\Program Files (x86)\Inno Setup 6\ISCC.exe" "%CURRENTPATH%\DashboardInstaller.iss"

echo code sign the Dashboard installer
..\code_signing_key\signing_tool\signtool.exe ^
	sign /v /f %RETVAL% ^
	/p "%PASSWORD%" ^
	/t http://timestamp.comodoca.com/authenticode ^
	"%CURRENTPATH%\Output\ClearDashboard.exe"

pause


:: ========== FUNCTIONS ==========
EXIT /B

:NORMALIZEPATH
  SET RETVAL=%~f1
  EXIT /B

:GET_THIS_DIR
pushd %~dp0
set THIS_DIR=%CD%
popd
goto :EOF