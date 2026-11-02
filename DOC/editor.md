# Comportamiento del editor

## Ventana principal

- **Menús**: Archivo (Nuevo, Abrir PNG como lienzo, Insertar imagen, Exportar PNG), Editar
  (Deshacer, Rehacer, Pegar imagen, Fijar imagen, Fijar todas, Eliminar imagen), Ver (zoom) e
  Impresora (Imprimir, Detectar modelo, Patrón de calibración, Diagnóstico de señales).
- Sin barras de herramientas: solo el menú, el lienzo, la columna lateral y la barra de estado.
- **Columna lateral** (derecha, como la caja de herramientas de GIMP), de arriba abajo:
  - **Herramientas**: botonera con iconos (pincel, insertar imagen, pegar, texto, cursor, fijar, eliminar; los
    dos últimos solo con una imagen seleccionada) y, algo separado y más grande, **Imprimir**
    (durante la impresión pasa a Cancelar). Debajo, muestras de color negro / blanco y tamaño de
    pincel (1-64).
  - **Propiedades** de la selección: conversión a B/N (Tramado / Umbral) en las imágenes; en los
    textos, contenido, fuente, tamaño, estilo y alineación; sin selección, un texto que lo explica.
  - **Impresora**: puerto o impresora Bluetooth + ⟳ (recarga los COM y busca por Bluetooth),
    modelo, ancho de etiqueta en mm (solo en la familia M110) y los datos de la impresora.
  - **Zoom** (abajo del todo): alejar, ×1-×8, acercar y ajustar al ancho.
- **Barra de estado**: posición del ratón en puntos y mm, alto que se imprimiría, mensajes
  (modelo detectado, resultado de la impresión) y progreso. **Aviso amarillo** (sin ventanas): lo último
  que se ha recortado (alto o ancho; hasta la siguiente edición) o que una imagen sobresale del
  papel.
- **Panel de la impresora** (derecha): modelo, conexión, dirección (MAC o puerto y VID/PID),
  número de serie, firmware, batería, papel, tapa y hora de la última lectura, con un botón de actualizar.
- Se puede arrastrar un fichero de imagen a la ventana para insertarlo.
- Icono `Phome.ico` en el ejecutable y en las ventanas.
- **Ayuda** (menú Ayuda y F1): páginas HTML en español e inglés que se abren en el navegador. F1 abre la
  sección de la zona que hay bajo el ratón (lienzo, herramientas, propiedades, impresora, zoom; en el
  diagnóstico, la suya). Los iconos de la ayuda son los mismos que los de los botones (fuente Segoe MDL2 Assets). La sección «Partes de la ventana» lleva un esquema SVG incrustado en cada página, con sus zonas enlazadas; si cambia la ventana hay que actualizarlo en los dos idiomas.

## Lienzo

- **Ancho** = ancho del cabezal del modelo (T02: 384). En la familia M110, el ancho de la
  etiqueta elegida, con tope en el cabezal.
- **Alto**: se cambia arrastrando la **marca de fin del lienzo** (línea azul con asa en el borde
  inferior) o con el **campo numérico sin etiqueta que flota a la derecha del asa** y se mueve con
  ella (seguido de «pt · mm»); por defecto 400, máximo
  20000. El arrastre ajusta a milímetros enteros (Mayús: punto a punto); mientras se arrastra, lo
  que se añadiría se ve como papel a medias y lo que se recortaría en rojo. Al soltar pasa lo mismo
  que con el campo: se aplica sin preguntar y, si se pierde dibujo, lo dice el aviso amarillo de
  la barra de estado; Ctrl+Z lo deshace.
- Bajo el lienzo hay **una pantalla de margen sin papel** (gris): con el desplazamiento al fondo,
  la marca de fin queda justo bajo la regla, así que siempre se puede arrastrar hacia abajo.
- Fondo blanco; solo existen blanco y negro.
- Zoom entero sin suavizado. Al abrir se elige el mayor que cabe a lo ancho. Ctrl+rueda
  cambia el zoom.
- **Reglas** en mm arriba y a la izquierda, siempre visibles al desplazarse, con la posición
  del ratón marcada en rojo.
- **Línea naranja discontinua**: dónde acaba lo que se imprimiría.

## Pincel

- Botón izquierdo: color activo; botón derecho: el contrario.
- Pincel redondo de N puntos; entre eventos de ratón se pinta el segmento entero.
- El círculo azul indica el tamaño del pincel bajo el ratón.

## Imágenes flotantes

- Origen: portapapeles (Ctrl+V, también una lista de ficheros copiada), fichero (Ctrl+I) o
  arrastrar y soltar. Al pegar se usa primero la copia PNG que dejan los navegadores y otros
  programas, que conserva la transparencia; el mapa de bits de Windows no la lleva (lo
  transparente saldría negro).
- Se insertan centradas en horizontal, arriba de la zona visible, reducidas al ancho del
  lienzo si son más anchas.
- **Clic dentro** de una imagen: seleccionarla y moverla. **Clic fuera**: deseleccionar y
  pintar.
- Tiradores de la seleccionada:
  - 4 esquinas: escalar manteniendo proporción (Mayús: libre).
  - 4 lados: escalar en un eje.
  - Círculo de arriba: girar (Mayús: pasos de 15°).
- Se guarda el original y se convierte a B/N al renderizar con la transformación actual: escalar
  o girar varias veces no degrada. La transparencia también se trama: con «Tramado» la opacidad
  se difunde como el gris (un degradado de transparencia sale como puntos cada vez más
  espaciados); con «Umbral», alfa < 50 % es transparente. Lo opaco tapa (también el blanco).
- **Intro**: fijar en la capa de píxeles. **Supr**: eliminar. **Esc**: deseleccionar.
- Lo que sobresale del lienzo no se imprime: se dibuja atenuado (35 % de opacidad) sobre el
  fondo gris, con el contorno punteado, para que se vea y se pueda recolocar.

## Qué se imprime

- Siempre todo el ancho.
- En vertical, desde la fila 0 (se respeta el margen superior que se deje en blanco) hasta la
  última fila con algún punto negro, incluidas las imágenes flotantes.
- Lienzo vacío: no se imprime y se avisa.

## Cambio de modelo, ancho de etiqueta o puerto

- La lista de modelos incluye los detectados sin probar («no verificado, experimental»). Los que
  trabajan con datos comprimidos (LZO) aparecen como «requiere compresión, no imprime»: se
  pueden elegir para dibujar con su ancho, pero se muestra ⚠ junto al modelo, el aviso en la
  barra de estado, e Imprimir (botón y menú) queda desactivado.

- La lista tiene los puertos COM y las impresoras Bluetooth LE («T02 — Bluetooth
  AA:BB:…»). Al abrir la ventana se buscan impresoras Bluetooth durante 5 s (también con ⟳);
  las encontradas se recuerdan y aparecen en la lista aunque no se haya buscado aún. No hace
  falta emparejarlas en Windows.
- Preferencia al arrancar: el último puerto usado; si no está, una impresora Bluetooth
  reconocida; si no, el primer puerto reconocido. Si la impresora seleccionada es un COM y la
  búsqueda encuentra el mismo modelo por Bluetooth, se cambia a Bluetooth (la T02 no imprime
  por USB) y se avisa en la barra de estado.
- Al elegir un puerto se busca su VID/PID USB o su nombre Bluetooth en la tabla de modelos
  conocidos; si no se reconoce se usa el último modelo elegido para ese puerto. El usuario
  puede cambiarlo.
- Al seleccionar una impresora se le piden número de serie, firmware, batería, papel y tapa (solo a
  la seleccionada; buscar no manda nada). Si el prefijo del número de serie indica otro modelo,
  se cambia a ese (con el aviso de ancho si hay dibujo). La batería se vuelve a leer cada 5
  minutos, salvo si la impresora está ocupada; imprimir, consultar y el diagnóstico nunca usan
  la conexión a la vez.
- Si el ancho cambia, se recorta o se añade blanco a la derecha sin preguntar (**nunca se
  escala**); si se ha recortado dibujo, lo dice el aviso amarillo de la barra de estado y Ctrl+Z
  lo deshace.

## Imprimir

- Antes de enviar se pregunta a la impresora por el papel y la tapa: si no tiene papel o la tapa está
  abierta, se avisa y no se imprime (la impresora no lo diría por sí sola).
- En segundo plano; el botón pasa a «Cancelar» y hay barra de progreso.
- Tras enviar se espera a que la impresora diga «terminado» (`1A 0F 0C`) antes de cerrar.
- La barra de estado muestra las líneas impresas y la respuesta interpretada (terminado,
  batería, SIN PAPEL, TAPA ABIERTA…). Si la impresora no confirma, dice «Enviado… sin
  confirmación» y, si era un puerto COM, recuerda que la T02 solo imprime por Bluetooth.

## Diagnóstico de señales

Manda líneas de bytes en hexadecimal (lo que va tras `#` es comentario) por la conexión
elegida, también Bluetooth, y muestra la respuesta en hex, ASCII e interpretada. Viene con las
consultas conocidas. `1F 11 14` (actualizar firmware) está bloqueado.

## Archivos

- **Exportar PNG**: lo que se imprimiría, en PNG de 1 bit.
- **Abrir PNG como lienzo**: sustituye el lienzo; el alto pasa a ser el de la imagen; si es más
  ancha que el lienzo se recorta la derecha (con aviso). Se convierte con umbral.

## Ajustes

`%APPDATA%\PhomemoPaint\settings.json`: último puerto, último modelo, modelo por puerto,
impresoras Bluetooth conocidas (dirección → nombre), alto del lienzo, ancho de etiqueta y
tamaño del pincel.

## Herramientas

| Herramienta | Tecla | Clic en el lienzo |
|---|---|---|
| Cursor | `V` | Selecciona y mueve objetos; en un sitio libre, deselecciona. Nunca pinta |
| Pincel | `B` | Siempre pinta, también encima de los objetos, que no se mueven |
| Texto | `T`, `Ctrl+T` | En un texto u otro objeto, lo selecciona; en un sitio libre, crea un texto |

Las letras solo actúan con el lienzo activo. Al añadir una imagen se pasa al cursor. La marca de fin
del lienzo se arrastra con cualquier herramienta.

## Dibujos a pincel

Los trazos no van a la capa de píxeles, sino a un objeto flotante que guarda los puntos.

- **Un objeto por dibujo.** El grupo sigue creciendo mientras se use el pincel y se cierra al cambiar
  de herramienta, con `Esc` o al fijarlo. No hay tiempo de espera: una pausa para pensar no debe
  partir un dibujo. Al pasar al cursor, el grupo queda seleccionado.
- **Solo se mueve.** Sin tiradores: ni escala ni giro.
- `Intro` fija y `Supr` elimina el grupo abierto aunque no esté seleccionado.
- `Ctrl+Z` quita el último trazo y el grupo sigue abierto.
- El fondo es transparente y el blanco es opaco: al fijarlo tapa lo que haya debajo.
- Pintar pegado al borde no avisa de que sobresale: se cuentan los puntos, no el grosor.

## Texto

Herramienta en la barra (`T` o `Ctrl+T`). Con ella, un clic en un sitio libre crea un texto de
ejemplo seleccionado y lleva el foco al cuadro de Propiedades; un clic fuera solo termina de editar,
para no llenar el lienzo de textos. La herramienta sigue activa hasta elegir otra.

- Es un objeto flotante más: se mueve, gira, fija, elimina y deshace como una imagen.
- Va dentro de un cuadro. Las líneas se parten por palabras al llegar a su ancho (por letras si una
  palabra no cabe) y las alineaciones se refieren a él. Al colocarlo, llega hasta el borde del papel.
- Los ocho tiradores cambian el cuadro, sin proporción y sin tocar la fuente. El alto nunca baja del
  que ocupa el texto, así no se pierde nada de lo escrito; si se cambia solo el ancho, el borde
  superior se queda quieto y el texto crece hacia abajo.
- Se pinta como trazado vectorial y umbral (`TextContent.BuildPath`), así que sale nítido con
  cualquier tamaño y giro. Las fuentes muy pequeñas pierden detalle: el cabezal tiene 8 puntos/mm.
- El tamaño va en puntos tipográficos convertidos con la resolución del modelo.
- Al escribir se queda quieto el borde superior. Escribir seguido es un paso de deshacer; cada
  cambio de formato, otro.
- Las muestras de color cambian el color del texto seleccionado.
- La fuente, el tamaño y el estilo elegidos se usan en los textos siguientes (no se guardan al
  cerrar). Un texto vacío se elimina al deseleccionarlo.
- En el cuadro, `Ctrl+V`, `Ctrl+Z` y `Ctrl+Y` son los suyos y `Esc` vuelve al lienzo.

## Para más adelante

Densidad y otros parámetros de impresión; márgenes por defecto tras
calibrar.
