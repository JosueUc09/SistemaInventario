# Sistema de Inventario Escolar — Avalonia + Supabase

Aplicación de escritorio en C# y Avalonia. Se instala en varias computadoras y todas trabajan con el mismo inventario alojado en Supabase. No es una aplicación web, pero requiere conexión a Internet para iniciar sesión y consultar o modificar datos.

## Seguridad incluida

- Supabase Auth para correo y contraseña.
- Data API protegida con Row Level Security (RLS).
- La aplicación solo contiene la `Publishable key`, diseñada para clientes públicos.
- La clave `service_role` nunca se copia al programa ni a `appsettings.json`.
- Una Edge Function crea y edita cuentas con privilegios administrativos.
- Triggers de PostgreSQL generan la auditoría automáticamente.

## Requisitos

- Una cuenta de Supabase.
- .NET 10 SDK en la computadora de desarrollo.
- Supabase CLI para desplegar la función desde la terminal.
- Internet en las computadoras donde se usará el sistema.

## Configuración de Supabase paso a paso

### 1. Crear el proyecto

1. Entre a <https://supabase.com/dashboard> e inicie sesión.
2. Seleccione **New project**.
3. Elija la organización, escriba un nombre como `inventario-escolar` y genere una contraseña segura para PostgreSQL.
4. Seleccione una región cercana a Costa Rica y cree el proyecto.
5. Guarde la contraseña en un administrador de contraseñas. La aplicación Avalonia no la utiliza.

### 2. Crear tablas, datos iniciales, auditoría y políticas RLS

1. En el panel del proyecto abra **SQL Editor**.
2. Seleccione **New query**.
3. Abra `supabase/01_schema_rls.sql`, copie todo su contenido y péguelo en el editor.
4. Presione **Run** y verifique que finalice sin errores.
5. En **Table Editor** deben aparecer: `roles`, `perfiles`, `categorias`, `ubicaciones`, `estados_activo`, `activos`, `mantenimientos` y `auditorias`.

El script activa RLS. No desactive estas políticas para corregir errores de permisos; eso expondría los datos.

### 3. Crear el primer administrador

1. Abra **Authentication > Users**.
2. Seleccione **Add user > Create new user**.
3. Escriba el correo y una contraseña de al menos 8 caracteres; marque el correo como confirmado si el panel lo solicita.
4. Copie el UUID del usuario recién creado.
5. Regrese a **SQL Editor > New query** y ejecute, sustituyendo los tres valores:

```sql
insert into public.perfiles (id, nombre, correo, role_id)
select
  'UUID-DEL-USUARIO'::uuid,
  'Nombre del administrador',
  'correo@ejemplo.com',
  id
from public.roles
where nombre = 'Administrador';
```

6. En **Table Editor > perfiles** confirme que existe el registro y que `activo` es `true`.

Este paso manual solo se hace una vez. Luego el administrador podrá crear otras cuentas desde la aplicación.

### 4. Instalar y autenticar Supabase CLI

Consulte la instalación oficial en <https://supabase.com/docs/guides/cli/getting-started> y compruebe:

```powershell
supabase --version
supabase login
```

El navegador solicitará autorización para la CLI.

### 5. Vincular el proyecto y desplegar la función de usuarios

1. En Supabase abra **Project Settings** y copie el **Reference ID** del proyecto.
2. Abra PowerShell en la carpeta `InventarioEscolarAvalonia`.
3. Ejecute:

```powershell
supabase link --project-ref SU_REFERENCE_ID
supabase functions deploy create-user
```

Supabase proporciona automáticamente a la función `SUPABASE_URL` y `SUPABASE_SERVICE_ROLE_KEY`. No copie esa clave en la aplicación. Verifique el despliegue en **Edge Functions > create-user**.

### 6. Obtener la URL y la clave pública

1. Abra **Project Settings > API Keys** (en algunas versiones aparece bajo **Settings > API**).
2. Copie **Project URL**.
3. Copie la **Publishable key** (`sb_publishable_...`). Si su proyecto todavía muestra claves antiguas, puede usar temporalmente la clave `anon`, nunca `service_role`.
4. Edite `appsettings.json`:

```json
{
  "Supabase": {
    "Url": "https://SU_REFERENCE_ID.supabase.co",
    "PublishableKey": "sb_publishable_SU_CLAVE"
  }
}
```

La clave publicable puede formar parte de una aplicación de escritorio porque la autorización real la aplican Auth y RLS. La clave secreta o `service_role` sí debe mantenerse privada.

### 7. Ejecutar y probar

```powershell
dotnet restore
dotnet run
```

Inicie sesión con el correo y la contraseña del administrador. Pruebe agregar un activo, crear un mantenimiento y revisar **Auditoría**. Después cree un segundo usuario desde **Usuarios** y verifique el inicio de sesión en otra computadora.

## Publicar para Windows

```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

El resultado queda en `bin/Release/net10.0/win-x64/publish`. Cada instalación debe conservar su `appsettings.json` configurado.

## Respaldos

El plan gratuito no debe considerarse un sistema de respaldo automático. Exporte periódicamente la base con `pg_dump` usando la cadena de conexión que aparece en **Connect** dentro del panel, y guarde el archivo cifrado en dos ubicaciones distintas. Nunca incluya la contraseña ni un respaldo real dentro del repositorio o del ZIP de instalación.

```powershell
pg_dump "CADENA_DE_CONEXION_DE_SUPABASE" --format=custom --file="inventario_2026-09-17.backup"
```

Antes de depender de los respaldos, pruebe una restauración en un proyecto de prueba.

## Estructura principal

```text
Models/                         Modelos JSON de la Data API
Services/SupabaseApiService.cs Auth, consultas y modificaciones
Views/                          Interfaz Avalonia
supabase/01_schema_rls.sql      Tablas, catálogos, RLS y auditoría
supabase/functions/create-user Función administrativa segura
appsettings.json                URL y clave pública
PRUEBAS.md                      Lista de pruebas manuales
```

## Notas

- La sesión se renueva automáticamente mientras el programa permanece abierto.
- `Profesor` puede consultar; `Administrador` y `Administrativo` pueden modificar inventario y mantenimientos.
- Solo `Administrador` accede a usuarios y auditoría.
- La eliminación de activos es lógica: se cambia `activo_registro` a `false`.
- Sin Internet el programa no puede trabajar; esta versión no incorpora sincronización sin conexión.
