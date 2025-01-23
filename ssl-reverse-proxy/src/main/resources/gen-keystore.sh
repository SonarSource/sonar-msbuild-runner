#!/bin/bash
# Run the script using wsl:
# wsl bash ./gen-keystore.sh
set -e

mkdir -p server_certs
mkdir -p client_certs

echo Generating server certificate...

openssl req \
    -newkey rsa:4096 \
    -x509 \
    -sha256 \
    -addext "subjectAltName = DNS:localhost" \
    -days 3650 \
    -nodes \
    -out server_certs/server.crt \
    -subj "/C=CH/ST=Geneva/L=Geneva/O=Server/OU=Dept" \
    -keyout server_certs/server.key

echo Generating client certificate...

# Generate Certificate Authority key
openssl genrsa \
    -passout pass:test42 \
    -des3 \
    -out client_certs/ca.key 4096 \

# Generate Certificate Authority certificate
openssl req \
    -passin pass:test42 \
    -new \
    -x509 \
    -days 365 \
    -key client_certs/ca.key \
    -out client_certs/ca.crt \
    -subj "/C=CH/ST=Geneva/L=Geneva/O=CertificateAuthority/OU=ExpertDepartment"

# Generating Client certificate key
openssl genrsa \
    -passout pass:test42 \
    -des3 \
    -out client_certs/user.key 4096

# Generating Client certificate certificate
openssl req \
    -passin pass:test42 \
    -new \
    -key client_certs/user.key \
    -out client_certs/user.csr \
    -subj "/C=CH/ST=Geneva/L=Geneva/O=UserOrg/OU=UserDepartment"

# Sign the certificate
openssl x509 \
    -passin pass:test42 \
    -req \
    -days 365 \
    -in client_certs/user.csr \
    -CA client_certs/ca.crt \
    -CAkey client_certs/ca.key \
    -set_serial 01 \
    -out client_certs/user.crt

# Generate a PKCS12 format certificate
openssl pkcs12 \
    -passin pass:test42 \
    -passout pass:test42 \
    -export \
    -out client_certs/user.p12 \
    -inkey client_certs/user.key \
    -in client_certs/user.crt \
    -certfile client_certs/ca.crt
