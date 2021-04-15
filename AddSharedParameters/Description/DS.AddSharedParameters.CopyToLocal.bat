@Echo off
xcopy "DS.AddSharedParameters" "%AppData%\Autodesk\Revit\Addins\2020\DS.AddSharedParameters" /Q /Y /I /E
if %ERRORLEVEL% == 0 (
  echo Complete successfully!
) ELSE (
	echo An ERROR has occurred
)
pause

