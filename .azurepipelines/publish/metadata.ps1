#!/usr/bin/pwsh

# Ensure use PowerShell 7
#Requires -Version 7

# GitHub API URL
$GitHubApiUrl = "https://api.github.com/repos/MFunction96/KorabliChsMod/releases"

# HTTP Headers
$Headers = @{
	"Accept"		= "application/vnd.github.v3+json"
	"User-Agent"	= "PowerShell"
	"X-GitHub-Api-Version" = "2022-11-28"
}

# Aquire GitHub Release metadata
try {
	$Releases = Invoke-RestMethod -Uri $GitHubApiUrl -Headers $Headers -Method Get
} catch {
	Write-Error "Unable to fetch GitHub Release data: $_"
	exit 1
}

# Modify URL to Cloudflare
$LatestRelease = $Releases | Where-Object { -not $_.prerelease } | Sort-Object published_at -Descending | Select-Object -First 1

foreach ($asset in $LatestRelease.assets) {
	$asset.browser_download_url = "https://warshipmod.mfbrain.xyz/korablichsmod/" + $asset.name
}

$FilteredReleases = @()
if ($LatestRelease) { $FilteredReleases += $LatestRelease }

$OptimizedMetadata = @($FilteredReleases | ForEach-Object {
	@{
		name		  = $_.name
		prerelease	= $_.prerelease
		published_at  = $_.published_at
		assets		= $_.assets | ForEach-Object {
			@{
				name				  = $_.name
				browser_download_url  = "https://warshipmod.mfbrain.xyz/korablichsmod/" + $_.name
			}
		}
	}
})

# Convert modified metadata to JSON
$ModifiedMetadata = ConvertTo-Json -Depth 10 -Compress $OptimizedMetadata

# Generate temporary file
$MetadataFile = New-TemporaryFile
$ModifiedMetadata | Set-Content -Path $MetadataFile.FullName -Encoding UTF8

# Upload to Cloudflare R2
try {
	aws s3api put-object --bucket warshipmod --key "korablichsmod/metadata.json" --body $MetadataFile
} catch {
	Write-Error $_
	exit 1
} finally {
	Remove-Item -Path $MetadataFile.FullName -Force
}

exit 0
