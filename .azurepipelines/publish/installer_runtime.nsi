!include "LogicLib.nsh"
!include "MUI2.nsh"
!include "WinMessages.nsh"
 
Name "考拉比汉社厂"
OutFile "KorabliChsModInstallerWithRuntime.exe"

!macro ShellExecWait verb app param workdir show exitoutvar ;only app and show must be != "", every thing else is optional
#define SEE_MASK_NOCLOSEPROCESS 0x40 
System::Store S
!if "${NSIS_PTR_SIZE}" > 4
!define /ReDef /math SYSSIZEOF_SHELLEXECUTEINFO 14 * ${NSIS_PTR_SIZE}
!else ifndef SYSSIZEOF_SHELLEXECUTEINFO
!define SYSSIZEOF_SHELLEXECUTEINFO 60
!endif
System::Call '*(&i${SYSSIZEOF_SHELLEXECUTEINFO})i.r0'
System::Call '*$0(i ${SYSSIZEOF_SHELLEXECUTEINFO},i 0x40,p $hwndparent,t "${verb}",t $\'${app}$\',t $\'${param}$\',t "${workdir}",i ${show})p.r0'
System::Call 'shell32::ShellExecuteEx(t)(pr0)i.r1 ?e' ; (t) to trigger A/W selection
${If} $1 <> 0
	System::Call '*$0(is,i,p,p,p,p,p,p,p,p,p,p,p,p,p.r1)' ;stack value not really used, just a fancy pop ;)
	System::Call 'kernel32::WaitForSingleObject(pr1,i-1)'
	System::Call 'kernel32::GetExitCodeProcess(pr1,*i.s)'
	System::Call 'kernel32::CloseHandle(pr1)'
${EndIf}
System::Free $0
!if "${exitoutvar}" == ""
	pop $0
!endif
System::Store L
!if "${exitoutvar}" != ""
	pop ${exitoutvar}
!endif
!macroend
 
RequestExecutionLevel User
ShowInstDetails Show
InstallDir $Appdata

!define MUI_ABORTWARNING_TEXT "Are you sure you wish to abort installation?"
 
Var CompletedText
CompletedText $CompletedText
 
!insertmacro MUI_PAGE_WELCOME
 
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_PAGE_CUSTOMFUNCTION_PRE InstFilesPre
!define MUI_PAGE_CUSTOMFUNCTION_SHOW InstFilesShow
!define MUI_PAGE_CUSTOMFUNCTION_LEAVE InstFilesLeave
Var MUI_HeaderText
Var MUI_HeaderSubText
!define MUI_INSTFILESPAGE_FINISHHEADER_TEXT "$MUI_HeaderText"
!define MUI_INSTFILESPAGE_FINISHHEADER_SUBTEXT "$MUI_HeaderSubText"
!insertmacro MUI_PAGE_INSTFILES
 
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_LANGUAGE "SimpChinese"

Var CurrentPage
Var UserIsMakingAbortDecision
Var UserAborted
Var SectionAborted
 
Function PauseIfUserIsMakingAbortDecision
  ${DoWhile} $UserIsMakingAbortDecision == "yes"
    Sleep 500
  ${Loop}
FunctionEnd
!define PauseIfUserIsMakingAbortDecision `Call PauseIfUserIsMakingAbortDecision`
 
!macro CheckUserAborted
  ${PauseIfUserIsMakingAbortDecision}
  ${If} $UserAborted == "yes"
    goto _userabort_aborted
  ${EndIf}
!macroend
!define CheckUserAborted `!insertmacro CheckUserAborted`
 
!macro EndUserAborted
  ${CheckUserAborted}
  goto _useraborted_end
  _userabort_aborted:
    ${If} $SectionAborted == ""
      StrCpy $SectionAborted "${__SECTION__}"
      DetailPrint "${__SECTION__} installation interrupted."
    ${ElseIf} $SectionAborted != "${__SECTION__}"
      DetailPrint "  ${__SECTION__} installation skipped."
    ${EndIf}
 
  _useraborted_end:
!macroend
!define EndUserAborted `!insertmacro EndUserAborted`
 
Function InstFilesPre
  StrCpy $CurrentPage "InstFiles"
  StrCpy $UserAborted "no"
FunctionEnd
 
Function InstFilesShow
  GetDlgItem $0 $HWNDPARENT 2
  EnableWindow $0 1
FunctionEnd

; Unicode True

; StrContains
; This function does a case sensitive searches for an occurrence of a substring in a string. 
; It returns the substring if it is found. 
; Otherwise it returns null(""). 
; Written by kenglish_hi
; Adapted from StrReplace written by dandaman32
 
 
Var STR_HAYSTACK
Var STR_NEEDLE
Var STR_CONTAINS_VAR_1
Var STR_CONTAINS_VAR_2
Var STR_CONTAINS_VAR_3
Var STR_CONTAINS_VAR_4
Var STR_RETURN_VAR
 
Function StrContains
  Exch $STR_NEEDLE
  Exch 1
  Exch $STR_HAYSTACK
  ; Uncomment to debug
  ;MessageBox MB_OK 'STR_NEEDLE = $STR_NEEDLE STR_HAYSTACK = $STR_HAYSTACK '
    StrCpy $STR_RETURN_VAR ""
    StrCpy $STR_CONTAINS_VAR_1 -1
    StrLen $STR_CONTAINS_VAR_2 $STR_NEEDLE
    StrLen $STR_CONTAINS_VAR_4 $STR_HAYSTACK
    loop:
      IntOp $STR_CONTAINS_VAR_1 $STR_CONTAINS_VAR_1 + 1
      StrCpy $STR_CONTAINS_VAR_3 $STR_HAYSTACK $STR_CONTAINS_VAR_2 $STR_CONTAINS_VAR_1
      StrCmp $STR_CONTAINS_VAR_3 $STR_NEEDLE found
      StrCmp $STR_CONTAINS_VAR_1 $STR_CONTAINS_VAR_4 done
      Goto loop
    found:
      StrCpy $STR_RETURN_VAR $STR_NEEDLE
      Goto done
    done:
   Pop $STR_NEEDLE ;Prevent "invalid opcode" errors and keep the
   Exch $STR_RETURN_VAR  
FunctionEnd
 
!macro _StrContainsConstructor OUT NEEDLE HAYSTACK
  Push `${HAYSTACK}`
  Push `${NEEDLE}`
  Call StrContains
  Pop `${OUT}`
!macroend
 
!define StrContains '!insertmacro "_StrContainsConstructor"'

Section "考拉比汉社厂"

	DetailPrint "安装 ${__SECTION__}"
	${CheckUserAborted}
	SetOutPath "$InstDir"
	File /r ${SOURCE}
	DetailPrint "创建快捷方式"
	CreateShortcut "$desktop\考拉比汉社厂.lnk" "$instdir\KorabliChsMod\KorabliChsMod.exe"
	DetailPrint "安装 ${__SECTION__} 完成"
	${EndUserAborted}

SectionEnd

Section ".NET 8.0运行环境"
	DetailPrint "检查 ${__SECTION__} 是否已安装"
	nsExec::ExecToStack "dotnet --list-runtimes"
	Pop $0
	Push "Microsoft.WindowsDesktop.App 8"
	Call StrContains
	Pop $0
	StrCmp $0 "" notfound
		DetailPrint "已安装 ${__SECTION__}"
		Goto done
notfound:
		DetailPrint "未安装 ${__SECTION__}"
		DetailPrint "安装 ${__SECTION__}"
    ${CheckUserAborted}
    !insertmacro ShellExecWait "runas" '"$instdir\KorabliChsMod\windowsdesktop-runtime-win-x64.exe"' '/install /passive /norestart' "" ${SW_SHOW} $1
		DetailPrint "安装 ${__SECTION__} 完成"
		${EndUserAborted}
done:

SectionEnd

Section -"Post"
  ${If} $UserAborted == "yes"
    StrCpy $CompletedText "Installation aborted."
    StrCpy $MUI_HeaderText "Installation Failed"
    StrCpy $MUI_HeaderSubText "Setup was aborted."
  ${Else}
    StrCpy $CompletedText "Completed"
    StrCpy $MUI_HeaderText "Installation Complete"
    StrCpy $MUI_HeaderSubText "Setup was completed successfully."
  ${EndIf}
SectionEnd
 
Function InstFilesLeave
  StrCpy $CurrentPage ""
FunctionEnd
 
!define MUI_CUSTOMFUNCTION_ABORT onUserAbort
Function onUserAbort
  StrCpy $UserIsMakingAbortDecision "yes"
  ${If} ${Cmd} `MessageBox MB_YESNO|MB_DEFBUTTON2 "${MUI_ABORTWARNING_TEXT}" IDYES`
    ${If} $CurrentPage == "InstFiles"
      StrCpy $UserAborted "yes"
      MessageBox MB_OK "User aborted during InstFiles."
      StrCpy $UserIsMakingAbortDecision "no"
      Abort
    ${Else}
      MessageBox MB_OK "User aborted elsewhere"
      StrCpy $UserIsMakingAbortDecision "no"
    ${EndIf}
  ${Else}
    StrCpy $UserIsMakingAbortDecision "no"
    Abort
  ${EndIf}
FunctionEnd