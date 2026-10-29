# Compilación y pruebas

## Requisitos

SDK de .NET 10 en Windows 10 versión 2004 (19041) o posterior. La biblioteca
`Phomemo.Printing` y sus pruebas compilan también en Linux y macOS; `Phomemo.Printing.Windows`
y la aplicación WinForms, solo en Windows.

## Comandos

```powershell
dotnet build PhomemoPaint.slnx
dotnet test tests/Phomemo.Printing.Tests
dotnet run --project src/PhomemoPaint
```

Publicación (lo que hace `build.ps1`):

```powershell
dotnet publish src/PhomemoPaint/PhomemoPaint.csproj -c Release -r win-x64 -o publish/PhomemoPaint
```

Sale un único `PhomemoPaint.exe` *framework-dependent* (`PublishSingleFile`, `SelfContained`
falso) con las DLL propias dentro. Ocupa unos **25 MB** porque incluye la proyección de las
APIs de Windows (`Microsoft.Windows.SDK.NET.dll`, necesaria para Bluetooth LE); sin
autocontenido no se puede recortar. Sin Bluetooth ocupaba 0,4 MB.

## Restricciones

- `Phomemo.Printing` y `Phomemo.Printing.Windows` tienen `IsAotCompatible=true`: los
  analizadores avisan de cualquier cosa que rompa Native AOT o el recorte (ahora mismo, ningún
  aviso). **Avisar al responsable del proyecto antes de introducir algo así.**
- En `Phomemo.Printing`: nada de WinForms, `System.Drawing` ni APIs de Windows. Lo específico
  de Windows va en `Phomemo.Printing.Windows`.
- La aplicación no puede ser AOT (WinForms).
- La serialización JSON de la aplicación usa generación de código (`SettingsJson`).

## Pruebas automáticas

`tests/Phomemo.Printing.Tests` (xUnit, 48 pruebas):

- `MonoBitmapTests`: empaquetado de bits, recorte y relleno sin escalar, última fila con tinta.
- `ProtocolTests`: bytes exactos de cabecera, bloques y pie (T02 y M110), bloques de 255 líneas
  en la familia M02, ajuste de ancho y un trabajo por cada modelo del catálogo.
- `PrinterTests`: envío completo, troceado, progreso, lectura de la respuesta y fin de la espera
  al recibir `1A 0F 0C`, usando `StreamTransport` (sin hardware).
- `PrinterMessagesTests`: interpretación de las respuestas `1A …` reales de la T02.
- `CatalogTests`: datos del T02, detección por VID/PID y por nombre Bluetooth.

`BleTransport` y `BleScanner` no tienen pruebas automáticas (necesitan la impresora).

## Pruebas manuales con la impresora

1. Encender la T02 y abrir PhomemoPaint: en unos 5 s debe aparecer «T02 — Bluetooth …»
   seleccionada. **Por USB no imprime** (sale en blanco).
2. Impresora → Patrón de calibración → Imprimir. Medir con una regla: ancho del marco
   (debe ser 48,0 mm en el T02), largo (50,0 mm con el alto por defecto de 400 puntos) y
   margen en blanco antes y después.
3. Impresora → Diagnóstico de señales: enviar las consultas y anotar respuestas en
   `DOC/protocolos.md`.

Para probar sin la interfaz, un programa de consola que referencie
`Phomemo.Printing.Windows` puede buscar con `BleScanner.ScanAsync` e imprimir con
`PhomemoPrinter` + `Transports.Create` (así se verificó el transporte BLE). Para probar la
interfaz, un programa aparte (`net10.0-windows10.0.19041.0`, hilo STA) puede cargar
`PhomemoPaint` por reflexión, crear `MainForm` y leer sus campos o usar `DrawToBitmap`.
