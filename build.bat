cd modules/EasyAPI
CALL build.bat
cd ../../
copy /b ^
  modules\EasyAPI\EasyAPI.cs +^
  modules\EasyUtils\EasyUtils.cs +^
  modules\EasyLCD\EasyLCD.cs ^
  EasyAPI.lib.cs
echo.> EasyAPI.min.cs
echo.>> EasyAPI.min.cs
echo /*** Ignore minified library code below ***/ >> EasyAPI.min.cs
CSharpMinifier\CSharpMinify --locals --members --types --spaces --regions --comments --namespaces --to-string-methods --enum-to-int --line-length 100000 --skip-compile EasyAPI.lib.cs >> EasyAPI.min.cs
copy /b ^
  modules\BootstrapEasyAPI\BootstrapEasyAPI.cs +^
  EasyAPI.min.cs ^
  EasyAPI.cs
copy /b ^
  modules\BootstrapEasyAPI\BootstrapEasyAPI.cs +^
  EasyAPI.lib.cs ^
  EasyAPI.debug.cs  
del EasyAPI.lib.cs
del EasyAPI.min.cs
pause
