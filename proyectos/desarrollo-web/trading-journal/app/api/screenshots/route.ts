import { getChatGPTUser } from "@/app/chatgpt-auth";
import { bucket, database, cleanupObjects } from "@/db/repository";
import { boundedBody, imageInfo } from "@/lib/server-validation";
export const dynamic = "force-dynamic";
const failure = (error: string, status: number) =>
  Response.json({ error }, { status });
export async function GET(request: Request) {
  try {
    const user = await getChatGPTUser();
    if (!user) return failure("Sin sesión", 401);
    const id = new URL(request.url).searchParams.get("id");
    const row = await database()
      .prepare(
        "SELECT object_key,mime FROM screenshots WHERE id=? AND user_id=?",
      )
      .bind(id, user.userId)
      .first<{ object_key: string; mime: string }>();
    if (!row) return failure("No encontrada", 404);
    const obj = await bucket().get(row.object_key);
    if (!obj) return failure("No encontrada", 404);
    return new Response(obj.body, {
      headers: {
        "Content-Type": row.mime,
        "Cache-Control": "private, no-store",
        "X-Content-Type-Options": "nosniff",
      },
    });
  } catch (e) {
    console.error("Screenshot read", e);
    return failure("No se pudo cargar la imagen.", 503);
  }
}
export async function POST(request: Request) {
  try {
    const user = await getChatGPTUser();
    if (!user) return failure("Iniciá sesión.", 401);
    if (
      request.headers.get("X-Journal-Request") !== "1" ||
      request.headers.get("sec-fetch-site") === "cross-site"
    )
      return failure("No permitido", 403);
    const body = await boundedBody(request, 5 * 1024 * 1024 + 20000);
    const form = await new Response(body, {
      headers: { "Content-Type": request.headers.get("Content-Type") ?? "" },
    }).formData();
    const file = form.get("file"),
      tradeId = form.get("tradeId"),
      kind = form.get("kind");
    if (
      !(file instanceof File) ||
      typeof tradeId !== "string" ||
      !["before", "after"].includes(String(kind))
    )
      return failure("Imagen o trade inválido.", 400);
    if (file.size > 5 * 1024 * 1024 || file.size === 0)
      return failure("Cada imagen debe pesar entre 1 byte y 5 MB.", 400);
    const buffer = await file.arrayBuffer(),
      info = imageInfo(new Uint8Array(buffer));
    if (!info)
      return failure(
        "Usá una imagen PNG, JPG o WebP válida, de hasta 60 megapíxeles.",
        400,
      );
    const db = database(),
      trade = await db
        .prepare("SELECT id FROM trades WHERE id=? AND user_id=?")
        .bind(tradeId, user.userId)
        .first();
    if (!trade) return failure("Trade no encontrado.", 404);
    const id = crypto.randomUUID(),
      key = user.userId + "/" + id;
    await bucket().put(key, buffer, {
      httpMetadata: { contentType: info.mime },
    });
    try {
      const inserted = await db
        .prepare(
          "INSERT INTO screenshots (id,user_id,trade_id,kind,name,mime,size,object_key) SELECT ?,?,?,?,?,?,?,? WHERE (SELECT count(*) FROM screenshots WHERE trade_id=?) < 10",
        )
        .bind(
          id,
          user.userId,
          tradeId,
          kind,
          file.name.slice(0, 200),
          info.mime,
          file.size,
          key,
          tradeId,
        )
        .run();
      if (!inserted.meta.changes) {
        await bucket().delete(key);
        return failure("Máximo 10 imágenes por trade.", 400);
      }
    } catch (e) {
      await bucket().delete(key);
      throw e;
    }
    return Response.json({ id });
  } catch (e) {
    console.error("Screenshot upload", e);
    return failure(
      "No se pudo subir la imagen. Usá un archivo válido de hasta 5 MB.",
      400,
    );
  }
}
export async function DELETE(request: Request) {
  try {
    const user = await getChatGPTUser();
    if (!user) return failure("Sin sesión", 401);
    if (
      request.headers.get("X-Journal-Request") !== "1" ||
      request.headers.get("sec-fetch-site") === "cross-site"
    )
      return failure("No permitido", 403);
    const id = new URL(request.url).searchParams.get("id"),
      db = database(),
      row = await db
        .prepare("SELECT object_key FROM screenshots WHERE id=? AND user_id=?")
        .bind(id, user.userId)
        .first<{ object_key: string }>();
    if (row) {
      await db.batch([
        db
          .prepare(
            "INSERT INTO storage_deletions (object_key,user_id) VALUES (?,?) ON CONFLICT(object_key) DO NOTHING",
          )
          .bind(row.object_key, user.userId),
        db
          .prepare("DELETE FROM screenshots WHERE id=? AND user_id=?")
          .bind(id, user.userId),
      ]);
      await cleanupObjects(user.userId);
    }
    return Response.json({ ok: true });
  } catch (e) {
    console.error("Screenshot delete", e);
    return failure("No se pudo eliminar la captura.", 500);
  }
}
