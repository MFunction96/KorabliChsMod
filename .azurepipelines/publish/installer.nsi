!include "LogicLib.nsh"
!include "MUI2.nsh"

Name "考拉比汉社厂"
OutFile "KorabliChsModInstaller.exe"

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