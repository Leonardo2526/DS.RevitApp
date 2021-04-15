@Echo off
xcopy "DS.AddProjectParameters" "%AppData%\Autodesk\Revit\Addins\2020\DS.AddProjectParameters" /Q /Y /I /E && xcopy "DS.AddProjectParameters.addin" "%AppData%\Autodesk\Revit\Addins\2020\" /Q /Y /I /E
if %ERRORLEVEL% == 0 (
  echo Complete successfully!
) ELSE (
	echo An ERROR has occurred
)
pause
