# Qapptia

[![Sitio web](https://img.shields.io/badge/web-qapptia.com-2563eb)](https://qapptia.com)
[![Última versión publicada](https://img.shields.io/github/v/release/necmc/Qapptia?include_prereleases&sort=semver&label=versi%C3%B3n&color=orange)](https://github.com/necmc/Qapptia/releases)
[![.NET 10](https://img.shields.io/badge/.NET-10-512bd4)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![Patrocinio y colaboraciones](https://img.shields.io/badge/patrocinio-colaboraciones-db61a2)](https://qapptia.com/sponsor)

Qapptia guarda y organiza tus evidencias desde la primera captura. Captura con un atajo, añade anotaciones y sigue probando. Encuéntralas por fecha o carpeta cuando las necesites.

## Qué puedes hacer

- Capturar la pantalla o un área desde la bandeja del sistema o con atajos configurables.
- En equipos con varios monitores, capturar el monitor donde está el cursor o todos los monitores en una sola imagen.
- Confirmar la captura con un indicador visual, incluso con el audio silenciado.
- Activar **Copiar al portapapeles automáticamente** en **Configuración → Captura** para pegar o compartir las capturas al instante.
- Ajustar la carpeta de guardado, los nombres de archivo y la organización de las subcarpetas.
- Consultar las evidencias por carpeta en Árbol o por fecha en Calendario.
- Anotar las imágenes con texto, flechas y formas desde el Editor.
- Usar el tema del sistema o elegir entre claro y oscuro.

Las capturas se guardan en formato PNG. Puedes abrirlas en otros visores y editores, adjuntarlas a reportes de errores o compartirlas directamente.

## Descargas

Descarga la última versión disponible de Qapptia desde cualquiera de estas fuentes:

- [Descargas en la web oficial](https://qapptia.com/download)
- [GitHub Releases](https://github.com/necmc/Qapptia/releases)

Para Windows hay un instalador y ediciones portátiles. Las integraciones nativas de Linux y macOS están en desarrollo.

## Desarrollo

Qapptia está desarrollado en C# con .NET y Avalonia. Usa PowerShell 7 y el SDK de .NET 10. Ejecuta los comandos desde la raíz del repositorio. El inicio mediante `-Run` y la generación del instalador se realizan en Windows. El instalador requiere Inno Setup.

```powershell
./build.ps1 -PackageAll  # Generar el instalador y las ediciones portátiles
./build.ps1 -Run         # Compilar, ejecutar las pruebas e iniciar Qapptia
./build.ps1 -Run -NoTest # Compilar e iniciar Qapptia sin ejecutar las pruebas
./build.ps1 -Clean       # Limpiar bin, obj y dist
```
