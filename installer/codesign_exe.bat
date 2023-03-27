@ECHO OFF

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

echo ========== BUILD 64-Bit Version of Plugin ==============
cd ..\ClearDashboard.WebApiParatextPlugin
dotnet clean --configuration Release
dotnet build  --configuration Release

echo ========== Check if there is any aqua stuff in the right places %CURRENTPATH%==============
pause

cd ..\..\installer

 robocopy %CURRENTPATH%\..\src\ClearDashboard.Aqua.Module\bin\Release\net7.0-windows\ %CURRENTPATH%\..\src\ClearDashboard.Wpf.Application\bin\Release\net7.0-windows\win-x64\publish ClearDashboard.Aqua.Module.* /IS /IT ;

 robocopy %CURRENTPATH%\..\src\ClearDashboard.Aqua.Module\bin\Release\net7.0-windows\en %CURRENTPATH%\..\src\ClearDashboard.Wpf.Application\bin\Release\net7.0-windows\win-x64\publish\en ClearDashboard.Aqua.Module.resources.dll /IS /IT ;

 robocopy %CURRENTPATH%\..\src\ClearDashboard.Aqua.Module\bin\Release\net7.0-windows\Services %CURRENTPATH%\..\src\ClearDashboard.Wpf.Application\bin\Release\net7.0-windows\win-x64\publish\Services vref.txt /IS /IT ;

echo code sign the WPF exe	
 ..\code_signing_key\signing_tool\signtool.exe ^
 	sign /v /f %RETVAL% ^
 	/p "%PASSWORD%" ^
 	/t http://timestamp.comodoca.com/authenticode ^
"%CURRENTPATH%\..\src\ClearDashboard.Wpf.Application\bin\Release\net7.0-windows\win-x64\publish\ClearDashboard.Wpf.Application.dll"

pause


::===================INNO Dashboard=====================
eco run the Inno Setup Compliler on Dashboard
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

