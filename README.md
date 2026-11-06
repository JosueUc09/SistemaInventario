# Sistema de Inventario Escolar

Aplicación de escritorio desarrollada en C# con Avalonia UI, conectada a una base de datos PostgreSQL alojada en Supabase. Permite gestionar activos, usuarios, mantenimientos y registros de auditoría.

## Requisitos previos

- Windows 10 u 11.
- .NET SDK 10.0 instalado.
- Visual Studio o Visual Studio Code.
- Conexión a Internet para acceder a Supabase.

## Pasos para ejecutar el programa

1. Descargar el proyecto desde el repositorio de GitHub y extraer los archivos.
2. Abrir la carpeta del proyecto en el entorno de desarrollo de preferencia. Se recomienda Visual Studio.
3. Abrir una terminal en la carpeta donde se encuentra el archivo `InventarioEscolar.csproj`.
4. Ejecutar los siguientes comandos en orden:

   ```powershell
   dotnet clean
   dotnet build
   dotnet run
   ```

5. Una vez iniciada la aplicación, ingresar con las credenciales de prueba proporcionadas por separado.

## Base de datos

El sistema utiliza Supabase como servicio de base de datos en la nube. La aplicación está diseñada para conectarse a una base de datos previamente configurada, por lo que no es necesario instalar PostgreSQL localmente.

**Nota:** Para ejecutar el proyecto correctamente, se requiere conexión a Internet y que el archivo `appsettings.json` contenga la configuración pública de Supabase.
