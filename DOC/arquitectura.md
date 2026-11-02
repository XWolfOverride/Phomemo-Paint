# Arquitectura

## Proyectos

```
PhomemoPaint/
├─ src/
│  ├─ Phomemo.Printing/          net10.0                        Comunicación (portable)
│  ├─ Phomemo.Printing.Windows/  net10.0-windows10.0.19041.0    Bluetooth LE (WinRT)
│  └─ PhomemoPaint/              net10.0-windows10.0.19041.0    Editor WinForms
├─ tests/
│  ├─ Phomemo.Printing.Tests/ xUnit
│  └─ PhomemoPaint.Tests/     xUnit (textos del editor)
├─ DOC/
├─ AGENTS.md  CLAUDE.md (@AGENTS.md)  README.md  LICENSE  build.ps1  run.cmd  PhomemoPaint.slnx
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
| `PrinterModel` | Id, nombre, familia, dpi, ancho del cabezal, si el ancho depende de la etiqueta, avance, `Verified`, `Experimental` y `RequiresCompression` (`CanPrint` falso: `BuildJob` lanza `NotSupportedException`). |
| `PrinterCatalog` | Lista de modelos, tabla `UsbIds` (VID, PID) → modelo, búsqueda por nombre Bluetooth. |
| `PrintOptions` | Densidad, coeficiente de calor (M02), velocidad, tipo de papel, avance. Lo no indicado sale del modelo. |
| `Protocols.IPrinterProtocol` | `BuildJob(imagen, modelo, opciones)` → trozos de bytes en orden, y `TransportHints` (troceado, pausas, espera de respuesta). |
| `Protocols.RasterProtocol` | Base común (`GS v 0`). Implementaciones: `M02Protocol`, `M110Protocol`, `D30Protocol`, `M04Protocol`, `M832Protocol`. `ProtocolFactory.For(modelo)`. |
| `Transport.IPrinterTransport` | `OpenAsync`, `WriteAsync`, `ReadAsync(timeout)`. |
| `Transport.SerialPortTransport` | `System.IO.Ports`, 128000 baudios, DTR/RTS activos. `ListPorts()`. |
| `Transport.StreamTransport` | Sobre cualquier `Stream` (pruebas, volcado a fichero). |
| `PhomemoPrinter` | `PrintAsync(imagen, opciones, progreso, cancelación)`: codifica, envía respetando los `TransportHints` y espera hasta `1A 0F 0C` (terminado) o, si no llega, hasta el silencio (mínimo ~0,5 s por KB, máximo 60 s). `QueryAsync(comando)` para experimentar con consultas; `QueryInfoAsync(soloBatería)` pide n.º de serie, firmware, batería, papel y tapa y devuelve un `PrinterInfo`; `QueryReadyAsync()` pregunta por el papel y la tapa (se usa antes de imprimir). |
| `PrintResult` | Bytes enviados, respuesta, `Finished` y `Messages`. |
| `PrinterMessages` | Interpreta las respuestas `1A tt …` (`PrinterMessage`: terminado, batería, papel, tapa, temperatura, firmware, n.º de serie, apagado automático). |
| `PrinterInfo` | Datos reunidos de las respuestas (n.º de serie, firmware, batería, papel, tapa, temperatura), `ModelFromSerial` y `Merge`. |
| `PortInfo`, `PortKind`, `IPortInfoProvider`, `BasicPortInfoProvider` | Puertos serie (con VID/PID si el sistema los da) y dispositivos Bluetooth LE (`PortInfo.BluetoothLE(dirección, nombre)`, `Name` = `BLE:<dirección>`). |
| `ModelDetector` | `Detect(PortInfo)` → `DetectionResult(modelo, origen)`: USB primero, luego nombre Bluetooth. El prefijo del n.º de serie (`PrinterCatalog.FindBySerialNumber`) se usa tras consultar a la impresora seleccionada. |

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
| `MainForm` | Ventana principal. Controles y disposición en `MainForm.Designer.cs` (**editable con el diseñador** de Windows Forms); en `MainForm.cs`, `ApplyTexts()` (textos en el idioma de la app; el diseñador guarda los españoles), `WireEvents()` (eventos, conectados por código) y la lógica. El campo del alto (`_height`) se crea por código porque flota dentro del lienzo. |
| `Editing.PaintDocument` | Capa de píxeles (`Bitmap` ARGB, solo blanco y negro) + `ImageObject` flotantes (imágenes, textos y dibujos). `ActiveInk`: grupo de trazos abierto. `Resize` sin escalar, `Flatten`, `ToPrintImage` (recorta a la última fila con tinta), `BeginStroke`, instantáneas. |
| `Editing.History` | Deshacer/rehacer (50 niveles). La capa se guarda como `MonoBitmap` (1 bit) para ahorrar memoria. |
| `Editing.ImageObject` | Imagen original, `TextContent` o lista de `BrushStroke` + centro, tamaño, ángulo y conversión. `Render()` lo transforma y pasa a B/N con caché; al alargar un trazo solo se pinta el tramo nuevo. |
| `Editing.BrushStroke`, `BrushPainter` | Puntos, grosor y color de un trazo, y el dibujo de cada tramo. |
| `Editing.TextContent` | Texto, fuente, tamaño en pt, estilo, alineación y color. `BuildPath(ancho)` parte las líneas y da el trazado y el alto. |
| `Editing.CanvasControl` | `ScrollableControl` con zoom, reglas fijas, herramientas (`CanvasTool`), tiradores y la marca de fin del lienzo; área de desplazamiento una pantalla más alta que el lienzo. Lanza `BeforeEdit` (instantánea), `Edited` y `HeightDragged` (alto pedido al arrastrar la marca; `MainForm` lo aplica como el campo numérico) y `TextRequested` (clic con la herramienta de texto). |
| `Editing.Imaging` | Conversiones `Bitmap` ↔ `MonoBitmap`, tramado Floyd-Steinberg / umbral (también de la opacidad), PNG de 1 bit, `FromClipboard` (prefiere la copia PNG con alfa). |
| `Editing.CalibrationPattern` | Patrón de medida (marco, marcas cada mm, cruz central). |
| `DiagnosticsForm` | Envía comandos hexadecimales sueltos y muestra las respuestas. |
| `PrinterInfoPanel` | `UserControl` editable con el diseñador (`PrinterInfoPanel.Designer.cs`): datos de la impresora (conexión, dirección, n.º de serie, firmware, batería, papel, tapa, hora) y botón de actualizar. El título y los selectores de puerto y modelo están en `MainForm`. |
| `IconButton` | Botón con icono de Segoe MDL2 Assets y tamaño en `IconSize`. La fuente no pasa por el diseñador, que la encogería. |
| `AppIcon` | Icono `Phome.ico` (recurso incrustado) para las ventanas; el del ejecutable lo pone `ApplicationIcon`. |
| `HelpViewer` | Ayuda: copia los recursos `help/…` (carpeta `Help` del proyecto) y el icono a `%TEMP%\PhomemoPaint\ayuda-<versión>` y abre la página del idioma en el navegador, pasando por `abrir.html` para no perder la sección (`#…`). Se vuelve a copiar cada vez (el liberador de espacio de Windows puede haberla borrado). |
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
