param(
	[Parameter(Mandatory = $true)]
	[string] $OutputFolder,
	[Parameter(Mandatory = $false)]
	[string] $Proxy = ""
)

$workFolder = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), [Guid]::NewGuid().ToString())
$publishFolder = [System.IO.Path]::Combine($workFolder, 'KorabliChsMod')
dotnet publish --output $publishFolder
try {
	Remove-Item -Path $OutputFolder -Force -Recurse
}
catch {

}
New-Item -Path $OutputFolder -ItemType Directory -Force

makensis.exe /X"SetCompressor /SOLID /FINAL lzma" /DSOURCE="$publishFolder" /DVERSION="$env:BIN_VER" /INPUTCHARSET UTF8 /OUTPUTCHARSET UTF8 .\.azurepipelines\publish\installer.nsi
Copy-Item -Path "./.azurepipelines/publish/KorabliChsModInstaller.exe" -Destination $OutputFolder -Force
Write-Output $env:GPG_PASSPHRASE | gpg --local-user 0x889174C5 --armor --batch --yes --output "$outputFolder/KorabliChsModInstaller.exe.sig" --passphrase-fd 0 --pinentry-mode loopback --detach-sign "$outputFolder/KorabliChsModInstaller.exe"
$hash = Get-FileHash -Path "$outputFolder/KorabliChsModInstaller.exe" -Algorithm SHA256
Write-Output $hash.Hash | Out-File -FilePath "$outputFolder/KorabliChsModInstaller.exe.sha256" -Encoding utf8 -Force

if ([string]::IsNullOrEmpty($Proxy)) {
	curl -sL -o $publishFolder/windowsdesktop-runtime-win-x64.zip https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.zip -A "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36 Edge/16.16299" --connect-timeout 60 --max-time 60 --retry 5 --retry-delay 5 --retry-max-time 60
} else {
	curl -sL -o $publishFolder/windowsdesktop-runtime-win-x64.zip https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.zip -A "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36 Edge/16.16299" --connect-timeout 60 --max-time 60 --retry 5 --retry-delay 5 --retry-max-time 60 --proxy $Proxy
}

makensis.exe /X"SetCompressor /SOLID /FINAL lzma" /DSOURCE="$publishFolder" /DVERSION="$env:BIN_VER" /INPUTCHARSET UTF8 /OUTPUTCHARSET UTF8 .\.azurepipelines\publish\installer_runtime.nsi
Copy-Item -Path "./.azurepipelines/publish/KorabliChsModInstallerWithRuntime.exe" -Destination $OutputFolder -Force
Write-Output $env:GPG_PASSPHRASE | gpg --local-user 0x889174C5 --armor --batch --yes --output "$outputFolder/KorabliChsModInstallerWithRuntime.exe.sig" --passphrase-fd 0 --pinentry-mode loopback --detach-sign "$outputFolder/KorabliChsModInstallerWithRuntime.exe"
$hash = Get-FileHash -Path "$outputFolder/KorabliChsModInstallerWithRuntime.exe" -Algorithm SHA256
Write-Output $hash.Hash | Out-File -FilePath "$outputFolder/KorabliChsModInstallerWithRuntime.exe.sha256" -Encoding utf8 -Force
try {
  Remove-Item -Path $workFolder -Force -Recurse
} catch {

}