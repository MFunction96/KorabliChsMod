param(
	[Parameter(Mandatory = $true)]
	[string] $OutputFolder
)

$workFolder = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), [Guid]::NewGuid().ToString())
$publishFolder = [System.IO.Path]::Combine($workFolder, 'publish')
dotnet publish --output $publishFolder
Compress-Archive -Path "$publishFolder/*" -DestinationPath "$outputFolder/KorabliChsMod.zip" -CompressionLevel Optimal -Force
Copy-Item -Path "$publishFolder/*" -Destination $outputFolder -Recurse -Force
try {
  Remove-Item -Path $workFolder -Force -Recurse
} catch {

}