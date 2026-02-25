## Self-signed certificate creation
### 1. Prepare a certificate configuration as a file named `cert.config`. Example content:
```
[req]
default_bit = 4096
distinguished_name = req_distinguished_name
prompt = no
x509_extensions = v3_ca

# Subject details
[req_distinguished_name]
countryName             = CZ
stateOrProvinceName     = Capital City of Prague
localityName            = Prague
organizationName        = Organization name
commonName              = Ogranization's Document Signature Service
emailAddress            = signatures@myorganization.example.com

[v3_ca]
basicConstraints = CA:FALSE
keyUsage=nonRepudiation, digitalSignature, keyEncipherment

# SAN extension
subjectAltName = @alt_names

# SAN entries for FHIR
[alt_names]
DNS.1 = www.myserever.example.com
```
Populate sections `req_distinguished_name` and `alt_names` with your actual data.
### 2. Execute following commands
```bash
openssl genrsa -out private-key.pem 3072
openssl req -new -x509 -key private-key.pem -outform DER -out cert.der -days 365 -config cert.config
openssl x509 -in cert.der -inform DER -outform PEM -out cert.pem
```

## Configuring the service to use the private key and public certificate
Use `appsettings.json` file for configuration. Populate property `SignatureCertificate`. Populate sub-properties `` and ``.
Example:
```
  "SignatureCertificate": {
    "PrivateKeyPath": "C:\\Users\\User\\Documents\\FHIR signing config\\private-key.pem",
    "CertificatePath": "C:\\Users\\User\\Documents\\FHIR signing config\\cert.pem"
  }
```

## Configuring signor's data
Configure signor's name in `SignorDisplay` sub-property of the `Signing` section.
Example:
```
  "Signing": {
    "SignorDisplay": "Test Organization"
  }
```

## Complete `appsettings.json` example
```json
{
  "SignatureCertificate": {
    "PrivateKeyPath": "C:\\Users\\User\\Documents\\FHIR signing config\\private-key.pem",
    "CertificatePath": "C:\\Users\\User\\Documents\\FHIR signing config\\cert.pem",
    "Password": ""
  },
  "Signing": {
    "SignorDisplay": "Test Organization"
  }
}
```

## API documentation

See Swagger documentation available as a file `swagger.json` or as a web page available at `http://localhost:5290/swagger` after service launch.