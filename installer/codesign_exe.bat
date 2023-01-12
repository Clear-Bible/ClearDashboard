@ECHO OFF

set "driveReactor=C"
set /p "driveReactor=Drive for dotNET_Reactor?: "
if /i "%driveReactor%" == "" goto :eof

set "driveInno=C"
set /p driveInno=Drive for Inno?: 
if /i "%driveInno%" == "" goto :eof

set CURRENTPATH=%cd%
set /p PASSWORD=<"..\code_signing_key\password.txt"
rem get the absolute path to the relative key file
CALL :NORMALIZEPATH "..\code_signing_key\ClearBible.pfx"


echo ========== PUBLISH 64-Bit Version of Dashboard ==============
cd ..\src\ClearDashboard.Wpf.Application
dotnet clean --configuration Release
dotnet publish -r win-x64 -c Release

pause

echo ========== BUILD 64-Bit Version of Paratext ==============
cd ..\ClearDashboard.WebApiParatextPlugin
dotnet clean --configuration Release
dotnet build  --configuration Release

pause

cd ..\..\installer


::===================Dashboard Obfuscation=====================
rem "%driveReactor%:\Program Files (x86)\Eziriz\.NET Reactor\dotNET_Reactor.Console.exe" -licensed -file "%CURRENTPATH%\..\src\ClearDashboard.Wpf.Application\bin\Release\net6.0-windows\win-x64\publish\ClearDashboard.Wpf.Application.dll"

echo code sign the WPF exe	
 ..\code_signing_key\signing_tool\signtool.exe ^
 	sign /v /f %RETVAL% ^
 	/p "%PASSWORD%" ^
 	/t http://timestamp.comodoca.com/authenticode ^
"%CURRENTPATH%\..\src\ClearDashboard.Wpf.Application\bin\Release\net7.0-windows\win-x64\publish\ClearDashboard.Wpf.Application.dll"

rem echo move new secure .exe file to the publish root
rem SET COPYCMD=/Y && move /Y "%CURRENTPATH%\..\src\ClearDashboard.Wpf.Application\bin\Release\net6.0-windows\win-x64\publish\ClearDashboard.Wpf.Application_Secure\ClearDashboard.Wpf.Application.dll" "%CURRENTPATH%\..\src\ClearDashboard.Wpf.Application\bin\Release\net6.0-windows\win-x64\publish\"

rem echo deleting secure folder and contents
rem del "%CURRENTPATH%\..\src\ClearDashboard.Wpf.Application\bin\Release\net6.0-windows\win-x64\publish\ClearDashboard.Wpf.Application_Secure\*" /S /Q
rem rmdir "%CURRENTPATH%\..\src\ClearDashboard.Wpf.Application\bin\Release\net6.0-windows\win-x64\publish\ClearDashboard.Wpf.Application_Secure"
pause

::===================Plugin Obfuscation=====================
echo code sign the plugin	
..\code_signing_key\signing_tool\signtool.exe ^
	sign /v /f %RETVAL% ^
	/p "%PASSWORD%" ^
	/t http://timestamp.comodoca.com/authenticode ^
	"%CURRENTPATH%\..\src\ClearDashboard.WebApiParatextPlugin\bin\Debug\net48\ClearDashboard.WebApiParatextPlugin_Secure\ClearDashboard.WebApiParatextPlugin.dll"

pause

::===================Dashboard=====================
eco run the Inno Setup Compliler on Dashboard
"%driveInno%:\Program Files (x86)\Inno Setup 6\ISCC.exe" "%CURRENTPATH%\DashboardInstaller.iss"

echo code sign the Dashboard installer
..\code_signing_key\signing_tool\signtool.exe ^
	sign /v /f %RETVAL% ^
	/p "%PASSWORD%" ^
	/t http://timestamp.comodoca.com/authenticode ^
	"%CURRENTPATH%\Output\ClearDashboard.exe"

::===================Plugin=====================
echo run the Inno Setup Compliler on plugin
"%driveInno%:\Program Files (x86)\Inno Setup 6\ISCC.exe" "%CURRENTPATH%\DashboardPluginInstaller.iss"

echo code sign the plugin installer
..\code_signing_key\signing_tool\signtool.exe ^
	sign /v /f %RETVAL% ^
	/p "%PASSWORD%" ^
	/t http://timestamp.comodoca.com/authenticode ^
	"%CURRENTPATH%\Output\ClearDashboardPlugin.exe"

pause

rem echo ========== BUILDING 64-Bit Version ==============
rem dotnet publish -r win-x64 -c Release /p:PublishSingleFile=true

:: ========== FUNCTIONS ==========
EXIT /B

:NORMALIZEPATH
  SET RETVAL=%~f1
  EXIT /B

