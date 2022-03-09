rem the following codes will keep a Window's double clicked icon open at the end of execution
if "%parent%"=="" set parent=%~0
if "%console_mode%"=="" (set console_mode=1& for %%x in (%cmdcmdline%) do if /i "%%~x"=="/c" set console_mode=0)

IF EXIST "C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\amd64\MSBuild.exe" (
  "C:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\amd64\MSBuild.exe" -t:checkuid .\ClearDashboard.Wpf\ClearDashboard.Wpf.csproj
) ELSE (
  "D:\Program Files\Microsoft Visual Studio\2022\Community\Msbuild\Current\Bin\amd64\MSBuild.exe" -t:checkuid .\ClearDashboard.Wpf\ClearDashboard.Wpf.csproj
)


rem the following codes will keep a Window's double clicked icon open at the end of execution
if "%parent%"=="%~0" ( if "%console_mode%"=="0" pause )