# Diseño De Despliegue En Railway

## Objetivo

Dejar `paginatintas` preparado para una mini producción en Railway sin migrar MySQL a PostgreSQL y sin incluir credenciales reales en el repositorio.

La implementación del repositorio dejará lista la aplicación, pero el owner todavía tendrá que crear los servicios de Railway, proporcionar las credenciales y configurar el dominio. Esas acciones no se pueden completar de forma segura desde el código.

## Resumen Para El Owner

El repositorio ya contiene la configuración de despliegue. El owner no debe modificar código para publicar la aplicación. Debe completar, en este orden:

1. Subir la rama `railway-deploy` a GitHub o fusionarla a `main`.
2. Crear un proyecto Railway con un servicio web y un servicio MySQL.
3. Configurar las variables del servicio web.
4. Desplegar y revisar los logs de build y migraciones.
5. Preparar los roles, usuarios y datos de negocio.
6. Generar el dominio temporal o conectar el dominio real.
7. Ejecutar las pruebas funcionales finales.

Si la base de datos está vacía, la aplicación crea las tablas mediante sus migraciones, pero no crea automáticamente roles, usuarios, fórmulas, tintas ni inventario.

## Requisitos Antes De Empezar

El owner debe tener:

- Acceso de escritura al repositorio `https://github.com/marianita13/paginatintas`.
- Una cuenta Railway con permisos para crear servicios y variables.
- Una tarjeta o método de pago si el plan seleccionado lo requiere.
- Acceso al proveedor DNS del dominio, si se usará un dominio propio.
- Un dump de la base local, si se conservarán datos existentes.
- Los datos del primer usuario administrador.
- Una nueva clave JWT y una nueva contraseña para MySQL.

## Arquitectura Final

Se usará un solo servicio web para API y frontend. El frontend se copiará dentro de la imagen de ASP.NET Core como `wwwroot`. De esta forma ambos quedan bajo el mismo dominio y el frontend puede consumir la API mediante `/api`.

```text
Navegador
   |
   v
Servicio web de Railway
   |- Frontend estático: /
   |- API ASP.NET Core: /api/*
   `- Healthcheck: /health
   |
   v
Servicio MySQL de Railway
```

El servicio MySQL será independiente dentro del mismo proyecto Railway y no debe exponerse públicamente para que la API se conecte a él por la red privada del proyecto.

## Qué Dejará Hecho El Repositorio

- `Dockerfile` multi-stage basado en .NET 9.
- Restauración y publicación de `API/API.csproj` desde la raíz de la solución.
- Copia del contenido de `Frontend/` a `wwwroot` dentro de la imagen final.
- Servido de `index.html`, CSS, JavaScript e imágenes desde la API.
- Endpoint público `/health` para que Railway pueda comprobar que el proceso está vivo.
- Configuración de Railway apuntando al Dockerfile correcto.
- Configuración del puerto proporcionado por Railway mediante `PORT`.
- URL del frontend configurada como `/api` en lugar de `localhost`.
- Exclusión de artefactos `bin`, `obj`, secretos y archivos innecesarios mediante `.dockerignore`.
- Variables de entorno documentadas, sin valores confidenciales.
- Configuración JSON sin contraseñas reales ni clave JWT real.
- Proyecto `API.Tests` con una prueba del endpoint `/health`.
- Eliminación de artefactos compilados `bin/obj` del repositorio.
- Guía `RAILWAY.md` con todos los pasos que debe ejecutar el owner.
- Mantención de MySQL, Pomelo y las migraciones EF Core actuales.

### Cambios Por Archivo

| Archivo o carpeta | Cambio realizado | Para qué sirve |
|---|---|---|
| `Dockerfile` | Compila la solución con el SDK .NET 9 y crea la imagen final con ASP.NET Core 9 | Railway puede construir y ejecutar la API |
| `railway.json` | Apunta al Dockerfile, define reinicios y `/health` | Railway sabe cómo desplegar y comprobar el servicio |
| `API/Program.cs` | Habilita archivos estáticos y agrega `/health` | El mismo proceso sirve frontend, API y healthcheck |
| `Frontend/script.js` | Cambia la API de `localhost` a `/api` | El navegador usa el dominio real de Railway |
| `API/appsettings*.json` | Sustituye valores reales por placeholders | No se publican secretos; Railway los inyecta por entorno |
| `API.Tests/` | Agrega prueba del endpoint `/health` | Deja una comprobación automatizada del despliegue |
| `.dockerignore` | Excluye archivos innecesarios del contexto Docker | Reduce el contexto y evita copiar artefactos locales |
| `.gitignore` | Ignora `bin`, `obj` y archivos `.env` | Evita volver a versionar secretos o compilados |
| `bin/` y `obj/` | Se eliminan del repositorio | Evita subir DLLs, configuraciones generadas y secretos antiguos |
| `docs/RAILWAY.md` | Documenta el procedimiento de entrega | El owner puede completar Railway sin adivinar pasos |

## Qué Debe Hacer El Owner Después

### 1. Acceso Al Código

El owner debe tener acceso al repositorio GitHub `marianita13/paginatintas` y subir la rama `railway-deploy`, o fusionarla a `main` antes de conectar Railway.

Railway debe conectarse al repositorio correcto y desplegar exactamente la rama que contiene esta preparación.

Si la rama todavía existe solo en el equipo local, el responsable puede publicarla con:

```bash
git switch railway-deploy
```

Después debe comprobar en GitHub que aparecen `Dockerfile`, `railway.json`, `API.Tests/` y `docs/RAILWAY.md` antes de conectar Railway.

### 2. Crear El Proyecto Railway

En Railway, el owner debe:

1. Crear un proyecto nuevo.
2. Añadir un servicio desde GitHub usando este repositorio.
3. Seleccionar la rama desplegable, inicialmente `railway-deploy`.
4. Confirmar que Railway detecta el `Dockerfile` de la raíz.
5. Añadir un servicio MySQL desde `New` o desde el template oficial.
6. Nombrar el servicio de base de datos `mysql` para que las referencias documentadas sean claras.
7. Nombrar el servicio web `web-api`, o sustituir `web-api` por el nombre real en las instrucciones de variables.

El servicio web debe usar la raíz del repositorio como contexto. No se debe configurar un comando alternativo de build o start: Railway debe usar el `Dockerfile` de la raíz.

Railway requiere Dockerfile para aplicaciones ASP.NET Core porque Railpack todavía no construye proyectos .NET automáticamente.

El servicio MySQL de Railway proporciona variables como `MYSQLHOST`, `MYSQLPORT`, `MYSQLUSER`, `MYSQLPASSWORD`, `MYSQLDATABASE` y `MYSQL_URL`.

### Resultado Esperado De Este Paso

Al terminar, Railway debe mostrar dos servicios dentro del mismo proyecto:

```text
web-api   Servicio que construye el Dockerfile y sirve la aplicación
mysql     Servicio privado que almacena la base de datos
```

El nombre `mysql` es importante porque se usa en las referencias de variables del siguiente paso.

### 3. Configurar La Conexión MySQL

En las variables del servicio API, el owner debe definir `ConnectionStrings__ConexMysql` usando las variables privadas del servicio MySQL. La forma recomendada es:

```text
Server=${{mysql.MYSQLHOST}};Port=${{mysql.MYSQLPORT}};Database=${{mysql.MYSQLDATABASE}};User ID=${{mysql.MYSQLUSER}};Password=${{mysql.MYSQLPASSWORD}};SslMode=Preferred;
```

Esta variable debe crearse en `web-api`, en la sección `Variables`. La expresión `${{mysql.NOMBRE}}` le indica a Railway que tome el valor privado desde el servicio `mysql`.

No se debe pegar directamente `MYSQL_URL` en `ConnectionStrings__ConexMysql` sin convertirlo. La aplicación usa el formato de conexión ADO.NET que aparece arriba, mientras que `MYSQL_URL` puede entregarse como una URL.

No se debe usar el usuario `root` de la instalación local ni publicar el puerto TCP de MySQL salvo que sea estrictamente necesario para importar datos.

### 4. Configurar La Autenticación

El owner debe completar en Railway estas variables del servicio API:

```text
JWT__Key=<clave aleatoria larga y privada>
JWT__Issuer=PaginatintasApi
JWT__Audience=PaginatintasApiUser
JWT__DurationInMinutes=20
```

`JWT__Key` debe ser una clave nueva para producción. No debe reutilizarse la clave que existía en los archivos de desarrollo.

### Nota Sobre El Historial Git

Los valores antiguos estuvieron incluidos en archivos generados dentro de commits anteriores. Esta preparación los elimina del árbol actual y evita que vuelvan a publicarse, pero no borra automáticamente el historial Git remoto.

Antes de usar producción, el owner debe:

1. Cambiar la contraseña de la base de datos.
2. Generar una nueva clave JWT.
3. Invalidar cualquier token de sesión que dependa de la clave anterior.
4. Evaluar con el administrador del repositorio si también se requiere limpiar el historial remoto.

### 5. Configurar El Entorno Y El Puerto

El owner debe definir:

```text
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:${PORT}
```

`PORT` lo proporciona Railway. No se debe inventar ni fijar un puerto público distinto. La variable `ASPNETCORE_URLS` hace que Kestrel escuche en todas las interfaces del contenedor y en el puerto que Railway asigna.

Después de modificar variables, Railway requiere revisar y desplegar los cambios staged antes de que lleguen al servicio activo.

### 6. Preparar La Base De Datos

El owner debe decidir si la base estará vacía o si se importarán datos existentes.

Para una base vacía:

1. Crear el servicio MySQL.
2. Configurar la variable de conexión en la API.
3. Desplegar la API.
4. Confirmar en los logs que las migraciones EF Core terminaron sin error.
5. Crear los roles requeridos.
6. Crear el usuario administrador inicial.

Las migraciones crean el esquema, pero no incluyen un seeder. Por eso una base vacía no contiene automáticamente los roles `Administrador`, `Operarios` y `Admin2`, ni contiene usuarios.

El registro de usuarios del proyecto requiere un `IdRol`. El owner debe crear primero los roles mediante un cliente MySQL autorizado o importar una base que ya los contenga. Después puede registrar el primer usuario mediante el procedimiento controlado del proyecto usando `POST /api/Usuario/register` con un cuerpo equivalente a:

Si la tabla `Rol` está vacía, puede ejecutar una instrucción equivalente a esta desde el cliente MySQL autorizado:

```sql
INSERT INTO `Rol` (`Nombre`)
VALUES ('Administrador'), ('Operarios'), ('Admin2');

SELECT `Id`, `Nombre` FROM `Rol`;
```

Si la tabla ya tiene roles, no ejecute el `INSERT`; consulte los IDs existentes y use el ID del rol `Administrador`.

```json
{
  "nombre": "NOMBRE_DEL_ADMINISTRADOR",
  "correo": "CORREO_DEL_ADMINISTRADOR",
  "password": "CONTRASENA_PROVISIONAL",
  "idRol": 1
}
```

El `idRol` del ejemplo es únicamente ilustrativo. Antes de registrar el usuario, el owner debe consultar la tabla `Rol` y usar el ID que corresponda exactamente al rol `Administrador`.

El endpoint `POST /api/Usuario/register` no exige autenticación en el código actual. Debe usarse únicamente durante una inicialización controlada por el owner y no debe convertirse en un formulario público de registro sin una modificación posterior de autenticación.

Después del registro, el owner debe comprobar que el login funciona y conservar las credenciales del administrador en un lugar seguro.

Para datos existentes:

1. Generar un dump de la base local.
2. Importarlo en MySQL de Railway usando una conexión temporal segura.
3. Confirmar tablas, usuarios, fórmulas, tintas, inventario y órdenes.
4. Ejecutar el despliegue de la API.
5. Confirmar en los logs la versión de migración aplicada.
6. Revisar que las migraciones pendientes no alteren negativamente el esquema importado.

La implementación no incluirá datos de negocio ni un seeder porque esos datos pertenecen al owner.

### 7. Configurar El Dominio

Railway entrega inicialmente un dominio temporal para verificar el despliegue. Para usar el dominio real, el owner debe:

1. Añadir el dominio en la configuración del servicio web.
2. Crear en el proveedor DNS el registro solicitado por Railway, normalmente un `CNAME`.
3. Esperar la propagación DNS.
4. Confirmar que Railway emite el certificado TLS.
5. Abrir el dominio con HTTPS y comprobar que carga el frontend.

Si el dominio todavía no está disponible, el dominio temporal de Railway permite validar la aplicación antes de cambiar DNS.

### Resultado Esperado De Este Paso

Al terminar, abrir el dominio debe mostrar `index.html` por HTTPS. Abrir `/health` debe devolver una respuesta HTTP `200` con un cuerpo similar a:

```json
{
  "status": "ok"
}
```

### 8. Validación Funcional Final

El owner debe comprobar, desde el dominio público:

- La página inicial carga con HTTPS.
- El login funciona.
- Se crea y conserva la sesión JWT.
- El refresh token permite renovar la sesión.
- Se cargan los Pantones.
- Se consultan las fórmulas.
- Se calcula una mezcla.
- Se consultan y actualizan empresas.
- Se consultan y actualizan inventarios.
- Se crea una orden.
- Se actualizan cajas y estado de una orden.
- Los permisos por rol funcionan.
- La aplicación sigue respondiendo después de reiniciar el servicio.
- `/health` responde correctamente.

## Criterio De Finalización

La publicación se considera terminada únicamente cuando se cumplen todos estos puntos:

- Railway muestra el build del servicio web como exitoso.
- El deployment está activo y tiene un dominio accesible.
- `/health` devuelve HTTP 200.
- La página carga sin errores de recursos por `localhost`.
- La API conecta con MySQL sin errores de migración.
- Existe al menos un rol `Administrador`.
- Existe al menos un usuario administrador que puede iniciar sesión.
- Se validó una lectura y una escritura en la base de datos.
- Se confirmó que los Pantones remotos cargan desde GitHub.
- Se realizó un backup de la base de producción.

## Variables Finales Que Debe Entregar El Owner

Estas variables son las únicas piezas confidenciales o dependientes de la cuenta que quedan para el final:

```text
ConnectionStrings__ConexMysql
JWT__Key
JWT__Issuer
JWT__Audience
JWT__DurationInMinutes
ASPNETCORE_ENVIRONMENT
ASPNETCORE_URLS
```

También debe entregar o configurar fuera del repositorio:

- Acceso al proyecto Railway.
- Acceso al repositorio GitHub.
- Dominio y proveedor DNS.
- Backup de la base local, si se conservarán datos existentes.
- Usuario administrador inicial.
- Política de backups y restauración.

### Tabla De Variables

| Variable | Dónde se configura | Qué debe contener |
|---|---|---|
| `ConnectionStrings__ConexMysql` | Variables de `web-api` | La cadena ADO.NET construida con las variables privadas de `mysql` |
| `JWT__Key` | Variables de `web-api` | Clave aleatoria larga y privada para producción |
| `JWT__Issuer` | Variables de `web-api` | `PaginatintasApi` |
| `JWT__Audience` | Variables de `web-api` | `PaginatintasApiUser` |
| `JWT__DurationInMinutes` | Variables de `web-api` | `20`, o el valor aprobado por el owner |
| `ASPNETCORE_ENVIRONMENT` | Variables de `web-api` | `Production` |
| `ASPNETCORE_URLS` | Variables de `web-api` | `http://+:${PORT}` |
| `PORT` | La proporciona Railway | No crearla ni modificarla manualmente |
| `MYSQLHOST`, `MYSQLPORT`, `MYSQLUSER`, `MYSQLPASSWORD`, `MYSQLDATABASE` | Las proporciona el servicio `mysql` | No copiarlas a mano; se referencian desde `ConnectionStrings__ConexMysql` |

Railway aplica los cambios de variables como cambios staged. Después de guardar variables, el owner debe revisar y desplegar esos cambios; de lo contrario, el servicio puede seguir usando la configuración anterior.

## Verificación De La Implementación

- Restaurar y compilar la solución en Release.
- Publicar `API/API.csproj` en Release.
- Construir la imagen Docker si Docker está disponible.
- Confirmar que la imagen contiene `API.dll` y el frontend.
- Confirmar que Railway apunta al Dockerfile y a `/health`.
- Confirmar que no quedan contraseñas ni claves JWT reales en archivos versionados.
- Confirmar que la guía permite al owner completar el despliegue sin modificar el código manualmente, salvo proporcionar valores de entorno y datos propios.

La verificación local de `dotnet test`, `dotnet publish` y `docker build` depende de disponer del SDK .NET 9 y del daemon Docker. Si el entorno local no los tiene, el primer build de Railway debe tratarse como una verificación de integración y sus logs deben revisarse antes de abrir el dominio al público.

## Fuera Del Alcance

- Migración de MySQL a PostgreSQL.
- Importación automática de datos de negocio.
- Creación automática de usuarios administrativos.
- Compra o configuración del dominio desde el repositorio.
- Creación de la cuenta Railway.
- CI/CD adicional.
- Rediseño de autenticación o lógica de negocio.
