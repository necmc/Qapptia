# Qapptia API Mock

Herramienta local para pruebas de ciclo de vida de red y verificación de versiones (`/api/v1/version`).

### Gestión de certificados HTTPS (localhost)
- **Añadir / Confiar certificado:** `dotnet dev-certs https --trust`
- **Quitar / Limpiar certificado:** `dotnet dev-certs https --clean`

### Ejecución
```powershell
.\run.ps1
```

### Comportamiento con Estado (Stateful)
Cuando se invoca un caso mediante `?case=xxxx`:
- El servidor **fija `xxxx` como el caso activo global**.
- A partir de ese momento, la app desktop (que consulta `GET /api/v1/version` sin parámetros) **responderá siempre con el caso fijado**.
- Para cambiar a otro caso: simplemente llama a `?case=otro_caso`.
- Para restaurar el comportamiento normal (`latest.json`): invoca `GET http://localhost:8080/api/v1/version?case=reset` (o `?case=latest`).
- Para consultar qué caso está activo actualmente: `GET http://localhost:8080/api/v1/case`.

### Casos de Prueba (`versions/cases/`)
- **Restablecer a versión oficial (`versions/latest.json`):**
  `GET http://localhost:8080/api/v1/version?case=reset`
- **Casos de prueba disponibles (`?case=xxxx`):**
  - Actualización beta superior (`v2.0.0-beta` -> `v2.0.0-beta.2`): `GET http://localhost:8080/api/v1/version?case=beta` (o `?case=update-beta`)
  - Versión estable superior (`v2.1.0`): `GET http://localhost:8080/api/v1/version?case=update`
  - Versión pre-release beta superior (`v2.1.0-beta.1`): `GET http://localhost:8080/api/v1/version?case=prerelease`
  - Versión pre-release alpha superior (`v2.1.0-alpha.1`): `GET http://localhost:8080/api/v1/version?case=alpha`
  - Misma versión / Al día (`v2.0.0-beta`): `GET http://localhost:8080/api/v1/version?case=uptodate`
  - Versión inferior / Previa (`v1.9.0`): `GET http://localhost:8080/api/v1/version?case=previous`
  - Error 503 Servidor no disponible: `GET http://localhost:8080/api/v1/version?case=503`
  - Error 500 Servidor interno: `GET http://localhost:8080/api/v1/version?case=500`
  - JSON corrupto / malformado: `GET http://localhost:8080/api/v1/version?case=corrupt`
  - Tiempo de espera agotado (Timeout): `GET http://localhost:8080/api/v1/version?case=timeout`

