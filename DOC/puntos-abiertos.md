# Puntos abiertos y decisiones

Las decisiones están resumidas también en `AGENTS.md`.

## Decididos (2026-10-29)

| # | Tema | Decisión |
|---|---|---|
| 1 | Nombre | `PhomemoPaint` |
| 2 | Native AOT | WinForms no lo admite: se acepta. Las bibliotecas no se publican con AOT pero **respetan sus limitaciones** (`IsAotCompatible`); avisar antes de romperlas |
| 2b | Publicación | Un único `.exe` *framework-dependent* (`PublishSingleFile`, sin runtime incluido, con las DLL propias dentro) |
| 3 | Scripts | `build.ps1` (publica) y `run.cmd` (doble clic, `dotnet run`). Sin `build.sh` ni `run.sh`: WinForms solo funciona en Windows. Sin script `publish`: ya lo hace `build.ps1` |
| 4 | Git y GitHub | Los gestiona el responsable del proyecto: los agentes no hacen commits, `push` ni crean repositorios |
| 5 | Licencia | MIT |
| 6 | Modelos | Catálogo completo; M02 y M110 implementadas, el resto experimentales |
| 7 | Ancho M110 | Selector de ancho de etiqueta (mm) con tope en el cabezal |
| 8 | Anchos sin confirmar | Valores provisionales marcados "no verificado" |
| 9 | Autodetección | Tabla de VID/PID USB → modelo (se refactoriza si se encuentra algo mejor). Recordar el modelo por puerto. Pruebas de señales (consultas de estado) aceptadas |
| 10 | Justificación | Izquierda |
| 11 | Densidad y parámetros | Para más adelante; v1 sin opciones de impresión en la UI |
| 12 | Alto por defecto | 400 puntos; el campo va en puntos |
| 13 | Qué se imprime | Desde la fila 0 (se respeta el margen superior) hasta la última fila con tinta. Se mostrará una **regla** en el borde del lienzo |
| 14 | Objetos imagen | Flotantes y editables hasta fijarlos |
| 15 | Color/gris a B/N | Tramado Floyd-Steinberg (opción de umbral) |
| 16 | Zoom | Sí, prioridad baja |
| 17 | Guardar | Exportar PNG y abrir PNG como lienzo |
| 18 | Deshacer/rehacer | Sí |
| 19 | Herramientas | Cursor, pincel y texto (ver 37, 39 y 40) |
| 20 | Objetos fuera del lienzo | Se quedan donde están; lo que sobresale no se imprime |
| 21 | Conexión de la T02 | **Bluetooth LE** (por USB no calienta). Transporte en un proyecto aparte, `Phomemo.Printing.Windows`, para que `Phomemo.Printing` siga portable. Búsqueda automática al abrir y con ⟳; se prefiere Bluetooth a USB para el mismo modelo |
| 22 | Cabecera M02 | `1F 11 37 64` + `1F 11 02 04` (probada); sin consultas en el pie; fin con `1A 0F 0C` o, si no llega, espera mínima de 4 s |
| 23 | Modelos detectados | Todos en el catálogo (75). Los que trabajan con compresión LZO se pueden elegir pero no imprimir: aviso ⚠ en la barra y Imprimir desactivado |
| 24 | Cabecera M02 ampliada | `1B 40`, densidad, coeficiente, papel `1F 11 0B`, `1F 11 35 00` (probada por BLE) |
| 25 | Información de la impresora | Panel lateral. Consulta (`1F 11 09/07/08/11/12`) solo a la impresora seleccionada; buscar no manda nada. Batería cada 5 min. El prefijo del n.º de serie fija el modelo |
| 26 | Calibración T02 | El patrón sale exacto: sin márgenes ni correcciones |
| 27 | Icono | `Phome.ico` en el ejecutable y en las ventanas |
| 28 | Densidad T02 | `1F 11 02 03` con coeficiente 100 por defecto (`PrinterModel.DefaultDarkness`): el mejor tramado en las pruebas (1, 2, 3, 4 con 100 y 4 con 150) |
| 29 | Sin papel o tapa abierta | Se preguntan `1F 11 11` y `1F 11 12` antes de cada impresión; sin papel o con la tapa abierta no se envía y se avisa (la impresora aceptaría el trabajo en silencio) |
| 30 | Alto del lienzo | Marca de fin arrastrable (en mm enteros; Mayús, en puntos) y una pantalla de margen sin papel bajo el lienzo; el campo numérico se mantiene sincronizado |
| 31 | Imágenes fuera del papel | Se dibujan atenuadas (35 %) fuera del papel para poder recolocarlas; no se imprimen |
| 32 | Avisos de recorte | Sin ventanas emergentes: el cambio se aplica y un aviso amarillo en la barra de estado dice qué se ha recortado (Ctrl+Z deshace) o que una imagen sobresale del papel |
| 33 | Transparencia | Se trama también la opacidad (Floyd-Steinberg); al pegar se usa la copia PNG del portapapeles, que conserva el alfa |
| 34 | Distribución | Sin barras. Columna derecha: herramientas (con Imprimir al final, separado y más grande), propiedades de la selección, impresora (con puerto, ⟳, modelo y ancho de etiqueta) y zoom abajo. El alto del lienzo es un campo que flota junto a la marca de fin |
| 35 | Ayuda | HTML incrustado en el exe (`src/PhomemoPaint/Help`), copiado a `%TEMP%` y abierto en el navegador; sin WebView2. F1 según la zona. Sin capturas: un esquema SVG dentro de cada página dibuja la ventana con datos ficticios, y sus zonas y botones enlazan con cada sección |
| 36 | Diseñador | `MainForm` y `PrinterInfoPanel` editables con el diseñador de Windows Forms (textos por `ApplyTexts`, eventos por `WireEvents`) |
| 37 | Texto | Objeto flotante: trazado vectorial y umbral, tamaño en pt según la resolución del modelo y edición en Propiedades. Los tiradores definen un cuadro donde se parten las líneas y al que se refieren las alineaciones; nunca es más bajo que el texto |
| 38 | Fuentes y diseñador | Ninguna propiedad `Font` en `Designer.cs`: `IconButton` y constructor de `MainForm`. El diseñador las encoge al guardar en pantallas escaladas |
| 39 | Pincel | Los trazos forman un objeto que solo se mueve. Se agrupan mientras dure el pincel; se cierra al cambiar de herramienta, con Esc o al fijarlo. Sin tiempo de espera, que partiría dibujos al hacer una pausa |
| 40 | Cursor | Herramienta propia para seleccionar y mover. El pincel nunca toca los objetos, para no pintar al querer mover ni al revés |

## Abiertos

- **Calibración mecánica de otros modelos.** En la T02 el patrón de calibración sale exacto y
  la regla coincide: no hace falta cambiar márgenes ni medidas. Repetir con otros modelos.
- **Densidad en la interfaz.** Decidido 3/100 por defecto en la T02; valorar si se ofrece un
  selector de densidad.
- **USB en otros modelos o firmwares.** En la T02 está descartado (la imagen se ignora y solo
  avanza el margen final; las consultas sí funcionan). Otros modelos podrían comportarse
  distinto.
- **Compresión LZO** para los modelos A4 marcados «requiere compresión». Ojo con la licencia: la
  biblioteca LZO/miniLZO es GPL, incompatible con MIT; habría que escribir un compresor propio
  que genere el formato LZO1X (conviene comprobar antes si esos modelos aceptan
  `1F 11 35 00` sin comprimir, como la M832).
- **Escala de grises.** La T02 (chip JieLi) no contesta a `1F 11 63`: se asume que no admite
  grises. Si otro modelo sí, habría que estudiar el formato de grises y la compresión LZO.
- **Bluetooth en Linux/macOS.** Si se porta, hará falta un `IPrinterTransport` BLE con las APIs
  de cada sistema.
- **Escala por monitor.** El formulario usa `AutoScaleMode.None`: los controles miden lo mismo en
  píxeles con cualquier escala de pantalla. Escalar bien exige pasar las coordenadas a 96 ppp.
