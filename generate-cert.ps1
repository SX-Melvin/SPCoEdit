$certFolder = Join-Path $env:USERPROFILE "SPCoEditCertificates"
New-Item -ItemType Directory -Force -Path $certFolder | Out-Null

$cert = New-SelfSignedCertificate `
    -Subject "CN=SPCoEdit" `
    -CertStoreLocation "Cert:\CurrentUser\My" `
    -KeyExportPolicy Exportable `
    -KeySpec Signature `
    -KeyAlgorithm RSA `
    -KeyLength 2048 `
    -HashAlgorithm SHA256 `
    -NotAfter (Get-Date).AddYears(1)

# Public certificate: upload this to Entra ID
Export-Certificate `
    -Cert $cert `
    -FilePath (Join-Path $certFolder "spcoedit.cer")

# Private certificate: deploy this to the application host
$pfxPassword = Read-Host "Choose a PFX password" -AsSecureString

Export-PfxCertificate `
    -Cert $cert `
    -FilePath (Join-Path $certFolder "spcoedit.pfx") `
    -Password $pfxPassword