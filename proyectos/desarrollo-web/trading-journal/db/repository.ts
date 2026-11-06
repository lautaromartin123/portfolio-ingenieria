import { env } from "cloudflare:workers";
import {
  type JournalData,
  type Trade,
  type Plan,
  emptyData,
  defaultPlan,
  checkPlan,
} from "@/lib/domain";
export function database() {
  if (!env.DB)
    throw new Error("El almacenamiento no está disponible. Intentá de nuevo.");
  return env.DB;
}
export function bucket() {
  if (!env.BUCKET)
    throw new Error("El almacenamiento de imágenes no está disponible.");
  return env.BUCKET;
}
export async function loadData(user: {
  userId: string;
  email: string;
  displayName: string;
}): Promise<JournalData> {
  const db = database();
  await db
    .prepare(
      "INSERT INTO users (id,email,name) VALUES (?,?,?) ON CONFLICT(id) DO UPDATE SET email=excluded.email,name=excluded.name",
    )
    .bind(user.userId, user.email, user.displayName)
    .run();
  const accountId = "a-" + user.userId;
  await db
    .prepare(
      "INSERT INTO trading_accounts (id,user_id,name,currency,initial_balance,timezone) VALUES (?,?,?,'USD',10000,?) ON CONFLICT(id) DO NOTHING",
    )
    .bind(accountId, user.userId, "Cuenta principal", defaultPlan.timezone)
    .run();
  const a = await db
    .prepare("SELECT * FROM trading_accounts WHERE id=? AND user_id=?")
    .bind(accountId, user.userId)
    .first<Record<string, unknown>>();
  if (!a) throw new Error("No se pudo abrir la cuenta.");
  const tables = [
    "trades",
    "daily_journals",
    "goals",
    "reviews",
    "habits",
    "screenshots",
  ] as const;
  const results = await db.batch<Record<string, unknown>>(
    tables.map((table) =>
      db.prepare(`SELECT * FROM ${table} WHERE user_id=?`).bind(user.userId),
    ),
  );
  const [p, s] = await Promise.all([
    db
      .prepare(
        "SELECT payload FROM trading_plans WHERE account_id=? AND user_id=?",
      )
      .bind(accountId, user.userId)
      .first<{ payload: string }>(),
    db
      .prepare("SELECT payload FROM user_settings WHERE user_id=?")
      .bind(user.userId)
      .first<{ payload: string }>(),
  ]);
  const data = emptyData();
  data.account = {
    id: accountId,
    name: String(a.name),
    currency: "USD",
    initialBalance: Number(a.initial_balance),
    timezone: String(a.timezone),
  };
  data.plan = p
    ? JSON.parse(p.payload)
    : { ...defaultPlan, timezone: data.account.timezone };
  data.settings = s ? JSON.parse(s.payload) : {};
  data.trades = results[0].results.map((r) => JSON.parse(String(r.payload)));
  data.journals = results[1].results.map((r) => JSON.parse(String(r.payload)));
  data.goals = results[2].results.map((r) => JSON.parse(String(r.payload)));
  data.reviews = results[3].results.map((r) => JSON.parse(String(r.payload)));
  data.habits = results[4].results.map((r) => ({
    id: String(r.id),
    name: String(r.name),
  }));
  data.screenshots = results[5].results.map((r) => ({
    id: String(r.id),
    tradeId: String(r.trade_id),
    kind: r.kind as "before" | "after",
    name: String(r.name),
    mime: String(r.mime),
    size: Number(r.size),
  }));
  return data;
}
export function tradeStatements(userId: string, t: Trade) {
  const db = database(),
    setupId = t.accountId + "-" + t.setup;
  return [
    db
      .prepare(
        "INSERT INTO setups (id,user_id,account_id,name) VALUES (?,?,?,?) ON CONFLICT(id) DO NOTHING",
      )
      .bind(setupId, userId, t.accountId, t.setup),
    db
      .prepare(
        "INSERT INTO trades (id,user_id,account_id,setup_id,entered_at,exited_at,instrument,direction,quantity,payload) VALUES (?,?,?,?,?,?,?,?,?,?) ON CONFLICT(id) DO UPDATE SET setup_id=excluded.setup_id,entered_at=excluded.entered_at,exited_at=excluded.exited_at,instrument=excluded.instrument,direction=excluded.direction,quantity=excluded.quantity,payload=excluded.payload WHERE trades.user_id=excluded.user_id",
      )
      .bind(
        t.id,
        userId,
        t.accountId,
        setupId,
        t.enteredAt,
        t.exitedAt,
        t.instrument,
        t.direction,
        t.quantity,
        JSON.stringify(t),
      ),
    db
      .prepare(
        "INSERT INTO trade_psychology (trade_id,user_id,payload) VALUES (?,?,?) ON CONFLICT(trade_id) DO UPDATE SET payload=excluded.payload WHERE trade_psychology.user_id=excluded.user_id",
      )
      .bind(t.id, userId, JSON.stringify(t.psychology)),
  ];
}
export function validateAgainstPlan(
  t: Trade,
  data: JournalData,
  preserve = false,
) {
  const old = data.trades.find((x) => x.id === t.id);
  const plan =
    preserve && old?.planSnapshot ? (old.planSnapshot as Plan) : data.plan;
  return {
    ...t,
    accountId: data.account.id,
    violations: checkPlan(t, plan, data.trades),
    planSnapshot: plan,
  };
}
export async function scopedId(userId: string, sourceId: string) {
  const hash = await crypto.subtle.digest(
    "SHA-256",
    new TextEncoder().encode(userId + "|" + sourceId),
  );
  return (
    "import-" +
    Array.from(new Uint8Array(hash))
      .map((b) => b.toString(16).padStart(2, "0"))
      .join("")
  );
}
export async function cleanupObjects(userId: string) {
  const db = database(),
    rows = await db
      .prepare(
        "SELECT object_key FROM storage_deletions WHERE user_id=? LIMIT 50",
      )
      .bind(userId)
      .all<{ object_key: string }>();
  if (!rows.results.length) return;
  try {
    await bucket().delete(rows.results.map((r) => r.object_key));
    await db.batch(
      rows.results.map((r) =>
        db
          .prepare(
            "DELETE FROM storage_deletions WHERE object_key=? AND user_id=?",
          )
          .bind(r.object_key, userId),
      ),
    );
  } catch (e) {
    console.error("Pending storage cleanup", e);
  }
}
