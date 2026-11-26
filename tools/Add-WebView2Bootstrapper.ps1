param([Parameter(Mandatory=$true)][string]$PublishDir)
$ErrorActionPreference = 'Stop'
$target = Join-Path (Resolve-Path $PublishDir).Path 'MicrosoftEdgeWebview2Setup.exe'
$url = 'https://go.microsoft.com/fwlink/p/?LinkId=2124703'
Invoke-WebRequest -UseBasicParsing -Uri $url -OutFile $target
$signature = Get-AuthenticodeSignature -FilePath $target
if ($signature.Status -ne 'Valid' -or $signature.SignerCertificate.Subject -notmatch 'CN=Microsoft Corporation(?:,|$)') {
    Remove-Item -LiteralPath $target -Force
    throw 'WebView2 bootstrapper did not pass Microsoft Authenticode verification.'
}
Write-Host 'Verified Microsoft WebView2 Evergreen bootstrapper included.'
