import { getChatGPTUser } from "@/app/chatgpt-auth";
import { database, loadData, tradeStatements, scopedId } from "@/db/repository";
import {
  tradeSchema,
  planSchema,
  dailySchema,
  goalSchema,
  reviewSchema,
  checkPlan,
  reconcileTrades,
  type Trade,
} from "@/lib/domain";
import { z } from "zod";
import { boundedBody } from "@/lib/server-validation";
import { backupDataSchema } from "@/lib/backup-schema";
export const dynamic = "force-dynamic";
export async function POST(request: Request) {
  try {
    const user = await getChatGPTUser();
    if (!user)
      return Response.json({ error: "Iniciá sesión." }, { status: 401 });
    if (
      request.headers.get("X-Journal-Request") !== "1" ||
      request.headers.get("sec-fetch-site") === "cross-site"
    )
      return new Response("No permitido", { status: 403 });
    const text = new TextDecoder().decode(
      await boundedBody(request, 12_000_000),
    );
    if (text.length > 12_000_000)
      return Response.json(
        { error: "Demasiados datos para una sola copia." },
        { status: 413 },
      );
    const backup = backupDataSchema
      .extend({
        trades: backupDataSchema.shape.trades.max(500),
        journals: backupDataSchema.shape.journals.max(100),
        goals: backupDataSchema.shape.goals.max(100),
        reviews: backupDataSchema.shape.reviews.max(100),
        habits: backupDataSchema.shape.habits.max(100),
      })
      .parse(JSON.parse(text));
    const data = await loadData(user),
      uid = user.userId,
      db = database(),
      statements: D1PreparedStatement[] = [],
      idMap: Record<string, string> = {},
      habitMap: Record<string, string> = {},
      now = new Date().toISOString();
    for (const h of backup.habits) {
      const mapped = await scopedId(uid, "habit-" + h.id);
      const existing = data.habits.find(
        (x) => x.id === h.id || x.id === mapped,
      );
      const id = existing?.id ?? mapped;
      habitMap[h.id] = id;
      statements.push(
        db
          .prepare(
            "INSERT INTO habits (id,user_id,name) VALUES (?,?,?) ON CONFLICT(id) DO UPDATE SET name=excluded.name WHERE habits.user_id=excluded.user_id",
          )
          .bind(id, uid, h.name),
      );
    }
    const history = [...data.trades];
    for (const raw of backup.trades.sort(
      (a, b) => Date.parse(a.enteredAt) - Date.parse(b.enteredAt),
    )) {
      const mapped = await scopedId(uid, raw.id),
        existing = data.trades.find((t) => t.id === raw.id || t.id === mapped);
      if (existing) {
        idMap[raw.id] = existing.id;
        continue;
      }
      const id = mapped,
        t: Trade = { ...raw, id, accountId: data.account.id };
      const snap = planSchema.safeParse(raw.planSnapshot);
      const applied = snap.success ? snap.data : backup.plan;
      t.planSnapshot = applied;
      t.violations = checkPlan(t, applied, history);
      history.push(t);
      idMap[raw.id] = id;
      statements.push(...tradeStatements(uid, t));
    }
    for (const raw of backup.journals) {
      const mappedHabits = await Promise.all(
        raw.habits.map(async (h) => {
          const id =
            habitMap[h] ??
            (data.habits.some((x) => x.id === h)
              ? h
              : await scopedId(uid, "habit-" + h));
          return data.habits.some((x) => x.id === id) ||
            Object.values(habitMap).includes(id)
            ? id
            : null;
        }),
      );
      const j = {
        ...raw,
        id: "j-" + uid + "-" + raw.date,
        habits: [
          ...new Set(mappedHabits.filter((h): h is string => Boolean(h))),
        ],
      };
      statements.push(
        db
          .prepare(
            "INSERT INTO daily_journals (id,user_id,date,payload) VALUES (?,?,?,?) ON CONFLICT(user_id,date) DO UPDATE SET payload=excluded.payload",
          )
          .bind(j.id, uid, j.date, JSON.stringify(j)),
        db.prepare("DELETE FROM journal_habits WHERE journal_id=?").bind(j.id),
        ...j.habits.map((h) =>
          db
            .prepare(
              "INSERT INTO journal_habits (id,journal_id,habit_id) VALUES (?,?,?)",
            )
            .bind(j.id + "-" + h, j.id, h),
        ),
      );
    }
    for (const raw of backup.goals) {
      const mapped = await scopedId(uid, "goal-" + raw.id);
      const id =
        data.goals.find((g) => g.id === raw.id || g.id === mapped)?.id ??
        mapped;
      const g = { ...raw, id };
      statements.push(
        db
          .prepare(
            "INSERT INTO goals (id,user_id,account_id,payload) VALUES (?,?,?,?) ON CONFLICT(id) DO UPDATE SET payload=excluded.payload WHERE goals.user_id=excluded.user_id",
          )
          .bind(id, uid, data.account.id, JSON.stringify(g)),
      );
    }
    for (const raw of backup.reviews) {
      const r = { ...raw, id: "r-" + uid + "-" + raw.kind + "-" + raw.start };
      statements.push(
        db
          .prepare(
            "INSERT INTO reviews (id,user_id,kind,start,payload) VALUES (?,?,?,?,?) ON CONFLICT(user_id,kind,start) DO UPDATE SET payload=excluded.payload",
          )
          .bind(r.id, uid, r.kind, r.start, JSON.stringify(r)),
      );
    }
    statements.push(
      db
        .prepare(
          "INSERT INTO trading_plans (id,user_id,account_id,payload,updated_at) VALUES (?,?,?,?,?) ON CONFLICT(account_id) DO UPDATE SET payload=excluded.payload,updated_at=excluded.updated_at WHERE trading_plans.user_id=excluded.user_id",
        )
        .bind(
          "p-" + data.account.id,
          uid,
          data.account.id,
          JSON.stringify(backup.plan),
          now,
        ),
      db
        .prepare(
          "UPDATE trading_accounts SET name=?,initial_balance=? WHERE id=? AND user_id=?",
        )
        .bind(
          backup.account.name,
          backup.account.initialBalance,
          data.account.id,
          uid,
        ),
      db
        .prepare(
          "INSERT INTO user_settings (user_id,payload) VALUES (?,?) ON CONFLICT(user_id) DO UPDATE SET payload=excluded.payload",
        )
        .bind(uid, JSON.stringify(backup.settings)),
    );
    const reconciled =
      history.length === data.trades.length
        ? []
        : reconcileTrades(history, backup.plan);
    for (const t of reconciled) {
      if (
        JSON.stringify(t) !== JSON.stringify(history.find((x) => x.id === t.id))
      )
        statements.push(...tradeStatements(uid, t));
    }
    await db.batch(statements);
    return Response.json({ ok: true, idMap, existingImages: data.screenshots });
  } catch (e) {
    console.error("Backup restore", e);
    return Response.json(
      {
        error:
          e instanceof z.ZodError
            ? "La copia contiene datos inválidos: " + e.issues[0].path.join(".")
            : "No se pudo restaurar la copia. No se modificaron los registros.",
      },
      { status: 400 },
    );
  }
}
