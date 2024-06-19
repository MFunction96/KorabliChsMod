param(
	[Parameter(Mandatory = $true)]
	[string] $OutputFolder
)

$workFolder = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), [Guid]::NewGuid().ToString())
$publishFolder = [System.IO.Path]::Combine($workFolder, 'publish')
dotnet publish --output $publishFolder
try {
	Remove-Item -Path $OutputFolder -Force -Recurse
}
catch {

}
New-Item -Path $OutputFolder -ItemType Directory -Force
Compress-Archive -Path "$publishFolder/*" -DestinationPath "$outputFolder/KorabliChsMod.zip" -CompressionLevel Optimal -Force
$hash = Get-FileHash -Path "$outputFolder/KorabliChsMod.zip" -Algorithm SHA256
Copy-Item -Path "$publishFolder/*" -Destination $outputFolder -Recurse -Force
$hash.Hash | Out-File "$outputFolder/KorabliChsMod.zip.sig"
try {
  Remove-Item -Path $workFolder -Force -Recurse
} catch {

}