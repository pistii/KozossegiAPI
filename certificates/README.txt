To generate key:
openssl req -x509 -newkey rsa:4096 -keyout private.key -out certificate.crt -days 3650 -nodes -subj "/C=XX/ST=StateName/L=CityName/O=CompanyName/OU=CompanySectionName/CN=CommonNameOrHostname"
openssl pkcs12 -export -out certificate.pfx -inkey private.key -in certificate.crt -passout pass:YourPasswordHere

Note the password is required also in the appsettings.json fájlban ahol meg kell adni a jelszót:
"Kestrel": {
  "EndPoints": {
    "Https": {
      "Url": "https://192.168.0.16:5000",
      "Certificate": {
        "Path": "certificates/certificate.pfx",
        "Password": "YourPasswordHere"
      }
    }
  }
}

The generated certificate recommended to be used in the frontend to avoid errors