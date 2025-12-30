# Create self-signed TLS certificates for local development
# Uses PowerShell native cmdlets (no OpenSSL required)

param(
    [string]$Namespace = "theshop",
    [string]$WebAppHost = "theshop.local",
    [string]$ApiHost = "api.theshop.local"
)

Write-Host "Creating self-signed TLS certificates for local development..." -ForegroundColor Cyan

# Create temp directory
$tempDir = "$env:TEMP\theshop-certs"
if (Test-Path $tempDir) { Remove-Item -Recurse -Force $tempDir }
New-Item -ItemType Directory -Force -Path $tempDir | Out-Null

# Function to create certificate and export to PEM format
function New-TlsCertificate {
    param(
        [string]$DnsName,
        [string]$CertPath,
        [string]$KeyPath
    )
    
    Write-Host "Generating certificate for $DnsName..." -ForegroundColor Yellow
    
    # Create self-signed certificate
    $cert = New-SelfSignedCertificate `
        -DnsName $DnsName, "localhost" `
        -CertStoreLocation "Cert:\CurrentUser\My" `
        -NotAfter (Get-Date).AddYears(1) `
        -KeyAlgorithm RSA `
        -KeyLength 2048 `
        -HashAlgorithm SHA256 `
        -KeyExportPolicy Exportable `
        -FriendlyName "TheShop Dev Certificate - $DnsName"
    
    # Export certificate (CER format, then convert to PEM)
    $certBytes = $cert.Export([System.Security.Cryptography.X509Certificates.X509ContentType]::Cert)
    $certPem = "-----BEGIN CERTIFICATE-----`n"
    $certPem += [Convert]::ToBase64String($certBytes, [System.Base64FormattingOptions]::InsertLineBreaks)
    $certPem += "`n-----END CERTIFICATE-----"
    Set-Content -Path $CertPath -Value $certPem -NoNewline
    
    # Export private key
    $keyBytes = $cert.PrivateKey.ExportRSAPrivateKey()
    $keyPem = "-----BEGIN RSA PRIVATE KEY-----`n"
    $keyPem += [Convert]::ToBase64String($keyBytes, [System.Base64FormattingOptions]::InsertLineBreaks)
    $keyPem += "`n-----END RSA PRIVATE KEY-----"
    Set-Content -Path $KeyPath -Value $keyPem -NoNewline
    
    # Remove from certificate store (we only need the files)
    Remove-Item "Cert:\CurrentUser\My\$($cert.Thumbprint)" -Force
    
    Write-Host "  ✓ Certificate created" -ForegroundColor Green
}

# Generate certificates
New-TlsCertificate -DnsName $WebAppHost -CertPath "$tempDir\webapp.crt" -KeyPath "$tempDir\webapp.key"
New-TlsCertificate -DnsName $ApiHost -CertPath "$tempDir\api.crt" -KeyPath "$tempDir\api.key"

# Create namespace if not exists
kubectl create namespace $Namespace --dry-run=client -o yaml | kubectl apply -f - 2>$null

# Create Kubernetes secrets
Write-Host "Creating Kubernetes TLS secrets in namespace '$Namespace'..." -ForegroundColor Yellow

kubectl create secret tls theshop-webapp-tls `
    --cert="$tempDir\webapp.crt" `
    --key="$tempDir\webapp.key" `
    -n $Namespace --dry-run=client -o yaml | kubectl apply -f -

kubectl create secret tls theshop-api-tls `
    --cert="$tempDir\api.crt" `
    --key="$tempDir\api.key" `
    -n $Namespace --dry-run=client -o yaml | kubectl apply -f -

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "TLS Secrets Created!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. Add to hosts file (Run as Administrator):" -ForegroundColor White
Write-Host "   notepad C:\Windows\System32\drivers\etc\hosts" -ForegroundColor Gray
Write-Host ""
Write-Host "   Add these lines:" -ForegroundColor White
Write-Host "   127.0.0.1 $WebAppHost" -ForegroundColor Cyan
Write-Host "   127.0.0.1 $ApiHost" -ForegroundColor Cyan
Write-Host ""
Write-Host "2. Enable ingress in values.yaml files:" -ForegroundColor White
Write-Host "   deploy/webapp/values.yaml -> ingress.enabled: true" -ForegroundColor Gray
Write-Host "   deploy/app/values.yaml -> ingress.enabled: true" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Commit and push" -ForegroundColor White
Write-Host ""
Write-Host "4. Access via HTTPS:" -ForegroundColor White
Write-Host "   https://$WebAppHost" -ForegroundColor Cyan
Write-Host "   https://$ApiHost" -ForegroundColor Cyan
Write-Host ""
Write-Host "Note: Browser will show certificate warning (self-signed)." -ForegroundColor Yellow
Write-Host "      Click 'Advanced' -> 'Proceed to site'" -ForegroundColor Yellow

# Cleanup temp files
Remove-Item -Recurse -Force $tempDir

Write-Host ""
Write-Host "Done!" -ForegroundColor Green
