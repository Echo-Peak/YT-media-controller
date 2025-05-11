
Name "Youtube Media Controller installer"

InstallDir $PROGRAMFILES\PCRemote
RequestExecutionLevel admin

OutFile "..\dist\YoutubeMediaControllerInstaller.exe"

Function InstallService 
  DetailPrint "Create YT Media Controller service"

  ExecWait 'sc create "YTMediaControllerService" binPath= "\"$PROGRAMFILES\YTMediaController\YTMediaControllerSrv.exe\"" start= auto DisplayName= "PCRemoteService"'
  WriteRegDWORD HKLM "SYSTEM\CurrentControlSet\Services\YTMediaControllerService" "DelayedAutostart" 0
  WriteRegDWORD HKLM "SYSTEM\CurrentControlSet\Services\YTMediaControllerService" "FailureActionsOnNonCrashFailures" 1
  WriteRegExpandStr HKLM "SYSTEM\CurrentControlSet\Services\YTMediaControllerService\Parameters" "AppDirectory" "$PROGRAMFILES\PCRemote"
  WriteRegStr HKLM "SYSTEM\CurrentControlSet\Services\YTMediaControllerService\Parameters\AppExit" "" "Restart"
FunctionEnd


Section "Info" Info

  SetOutPath "$INSTDIR"
  File "..\backend\settings.json"
  File "..\backend\YTMediaControllerSrv\YTMediaControllerSrv\bin\Release\YTMediaControllerSrv.exe"

  SetOutPath "$INSTDIR/BrowserExtension"
  File /r "..\extention\build"
  ; call InstallService
SectionEnd