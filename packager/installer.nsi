!include "LogicLib.nsh"
!define APP_NAME "Youtube Media Controller installer"
Name "Youtube Media Controller installer"

!ifndef APP_VERSION_NUM
  !define APP_VERSION_NUM "1.0.0.0"
!endif

!ifndef APP_VERSION_STR
  !define APP_VERSION_STR "1.0.0-alpha.0"
!endif

VIProductVersion "${APP_VERSION_NUM}"

VIAddVersionKey /LANG=1033 "ProductName"       "${APP_NAME}"
VIAddVersionKey /LANG=1033 "CompanyName"       "${APP_COMPANY}"
VIAddVersionKey /LANG=1033 "FileDescription"   "${APP_NAME} Installer"
VIAddVersionKey /LANG=1033 "OriginalFilename"  "YoutubeMediaControllerInstaller.exe"
VIAddVersionKey /LANG=1033 "ProductVersion"    "${APP_VERSION_STR}"
VIAddVersionKey /LANG=1033 "FileVersion"       "${APP_VERSION_STR}"

InstallDir $PROGRAMFILES\YTMediaController
RequestExecutionLevel admin

OutFile "..\dist\YoutubeMediaControllerInstaller.exe"

!ifndef INSTALLER_BUILD_ENV
  !define INSTALLER_BUILD_ENV "Alpha"
!endif

Function InstallCoreService 
  DetailPrint "Create YT Media Controller service"

  ExecWait 'sc create "YTMediaControllerService" binPath= "\"$PROGRAMFILES\YTMediaController\YTMediaControllerSrv.exe\"" start= auto DisplayName= "YT Media Controller Service"'
  WriteRegDWORD HKLM "SYSTEM\CurrentControlSet\Services\YTMediaControllerService" "DelayedAutostart" 0
  WriteRegDWORD HKLM "SYSTEM\CurrentControlSet\Services\YTMediaControllerService" "FailureActionsOnNonCrashFailures" 1
  WriteRegExpandStr HKLM "SYSTEM\CurrentControlSet\Services\YTMediaControllerService\Parameters" "AppDirectory" "$PROGRAMFILES\YTMediaController"
  WriteRegStr HKLM "SYSTEM\CurrentControlSet\Services\YTMediaControllerService\Parameters\AppExit" "" "Restart"

  ExecWait 'sc start "YTMediaControllerService"'
  DetailPrint "YT Media Controller service created and started"
FunctionEnd

Function InstallUpdaterService 
  DetailPrint "Create YT Media Controller updater service"

  ExecWait 'sc create "YTMediaControllerUpdaterService" binPath= "\"$PROGRAMFILES\YTMediaController\YTMediaControllerUpdaterSrv.exe\"" start= auto DisplayName= "YT Media Controller Updater Service"'
  WriteRegDWORD HKLM "SYSTEM\CurrentControlSet\Services\YTMediaControllerUpdaterService" "DelayedAutostart" 0
  WriteRegDWORD HKLM "SYSTEM\CurrentControlSet\Services\YTMediaControllerUpdaterService" "FailureActionsOnNonCrashFailures" 1
  WriteRegExpandStr HKLM "SYSTEM\CurrentControlSet\Services\YTMediaControllerUpdaterService\Parameters" "AppDirectory" "$PROGRAMFILES\YTMediaController"
  WriteRegStr HKLM "SYSTEM\CurrentControlSet\Services\YTMediaControllerUpdaterService\Parameters\AppExit" "" "Restart"

  ExecWait 'sc start "YTMediaControllerUpdaterService"'
  DetailPrint "YT Media Controller updater service created and started"
FunctionEnd

Section "Info" Info

  SetOutPath "$INSTDIR"
  File "..\backend\YTMediaControllerSrv\YTMediaControllerSrv\bin\${INSTALLER_BUILD_ENV}\net9.0-windows\win-x86\publish\YTMediaControllerSrv.exe"
  File "..\backend\YTMediaControllerSrv\YTMediaControllerUpdaterSrv\bin\${INSTALLER_BUILD_ENV}\net9.0-windows\win-x86\publish\YTMediaControllerUpdaterSrv.exe"
  File "..\dist\YoutubeMediaControllerUninstaller.exe"
  File "..\frontend\src-tauri\target\release\ytmc_frontend.exe"

  SetOutPath "$INSTDIR\bin"
  File /r "..\backend\externalBins\*.exe"

  call InstallCoreService
  call InstallUpdaterService
SectionEnd