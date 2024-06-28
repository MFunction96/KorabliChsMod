param(
	[Parameter(Mandatory = $true)]
	[int] $Id,
	[Parameter(Mandatory = $true)]
	[string] $ZipPath,
	[Parameter(Mandatory = $false)]
	[string] $InstallPath = $PSScriptRoot
)

try {
	Stop-Process -Id $id -Force
	Wait-Process -Id $id -Timeout 60
	$files = Get-Content -Path "$InstallPath/FileList.txt"
	foreach ($file in $files) {
		if ($file.EndsWith(".log")) {
			continue
		}
		
		Remove-Item -Path "$InstallPath/$file" -Force
	}
	Expand-Archive -Path $ZipPath -DestinationPath $InstallPath -Force
	Start-Process -FilePath "$InstallPath/KorabliChsMod.exe" -WorkingDirectory $InstallPath
}
catch {
	$_.Exception | Out-File -FilePath "$env:AppData/KorabliChsMod/Update.err"
}