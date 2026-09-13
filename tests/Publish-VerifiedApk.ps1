$ErrorActionPreference = 'Stop'
$workspace = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$sourceApk = Join-Path $workspace 'Money/bin/Release/net10.0-android/publish/com.companyname.money-Signed.apk'
$destination = 'D:\Publicacao\Money.Mobile'
$toolsDirectory = 'D:\Program Files (x86)\Microsoft Visual Studio\Shared\Android\android-sdk\build-tools\36.0.0'

$metadata = & (Join-Path $toolsDirectory 'aapt.exe') dump badging $sourceApk
if ($LASTEXITCODE -ne 0 -or ($metadata -join "`n") -notmatch "versionCode='8' versionName='1.0.3'") {
    throw 'O APK não corresponde à versão 1.0.3 (8).'
}
& (Join-Path $toolsDirectory 'apksigner.bat') verify $sourceApk
if ($LASTEXITCODE -ne 0) { throw 'A assinatura do APK não é válida.' }

New-Item -ItemType Directory -Force -Path $destination | Out-Null
$publishedApk = Join-Path $destination 'Money.Mobile.apk'
Copy-Item -LiteralPath $sourceApk -Destination $publishedApk -Force
$sourceHash = (Get-FileHash -LiteralPath $sourceApk -Algorithm SHA256).Hash
$publishedHash = (Get-FileHash -LiteralPath $publishedApk -Algorithm SHA256).Hash
if ($sourceHash -ne $publishedHash) { throw 'O arquivo publicado difere do APK compilado.' }
Copy-Item -LiteralPath (Join-Path $workspace 'AUDITORIA-2026-09-04.md') -Destination $destination -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'audit.log') -Destination (Join-Path $destination 'testes-auditoria.log') -Force
Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'android-release.log') -Destination (Join-Path $destination 'compilacao-release.log') -Force
Set-Content -LiteralPath (Join-Path $destination 'SHA256.txt') -Value "$publishedHash  Money.Mobile.apk" -Encoding utf8
Write-Output "PUBLICADO: $publishedApk"
Write-Output "SHA256: $publishedHash"
Get-Item -LiteralPath $publishedApk | Select-Object FullName,Length,LastWriteTime
