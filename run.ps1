# Check if the certificate file already exists
$certPath = "$env:USERPROFILE\.aspnet\https\aspnetapp.pfx"

if (-not (Test-Path $certPath)) {
    Write-Host "Generating HTTPS development certificate..." -ForegroundColor Cyan
    # Create the directory if it does not exist
    New-Item -ItemType Directory -Force -Path (Split-Path $certPath) | Out-Null
    # Export the certificate
    dotnet dev-certs https -ep $certPath -p "cryptic_password_123"
    # Trust the certificate
    dotnet dev-certs https --trust
} else {
    Write-Host "HTTPS certificate already exists. Skipping generation." -ForegroundColor Green
}

Write-Host "Certificate setup complete. You can now run docker-compose up." -ForegroundColor Cyan
