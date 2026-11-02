# PhomemoPaint — contexto para agentes de IA

Editor de dibujo en blanco y negro (WinForms) que imprime en impresoras térmicas Phomemo por
Bluetooth LE o puerto serie (USB CDC-ACM o Bluetooth SPP). Probado con una **T02** (384 puntos
de ancho, 203 dpi).

## Estado actual (2026-11-02)

- v0.2: 167 pruebas xUnit en verde (127 de impresión y 40 del editor); capa `Phomemo.Printing` (catálogo de 75 modelos, los que requieren compresión LZO sin poder imprimir), transporte Bluetooth LE en
  `Phomemo.Printing.Windows`, editor completo (cursor, pincel, texto, imágenes flotantes con tiradores,
  reglas, deshacer, exportar/abrir PNG, patrón de calibración, diagnóstico de señales),
  `build.ps1` y `run.cmd`.
- **La T02 solo imprime por Bluetooth LE** (servicio `FF00`). Por USB (`4C4A:4155`) responde a
  las consultas pero no imprime la imagen.
- Impresión por BLE probada desde la aplicación: el patrón de calibración sale exacto. Panel
  de información de la impresora (n.º de serie, batería…) con lectura periódica.
- Densidad por defecto de la T02: 3 (coeficiente 100), la mejor en las pruebas.
- Pendiente: compresión LZO, ¿selector de densidad?, escala por monitor.
  Ver `DOC/puntos-abiertos.md`.
- **Git y GitHub los gestiona el responsable del proyecto: no hacer commits, `push` ni crear
  repositorios.**
- **Documentación impersonal:** sin nombres ni datos personales (direcciones, números de serie,
  puertos de una máquina concreta). Las pruebas se registran como «probado», sin decir quién.
  Sin referencias a aplicaciones de terceros ni a su código.

## Decisiones tomadas

- Tres capas: `Phomemo.Printing` (net10.0, portable, `IsAotCompatible`, sin WinForms ni APIs
  de Windows), `Phomemo.Printing.Windows` (transportes con APIs de Windows: Bluetooth LE por
  WinRT; también `IsAotCompatible`) y `PhomemoPaint` (WinForms). La UI solo entrega un
  `MonoBitmap` y `PrintOptions`. **Avisar antes de introducir algo que rompa Native AOT en las
  bibliotecas.**
- Publicación: un único `.exe` framework-dependent (`PublishSingleFile`), sin runtime incluido
  (~25 MB por la proyección de WinRT). Requiere Windows 10 2004+. `build.ps1` para publicar y
  `run.cmd` para ejecutar con doble clic; sin `build.sh` ni `run.sh` (solo Windows). MIT.
- Protocolo M02/T02: cabecera ampliada probada (`1F 11 02 dd`, `1F 11 37 64`, `1F 11 0B`,
  `1F 11 35 00`, `GS v 0`, `ESC d`); `1A 0F 0C` indica fin pero es poco fiable (hay espera mínima).
  Sin papel o con la tapa abierta acepta el trabajo en silencio: se comprueba antes de imprimir.
  Respuestas `1A …` interpretadas por `PrinterMessages`. `1F 11 14` (actualizar firmware) bloqueado en el diagnóstico.
- Ancho del lienzo = cabezal del modelo (en M110, ancho de etiqueta). Alto en puntos, 400 por
  defecto. Se imprime desde la fila 0 hasta la última con tinta.
- Cambiar de modelo no escala: recorta o rellena a la derecha, con aviso si hay dibujo.
- Detección: tabla VID/PID (`PrinterCatalog.UsbIds`), nombre Bluetooth, recuerdo del modelo
  por puerto y prefijo del n.º de serie (solo se consulta a la impresora seleccionada).
  Búsqueda BLE al abrir y con ⟳; las impresoras BLE vistas se recuerdan. Si el mismo modelo aparece por USB y BLE se elige BLE.
- Imágenes flotantes editables hasta fijarlas; tramado Floyd-Steinberg por defecto.
- Textos: objetos flotantes dibujados como trazado vectorial con umbral, tamaño en pt físicos. Los
  tiradores definen el cuadro: ajuste de línea y alineación se refieren a él, la fuente no cambia.
- Pincel: los trazos son un objeto que solo se mueve; se agrupan mientras dure la herramienta (sin
  tiempo de espera). El cursor es la única forma de mover con la seguridad de no pintar.
- Fuentes: nunca en `Designer.cs`, el diseñador las encoge al guardar (`IconButton` y constructor).
- Sin opciones de densidad/velocidad en la UI por ahora.
- phomemo-tools (GPLv3) y phomemo-protocol (MIT) son solo referencia del protocolo; no se
  copia su código. El resto sale de pruebas con la impresora.

## Documentación

- [DOC/protocolos.md](DOC/protocolos.md): conexión BLE, bytes de cada familia, respuestas,
  consultas, tabla de modelos y registro de pruebas.
- [DOC/arquitectura.md](DOC/arquitectura.md): proyectos, clases y flujo de impresión.
- [DOC/editor.md](DOC/editor.md): comportamiento de la aplicación.
- [DOC/compilacion-y-pruebas.md](DOC/compilacion-y-pruebas.md): compilar, probar, restricciones.
- [DOC/puntos-abiertos.md](DOC/puntos-abiertos.md): decisiones y pendientes.

## Compilar y probar

```powershell
dotnet build PhomemoPaint.slnx
dotnet test PhomemoPaint.slnx
.\build.ps1        # publica PhomemoPaint.exe en la raíz
```
