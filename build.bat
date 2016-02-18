cd modules/EasyAPI
CALL build.bat
cd ../../
copy /b ^
  modules\BootstrapEasyAPI\BootstrapEasyAPI.cs +^
  modules\EasyAPI\EasyAPI.cs +^
  modules\EasyUtils\EasyUtils.cs +^
  modules\EasyLCD\EasyLCD.cs ^
  EasyAPI.cs
pause