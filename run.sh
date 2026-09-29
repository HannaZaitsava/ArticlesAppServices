#!/bin/bash

# Path to the certificate
CERT_PATH="$HOME/.aspnet/https/aspnetapp.pfx"

# Check if the certificate file already exists
if [ ! -f "$CERT_PATH" ]; then
    echo "Generating HTTPS development certificate..."
    # Create the directory if it does not exist
    mkdir -p "$(dirname "$CERT_PATH")"
    # Export the certificate
    dotnet dev-certs https -ep "$CERT_PATH" -p "cryptic_password_123"
    # Trust the certificate
    dotnet dev-certs https --trust
else
    echo "HTTPS certificate already exists. Skipping generation."
fi

echo "Certificate setup complete. You can now run docker-compose up."
