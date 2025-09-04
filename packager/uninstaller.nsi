!include "LogicLib.nsh"

Name "YT Media Controller Uninstaller"
OutFile "..\dist\YoutubeMediaControllerUninstaller.exe"
RequestExecutionLevel admin
ShowInstDetails show

!define APP_KEY        "YTMediaController"
!define APP_NAME       "YT Media Controller"
!define SERVICE_CORE   "YTMediaControllerService"
!define SERVICE_UPD    "YTMediaControllerUpdaterService"
!define FW_RULE_NAME   "YTMediaController"

Function GetInstallLocation
  SetShellVarContext all
  StrCpy $INSTDIR ""

  SetRegView 64
  ReadRegStr $0 HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_KEY}" "InstallLocation"

  ${If} $0 == ""
    SetRegView 32
    ReadRegStr $0 HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_KEY}" "InstallLocation"
  ${EndIf}

  ${If} $0 == ""
    StrCpy $INSTDIR "$PROGRAMFILES\YTMediaController"
  ${Else}
    StrCpy $INSTDIR "$0"
  ${EndIf}

  DetailPrint "Resolved install location: $INSTDIR"
FunctionEnd

Function KillIfRunning
  Exch $0
  nsExec::ExecToLog 'taskkill /IM "$0" /F'
  Pop $1
FunctionEnd

Function StopAndDeleteService
  Exch $0
  DetailPrint "Stopping service: $0"
  nsExec::ExecToLog 'sc stop "$0"'
  Sleep 1500
  DetailPrint "Deleting service: $0"
  nsExec::ExecToLog 'sc delete "$0"'
  DeleteRegKey HKLM "SYSTEM\CurrentControlSet\Services\$0"
FunctionEnd

Function RemoveFirewallRule
  DetailPrint "Removing Windows Firewall rule: ${FW_RULE_NAME}"
  nsExec::ExecToLog 'cmd.exe /C netsh advfirewall firewall delete rule name="${FW_RULE_NAME}"'
  Pop $0
  ${If} $0 != 0
    DetailPrint "Firewall rule may already be gone (code $0)"
  ${EndIf}
FunctionEnd

Section "Uninstall ${APP_NAME}"

  Call GetInstallLocation

  DetailPrint "Stopping related processes"
  Push "YTMediaControllerHost.exe"
  Call KillIfRunning
  Push "YTMediaControllerSrv.exe"
  Call KillIfRunning
  Push "YTMediaControllerUpdaterSrv.exe"
  Call KillIfRunning

  Push "${SERVICE_UPD}"
  Call StopAndDeleteService
  Push "${SERVICE_CORE}"
  Call StopAndDeleteService

  Call RemoveFirewallRule

  SetRegView 64
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_KEY}"
  SetRegView 32
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\${APP_KEY}"

  DetailPrint "Removing files and directories"
  Delete /REBOOTOK "$INSTDIR\settings.json"
  Delete /REBOOTOK "$INSTDIR\YTMediaControllerSrv.exe"
  Delete /REBOOTOK "$INSTDIR\YTMediaControllerHost.exe"
  Delete /REBOOTOK "$INSTDIR\YTMediaControllerUpdaterSrv.exe"
  Delete /REBOOTOK "$INSTDIR\Uninstall.exe"

  RMDir /r "$INSTDIR\bin"
  RMDir /r "$INSTDIR\BrowserExtension"
  RMDir /r "$INSTDIR"

  DetailPrint "Uninstall complete"

SectionEnd