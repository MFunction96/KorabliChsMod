param(
	[Parameter(Mandatory = $true)]
	[string] $Source,
	[Parameter(Mandatory = $true)]
	[string] $Site,
	[Parameter(Mandatory = $true)]
	[string] $Username,
	[Parameter(Mandatory = $true)]
	[string] $Repository,
	[Parameter(Mandatory = $false)]
	[string] $Branch = 'main'
)

if ([string]::IsNullOrEmpty($env:TMP)) {
	$env:TMP = '/tmp'
}

$tmpPath = [System.IO.Path]::GetTempPath()
Set-Location $tmpPath
try {
	Remove-Item -Path $Repository -Force -Recurse
}
catch {
	<#Do this if a terminating exception happens#>
}

$uri = "git@$Site.com:$Username/$Repository.git"
git clone $Source --branch $Branch
Set-Location $Repository
git push $uri --follow-tags
git push $uri --tags

Set-Location $tmpPath
try {
	Remove-Item -Path $Repository -Force -Recurse
}
catch {
	<#Do this if a terminating exception happens#>
}