!include "MUI2.nsh"
!include "LogicLib.nsh"

Name "考拉比汉社厂"
OutFile "KorabliChsModInstaller.exe"
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
		ExecWait "$0 /S"
	${EndIf}
FunctionEnd

Function .onInit
	Call CheckForOldVersion
FunctionEnd

Section "KorabliChsMod"
	ExecWait "taskkill /F /IM /T KorabliChsMod.exe"
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
	Delete "$InstDir\KorabliChsMod.exe.sha256"
	Delete "$InstDir\KorabliChsMod.log"
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
