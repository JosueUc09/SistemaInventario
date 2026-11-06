-- EJECUTAR EN SQL EDITOR SOLO DESPUÉS DE 01_schema_rls.sql.
-- Migración no destructiva: índices B-tree para joins, filtros y ordenaciones.
create index if not exists idx_activos_visibles_id on public.activos (id) where activo_registro = true;
create index if not exists idx_activos_categoria on public.activos (categoria_id);
create index if not exists idx_activos_ubicacion on public.activos (ubicacion_id);
create index if not exists idx_activos_estado on public.activos (estado_activo_id);
create index if not exists idx_mantenimientos_activo_fecha on public.mantenimientos (activo_id, fecha desc);
create index if not exists idx_mantenimientos_fecha on public.mantenimientos (fecha desc);
create index if not exists idx_auditoria_fecha on public.auditorias (fecha desc);
create index if not exists idx_auditoria_usuario_fecha on public.auditorias (usuario_id, fecha desc);
-- Las restricciones UNIQUE de numero_activo y numero_serie crean índices únicos automáticamente.
-- No se ofrecen garantías de rendimiento: examinar EXPLAIN ANALYZE según datos reales.
