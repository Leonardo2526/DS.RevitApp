@Echo off
xcopy "DS.RibbonTab" "%AppData%\Autodesk\Revit\Addins\2020\DS.RibbonTab" /Q /Y /I /E && xcopy "DS.RibbonTab.addin" "%AppData%\Autodesk\Revit\Addins\2020\" /Q /Y /I /E
if %ERRORLEVEL% == 0 (
  echo Complete successfully!
) ELSE (
	echo An ERROR has occurred
)
pause
