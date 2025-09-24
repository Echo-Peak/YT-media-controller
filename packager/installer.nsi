!include "LogicLib.nsh"

Name "Youtube Media Controller installer"

InstallDir $PROGRAMFILES\YTMediaController
RequestExecutionLevel admin

OutFile "..\dist\YoutubeMediaControllerInstaller.exe"

!ifndef INSTALLER_ENV
  !define INSTALLER_ENV "Staging"
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
  File "..\backend\YTMediaControllerSrv\YTMediaControllerSrv\bin\${INSTALLER_ENV}\YTMediaControllerSrv.exe"
  File "..\backend\YTMediaControllerSrv\YTMediaControllerHost\bin\${INSTALLER_ENV}\YTMediaControllerHost.exe"
  File "..\backend\YTMediaControllerSrv\YTMediaControllerUpdaterSrv\bin\${INSTALLER_ENV}\YTMediaControllerUpdaterSrv.exe"
  File "..\dist\YoutubeMediaControllerUninstaller.exe"

  SetOutPath "$INSTDIR\bin"
  File /r "..\backend\externalBins\*.exe"

  SetOutPath "$INSTDIR\BrowserExtension"
  File /r "..\dist\browser-extension-unpacked\*"
  call InstallCoreService
  call InstallUpdaterService
SectionEnd