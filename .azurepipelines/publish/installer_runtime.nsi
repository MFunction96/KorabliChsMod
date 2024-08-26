!include "MUI2.nsh"
!include "LogicLib.nsh"
!include "WinMessages.nsh"

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

Name "考拉比汉社厂"
OutFile "KorabliChsModInstallerWithRuntime.exe"
InstallDir $AppData\KorabliChsMod
RequestExecutionLevel user
ShowInstDetails show

!define MUI_ABORTWARNING_TEXT "确定要取消安装吗?"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!define MUI_FINISHPAGE_RUN
!define MUI_FINISHPAGE_RUN_TEXT "运行 考拉比汉社厂"
!define MUI_FINISHPAGE_RUN_FUNCTION "LaunchLink"
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_LANGUAGE "SimpChinese"

Function LaunchLink
	ExecShell "" "$InstDir\KorabliChsMod.exe"
FunctionEnd

Function CheckForOldVersion
	ReadRegStr $0 HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\KorabliChsMod" "UninstallString"
	${If} $0 != ""
		# ExecWait "$0 /S"
	${EndIf}
FunctionEnd

Function .onInit
	Call CheckForOldVersion
FunctionEnd

Section "KorabliChsMod"
	SetOutPath "$InstDir"
	File /r "${SOURCE}\*.*"
	
	; 创建中文名快捷方式
	CreateShortcut "$Desktop\考拉比汉社厂.lnk" "$InstDir\KorabliChsMod.exe"

	; 生成卸载程序
	WriteUninstaller "$InstDir\uninstall.exe"

	; 在控制面板的“程序和功能”中注册卸载程序（使用英文名）
	WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\KorabliChsMod" "DisplayName" "考拉比汉社厂"
	WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\KorabliChsMod" "UninstallString" "$InstDir\uninstall.exe"
	WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\KorabliChsMod" "InstallLocation" "$InstDir"
	WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\KorabliChsMod" "DisplayIcon" "$InstDir\KorabliChsMod.exe"
	WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\KorabliChsMod" "Publisher" "MFunction"
	WriteRegStr HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\KorabliChsMod" "DisplayVersion" "${VERSION}"
SectionEnd

Section "Uninstall"
	; 删除中文名快捷方式
	Delete "$Desktop\考拉比汉社厂.lnk"
	; 删除安装程序文件
	Delete "$InstDir\KorabliChsMod.Core.deps.json"
	Delete "$InstDir\KorabliChsMod.Core.dll"
	Delete "$InstDir\KorabliChsMod.Core.pdb"
	Delete "$InstDir\KorabliChsMod.deps.json"
	Delete "$InstDir\KorabliChsMod.dll"
	Delete "$InstDir\KorabliChsMod.exe"
	Delete "$InstDir\KorabliChsMod.pdb"
	Delete "$InstDir\KorabliChsMod.runtimeconfig.json"
	Delete "$InstDir\Microsoft.Extensions.Configuration.Abstractions.dll"
	Delete "$InstDir\Microsoft.Extensions.Configuration.Binder.dll"
	Delete "$InstDir\Microsoft.Extensions.Configuration.CommandLine.dll"
	Delete "$InstDir\Microsoft.Extensions.Configuration.dll"
	Delete "$InstDir\Microsoft.Extensions.Configuration.EnvironmentVariables.dll"
	Delete "$InstDir\Microsoft.Extensions.Configuration.FileExtensions.dll"
	Delete "$InstDir\Microsoft.Extensions.Configuration.Json.dll"
	Delete "$InstDir\Microsoft.Extensions.Configuration.UserSecrets.dll"
	Delete "$InstDir\Microsoft.Extensions.DependencyInjection.Abstractions.dll"
	Delete "$InstDir\Microsoft.Extensions.DependencyInjection.dll"
	Delete "$InstDir\Microsoft.Extensions.Diagnostics.Abstractions.dll"
	Delete "$InstDir\Microsoft.Extensions.Diagnostics.dll"
	Delete "$InstDir\Microsoft.Extensions.FileProviders.Abstractions.dll"
	Delete "$InstDir\Microsoft.Extensions.FileProviders.Physical.dll"
	Delete "$InstDir\Microsoft.Extensions.FileSystemGlobbing.dll"
	Delete "$InstDir\Microsoft.Extensions.Hosting.Abstractions.dll"
	Delete "$InstDir\Microsoft.Extensions.Hosting.dll"
	Delete "$InstDir\Microsoft.Extensions.Logging.Abstractions.dll"
	Delete "$InstDir\Microsoft.Extensions.Logging.Configuration.dll"
	Delete "$InstDir\Microsoft.Extensions.Logging.Console.dll"
	Delete "$InstDir\Microsoft.Extensions.Logging.Debug.dll"
	Delete "$InstDir\Microsoft.Extensions.Logging.dll"
	Delete "$InstDir\Microsoft.Extensions.Logging.EventLog.dll"
	Delete "$InstDir\Microsoft.Extensions.Logging.EventSource.dll"
	Delete "$InstDir\Microsoft.Extensions.Options.ConfigurationExtensions.dll"
	Delete "$InstDir\Microsoft.Extensions.Options.dll"
	Delete "$InstDir\Microsoft.Extensions.Primitives.dll"
	Delete "$InstDir\Newtonsoft.Json.Bson.dll"
	Delete "$InstDir\Newtonsoft.Json.dll"
	Delete "$InstDir\pineapple.ico"
	Delete "$InstDir\Serilog.dll"
	Delete "$InstDir\Serilog.Extensions.Hosting.dll"
	Delete "$InstDir\Serilog.Extensions.Logging.dll"
	Delete "$InstDir\Serilog.Sinks.File.dll"
	Delete "$InstDir\Skidbladnir.Core.Extension.dll"
	Delete "$InstDir\Skidbladnir.IO.File.dll"
	; 删除卸载程序
	Delete "$InstDir\uninstall.exe"
	; 删除安装目录
	RMDir "$InstDir"

	; 删除注册表信息（使用英文名）
	DeleteRegKey HKCU "Software\Microsoft\Windows\CurrentVersion\Uninstall\KorabliChsMod"
SectionEnd

Section ".NET 8.0运行环境"
	DetailPrint "检查 .NET 8.0 运行环境是否已安装"
	nsExec::ExecToStack "dotnet --list-runtimes"
	Pop $0
	Push "Microsoft.WindowsDesktop.App 8"
	Call StrContains
	Pop $0
	StrCmp $0 "" notfound
		DetailPrint "已安装 .NET 8.0 运行环境"
		Goto done
notfound:
	DetailPrint "未安装 .NET 8.0 运行环境"
	DetailPrint "安装 .NET 8.0 运行环境"
	!insertmacro ShellExecWait "runas" '"$InstDir\windowsdesktop-runtime-win-x64.exe"' '/install /passive /norestart' "" ${SW_SHOW} $1
	Delete "$InstDir\windowsdesktop-runtime-win-x64.exe"
	DetailPrint "安装 .NET 8.0 运行环境 完成"
done:
	Delete "$InstDir\windowsdesktop-runtime-win-x64.exe"
SectionEnd