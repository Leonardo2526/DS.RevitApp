@Echo off
xcopy "DS.RVTtoNWC" "%AppData%\Autodesk\Revit\Addins\2020\DS.RVTtoNWC" /Q /Y /I /E
if %ERRORLEVEL% == 0 (
  echo Complete successfully!
) ELSE (
	echo An ERROR has occurred
)
pause

