$workFolder = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), [Guid]::NewGuid().ToString())
$publishFolder = [System.IO.Path]::Combine($workFolder, 'publish')
$installerFolder = [System.IO.Path]::Combine($workFolder, 'Installer')
dotnet publish --output $publishFolder
7z.exe a -r -t7z -m0=lzma -mx=9 -mfb=64 -md=32m -ms=on "$installerFolder/KorabliChsMod.7z" "$publishFolder/*"
Copy-Item -Path 'C:\Program Files\7-Zip\7z.sfx' -Destination $installerFolder
Copy-Item -Path '.\.azurepipelines\publish\config.txt' -Destination $installerFolder
Set-Location $installerFolder
copy /b 7z.sfx + config.txt + KorabliChsMod.7z KorabliChsModSetup.exe
try {
  # Remove-Item -Path $workFolder-Force -Recurse
} catch {

}