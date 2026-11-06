import { createClient } from "https://esm.sh/@supabase/supabase-js@2";

const headers = { "Content-Type": "application/json" };

Deno.serve(async (request) => {
  if (request.method !== "POST")
    return new Response(JSON.stringify({ error: "Método no permitido" }), { status: 405, headers });

  try {
    const authorization = request.headers.get("Authorization") ?? "";
    const token = authorization.replace(/^Bearer\s+/i, "");
    if (!token) return new Response(JSON.stringify({ error: "Falta la sesión" }), { status: 401, headers });

    const url = Deno.env.get("SUPABASE_URL")!;
    const serviceKey = Deno.env.get("SUPABASE_SERVICE_ROLE_KEY")!;
    const admin = createClient(url, serviceKey, { auth: { persistSession: false } });
    const { data: callerData, error: callerError } = await admin.auth.getUser(token);
    if (callerError || !callerData.user)
      return new Response(JSON.stringify({ error: "Sesión inválida" }), { status: 401, headers });

    const { data: caller } = await admin.from("perfiles")
      .select("activo, roles(nombre)").eq("id", callerData.user.id).single();
    if (!caller?.activo || caller.roles?.nombre !== "Administrador")
      return new Response(JSON.stringify({ error: "Se requiere el rol Administrador" }), { status: 403, headers });

    const body = await request.json();
    if (!body.nombre || !body.correo || !body.role_id)
      return new Response(JSON.stringify({ error: "Faltan campos obligatorios" }), { status: 400, headers });

    if (body.id) {
      const attributes: Record<string, unknown> = { email: body.correo, email_confirm: true };
      if (body.password) attributes.password = body.password;
      const { error: authError } = await admin.auth.admin.updateUserById(body.id, attributes);
      if (authError) throw authError;
      const { error: profileError } = await admin.from("perfiles").update({
        nombre: body.nombre, correo: body.correo, role_id: body.role_id, activo: body.activo ?? true
      }).eq("id", body.id);
      if (profileError) throw profileError;
      return new Response(JSON.stringify({ id: body.id }), { status: 200, headers });
    }

    if (!body.password || body.password.length < 8)
      return new Response(JSON.stringify({ error: "La contraseña requiere al menos 8 caracteres" }), { status: 400, headers });

    const { data: created, error: authError } = await admin.auth.admin.createUser({
      email: body.correo, password: body.password, email_confirm: true,
      user_metadata: { nombre: body.nombre }
    });
    if (authError || !created.user) throw authError ?? new Error("No se creó el usuario");

    const { error: profileError } = await admin.from("perfiles").insert({
      id: created.user.id, nombre: body.nombre, correo: body.correo,
      role_id: body.role_id, activo: body.activo ?? true
    });
    if (profileError) {
      await admin.auth.admin.deleteUser(created.user.id);
      throw profileError;
    }
    return new Response(JSON.stringify({ id: created.user.id }), { status: 201, headers });
  } catch (error) {
    return new Response(JSON.stringify({ error: error instanceof Error ? error.message : String(error) }),
      { status: 400, headers });
  }
});
