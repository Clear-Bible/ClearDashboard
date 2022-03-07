set ProjectName="ClearDashboardPlugin"
set TargetPath=.\bin\Debug\net48

if exist "%ParatextInstallDir%\plugins\%ProjectName%"\ (
  del /F /Q "%ParatextInstallDir%\plugins\%ProjectName%"\*.*
) else (
  mkdir "%ParatextInstallDir%\plugins\%ProjectName%"
)

@echo Copying files to %ParatextInstallDir%\plugins\%ProjectName%
xcopy "%TargetPath%\*.dll" "%ParatextInstallDir%\plugins\%ProjectName%" /y /i
xcopy "%TargetPath%\*.pdb" "%ParatextInstallDir%\plugins\%ProjectName%" /y /i
xcopy "%TargetPath%\Plugin.bmp" "%ParatextInstallDir%\plugins\%ProjectName%" /y /i

rename "%ParatextInstallDir%\plugins\%ProjectName%\ClearDashboard.ParatextPlugin.dll" "ClearDashboard.ParatextPlugin.ptxplg"