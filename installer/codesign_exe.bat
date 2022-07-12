cd src\ClearRun.Console
set CURRENTPATH=%cd%
set /p PASSWORD=<"..\code_signing_key\password.txt"
rem get the absolute path to the relative key file
CALL :NORMALIZEPATH "..\code_signing_key\ClearBible.pfx"

::===================Dashboard Obfuscation=====================
"C:\Program Files (x86)\Eziriz\.NET Reactor\dotNET_Reactor.Console.exe" -licensed -file "%CURRENTPATH%\..\src\ClearDashboard.WPF\bin\Debug\net6.0-windows\ClearDashboard.Wpf.dll"
del "%CURRENTPATH%\..\src\ClearDashboard.WPF\bin\Debug\net6.0-windows\ClearDashboard.Wpf.dll"

rem code sign the installer	
..\code_signing_key\signing_tool\signtool.exe ^
	sign /v /f %RETVAL% ^
	/p "%PASSWORD%" ^
	/t http://timestamp.comodoca.com/authenticode ^
	"%CURRENTPATH%\..\src\ClearDashboard.WPF\bin\Debug\net6.0-windows\ClearDashboard.Wpf_Secure\ClearDashboard.Wpf.dll"

::===================Plugin Obfuscation=====================
"C:\Program Files (x86)\Eziriz\.NET Reactor\dotNET_Reactor.Console.exe" -file "%CURRENTPATH%\..\src\ClearDashboard.WebApiParatextPlugin\bin\Debug\net48\ClearDashboard.WebApiParatextPlugin.dll"
del "%CURRENTPATH%\..\src\ClearDashboard.WebApiParatextPlugin\bin\Debug\net48\ClearDashboard.WebApiParatextPlugin.dll"

rem code sign the installer	
..\code_signing_key\signing_tool\signtool.exe ^
	sign /v /f %RETVAL% ^
	/p "%PASSWORD%" ^
	/t http://timestamp.comodoca.com/authenticode ^
	"%CURRENTPATH%\..\src\ClearDashboard.WebApiParatextPlugin\bin\Debug\net48\ClearDashboard.WebApiParatextPlugin_Secure\ClearDashboard.WebApiParatextPlugin.dll"

::===================Dashboard=====================
rem run the Inno Setup Compliler
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "%CURRENTPATH%\DashboardInstaller.iss"

rem code sign the installer	
..\code_signing_key\signing_tool\signtool.exe ^
	sign /v /f %RETVAL% ^
	/p "%PASSWORD%" ^
	/t http://timestamp.comodoca.com/authenticode ^
	"%CURRENTPATH%\Output\ClearDashboard.exe"

::===================Plugin=====================
rem run the Inno Setup Compliler
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "%CURRENTPATH%\DashboardPluginInstaller.iss"

rem code sign the installer	
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

