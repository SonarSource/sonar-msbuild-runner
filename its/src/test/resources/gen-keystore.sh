#!/bin/bash
# Run the script using wsl:
# wsl bash ./gen-keystore.sh

# Step 1: Generate a private key
openssl genpkey -algorithm RSA -out private.key

# Step 2: Generate a self-signed certificate
openssl req -new -x509 -key private.key -out certificate.crt -days 365 -subj "/CN=localhost"

# Step 3: Convert the private key and certificate to a .p12 file
openssl pkcs12 -export -out keystore.p12 -inkey private.key -in certificate.crt -password pass:your_password

# Clean up
rm private.key
