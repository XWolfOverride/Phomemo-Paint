# Comportamiento del editor

## Ventana principal

- **Menús**: Archivo (Nuevo, Abrir PNG como lienzo, Insertar imagen, Exportar PNG), Editar
  (Deshacer, Rehacer, Pegar imagen, Fijar imagen, Fijar todas, Eliminar imagen), Ver (zoom) e
  Impresora (Imprimir, Detectar modelo, Patrón de calibración, Diagnóstico de señales).
- **Barra de impresora**: puerto o impresora Bluetooth + ⟳ (recarga los COM y busca por
  Bluetooth), modelo, ancho de etiqueta en mm (solo en modelos de la familia M110), alto del
  lienzo en puntos e Imprimir.
- **Barra de herramientas**: Negro / Blanco, tamaño de pincel (1-64), zoom (×1-×8), Insertar /
  Fijar / Eliminar imagen y conversión a B/N de la imagen seleccionada (Tramado / Umbral).
- **Barra de estado**: posición del ratón en puntos y mm, alto que se imprimiría, mensajes
  (modelo detectado, resultado de la impresión) y progreso.
- Se puede arrastrar un fichero de imagen a la ventana para insertarlo.

## Lienzo

- **Ancho** = ancho del cabezal del modelo (T02: 384). En la familia M110, el ancho de la
  etiqueta elegida, con tope en el cabezal.
- **Alto** = campo numérico en puntos; por defecto 400, máximo 20000. Al reducirlo se avisa
  si hay dibujo en la parte que se pierde; al aumentarlo se añade blanco.
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
  arrastrar y soltar.
- Se insertan centradas en horizontal, arriba de la zona visible, reducidas al ancho del
  lienzo si son más anchas.
- **Clic dentro** de una imagen: seleccionarla y moverla. **Clic fuera**: deseleccionar y
  pintar (el pincel pinta por debajo de las imágenes flotantes).
- Tiradores de la seleccionada:
  - 4 esquinas: escalar manteniendo proporción (Mayús: libre).
  - 4 lados: escalar en un eje.
  - Círculo de arriba: girar (Mayús: pasos de 15°).
- Se guarda el original y se convierte a B/N al renderizar con la transformación actual: escalar
  o girar varias veces no degrada. Las zonas transparentes (alfa < 128) dejan ver lo de debajo;
  el resto tapa (también el blanco).
- **Intro**: fijar en la capa de píxeles. **Supr**: eliminar. **Esc**: deseleccionar.
- Lo que sobresale del lienzo no se imprime; el contorno completo se dibuja punteado.

## Qué se imprime

- Siempre todo el ancho.
- En vertical, desde la fila 0 (se respeta el margen superior que se deje en blanco) hasta la
  última fila con algún punto negro, incluidas las imágenes flotantes.
- Lienzo vacío: no se imprime y se avisa.

## Cambio de modelo, ancho de etiqueta o puerto

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
- Si el ancho cambia y hay algo dibujado se pide confirmación indicando si se recortará la
  derecha o se añadirá blanco. **Nunca se escala.** Si se cancela, se vuelve al modelo anterior.

## Imprimir

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

## Para más adelante

Herramienta de texto; densidad y otros parámetros de impresión; márgenes por defecto tras
calibrar.
