# PhomemoPaint

Editor de dibujo en blanco y negro para Windows que imprime directamente en impresoras
térmicas **Phomemo** por Bluetooth LE o por puerto serie (USB o Bluetooth clásico), sin drivers
y sin emparejar.

> La **T02** actual solo imprime por **Bluetooth**: por USB responde pero sale en blanco.
> PhomemoPaint la encuentra sola al abrirse (no aparece en el buscador de Bluetooth de Windows
> porque es Bluetooth LE).

- Lienzo con el ancho exacto del cabezal de la impresora (T02: 384 puntos = 48 mm) y el alto
  que tú elijas; regla en milímetros en los bordes.
- Pincel blanco o negro con tamaño ajustable.
- Pegar o insertar imágenes (PNG, JPG, BMP, GIF) y moverlas, escalarlas y girarlas con los
  tiradores; se pasan a blanco y negro con tramado o umbral.
- Solo se imprime hasta la última línea dibujada, para no gastar papel.
- Busca impresoras Bluetooth y detecta el modelo por su nombre o por el identificador USB del
  puerto; avisa si el ancho cambia.
- Muestra lo que dice la impresora: terminado, batería, sin papel, tapa abierta…
- Patrón de calibración y diagnóstico de señales para estudiar la impresora.

Modelos: T02, M02, M02 Pro, M02S, M110, M120, M220, M421 y, de forma experimental, D30,
M04S/M04AS y M832. Solo el T02 se ha probado con hardware real; ver
[DOC/protocolos.md](DOC/protocolos.md).

## Compilar desde fuentes

Abre PowerShell y ejecuta:

```powershell
irm https://raw.githubusercontent.com/XWolfOverride/PhomemoPaint/main/build.ps1 | iex
```

**Para actualizar, repite el mismo comando.**

El script comprueba los requisitos (no instala nada; si falta algo te dice el comando exacto),
descarga o actualiza el código y deja `PhomemoPaint.exe` en la carpeta del proyecto.
Para compilar un fork: `$env:PHOMEMOPAINT_REPO_URL = "https://github.com/<usuario>/PhomemoPaint"`.

Requisitos:

- Windows 10 (versión 2004 o posterior) u 11, x64 o ARM64. Para Bluetooth, un adaptador
  Bluetooth 4.0 o posterior.
- git: `winget install Git.Git`
- SDK de .NET 10: `winget install Microsoft.DotNet.SDK.10`

El `.exe` es un único fichero (unos 25 MB) pero necesita el runtime de escritorio de .NET 10
(incluido en el SDK). Las aplicaciones WinForms no admiten Native AOT.

### Instalarlo en el PATH

Sin permisos de administrador:

```powershell
mkdir $HOME\bin -Force | Out-Null
copy PhomemoPaint.exe $HOME\bin
$p = [Environment]::GetEnvironmentVariable("Path", "User")
[Environment]::SetEnvironmentVariable("Path", "$p;$HOME\bin", "User")
```

## Idioma

Español o inglés según el idioma de Windows. Para forzarlo: `PHOMEMOPAINT_LANG=es` o `en`.

## Documentación técnica

- [DOC/arquitectura.md](DOC/arquitectura.md): proyectos y clases.
- [DOC/protocolos.md](DOC/protocolos.md): bytes de cada familia de impresoras.
- [DOC/editor.md](DOC/editor.md): comportamiento del editor.
- [DOC/compilacion-y-pruebas.md](DOC/compilacion-y-pruebas.md): compilar y probar.
- [DOC/puntos-abiertos.md](DOC/puntos-abiertos.md): decisiones y pendientes.
- [AGENTS.md](AGENTS.md): contexto para agentes de IA.

## Agradecimientos

El protocolo se ha tomado de [vivier/phomemo-tools](https://github.com/vivier/phomemo-tools)
y [mitchelloharawild/phomemo-protocol](https://github.com/mitchelloharawild/phomemo-protocol),
completado con pruebas en una T02. No se ha copiado su código.

## Licencia

MIT. © XWolfOverride.
