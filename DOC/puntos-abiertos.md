# Puntos abiertos y decisiones

Las decisiones están resumidas también en `AGENTS.md`.

## Decididos (2026-10-29)

| # | Tema | Decisión |
|---|---|---|
| 1 | Nombre | `PhomemoPaint` |
| 2 | Native AOT | WinForms no lo admite: se acepta. Las bibliotecas no se publican con AOT pero **respetan sus limitaciones** (`IsAotCompatible`); avisar antes de romperlas |
| 2b | Publicación | Un único `.exe` *framework-dependent* (`PublishSingleFile`, sin runtime incluido, con las DLL propias dentro) |
| 3 | Scripts | Solo `build.ps1` |
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
| 19 | Otras herramientas | No en v1 (texto, más adelante) |
| 20 | Objetos fuera del lienzo | Se quedan donde están; lo que sobresale no se imprime |
| 21 | Conexión de la T02 | **Bluetooth LE** (por USB no calienta). Transporte en un proyecto aparte, `Phomemo.Printing.Windows`, para que `Phomemo.Printing` siga portable. Búsqueda automática al abrir y con ⟳; se prefiere Bluetooth a USB para el mismo modelo |
| 22 | Cabecera M02 | `1F 11 37 64` + `1F 11 02 04` (probada); sin consultas en el pie; fin con `1A 0F 0C` o, si no llega, espera mínima de 4 s |

## Abiertos

- **Calibración mecánica.** Hacer pruebas con la impresora para saber qué margen conviene
  dejar antes y después de la impresión y comprobar las dimensiones reales. Para eso el editor
  incluye un **patrón de calibración** (Impresora → Patrón de calibración) y la regla en mm.
  Cuando haya medidas, decidir los márgenes por defecto de cada modelo.
- **Densidad del T02.** `1F 11 02 nn` (1-4) y el coeficiente `1F 11 37 nn` (100 o 150).
  Probar qué valores dan mejor negro.
- **Identificar el modelo por el número de serie.** `1F 11 09` devuelve el número de serie;
  su prefijo podría distinguir revisiones si aparecen modelos con el mismo nombre Bluetooth.
- **Primera impresión desde la aplicación.** El transporte BLE está probado desde consola
  (barras, marco, ajedrez); falta imprimir el patrón de calibración desde PhomemoPaint.
- **USB en otros modelos o firmwares.** En la T02 está descartado (la imagen se ignora y solo
  avanza el margen final; las consultas sí funcionan). Otros modelos podrían comportarse
  distinto.
- **`1A 06 88` tras imprimir** por BLE (significa «sin papel») apareció una vez justo después de
  `1A 0F 0C`, poco antes de acabarse el rollo. Observar si se repite con papel de sobra.
- **Cabecera ampliada.** Añade `1F 11 0B` (papel en rollo) y `1F 11 35 00` (sin compresión).
  La cabecera actual funciona; decidir si se amplía tras probarla por Bluetooth.
- **Escala de grises.** La T02 (chip JieLi) no contesta a `1F 11 63`: se asume que no admite
  grises. Si otro modelo sí, habría que estudiar el formato de grises y la compresión LZO.
- **Bluetooth en Linux/macOS.** Si se porta, hará falta un `IPrinterTransport` BLE con las APIs
  de cada sistema.
- **Texto.** Herramienta de texto, para una versión posterior.
