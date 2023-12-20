cd ..\src
powershell -Command "& {gci -inc bin,obj -rec | rm -rec -force}"
pause