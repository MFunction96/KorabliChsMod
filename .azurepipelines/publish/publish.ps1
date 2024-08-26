param(
    [Parameter(Mandatory = $true)]
    [string] $OutputFolder,
    [Parameter(Mandatory = $false)]
    [string] $Proxy = ""
)

$workFolder = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), [Guid]::NewGuid().ToString())
$publishFolder = [System.IO.Path]::Combine($workFolder, 'KorabliChsMod')

function Invoke-Signing {
    param (
        [string] $FilePath,
        [string] $CertPath,
        [string] $CertPassword
    )

    & "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\SignTool.exe" sign /f $CertPath /p $CertPassword /fd SHA256 $FilePath
}

function Invoke-Hashing {
    param (
        [string] $FilePath
    )

    $hash = Get-FileHash -Path $FilePath -Algorithm SHA256
    $hash.Hash | Out-File -FilePath "$FilePath.sha256" -Encoding utf8 -Force
}

function Get-WebFile {
    param (
        [string] $Url,
        [string] $OutputPath,
        [string] $Proxy = ""
    )

    $curlArgs = @(
        "-sL",
        "-o", $OutputPath,
        "--connect-timeout", "60",
        "--max-time", "60",
        "--retry", "5",
        "--retry-delay", "5",
        "--retry-max-time", "60"
    )

    if ($Proxy) {
        $curlArgs += "--proxy", $Proxy
    }

    & curl @curlArgs $Url
}

dotnet publish --output $publishFolder

try {
    Remove-Item -Path $OutputFolder -Force -Recurse
} catch {}

New-Item -Path $OutputFolder -ItemType Directory -Force

# 签名生成的程序文件
Invoke-Signing "$publishFolder\KorabliChsMod.exe" "D:\Certificate\Xanadu_CodeSign_RSA_ICA1-PKCS8.pfx" $env:PFX_PASSWORD
Invoke-Hashing "$publishFolder\KorabliChsMod.exe"

# 生成安装包
& "makensis.exe" /X"SetCompressor /SOLID /FINAL lzma" /DSOURCE="$publishFolder" /DVERSION="$env:BIN_VER" /INPUTCHARSET UTF8 /OUTPUTCHARSET UTF8 ".\.azurepipelines\publish\installer.nsi"
Copy-Item -Path ".\.azurepipelines\publish\KorabliChsModInstaller.exe" -Destination $OutputFolder -Force

Invoke-Signing "$OutputFolder\KorabliChsModInstaller.exe" "D:\Certificate\Xanadu_CodeSign_RSA_ICA1-PKCS8.pfx" $env:PFX_PASSWORD
Invoke-Hashing "$OutputFolder\KorabliChsModInstaller.exe"

Get-WebFile "https://aka.ms/dotnet/8.0/windowsdesktop-runtime-win-x64.exe" "$publishFolder/windowsdesktop-runtime-win-x64.exe" $Proxy

# 生成带运行时的安装包
& "makensis.exe" /X"SetCompressor /SOLID /FINAL lzma" /DSOURCE="$publishFolder" /DVERSION="$env:BIN_VER" /INPUTCHARSET UTF8 /OUTPUTCHARSET UTF8 ".\.azurepipelines\publish\installer_runtime.nsi"
Copy-Item -Path ".\.azurepipelines\publish\KorabliChsModInstallerWithRuntime.exe" -Destination $OutputFolder -Force

Invoke-Signing "$OutputFolder\KorabliChsModInstallerWithRuntime.exe" "D:\Certificate\Xanadu_CodeSign_RSA_ICA1-PKCS8.pfx" $env:PFX_PASSWORD
Invoke-Hashing "$OutputFolder\KorabliChsModInstallerWithRuntime.exe"

try {
    Remove-Item -Path $workFolder -Force -Recurse
} catch {}
