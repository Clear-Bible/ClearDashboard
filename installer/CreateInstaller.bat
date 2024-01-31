@ECHO OFF

::========== get the drive path for Inno Setup ==============
set "driveInno=C"
set /p driveInno=Drive for Inno?: 
if /i "%driveInno%" == "" goto :eof

::========== get the drive path for Visual Studio ==============
set "driveVisualStudio=C"
set /p driveVisualStudio=Drive for Visual Studio?: 
if /i "%driveVisualStudio%" == "" goto :eof

::========== get the version number ==============
set /p versionNumber=Dashboard Version Number?: 
if /i "%versionNumber%" == "" goto :eof

::========== get the local directory path ==============
set CURRENTPATH=%cd%

::========== get the code signing key password from the file ==============
set /p PASSWORD=<"..\code_signing_key\password.txt"

::========== get the absolute path to the relative key file ==============
CALL :NORMALIZEPATH "..\code_signing_key\ClearBible.pfx"


::========== BUILD the 64-Bit Version of ClearDashboard ==============
cd ..\src

::========== Deleting all BIN and OBJ folders... ==============
echo [101;93m  Deleting all BIN and OBJ folders...  [0m
@echo Deleting all BIN and OBJ folders...
for /d /r . %%d in (bin,obj) do @if exist "%%d" rd /s/q "%%d"
echo [101;93m  BIN and OBJ folders successfully deleted  [0m
@echo BIN and OBJ folders successfully deleted
echo

 
::========== Restore the Nuget Packages ==============
echo [101;93m  Restore the Nuget Packages  [0m
dotnet restore


::========== Build the Solution ==============
echo [101;93m  Build the Solution  [0m
"%driveVisualStudio%:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\amd64\MSBuild.exe"  %CURRENTPATH%\..\src\ClearDashboard.sln /t:Rebuild /v:diag /nologo /clp:NoSummary;Verbosity=minimal /bl /property:Configuration=Release


::========== Copy over Missing Runtimes that somehow MSBuild misses ==============
echo [101;93m  Copy over Missing Runtimes that somehow MSBuild misses  [0m
copy "%CURRENTPATH%\..\installer\Runtimes\*.*" "%CURRENTPATH%\..\src\ClearDashboard.Wpf.Application\bin\Release\net7.0-windows\runtimes\win-x64\native\"


::========== Copy and run SetVersion Program ===================
echo [101;93m  Copy and run SetVersion Program  [0m
cd ..\installer

copy "%CURRENTPATH%\..\tools\SetVersionInfo\bin\Release\net7.0\SetVersionInfo.*" .
copy "%CURRENTPATH%\..\tools\SetVersionInfo\bin\Release\net7.0\System.CommandLine.*" .

SetVersionInfo.exe --input-version-number %versionNumber%


::========== PUBLISH and SIGN ClearDashboard.Wpf.Application ==============
echo [101;93m  PUBLISH and SIGN ClearDashboard.Wpf.Application  [0m
cd ..\src\ClearDashboard.Wpf.Application

dotnet publish ClearDashboard.Wpf.Application.csproj -p:PublishProfile=FolderProfile


::========== Code Sign the WPF exe ==============
echo [101;93m  Code Sign the WPF exe  [0m
 ..\code_signing_key\signing_tool\signtool.exe ^
 	sign /v /f %RETVAL% ^
 	/p "%PASSWORD%" ^
 	/t http://timestamp.comodoca.com/authenticode ^
"%CURRENTPATH%\..\src\ClearDashboard.Wpf.Application\bin\Release\net7.0-windows\publish\win-x64\ClearDashboard.Wpf.Application.dll"


::========== PUBLISH and SIGN the PluginManager ==============
echo [101;93m  PUBLISH and SIGN the PluginManager  [0m
cd ../../tools/PluginManager
dotnet publish PluginManager.csproj -p:PublishProfile=FolderProfile


::========== Code Sign the PluginManager.dll ==============
echo [101;93m  Code Sign the PluginManager.dll  [0m
 ..\code_signing_key\signing_tool\signtool.exe ^
 	sign /v /f %RETVAL% ^
 	/p "%PASSWORD%" ^
 	/t http://timestamp.comodoca.com/authenticode ^
"%CURRENTPATH%\..\tools\PluginManager\bin\Release\net7.0-windows\publish\win-x64\PluginManager.dll"


::========== PUBLISH ResetCurrentUser ==============
echo [101;93m  PUBLISH ResetCurrentUser  [0m
cd ../ResetCurrentUser
dotnet publish ResetCurrentUser.csproj -p:PublishProfile=FolderProfile


::=================== PUBLISH AQuA Files <Check if there is any aqua stuff in the right places> =====================
echo [101;93m  PUBLISH AQuA Files <Check if there is any aqua stuff in the right places>  [0m
cd ..\..\installer
robocopy %CURRENTPATH%\..\src\ClearDashboard.Aqua.Module\bin\Release\net7.0-windows\ %CURRENTPATH%\..\src\ClearDashboard.Wpf.Application\bin\Release\net7.0-windows\publish\win-x64\Aqua ClearDashboard.Aqua.Module.* /IS /IT ;
robocopy %CURRENTPATH%\..\src\ClearDashboard.Aqua.Module\bin\Release\net7.0-windows\en %CURRENTPATH%\..\src\ClearDashboard.Wpf.Application\bin\Release\net7.0-windows\publish\win-x64\Aqua\en ClearDashboard.Aqua.Module.resources.dll /IS /IT ;
robocopy %CURRENTPATH%\..\src\ClearDashboard.Aqua.Module\bin\Release\net7.0-windows\Services %CURRENTPATH%\..\src\ClearDashboard.Wpf.Application\bin\Release\net7.0-windows\publish\win-x64\Aqua\Services vref.txt /IS /IT ;

rem echo CompressXMLs in Resources folder
rem ..\tools\CompressXML\CompressXML\bin\Release\net5.0\CompressTreeXML.exe


::=================== COMPILE the ClearDashboard Installer =====================
echo [101;93m  run the Inno Setup Compliler on Dashboard  [0m
"%driveInno%:\Program Files (x86)\Inno Setup 6\ISCC.exe" "%CURRENTPATH%\DashboardInstaller.iss"


::=================== Code Sign the Dashboard installer =====================
echo [101;93m  Code Sign the Dashboard installer  [0m
..\code_signing_key\signing_tool\signtool.exe ^
	sign /v /f %RETVAL% ^
	/p "%PASSWORD%" ^
	/t http://timestamp.comodoca.com/authenticode ^
	"%CURRENTPATH%\Output\ClearDashboard.exe"


::=================== Make a version specific copy as well as the generic ClearDashboard.exe  =====================
copy "%CURRENTPATH%\Output\ClearDashboard.exe" "%CURRENTPATH%\Output\ClearDashboard_%versionNumber%.exe"

echo [101;93m  INSTALLER CREATED  [0m
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