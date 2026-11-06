import { getChatGPTUser } from "@/app/chatgpt-auth";
import {
  database,
  bucket,
  loadData,
  tradeStatements,
  validateAgainstPlan,
  scopedId,
  cleanupObjects,
} from "@/db/repository";
import {
  dailySchema,
  goalSchema,
  planSchema,
  reviewSchema,
  tradeSchema,
  checkPlan,
  reconcileTrades,
  type Trade,
} from "@/lib/domain";
import { z } from "zod";
import { boundedBody } from "@/lib/server-validation";
export const dynamic = "force-dynamic";
const reply = (data: unknown, status = 200) =>
  Response.json(data, {
    status,
    headers: {
      "Cache-Control": "no-store",
      "X-Content-Type-Options": "nosniff",
    },
  });
export async function GET() {
  try {
    const user = await getChatGPTUser();
    if (!user)
      return reply({ error: "Iniciá sesión para abrir tu journal." }, 401);
    await cleanupObjects(user.userId);
    return reply({
      data: await loadData(user),
      user: { name: user.displayName, email: user.email },
    });
  } catch (e) {
    console.error("Journal load", e);
    return reply(
      { error: "No pudimos cargar tu journal. Volvé a intentar." },
      503,
    );
  }
}
export async function POST(request: Request) {
  try {
    const user = await getChatGPTUser();
    if (!user)
      return reply({ error: "Tu sesión expiró. Volvé a iniciar sesión." }, 401);
    if (
      request.headers.get("X-Journal-Request") !== "1" ||
      request.headers.get("sec-fetch-site") === "cross-site"
    )
      return reply({ error: "Solicitud no autorizada." }, 403);
    const text = new TextDecoder().decode(
      await boundedBody(request, 12_000_000),
    );
    if (text.length > 12_000_000)
      return reply({ error: "Archivo demasiado grande." }, 413);
    const { action, payload } = z
      .object({ action: z.string(), payload: z.unknown() })
      .parse(JSON.parse(text));
    const data = await loadData(user),
      db = database(),
      uid = user.userId;
    if (action === "saveTrade") {
      let t = tradeSchema.parse(payload);
      const owner = await db
        .prepare("SELECT user_id FROM trades WHERE id=?")
        .bind(t.id)
        .first<{ user_id: string }>();
      if (owner && owner.user_id !== uid)
        return reply({ error: "Operación no encontrada." }, 404);
      t = validateAgainstPlan(t, data, true);
      const reconciled = reconcileTrades(
        [...data.trades.filter((x) => x.id !== t.id), t],
        data.plan,
      );
      const changed = reconciled.filter(
        (x) =>
          JSON.stringify(x) !==
          JSON.stringify(data.trades.find((y) => y.id === x.id)),
      );
      await db.batch(changed.flatMap((x) => tradeStatements(uid, x)));
      return reply({ trade: reconciled.find((x) => x.id === t.id) });
    }
    if (action === "deleteTrade") {
      const id = z.string().parse(payload);
      const images = await db
        .prepare(
          "SELECT object_key FROM screenshots WHERE trade_id=? AND user_id=?",
        )
        .bind(id, uid)
        .all<{ object_key: string }>();
      const reconciled = reconcileTrades(
        data.trades.filter((t) => t.id !== id),
        data.plan,
      );
      await db.batch([
        ...images.results.map((x) =>
          db
            .prepare(
              "INSERT INTO storage_deletions (object_key,user_id) VALUES (?,?) ON CONFLICT(object_key) DO NOTHING",
            )
            .bind(x.object_key, uid),
        ),
        db.prepare("DELETE FROM trades WHERE id=? AND user_id=?").bind(id, uid),
        ...reconciled
          .filter(
            (x) =>
              JSON.stringify(x) !==
              JSON.stringify(data.trades.find((y) => y.id === x.id)),
          )
          .flatMap((t) => tradeStatements(uid, t)),
      ]);
      await cleanupObjects(uid);
      return reply({ ok: true });
    }
    if (action === "savePlan") {
      const plan = planSchema.parse(payload);
      plan.updatedAt = new Date().toISOString();
      await db
        .prepare(
          "INSERT INTO trading_plans (id,user_id,account_id,payload,updated_at) VALUES (?,?,?,?,?) ON CONFLICT(account_id) DO UPDATE SET payload=excluded.payload,updated_at=excluded.updated_at WHERE trading_plans.user_id=excluded.user_id",
        )
        .bind(
          "p-" + data.account.id,
          uid,
          data.account.id,
          JSON.stringify(plan),
          plan.updatedAt,
        )
        .run();
      return reply({ ok: true });
    }
    if (action === "saveJournal") {
      const j = dailySchema.parse(payload),
        id = "j-" + uid + "-" + j.date;
      j.id = id;
      const validHabits = data.habits.map((h) => h.id);
      j.habits = j.habits.filter((h) => validHabits.includes(h));
      await db.batch([
        db
          .prepare(
            "INSERT INTO daily_journals (id,user_id,date,payload) VALUES (?,?,?,?) ON CONFLICT(user_id,date) DO UPDATE SET payload=excluded.payload",
          )
          .bind(id, uid, j.date, JSON.stringify(j)),
        db.prepare("DELETE FROM journal_habits WHERE journal_id=?").bind(id),
        ...j.habits.map((h) =>
          db
            .prepare(
              "INSERT INTO journal_habits (id,journal_id,habit_id) VALUES (?,?,?)",
            )
            .bind(id + "-" + h, id, h),
        ),
      ]);
      return reply({ ok: true });
    }
    if (action === "deleteJournal") {
      const date = z.string().parse(payload);
      await db
        .prepare("DELETE FROM daily_journals WHERE date=? AND user_id=?")
        .bind(date, uid)
        .run();
      return reply({ ok: true });
    }
    if (action === "saveGoal") {
      const g = goalSchema.parse(payload);
      const old = await db
        .prepare("SELECT user_id FROM goals WHERE id=?")
        .bind(g.id)
        .first<{ user_id: string }>();
      if (old && old.user_id !== uid)
        return reply({ error: "Objetivo no encontrado." }, 404);
      await db
        .prepare(
          "INSERT INTO goals (id,user_id,account_id,payload) VALUES (?,?,?,?) ON CONFLICT(id) DO UPDATE SET payload=excluded.payload WHERE goals.user_id=excluded.user_id",
        )
        .bind(g.id, uid, data.account.id, JSON.stringify(g))
        .run();
      return reply({ ok: true });
    }
    if (action === "deleteGoal") {
      await db
        .prepare("DELETE FROM goals WHERE id=? AND user_id=?")
        .bind(z.string().parse(payload), uid)
        .run();
      return reply({ ok: true });
    }
    if (action === "saveReview") {
      const r = reviewSchema.parse(payload);
      r.id = "r-" + uid + "-" + r.kind + "-" + r.start;
      await db
        .prepare(
          "INSERT INTO reviews (id,user_id,kind,start,payload) VALUES (?,?,?,?,?) ON CONFLICT(user_id,kind,start) DO UPDATE SET payload=excluded.payload",
        )
        .bind(r.id, uid, r.kind, r.start, JSON.stringify(r))
        .run();
      return reply({ ok: true });
    }
    if (action === "deleteReview") {
      await db
        .prepare("DELETE FROM reviews WHERE id=? AND user_id=?")
        .bind(z.string().parse(payload), uid)
        .run();
      return reply({ ok: true });
    }
    if (action === "saveHabit") {
      const h = z
        .object({
          id: z.string().max(100),
          name: z.string().trim().min(1).max(100),
        })
        .parse(payload);
      await db
        .prepare(
          "INSERT INTO habits (id,user_id,name) VALUES (?,?,?) ON CONFLICT(id) DO UPDATE SET name=excluded.name WHERE habits.user_id=excluded.user_id",
        )
        .bind(h.id, uid, h.name)
        .run();
      return reply({ ok: true });
    }
    if (action === "deleteHabit") {
      const id = z.string().parse(payload);
      await db.batch([
        db.prepare("DELETE FROM habits WHERE id=? AND user_id=?").bind(id, uid),
        ...data.journals
          .filter((j) => j.habits.includes(id))
          .map((j) =>
            db
              .prepare(
                "UPDATE daily_journals SET payload=? WHERE id=? AND user_id=?",
              )
              .bind(
                JSON.stringify({
                  ...j,
                  habits: j.habits.filter((h) => h !== id),
                }),
                j.id,
                uid,
              ),
          ),
      ]);
      return reply({ ok: true });
    }
    if (action === "saveSettings") {
      const s = z
        .object({
          widgets: z.array(z.enum(["equity", "calendar", "trades", "goals"])),
          defaults: z
            .object({
              instrument: z.string().max(30),
              pointValue: z.number().positive(),
              quantity: z.number().positive(),
              commissions: z.number().min(0),
              setup: z.string().max(100),
              timeframe: z.string().max(30),
              session: z.string().max(60),
            })
            .optional(),
        })
        .parse(payload);
      await db
        .prepare(
          "INSERT INTO user_settings (user_id,payload) VALUES (?,?) ON CONFLICT(user_id) DO UPDATE SET payload=excluded.payload",
        )
        .bind(uid, JSON.stringify(s))
        .run();
      return reply({ ok: true });
    }
    if (action === "saveAccount") {
      const a = z
        .object({
          name: z.string().trim().min(1).max(100),
          initialBalance: z.number().finite().min(0).max(1e12),
        })
        .parse(payload);
      await db
        .prepare(
          "UPDATE trading_accounts SET name=?,initial_balance=? WHERE id=? AND user_id=?",
        )
        .bind(a.name, a.initialBalance, data.account.id, uid)
        .run();
      return reply({ ok: true });
    }
    if (action === "importTrades") {
      const items = z.array(tradeSchema).min(1).max(500).parse(payload);
      const existing = new Set(data.trades.map((t) => t.id));
      const rows: Trade[] = [];
      for (const input of items.sort(
        (a, b) => Date.parse(a.enteredAt) - Date.parse(b.enteredAt),
      )) {
        if (existing.has(input.id)) continue;
        const mappedId = await scopedId(uid, input.id);
        if (existing.has(mappedId)) continue;
        const t = { ...input, id: mappedId, accountId: data.account.id };
        t.violations = checkPlan(t, data.plan, [...data.trades, ...rows]);
        t.planSnapshot = data.plan;
        rows.push(t);
        existing.add(input.id);
      }
      if (rows.length) {
        const reconciled = reconcileTrades(
          [...data.trades, ...rows],
          data.plan,
        );
        await db.batch(
          reconciled
            .filter(
              (x) =>
                JSON.stringify(x) !==
                JSON.stringify(data.trades.find((y) => y.id === x.id)),
            )
            .flatMap((t) => tradeStatements(uid, t)),
        );
      }
      return reply({
        imported: rows.length,
        skipped: items.length - rows.length,
      });
    }
    return reply({ error: "Acción desconocida." }, 400);
  } catch (e) {
    if (e instanceof z.ZodError)
      return reply(
        {
          error: e.issues
            .map(
              (i) =>
                (i.path.join(".") ? i.path.join(".") + ": " : "") + i.message,
            )
            .join(" · "),
        },
        400,
      );
    if (e instanceof SyntaxError)
      return reply({ error: "El archivo no tiene un formato válido." }, 400);
    console.error("Journal write", e);
    return reply(
      { error: "No se pudo guardar. Tus cambios siguen en el formulario." },
      500,
    );
  }
}
