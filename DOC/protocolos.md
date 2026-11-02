# Protocolos de las impresoras Phomemo

Resumen de lo que hay que mandar a cada modelo. Fuentes:

- [vivier/phomemo-tools](https://github.com/vivier/phomemo-tools) (GPLv3): README secciones
  4-5, filtros CUPS `cups/filter/rastertop*.py`, drivers `cups/drv/*.drv` y backend
  `cups/backend/phomemo.py`.
- [mitchelloharawild/phomemo-protocol](https://github.com/mitchelloharawild/phomemo-protocol)
  (MIT): `commands.ts`, `deviceModels.ts`, `serial.ts`.
- Pruebas con una T02, recogidas en el registro del final.

Solo se usan como **referencia del protocolo**; no se copia código (phomemo-tools es GPLv3).

## Conexión: Bluetooth LE (T02)

**La T02 probada solo imprime por Bluetooth LE.** Por USB (CDC-ACM, VID/PID `4C4A:4155`) acepta
los trabajos, responde a las consultas, avanza el papel y a veces contesta «terminado», pero
**no calienta el cabezal**: sale en blanco. Se probaron por USB varias cabeceras, densidades y
coeficientes de calor, `ESC 7`, `DC2 #`, texto ESC/POS y la secuencia de la familia M110: todo
en blanco; con la cabecera ampliada (papel `1F 11 0B` y compresión `1F 11 35 00`) el papel solo
avanza el margen final. Las consultas sí funcionan por USB. Probablemente el USB de esta revisión
solo sirve para cargar.

En Windows no aparece en el buscador de Bluetooth porque es BLE (no se empareja ni pide PIN).
La aplicación la busca con `BluetoothLEAdvertisementWatcher`:

| Dato | Valor |
|---|---|
| Nombre anunciado | `T02` |
| Dirección | La MAC que imprime la autoprueba de la impresora |
| Servicio anunciado | `AF30` |
| Servicio GATT | `FF00` |
| `FF01` | Lectura (no usada) |
| `FF02` | Escritura (con y sin respuesta): aquí van los bytes del trabajo |
| `FF03` | Notificación: respuestas de la impresora |
| MTU | `MaxPduSize` 200 en Windows |

Control de flujo en `FF03` (no son respuestas de estado):

- `02 nn …`: **tamaño de paquete** (la T02 manda `02 B6 00` ⇒ 182 bytes; por defecto 182).
- `01 nn`: la impresora **concede nn tokens**. Llega `01 01` al suscribirse y otro tras cada
  paquete.
- `1A 36 nn`: **paquetes por token** (`01` ⇒ 5; por defecto 1). No observado en la T02.

`BleTransport` manda trozos de `min(tamaño anunciado, MaxPduSize - 3)` y espera un token cada
«paquetes por token» trozos. Si no llega un token en 400 ms, pasa a pausas fijas de 20 ms.
`BleTransport.Trace` permite ver la conexión y las notificaciones.

Detalles de Windows: tras cerrar, Windows mantiene un rato la conexión y la impresora **deja de
anunciarse** (la búsqueda no la encuentra, pero conectar por dirección funciona). Si no se
libera el `GattDeviceService`, la conexión siguiente no recibe notificaciones; `BleTransport` lo
libera y reintenta el descubrimiento hasta 3 veces.

**El aviso de fin `1A 0F 0C` es poco fiable** (también lo notan en phomemo-tools): llega unos
2,5 s después de enviar, a veces tarde (dentro del trabajo siguiente) o no llega.
`PhomemoPrinter` solo lo cuenta si llega después de terminar de enviar, y si no llega mantiene la
conexión al menos 4 s (o 0,5 s por KB).

Bluetooth clásico (SPP, UUID `1101`) no se ha probado con la T02. **Detectado:** la conexión
SPP puede hacerse **sin emparejar** (socket RFCOMM inseguro), lo que explica que en otros
sistemas tampoco pida PIN; la T02 sería de modo dual (SPP + BLE). **Detectado:** al conectar
por BLE se pide MTU 185 (182 + 3), que encaja con el tamaño de paquete anunciado.

## Conceptos comunes

- Todas usan el comando ESC/POS **`GS v 0`** para la imagen:
  `1D 76 30 mm  xL xH  yL yH  <datos>`
  - `mm` = modo: 0 normal, 1 doble ancho, 2 doble alto, 3 cuádruple. Siempre 0.
  - `xL xH` = bytes por línea (16 bits little-endian) = `ceil(ancho / 8)`.
  - `yL yH` = número de líneas de este bloque (16 bits LE).
  - Datos: 1 bit por punto, **MSB primero**, **1 = negro** (punto quemado), fila a fila.
- Punto cuadrado: resolución horizontal = vertical (203 dpi = 8 puntos/mm; 300 dpi ≈ 11,8 puntos/mm).
- El ancho que se envía (`xL xH`) es el ancho del cabezal del modelo. Si la imagen es más
  estrecha se rellena a la derecha con blanco.
- Por USB aparece un **puerto serie CDC-ACM** (COM en Windows, `/dev/ttyACM*` en Linux,
  `/dev/cu.usbmodem*` en macOS). Por Bluetooth clásico, un puerto SPP/RFCOMM. Por Bluetooth LE,
  la característica GATT `FF02` (ver arriba). El protocolo es el mismo byte a byte, **pero la
  T02 solo imprime por BLE**. La velocidad configurada no importa en CDC-ACM; se usa 128000
  baudios (valor de `serial.ts`) por si algún adaptador la respeta.
- **No cerrar la conexión en cuanto se termina de escribir**: el backend CUPS espera la
  respuesta de la impresora (hasta 8 s) porque si se cierra antes "se para la impresión".

## Familia M02 (M02, M02 Pro, M02S, T02)

ESC/POS "plano". Cabecera ampliada que manda `M02Protocol`, **probada en la T02 por Bluetooth**:

| Paso | Bytes | Significado |
|---|---|---|
| Cabecera | `1B 40` | ESC @: inicializar |
| | `1F 11 02 nn` | Densidad (`PrintOptions.Darkness`; si no, la del modelo: **3 en la T02**, probada como la mejor; 4 en el resto) |
| | `1F 11 37 nn` | Coeficiente de calor (`PrintOptions.HeatCoefficient`): 100 (0x64) por defecto; 150 (0x96) más negro |
| | `1F 11 0B` | Papel en rollo (`1F 11 26` con marcas) |
| | `1F 11 35 00` | Datos sin comprimir |
| Imagen | `GS v 0` en bloques de **máx. 255 líneas** | Si hay más, se encadenan bloques |
| Pie | `1B 64 nn` | ESC d: imprimir y avanzar n líneas |

La cabecera corta anterior (`1B 40`, `1B 61 00`, `1F 11 37 64`, `1F 11 02 04`) también imprime.

**Sin papel o con la tapa abierta la impresora acepta el trabajo en silencio** (no manda error ni
`1A 0F 0C`). Por eso, antes de imprimir se preguntan `1F 11 11` (papel) y `1F 11 12` (tapa) en la
misma conexión (`PhomemoPrinter.QueryReadyAsync`) y no se envía el trabajo si contesta `1A 06 88` o
`1A 05 99`. Con la tapa abierta el papel también sale como agotado: se avisa primero de la tapa.

phomemo-tools añade al pie las consultas `1F 11 08/0E/07/09`; aquí ya no se mandan. Al
terminar, la impresora responde `1A 0F 0C` (ver «aviso de fin» más arriba).

El avance tras imprimir para poder cortar es 2 líneas en M02 y 4 en T02 (opción `FeedLines` de CUPS).

### Respuestas `1A tt …`

`PrinterMessages.Parse` las interpreta. Columna «T02»: visto en las pruebas.

| Respuesta | Significado | T02 |
|---|---|---|
| `1A 03 A8` / `A9` | Temperatura normal / cabezal recalentado | `A8` |
| `1A 04 nn` | Batería en %. `A1`/`A2`/`A3`: batería baja (10, 5, 3 %) | sí |
| `1A 05 98` / `99` | Tapa cerrada / abierta | `98` al cerrar la tapa |
| `1A 06 88` / `89` | Sin papel / hay papel | sí |
| `1A 07 a b c` | Versión del firmware | `1.0.0` |
| `1A 08 …` | Número de serie en ASCII (15 caracteres) | sí |
| `1A 09 n` | Apagado automático: n × 5 minutos | `06` (30 min) |
| `1A 0B B8` | Impresión cancelada desde la impresora | — |
| `1A 0E B8` / `B9` | Cuchilla pulsada / suelta | — |
| `1A 0F 0C` | Trabajo terminado | sí (poco fiable) |
| `1A 16 nn` / `1A 17 nn` | Tipo de chip: 3 o 7 = JieLi, el resto normal | `1A 17 03` |
| `1A 1D nn` | 0 = libre, otro = ocupada | — |
| `1A 3B d0 d1 d2 d3` | Capacidades: bit 2 de d0 = escala de grises (2^(d1>>4) niveles); bit 3 de d1 = doble dpi; bit 0 de d0 = varias densidades | no responde |
| `1A 49 01 nn` | Salud de la batería | — |

### Consultas

| Comando | Respuesta | T02 |
|---|---|---|
| `1F 11 08` | Batería (`1A 04`) | sí |
| `1F 11 07` | Firmware (`1A 07`) | sí |
| `1F 11 09` | Número de serie (`1A 08`) | sí |
| `1F 11 0E` | Apagado automático (`1A 09`) | sí |
| `1F 11 11` | Papel (`1A 06`) | sí |
| `1F 11 13` | Temperatura (`1A 03`) | sí |
| `1F 11 38` | Tipo de chip (`1A 16`/`1A 17`) | sí |
| `1F 11 63` | Capacidades (`1A 3B`) | sin respuesta ⇒ se asume sin escala de grises |
| `1F 11 12` | Tapa (`1A 05 98` cerrada / `99` abierta) | sí |

Por USB, una consulta repetida (`1F 11 07`) dejó de contestar hasta reiniciar la impresora; por
Bluetooth no ha pasado.

### Otros comandos conocidos (sin probar: **no mandarlos a ciegas**)

| Comando | Uso |
|---|---|
| `1F 11 14` + tamaño (+ CRC) | **Inicia una actualización de firmware.** `1F 11 15`/`16` confirman o rechazan el CRC. La ventana de diagnóstico bloquea `1F 11 14` |
| `1B 4E 04 n` | Densidad |
| `1B 4E 07 n` | Apagado automático (n × 5 min) |
| `1F 11 35 01` | Activa la compresión: los datos del bloque `GS v 0` van en trozos LZO de 4096 bytes |
| `1F 11 5F` / `60` / `61` | Actualización de firmware de la pantalla (modelos con pantalla) |
| `1F 11 71 nn`, `1F 11 88 nn`, `1F 11 3C 00`, `1F 11 40`, `1F 11 41`, `1F 11 2E/2F/3E/45/46/47`, `1B 4E 17 00`, `1B 4E 1A 00` | Sin identificar |

## Familia M110 (M110, M120, M220, M421)

| Paso | Bytes | Significado |
|---|---|---|
| Cabecera | `1B 4E 0D vv` | Velocidad 1 (lenta) … 5 (rápida) |
| | `1B 4E 04 dd` | Densidad 1 … 15 |
| | `1F 11 tt` | Tipo de papel: `0A` etiqueta con huecos, `0B` continuo, `26` con marcas |
| Imagen | **un único** `GS v 0` con toda la altura (16 bits) | |
| Pie | `1F F0 05 00`, `1F F0 03 00` | Sin documentar |

En esta familia el ancho imprimible depende de la etiqueta cargada, no es fijo.

## Otras familias (menos probadas)

- **D30** (`rastertopd30.py`): cabecera `1F 11 24 00` + `1B 40`; un solo `GS v 0`; la imagen
  va **girada 90°** (la etiqueta es estrecha y se imprime "de lado"); el avance se hace mandando
  líneas en blanco, no con ESC d.
- **M04S / M04AS** (`rastertopm04.py`): `1F 11 02 dd` densidad, `1F 11 37 hh` calor/velocidad,
  `1F 11 0B` continuo, `1F 11 35 00` sin compresión; **un único bloque** (ignora bloques
  encadenados); avance `1B 64 06`. Buffer pequeño: enviar en trozos de 256 bytes con pausas de
  20 ms y mantener la conexión abierta `max(8 s, bytes/1000 s)`.
- **M832** (`rastertopm832.py`): `1F 11 tt` tipo de papel, `1F 11 02 dd` densidad opcional,
  `1F 11 35 00`; un único bloque; avance `1B 64 01` por página y `1B 64 02` al final;
  consulta de estado `1F 11 11`.

## Tabla de modelos

| Modelo | Familia | dpi | Ancho (puntos) | Bytes/línea | Fiabilidad del dato |
|---|---|---|---|---|---|
| T02 | M02 | 203 | 384 (48 mm) | 48 | Documentado en ambas fuentes y probado |
| M02 | M02 | 203 | 384 (48 mm) | 48 | Documentado en ambas fuentes |
| M02 Pro | M02 | 300 | 576 (48,8 mm) | 72 | Detectado (sin probar) |
| M02S | M02 | 300 | 576 (48,8 mm) | 72 | Detectado (sin probar) |
| M110 / M120 | M110 | 203 | según etiqueta (máx. 384 = 48 mm) | variable | Ancho máx. por verificar |
| M220 | M110 | 203 | según etiqueta (máx. ~576 = 72 mm) | variable | **Por verificar** |
| M421 | M110 | 203 | según etiqueta (4"×6" ⇒ ~812) | variable | **Por verificar** |
| D30 | D30 | 203 | 96 (12 mm, imagen girada) | 12 | **Por verificar** |
| M04S / M04AS | M04 | 300 | 590 (50 mm) o 1232 (110 mm) | 74 / 154 | Documentado en el `.drv`; detectado como ancho máximo 1228 |
| M832 | M832 | 300 | 1299 (110 mm) | 163 | Documentado (probado con 110 mm) |

### Otros modelos detectados (en el catálogo, sin probar)

Detectado, **sin probar**. Comparten el juego de comandos `1F 11 xx` y respuestas `1A …` de la
familia M02 y, en general, la secuencia de cabecera ampliada (densidad, coeficiente, papel,
compresión); algunos del grupo A4 que van por wifi o solo por BLE tienen variaciones no
estudiadas. «Comp.» = trabajan con la compresión LZO (`1F 11 35 01`), que esta aplicación no
implementa: aparecen en el catálogo marcados «requiere compresión» y no se puede imprimir en
ellos. Los A4 sin compresión usan el protocolo M832 (un solo bloque, datos sin comprimir); los
M03 y P3100, el M02. La M832 se deja imprimible porque phomemo-tools la usa sin compresión.

| Grupo | Modelos | dpi | Ancho (puntos) | Notas |
|---|---|---|---|---|
| M02 (203 dpi) | M02C, M02A, M02D, M02E, M02X, M02L, M02X/L, MR2, KP-Q1, Q02, Y02C | 203 | 384 (48 mm) | Como la T02 |
| M02 (300 dpi) | M02PRE, M02S-H, M02H, Y02S | 300 | 576 (48,8 mm) | Como M02S |
| M03 | M03, M03A, TP31 | 203 | 576 (72 mm) | |
| M03 (300 dpi) | M03S, M03AS | 300 | 864 (73 mm) | |
| M04 | M04S, M04AS | 300 | 1228 (104 mm) | |
| M04 (203 dpi) | M04AS-H | 203 | 832 (104 mm) | |
| P3100 | P3100, P3100D, P3100J, P3100DJ, P1000, P1000C | 203 | 82 (10 mm) | Etiquetas; avance final de 4 líneas |
| A4 (203 dpi) | M08F, M08D, M08F-WS, A884, A88U, A28U, TP81, TP82, TP83, PR20, PR20Lite, S821, S821K, S823, T831 | 203 | 1728 (216 mm) | Varios con comp. |
| A4 (300 dpi) | M831-M835, M832T, H831-H833, P831, P832, Q300-Q302, R831, G100, G100Pro, A281M, A282M, A285M | 300 | 2551 (216 mm) | Casi todos con comp. G100: prioridad `1B 4E 62 …` |

Otros comportamientos detectados (sin probar):

- Densidad en 4 niveles: nivel 1 ⇒ `1F 11 02 01`, 2 ⇒ `02`, 3 ⇒ `04`, 4 ⇒ `04` con coeficiente
  `1F 11 37 96` (150) en vez de `64`.
- Papel: `1F 11 0B` rollo continuo, `1F 11 26` con marcas, `1F 11 38` otro tipo de etiqueta.
- Si no llega el aviso de fin, se da el trabajo por terminado tras unos 110 ms por línea impresa
  (por un factor del modelo).
- Al conectar se piden, por este orden, `1F 11 38`, `07`, `63`, `5E`, `09` y, en modelos con
  batería, `08` y `0E`.

## Identificación del modelo (autodetección)

- **USB VID/PID**: el backend CUPS usa `0493:B002` ⇒ M02 y `0493:8760` ⇒ M110. La T02 probada
  aparece como `4C4A:4155` («Dispositivo serie USB», chip JieLi), así que el VID/PID no es
  fiable: cambia entre lotes/chips.
- **Nombre Bluetooth**: `Mr.in_<modelo>` (p. ej. `Mr.in_M02`), `T02`, `M832…`. Es lo que
  identifica a la T02 en la búsqueda BLE.
- **Número de serie** (`1F 11 09`): los 4 primeros caracteres indican el tipo de impresora.
  Prefijos detectados (solo se ha probado uno de los de la T02):

  | Prefijo | Modelo |
  |---|---|
  | `M001`, `Q096` | M02 |
  | `Q003`, `Q021`, `Q071`, `Q075` | M02S |
  | `Q005`, `Q070` | M02 Pro |
  | `Q022` | M02E |
  | `Q123` | M02X |
  | `Q127`, `Q167` | M02D |
  | `Q020`, `Q095`, `Q114` | T02 |
  | `Q008` | M03 |
  | `Q016` | M04S |
  | `Q032`, `Q078` | P3100 |
  | `Q051` | P3100D |

- **Modelos derivados** (detectado): `T02E`, `Q02E` y `C02E` se tratan como T02.

Implementado: tabla VID/PID en `PrinterCatalog.UsbIds` + nombre Bluetooth + último modelo
usado en el puerto + prefijo del número de serie (`PrinterCatalog.SerialPrefixes`). El número de
serie solo se pide a la impresora seleccionada: la búsqueda (anuncios BLE y registro de puertos
COM) no manda nada a ningún dispositivo. Si la misma impresora aparece por USB y por Bluetooth, se elige Bluetooth.
El usuario siempre puede elegir a mano.

### Registro de pruebas

Todas con una T02.

| Fecha | Conexión | Enviado | Respuesta | Resultado |
|---|---|---|---|---|
| 2026-10-29 | USB | `1F 11 07`, `08`, `09`, `0E`, `11` (128000 baudios, DTR/RTS activos) | ninguna en 2,4 s | La impresora estaba en reposo (por USB el LED no lo distingue). |
| 2026-10-29 | USB | `1B 40`, `1F 11 08/0E/07/09/11/13/0F`, `10 04 01`, `1D 49 01` | `08`: nada; `0E`: `1A 09 06`; `07`: `1A 07 01 00 00`; `09`: `1A 08` + n.º de serie; `11`: `1A 06 89`; `13`: `1A 03 A8`; resto nada | `1B 40` contesta a veces `1A 0F 0C`. |
| 2026-10-29 | USB | Trabajos de imagen y texto con varias cabeceras | `1A 0F 0C`, `1A 04 64` | **Avanza el papel pero sale en blanco** en todas las variantes. |
| 2026-10-29 | BLE `FF02` | Cabecera M02 con `1F 11 37 64`, barras de 6 mm | `02 B6 00`, `01 01` por trozo, `1A 0F 0C`, `1A 06 88` | **Imprime.** |
| 2026-10-29 | BLE, `PhomemoPrinter` + `BleTransport` | Marco + diagonales, 384 × 120, 5784 bytes | `1A 06 89 1A 0F 0C` en ~4 s | Terminado confirmado. |
| 2026-10-29 | BLE, control por tokens | Ajedrez 384 × 64 y dos trabajos de 384 × 32 seguidos | `02 B6 00`, `01 01` por paquete de 182; el `1A 0F 0C` del primero llegó dentro del segundo | Imprime. Fin poco fiable ⇒ espera mínima de 4 s; con ella, los dos confirmados. `1A 05 98` al cerrar la tapa tras cambiar el rollo. |
| 2026-10-29 | USB | `1F 11 38`, `1F 11 63` y cabecera ampliada (`1F 11 0B`, `1F 11 35 00`) | `1A 17 03`; nada; nada | **Solo avanza el margen final**: la imagen se ignora. USB descartado para imprimir en la T02. |
| 2026-10-29 | BLE | `1F 11 08`, `38`, `63`, `07` | `1A 04 46`, `1A 17 03`, nada, `1A 07 01 00 00` | Chip JieLi; sin respuesta de capacidades. |
| 2026-10-29 | BLE, desde PhomemoPaint | `1F 11 09`, `07`, `08`, `11` al seleccionar la impresora | n.º de serie, firmware, batería, papel | Leído en ~2 s; el prefijo del n.º de serie identifica la T02. |
| 2026-10-29 | BLE | Cabecera ampliada; 4 tiras con texto, rampa de gris tramada y cuadros 0-100 %: densidad 1, 2 y 4 con coeficiente 100, y densidad 4 con 150 | `1A 0F 0C` en las cuatro | Imprime. Resultado visual: ver puntos abiertos. |
| 2026-10-29 | BLE | Misma tira con densidad 3, coeficiente 100 (`1F 11 02 03`) | `1A 0F 0C` | Imprime. **3/100 da el mejor tramado** (2/100 y 4/100 también correctos; 1/100 y 4/150 peores). |
| 2026-10-29 | BLE, sin papel | Consulta `1F 11 09/07/08/11` y después un trabajo de 384 × 32 | `1A 06 88` (sin papel), batería `1A 04 64`; al trabajo, **nada** (ni `1A 0F 0C`) | La impresora acepta el trabajo sin avisar: hay que preguntar por el papel antes de imprimir. |
| 2026-10-29 | BLE, tapa abierta | `1F 11 11`, `1F 11 12`, `1F 11 08` y un trabajo de 384 × 32 | `1A 06 88`; `1A 05 99`; `1A 04 64` (y `1A 05 99` espontáneo); al trabajo, nada propio | `1F 11 12` identificado como consulta de la tapa. El trabajo se acepta en silencio. |
