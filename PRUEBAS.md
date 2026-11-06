# Pruebas manuales

Realice estas pruebas después de completar `README.md` y configurar `appsettings.json`.

## Configuración y acceso

1. Inicie con URL o clave incorrecta: debe aparecer un mensaje de configuración o conexión.
2. Inicie con contraseña incorrecta: el acceso debe rechazarse.
3. Inicie como administrador: deben aparecer Usuarios y Auditoría.
4. Inicie como profesor: no debe aparecer Usuarios; RLS debe impedir modificaciones no autorizadas.

## Inventario compartido

1. Abra el programa en dos computadoras con cuentas diferentes.
2. Agregue un activo desde la primera.
3. Presione Actualizar en la segunda y confirme que aparece.
4. Edite el activo y compruebe que números de serie y activo duplicados son rechazados.
5. Desactive el activo y confirme que deja de aparecer sin borrarse físicamente.

## Usuarios

1. Como administrador, cree un usuario con una contraseña de al menos 8 caracteres.
2. Confirme que aparece tanto en Authentication > Users como en `perfiles`.
3. Inicie sesión con la nueva cuenta.
4. Edite correo, rol o contraseña desde la aplicación y vuelva a iniciar sesión.
5. Confirme que el administrador no puede desactivar su propia cuenta desde la interfaz.

## Mantenimiento y auditoría

1. Registre un mantenimiento.
2. Compruebe el listado y los indicadores.
3. Abra Auditoría y confirme que registra usuario, equipo, tabla, acción y valores.
4. Revise que un usuario no administrador no pueda consultar `auditorias` mediante la Data API.

## Sesión y fallos

1. Deje el programa abierto más de una hora y vuelva a consultar: la sesión debe renovarse.
2. Desconecte Internet y actualice: debe mostrarse el error sin cerrar el programa inesperadamente.
3. Restaure Internet y vuelva a intentar.

## Respaldo

1. Genere un respaldo con `pg_dump`.
2. Restaure el archivo en un proyecto Supabase de prueba.
3. Compare cantidades de las tablas y pruebe iniciar sesión con un usuario de prueba.
