# Qapptia API Mock

Herramienta local para pruebas de ciclo de vida de red (`/api/version`, `/download`, `/sponsor`).

### Gestión de certificados HTTPS (localhost)
- **Añadir / Confiar certificado:** `dotnet dev-certs https --trust`
- **Quitar / Limpiar certificado:** `dotnet dev-certs https --clean`

### Ejecución
```powershell
.\run.ps1
```

### Casos de Prueba
- **Consultar versión actual (usa el caso activo):**
  `GET https://localhost:8080/api/version`
- **Cambiar y persistir caso de prueba:**
  - Versión estable superior (`v2.1.0`): `GET https://localhost:8080/api/version?case=update`
  - Versión pre-release beta superior (`v2.1.0-beta.1`): `GET https://localhost:8080/api/version?case=prerelease`
  - Versión pre-release alpha superior (`v2.1.0-alpha.1`): `GET https://localhost:8080/api/version?case=alpha`
  - Misma versión / Al día (`v2.0.0-beta`): `GET https://localhost:8080/api/version?case=uptodate`
  - Versión inferior / Previa (`v1.9.0`): `GET https://localhost:8080/api/version?case=previous`
  - Error 503 Servidor no disponible: `GET https://localhost:8080/api/version?case=503`
  - Error 500 Servidor interno: `GET https://localhost:8080/api/version?case=500`
  - JSON corrupto / malformado: `GET https://localhost:8080/api/version?case=corrupt`
  - Tiempo de espera agotado (Timeout): `GET https://localhost:8080/api/version?case=timeout`

