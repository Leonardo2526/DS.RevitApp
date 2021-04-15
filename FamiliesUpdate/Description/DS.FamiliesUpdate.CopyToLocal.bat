@Echo off
xcopy "DS.FamiliesUpdate\*.*" "%AppData%\Autodesk\Revit\Addins\2020\DS.FamiliesUpdate" /Q /Y /I /E && xcopy "DS.FamiliesUpdate.addin" "%AppData%\Autodesk\Revit\Addins\2020\" /Q /Y /I /E
if %ERRORLEVEL% == 0 (
  echo Complete successfully!
) ELSE (
	echo An ERROR has occurred
)
pause

