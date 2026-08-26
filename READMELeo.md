# TestDevBackMiddle

Prueba técnica de desarrollo Backend Middle realizada con **ASP.NET Core 8**, **Entity Framework Core 8** y **SQL Server 2019**.

El proyecto implementa una API REST para administrar registros de login y logout de usuarios, consultas SQL para obtener estadísticas sobre el tiempo de conexión y un endpoint para generar un archivo CSV con las horas trabajadas por usuario.

---

## Tecnologías utilizadas

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core 8
- SQL Server 2019
- Docker
- SQL
- Postman

---

# Estructura del proyecto

```text
TestDevBackMiddle/
│
├── Controllers/
│   ├── LoginsController.cs
│   └── ReportsController.cs
│
├── Data/
│   └── AppDbContext.cs
│
├── Models/
│   ├── User.cs
│   ├── Login.cs
│   └── Area.cs
│
├── Migrations/
│
├── Sql/
│   ├── seed-data.sql
│   ├── seed-areas.sql
│   ├── mayor-tiempo-logueado.sql
│   ├── menor-tiempo-logueado.sql
│   └── promedio-logueo-mensual.sql
│
├── appsettings.json
├── Program.cs
├── TestDevBackMiddle.csproj
└── README.md
```

---

# Base de datos

La aplicación utiliza la base de datos:

```text
CCenterRIA
```

Las tablas utilizadas son:

### ccUsers

Contiene la información de los usuarios.

Campos principales:

```text
User_id
Login
Nombres
ApellidoPaterno
ApellidoMaterno
IDArea
```

### ccloglogin

Contiene los movimientos de inicio y cierre de sesión.

```text
Id
User_id
Extension
TipoMov
fecha
```

Donde:

```text
TipoMov = 1 → Login
TipoMov = 0 → Logout
```

Se agregó el campo `Id` como identificador único para poder realizar correctamente las operaciones de actualización y eliminación desde la API.

### ccRIACat_Areas

Contiene el catálogo de áreas.

```text
Id
IDArea
AreaName
StatusArea
CreateDate
```

Se utiliza un `Id` interno como llave primaria debido a que los datos proporcionados contienen valores repetidos en `IDArea`.

---

# Requisitos

Para ejecutar el proyecto es necesario tener instalado:

- .NET SDK 8
- Docker Desktop
- Entity Framework Core CLI
- Git

Comprobar .NET:

```bash
dotnet --version
```

El proyecto fue desarrollado utilizando .NET 8.

Comprobar Docker:

```bash
docker --version
```

Comprobar Entity Framework:

```bash
dotnet ef --version
```

En caso de no tener las herramientas de EF Core 8:

```bash
dotnet tool install --global dotnet-ef --version 8.*
```

---

# 1. Levantar SQL Server con Docker

Con Docker Desktop iniciado, ejecutar:

```bash
docker run -e 'ACCEPT_EULA=Y' \
-e 'MSSQL_SA_PASSWORD=YourStrong!Passw0rd' \
-p 1433:1433 \
--name sqlserver \
-d mcr.microsoft.com/mssql/server:2019-latest
```

Comprobar que el contenedor se encuentre activo:

```bash
docker ps
```

Debe aparecer un contenedor similar a:

```text
IMAGE                                        PORTS
mcr.microsoft.com/mssql/server:2019-latest   0.0.0.0:1433->1433/tcp
```

Si el contenedor ya existe pero está detenido:

```bash
docker start sqlserver
```

---

# 2. Configuración de conexión

La conexión a SQL Server se encuentra configurada en:

```text
appsettings.json
```

La configuración utilizada durante el desarrollo apunta al SQL Server ejecutado mediante Docker en:

```text
localhost:1433
```

Las credenciales deben coincidir con las utilizadas al crear el contenedor.

---

# 3. Restaurar dependencias

Desde la raíz del proyecto ejecutar:

```bash
dotnet restore
```

Después comprobar que el proyecto compile correctamente:

```bash
dotnet build
```

El resultado esperado es:

```text
Compilación correcta.
0 Advertencia(s)
0 Errores
```

---

# 4. Crear la base de datos

El proyecto utiliza **Entity Framework Core Migrations** para crear la estructura de la base de datos.

Ejecutar:

```bash
dotnet ef database update
```

Entity Framework creará automáticamente:

```text
CCenterRIA
```

junto con las tablas necesarias.

---

# 5. Cargar los datos iniciales

Los datos entregados originalmente para la prueba fueron preparados en scripts SQL para facilitar la reproducción del proyecto.

## Cargar usuarios y movimientos

Copiar el script al contenedor:

```bash
docker cp Sql/seed-data.sql sqlserver:/tmp/seed-data.sql
```

Ejecutarlo:

```bash
MSYS_NO_PATHCONV=1 docker exec -it sqlserver /opt/mssql-tools18/bin/sqlcmd \
-S localhost \
-U sa \
-P 'YourStrong!Passw0rd' \
-C \
-d CCenterRIA \
-i /tmp/seed-data.sql
```

> `MSYS_NO_PATHCONV=1` es necesario cuando el comando se ejecuta desde Git Bash en Windows para evitar que Git Bash transforme `/opt/...` en una ruta de Windows.

Después de cargar los datos se deben tener:

```text
TotalUsers
-----------
137

TotalLogins
-----------
10000
```

## Cargar áreas

Copiar:

```bash
docker cp Sql/seed-areas.sql sqlserver:/tmp/seed-areas.sql
```

Ejecutar:

```bash
MSYS_NO_PATHCONV=1 docker exec -it sqlserver /opt/mssql-tools18/bin/sqlcmd \
-S localhost \
-U sa \
-P 'YourStrong!Passw0rd' \
-C \
-d CCenterRIA \
-i /tmp/seed-areas.sql
```

Esto carga los registros del catálogo de áreas utilizados por el reporte CSV.

---

# 6. Ejecutar la API

Desde la raíz del proyecto:

```bash
dotnet run
```

Durante el desarrollo la aplicación se ejecutó en:

```text
http://localhost:5293
```

El puerto puede cambiar dependiendo de la configuración local mostrada por `dotnet run`.

---

# Ejercicio 1 - API REST

Se implementó un CRUD para administrar los movimientos de login y logout almacenados en `ccloglogin`.

Los endpoints son:

```text
GET     /logins
POST    /logins
PUT     /logins/{id}
DELETE  /logins/{id}
```

---

## GET /logins

Obtiene los registros de login/logout.

### Ejemplo

```http
GET http://localhost:5293/logins
```

No requiere body.

La respuesta contiene los movimientos almacenados en `ccloglogin`.

---

## POST /logins

Permite registrar un nuevo login o logout.

### Ejemplo de login

```http
POST http://localhost:5293/logins
Content-Type: application/json
```

Body:

```json
{
  "userId": 70,
  "extension": 10,
  "tipoMov": 1,
  "fecha": "2026-08-27T15:30:00"
}
```

Donde:

```text
tipoMov = 1 → Login
tipoMov = 0 → Logout
```

Si el registro es válido se obtiene:

```text
201 Created
```

---

# Validaciones del POST

El endpoint valida diferentes escenarios antes de almacenar un movimiento.

## Usuario inexistente

Ejemplo:

```json
{
  "userId": 999999,
  "extension": 10,
  "tipoMov": 1,
  "fecha": "2026-08-27T15:30:00"
}
```

Resultado esperado:

```text
400 Bad Request
```

con un mensaje indicando que el usuario no existe.

---

## TipoMov inválido

Solo se aceptan:

```text
0
1
```

Por ejemplo:

```json
{
  "userId": 70,
  "extension": 10,
  "tipoMov": 5,
  "fecha": "2026-08-27T15:30:00"
}
```

Resultado:

```text
400 Bad Request
```

---

## Login consecutivo

Si el último movimiento del usuario ya es:

```text
TipoMov = 1
```

no se permite registrar otro login.

Ejemplo:

```text
LOGIN
LOGIN ❌
```

La API responde:

```text
400 Bad Request
```

con:

```text
El usuario ya tiene una sesión abierta.
```

---

## Logout sin login previo

No se permite:

```text
LOGOUT
LOGOUT ❌
```

ni registrar un logout si el usuario no tiene una sesión abierta.

La API responde:

```text
400 Bad Request
```

---

## Validación de fecha

Un movimiento nuevo debe tener una fecha posterior al último movimiento registrado para ese usuario.

Por ejemplo, si el último movimiento fue:

```text
2026-08-26 15:30
```

esto será rechazado:

```json
{
  "userId": 70,
  "extension": 10,
  "tipoMov": 1,
  "fecha": "2026-08-25T15:30:00"
}
```

Resultado:

```text
400 Bad Request
```

con:

```text
La fecha del movimiento debe ser posterior al último movimiento registrado.
```

---

# PUT /logins/{id}

Permite modificar un movimiento existente.

Ejemplo:

```http
PUT http://localhost:5293/logins/10001
Content-Type: application/json
```

Body:

```json
{
  "userId": 70,
  "extension": 25,
  "tipoMov": 0,
  "fecha": "2026-08-26T16:00:00"
}
```

Si existe:

```text
200 OK
```

Si no existe:

```text
404 Not Found
```

Además se valida que la modificación no genere una secuencia inválida de movimientos con el registro anterior o siguiente.

Por ejemplo, no se debe generar:

```text
LOGIN
LOGIN
LOGOUT
```

---

# DELETE /logins/{id}

Elimina un movimiento.

Ejemplo:

```http
DELETE http://localhost:5293/logins/10001
```

Si existe:

```text
200 OK
```

Respuesta:

```text
El registro con ID 10001 fue eliminado correctamente.
```

Si no existe:

```text
404 Not Found
```

Respuesta:

```text
El registro con ID 10001 no existe.
```

---

# Ejercicio 2 - Consultas SQL

Las consultas solicitadas se encuentran separadas en archivos para facilitar su ejecución y revisión.

```text
Sql/
├── mayor-tiempo-logueado.sql
├── menor-tiempo-logueado.sql
└── promedio-logueo-mensual.sql
```

Las sesiones se calculan emparejando:

```text
TipoMov = 1 → Login
TipoMov = 0 → Logout
```

y obteniendo la diferencia entre ambas fechas mediante `DATEDIFF`.

---

# Usuario con mayor tiempo logueado

Archivo:

```text
Sql/mayor-tiempo-logueado.sql
```

Para probarlo:

```bash
docker cp Sql/mayor-tiempo-logueado.sql sqlserver:/tmp/mayor-tiempo-logueado.sql
```

Después:

```bash
MSYS_NO_PATHCONV=1 docker exec -it sqlserver /opt/mssql-tools18/bin/sqlcmd \
-S localhost \
-U sa \
-P 'YourStrong!Passw0rd' \
-C \
-d CCenterRIA \
-i /tmp/mayor-tiempo-logueado.sql
```

Con los datos proporcionados el resultado esperado es:

```text
User_id: 92
Tiempo total: 361 días, 12 horas, 51 minutos, 8 segundos
```

---

# Usuario con menor tiempo logueado

Archivo:

```text
Sql/menor-tiempo-logueado.sql
```

Copiar:

```bash
docker cp Sql/menor-tiempo-logueado.sql sqlserver:/tmp/menor-tiempo-logueado.sql
```

Ejecutar:

```bash
MSYS_NO_PATHCONV=1 docker exec -it sqlserver /opt/mssql-tools18/bin/sqlcmd \
-S localhost \
-U sa \
-P 'YourStrong!Passw0rd' \
-C \
-d CCenterRIA \
-i /tmp/menor-tiempo-logueado.sql
```

Resultado esperado:

```text
User_id: 90
Tiempo total: 244 días, 0 horas, 43 minutos, 15 segundos
```

---

# Promedio de logueo por usuario y mes

Archivo:

```text
Sql/promedio-logueo-mensual.sql
```

Copiar:

```bash
docker cp Sql/promedio-logueo-mensual.sql sqlserver:/tmp/promedio-logueo-mensual.sql
```

Ejecutar:

```bash
MSYS_NO_PATHCONV=1 docker exec -it sqlserver /opt/mssql-tools18/bin/sqlcmd \
-S localhost \
-U sa \
-P 'YourStrong!Passw0rd' \
-C \
-d CCenterRIA \
-i /tmp/promedio-logueo-mensual.sql
```

La consulta devuelve:

```text
User_id
Año
Mes
PromedioSegundos
PromedioTiempo
```

Como comprobación, para:

```text
Usuario: 70
Año: 2023
Mes: Enero
```

el resultado esperado es:

```text
3 días, 14 horas, 1 minuto, 16 segundos
```

---

# Ejercicio 3 - Generación de CSV

Se implementó un endpoint que genera un reporte CSV con las horas trabajadas por usuario.

Endpoint:

```http
GET /reports/worked-hours/csv
```

El reporte contiene:

```text
Login
Nombre completo
Área
Total de horas trabajadas
```

---

## Probar desde Postman

Iniciar primero la API:

```bash
dotnet run
```

Después realizar:

```http
GET http://localhost:5293/reports/worked-hours/csv
```

No requiere body.

La API responde con:

```text
200 OK
```

y genera:

```text
horas-trabajadas.csv
```

En Postman también puede utilizarse:

```text
Send and Download
```

para guardar directamente el archivo.

---

# Contenido del CSV

Ejemplo de estructura:

```csv
Login,NombreCompleto,Area,TotalHoras
usuario1,Juan Perez Lopez,Default,123.50
usuario2,Maria Garcia Ruiz,Default,98.25
```

El nombre completo se construye utilizando:

```text
Nombres
ApellidoPaterno
ApellidoMaterno
```

Los espacios adicionales son eliminados antes de generar el reporte.

El área se obtiene relacionando:

```text
ccUsers.IDArea
```

con:

```text
ccRIACat_Areas.IDArea
```

El total de horas se calcula recorriendo cronológicamente los movimientos de cada usuario y sumando cada pareja:

```text
LOGIN → LOGOUT
```

---

# Reiniciar completamente la base de datos

Durante las pruebas de los endpoints `POST`, `PUT` y `DELETE` se modifican los datos.

Si se desea regresar al estado inicial antes de ejecutar nuevamente las consultas SQL, se puede reconstruir completamente la base.

## 1. Eliminar la base

```bash
dotnet ef database drop -f
```

## 2. Ejecutar nuevamente las migraciones

```bash
dotnet ef database update
```

Esto vuelve a crear:

```text
CCenterRIA
```

y todas sus tablas.

## 3. Cargar nuevamente los datos

```bash
docker cp Sql/seed-data.sql sqlserver:/tmp/seed-data.sql
```

```bash
MSYS_NO_PATHCONV=1 docker exec -it sqlserver /opt/mssql-tools18/bin/sqlcmd \
-S localhost \
-U sa \
-P 'YourStrong!Passw0rd' \
-C \
-d CCenterRIA \
-i /tmp/seed-data.sql
```

Después cargar las áreas:

```bash
docker cp Sql/seed-areas.sql sqlserver:/tmp/seed-areas.sql
```

```bash
MSYS_NO_PATHCONV=1 docker exec -it sqlserver /opt/mssql-tools18/bin/sqlcmd \
-S localhost \
-U sa \
-P 'YourStrong!Passw0rd' \
-C \
-d CCenterRIA \
-i /tmp/seed-areas.sql
```

La base queda nuevamente con los datos originales.

---

# Comprobación rápida del proyecto

Una forma rápida de comprobar toda la solución es seguir este orden:

### 1. Comprobar Docker

```bash
docker ps
```

### 2. Comprobar compilación

```bash
dotnet build
```

### 3. Comprobar migraciones

```bash
dotnet ef database update
```

### 4. Ejecutar API

```bash
dotnet run
```

### 5. Probar CRUD

```text
GET    /logins
POST   /logins
PUT    /logins/{id}
DELETE /logins/{id}
```

### 6. Probar consultas SQL

```text
mayor-tiempo-logueado.sql
menor-tiempo-logueado.sql
promedio-logueo-mensual.sql
```

### 7. Probar CSV

```text
GET /reports/worked-hours/csv
```

---

# Resultados de referencia

Con los datos proporcionados para la prueba se comprobaron los siguientes resultados:

```text
Usuario con mayor tiempo:
User_id 92
361 días, 12 horas, 51 minutos, 8 segundos

Usuario con menor tiempo:
User_id 90
244 días, 0 horas, 43 minutos, 15 segundos

Promedio usuario 70 - Enero 2023:
3 días, 14 horas, 1 minuto, 16 segundos
```

Estos valores permiten comprobar rápidamente que el emparejamiento de login/logout y los cálculos de tiempo se están realizando correctamente.

---

# Consideraciones

Los movimientos se ordenan por fecha antes de calcular las sesiones.

Una sesión válida está formada por:

```text
Login (TipoMov = 1)
        ↓
Logout (TipoMov = 0)
```

La API contiene validaciones para evitar registros consecutivos incompatibles y fechas inválidas.

Para las pruebas de `POST`, `PUT` y `DELETE` se recomienda utilizar registros creados específicamente para pruebas y posteriormente restaurar la base antes de comprobar las consultas del Ejercicio 2.

Los archivos SQL se mantuvieron separados para que cada ejercicio pueda revisarse y ejecutarse individualmente.

---

# Ejecución resumida

Para una instalación desde cero:

```bash
# Restaurar proyecto
dotnet restore

# Compilar
dotnet build

# Crear base de datos
dotnet ef database update

# Cargar usuarios y movimientos
docker cp Sql/seed-data.sql sqlserver:/tmp/seed-data.sql

MSYS_NO_PATHCONV=1 docker exec -it sqlserver /opt/mssql-tools18/bin/sqlcmd \
-S localhost \
-U sa \
-P 'YourStrong!Passw0rd' \
-C \
-d CCenterRIA \
-i /tmp/seed-data.sql

# Cargar áreas
docker cp Sql/seed-areas.sql sqlserver:/tmp/seed-areas.sql

MSYS_NO_PATHCONV=1 docker exec -it sqlserver /opt/mssql-tools18/bin/sqlcmd \
-S localhost \
-U sa \
-P 'YourStrong!Passw0rd' \
-C \
-d CCenterRIA \
-i /tmp/seed-areas.sql

# Ejecutar API
dotnet run
```

Después la API estará lista para probar los endpoints y generar el reporte CSV.