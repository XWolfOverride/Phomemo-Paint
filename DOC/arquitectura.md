# Arquitectura

## Proyectos

```
PhomemoPaint/
├─ src/
│  ├─ Phomemo.Printing/          net10.0                        Comunicación (portable)
│  ├─ Phomemo.Printing.Windows/  net10.0-windows10.0.19041.0    Bluetooth LE (WinRT)
│  └─ PhomemoPaint/              net10.0-windows10.0.19041.0    Editor WinForms
├─ tests/
│  └─ Phomemo.Printing.Tests/ xUnit
├─ DOC/
├─ AGENTS.md  CLAUDE.md (@AGENTS.md)  README.md  LICENSE  build.ps1  PhomemoPaint.slnx
```

Dependencias: `PhomemoPaint` → `Phomemo.Printing.Windows` → `Phomemo.Printing`, nunca al
revés. `Phomemo.Printing` solo usa la BCL y `System.IO.Ports` (paquete de Microsoft, funciona en
Windows, Linux y macOS). Lo que necesita APIs de Windows (Bluetooth LE) va en
`Phomemo.Printing.Windows`; en otro sistema se escribiría el equivalente (p. ej. BlueZ en Linux)
implementando `IPrinterTransport`. Las dos bibliotecas tienen `IsAotCompatible=true` y compilan
sin avisos.

## Phomemo.Printing

| Tipo | Papel |
|---|---|
| `MonoBitmap` | Imagen de 1 bit (MSB primero, 1 = negro). **Lo único que la UI entrega para imprimir.** `LastInkRow`, `CropRows`, `WithWidth` (recorta o rellena a la derecha, nunca escala). |
| `PrinterModel` | Id, nombre, familia, dpi, ancho del cabezal, si el ancho depende de la etiqueta, avance, `Verified`, `Experimental`. |
| `PrinterCatalog` | Lista de modelos, tabla `UsbIds` (VID, PID) → modelo, búsqueda por nombre Bluetooth. |
| `PrintOptions` | Densidad, velocidad, tipo de papel, avance. Lo no indicado sale del modelo. |
| `Protocols.IPrinterProtocol` | `BuildJob(imagen, modelo, opciones)` → trozos de bytes en orden, y `TransportHints` (troceado, pausas, espera de respuesta). |
| `Protocols.RasterProtocol` | Base común (`GS v 0`). Implementaciones: `M02Protocol`, `M110Protocol`, `D30Protocol`, `M04Protocol`, `M832Protocol`. `ProtocolFactory.For(modelo)`. |
| `Transport.IPrinterTransport` | `OpenAsync`, `WriteAsync`, `ReadAsync(timeout)`. |
| `Transport.SerialPortTransport` | `System.IO.Ports`, 128000 baudios, DTR/RTS activos. `ListPorts()`. |
| `Transport.StreamTransport` | Sobre cualquier `Stream` (pruebas, volcado a fichero). |
| `PhomemoPrinter` | `PrintAsync(imagen, opciones, progreso, cancelación)`: codifica, envía respetando los `TransportHints` y espera hasta `1A 0F 0C` (terminado) o, si no llega, hasta el silencio (mínimo ~0,5 s por KB, máximo 60 s). `QueryAsync(comando)` para experimentar con consultas. |
| `PrintResult` | Bytes enviados, respuesta, `Finished` y `Messages`. |
| `PrinterMessages` | Interpreta las respuestas `1A tt …` (`PrinterMessage`: terminado, batería, papel, tapa, temperatura, firmware, n.º de serie, apagado automático). |
| `PortInfo`, `PortKind`, `IPortInfoProvider`, `BasicPortInfoProvider` | Puertos serie (con VID/PID si el sistema los da) y dispositivos Bluetooth LE (`PortInfo.BluetoothLE(dirección, nombre)`, `Name` = `BLE:<dirección>`). |
| `ModelDetector` | `Detect(PortInfo)` → `DetectionResult(modelo, origen)`: USB primero, luego nombre Bluetooth. |

Los textos de esta biblioteca no se muestran al usuario (son excepciones); la UI traduce los
resultados (`DetectionSource`, etc.).

## Phomemo.Printing.Windows

| Tipo | Papel |
|---|---|
| `BleTransport` | `IPrinterTransport` por GATT: servicio `FF00`, escribe en `FF02` en trozos del tamaño anunciado (`02 nn`, 182 en la T02), con control de flujo por tokens (`01 nn`, `1A 36 nn`), y entrega por `ReadAsync` las notificaciones de `FF03` que no son de control de flujo. No necesita emparejar. |
| `BleScanner` | `ScanAsync(duración)`: anuncios BLE con nombre reconocido por el catálogo o servicio `AF30`/`FF00`. |
| `BleAddress` | Dirección numérica ↔ `AA:BB:CC:DD:EE:FF`. |
| `Transports` | `Create(PortInfo)`: `BleTransport` o `SerialPortTransport` según `PortKind`. |

## PhomemoPaint (WinForms)

| Tipo | Papel |
|---|---|
| `MainForm` | Ventana, menús y barras; puerto/modelo/ancho/alto; imprimir en segundo plano. |
| `Editing.PaintDocument` | Capa de píxeles (`Bitmap` ARGB, solo blanco y negro) + `ImageObject` flotantes. `Resize` sin escalar, `Flatten`, `ToPrintImage` (recorta a la última fila con tinta), `Stroke`, instantáneas. |
| `Editing.History` | Deshacer/rehacer (50 niveles). La capa se guarda como `MonoBitmap` (1 bit) para ahorrar memoria. |
| `Editing.ImageObject` | Imagen original + centro, tamaño, ángulo y conversión. `Render()` la transforma y pasa a B/N con caché. |
| `Editing.CanvasControl` | `ScrollableControl` con zoom, reglas fijas, pincel y tiradores. Lanza `BeforeEdit` (instantánea) y `Edited`. |
| `Editing.Imaging` | Conversiones `Bitmap` ↔ `MonoBitmap`, tramado Floyd-Steinberg / umbral, PNG de 1 bit. |
| `Editing.CalibrationPattern` | Patrón de medida (marco, marcas cada mm, cruz central). |
| `DiagnosticsForm` | Envía comandos hexadecimales sueltos y muestra las respuestas. |
| `WindowsPortInfoProvider` | VID/PID de cada COM leyendo `HKLM\SYSTEM\CurrentControlSet\Enum\USB`. |
| `AppSettings` | JSON en `%APPDATA%\PhomemoPaint` (generación de código). |
| `S` | Textos en español/inglés (`PHOMEMOPAINT_LANG`). |

## Flujo de impresión

```
PaintDocument.ToPrintImage()            (Bitmap → MonoBitmap, recortado en alto)
  → PhomemoPrinter(Transports.Create(puerto), modelo).PrintAsync(...)
      → ProtocolFactory.For(modelo).BuildJob(...)   cabecera + bloques GS v 0 + pie
      → escritura troceada + espera hasta 1A 0F 0C (terminado)
```
