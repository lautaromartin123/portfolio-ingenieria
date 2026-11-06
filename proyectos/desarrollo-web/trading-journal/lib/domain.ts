import { z } from "zod";
export const emotions = [
  "Calma",
  "Confianza",
  "Ansiedad",
  "Frustración",
  "Euforia",
  "Miedo",
  "Cansancio",
];
const rating = z.number().int().min(1).max(5),
  note = z.string().max(20000).default("");
export const psychologySchema = z.object({
  before: z.string().max(60).default("Calma"),
  after: z.string().max(60).default("Calma"),
  confidence: rating.default(3),
  quality: rating.default(3),
  adherence: rating.default(5),
  impulsive: z.boolean().default(false),
  movedStop: z.boolean().default(false),
  earlyClose: z.boolean().default(false),
  overtraded: z.boolean().default(false),
  fomo: z.boolean().default(false),
  error: note,
  lesson: note,
});
export const tradeSchema = z
  .object({
    id: z.string().min(1).max(100),
    accountId: z.string().min(1).max(100),
    enteredAt: z
      .string()
      .datetime()
      .transform((s) => new Date(s).toISOString()),
    exitedAt: z
      .string()
      .datetime()
      .transform((s) => new Date(s).toISOString()),
    instrument: z
      .string()
      .trim()
      .min(1)
      .max(30)
      .transform((s) => s.toUpperCase()),
    direction: z.enum(["Long", "Short"]),
    entry: z.number().finite().positive(),
    exit: z.number().finite().positive(),
    stop: z.number().finite().positive(),
    target: z.number().finite().positive(),
    quantity: z.number().finite().positive().max(1e8),
    pointValue: z.number().finite().positive().max(1e8),
    commissions: z.number().finite().min(0).max(1e9),
    setup: z.string().trim().min(1).max(100),
    timeframe: z.string().min(1).max(30),
    session: z.string().min(1).max(60),
    notes: note,
    psychology: psychologySchema,
    violations: z.array(z.string()).default([]),
    planSnapshot: z.unknown().optional(),
  })
  .superRefine((t, c) => {
    if (Date.parse(t.exitedAt) < Date.parse(t.enteredAt))
      c.addIssue({
        code: "custom",
        path: ["exitedAt"],
        message: "La salida debe ser posterior a la entrada.",
      });
    if (t.direction === "Long" ? t.stop >= t.entry : t.stop <= t.entry)
      c.addIssue({
        code: "custom",
        path: ["stop"],
        message: "El stop inicial debe estar del lado de pérdida.",
      });
    if (t.direction === "Long" ? t.target <= t.entry : t.target >= t.entry)
      c.addIssue({
        code: "custom",
        path: ["target"],
        message: "El take profit debe estar del lado de ganancia.",
      });
    if (
      !Number.isFinite(
        Math.abs(t.entry - t.stop) * t.quantity * t.pointValue,
      ) ||
      Math.abs(t.entry - t.stop) * t.quantity * t.pointValue === 0 ||
      Math.abs(t.entry - t.stop) * t.quantity * t.pointValue > 1e12 ||
      !Number.isFinite(
        Math.abs(t.target - t.entry) / Math.abs(t.entry - t.stop),
      ) ||
      !Number.isFinite((t.exit - t.entry) * t.quantity * t.pointValue) ||
      Math.abs((t.exit - t.entry) * t.quantity * t.pointValue) > 1e12
    )
      c.addIssue({
        code: "custom",
        message: "El tamaño de la operación excede el límite.",
      });
  });
export type Trade = z.infer<typeof tradeSchema>;
export type Psychology = z.infer<typeof psychologySchema>;
export const planSchema = z.object({
  instruments: z.array(z.string().min(1).max(30)).min(1).max(100),
  start: z.string().regex(/^([01]\d|2[0-3]):[0-5]\d$/),
  end: z.string().regex(/^([01]\d|2[0-3]):[0-5]\d$/),
  maxRisk: z.number().finite().positive(),
  maxDailyLoss: z.number().finite().positive(),
  maxTrades: z.number().int().min(1).max(10000),
  minRR: z.number().finite().positive(),
  setups: z.array(z.string().min(1).max(100)).min(1).max(100),
  rules: z.string().max(20000),
  timezone: z.string().refine((s) => {
    try {
      new Intl.DateTimeFormat("en", { timeZone: s });
      return true;
    } catch {
      return false;
    }
  }, "Zona horaria inválida"),
  updatedAt: z.string().optional(),
});
export type Plan = z.infer<typeof planSchema>;
export const defaultPlan: Plan = {
  instruments: ["MES", "MNQ", "ES", "NQ"],
  start: "09:30",
  end: "16:00",
  maxRisk: 100,
  maxDailyLoss: 300,
  maxTrades: 5,
  minRR: 2,
  setups: ["Pullback", "Breakout", "Reversal"],
  rules:
    "Esperar confirmación. No aumentar el riesgo para recuperar pérdidas. Pausar después de dos pérdidas consecutivas.",
  timezone: "America/Argentina/Buenos_Aires",
};
export const dateSchema = z
  .string()
  .regex(/^\d{4}-\d{2}-\d{2}$/)
  .refine(
    (s) =>
      !Number.isNaN(Date.parse(s)) &&
      new Date(s).toISOString().slice(0, 10) === s,
    "Fecha inválida",
  );
export const dailySchema = z.object({
  id: z.string().max(100),
  date: dateSchema,
  mood: z.string().max(60),
  energy: rating,
  sleep: rating,
  objective: note,
  well: note,
  improve: note,
  learned: note,
  reflection: note,
  gratitude: note,
  tasks: z
    .array(z.object({ text: z.string().min(1).max(1000), done: z.boolean() }))
    .max(100),
  habits: z.array(z.string().max(100)).max(100),
});
export type DailyJournal = z.infer<typeof dailySchema>;
export const goalSchema = z
  .object({
    id: z.string().max(100),
    title: z.string().trim().min(1).max(200),
    kind: z.enum(["plan", "journal", "risk", "overtrade", "consistency"]),
    target: z.number().int().min(1).max(10000),
    threshold: z.number().finite().min(0).max(1e9),
    start: dateSchema,
    end: dateSchema,
    archived: z.boolean().default(false),
  })
  .refine(
    (g) =>
      g.end >= g.start &&
      Date.parse(g.end) - Date.parse(g.start) <= 3660 * 86400000,
    "El objetivo debe durar entre 1 día y 10 años",
  );
export type Goal = z.infer<typeof goalSchema>;
export const reviewSchema = z.object({
  id: z.string().max(100),
  kind: z.enum(["daily", "weekly", "monthly"]),
  start: dateSchema,
  keep: note,
  improve: note,
  next: note,
});
export type Review = z.infer<typeof reviewSchema>;
export type Screenshot = {
  id: string;
  tradeId: string;
  kind: "before" | "after";
  name: string;
  mime: string;
  size: number;
};
export type Habit = { id: string; name: string };
export type Account = {
  id: string;
  name: string;
  currency: "USD";
  initialBalance: number;
  timezone: string;
};
export type JournalData = {
  account: Account;
  trades: Trade[];
  plan: Plan;
  journals: DailyJournal[];
  goals: Goal[];
  reviews: Review[];
  habits: Habit[];
  screenshots: Screenshot[];
  settings: Record<string, unknown>;
};
export const money = (n: number) =>
  new Intl.NumberFormat("en-US", {
    style: "currency",
    currency: "USD",
    maximumFractionDigits: 2,
  }).format(n);
export const signed = (n: number) => (n > 0 ? "+" : "") + money(n);
export const round = (n: number) =>
  (Math.sign(n) * Math.round((Math.abs(n) + Number.EPSILON) * 100)) / 100;
export function calc(t: Trade) {
  const sign = t.direction === "Long" ? 1 : -1,
    gross = sign * (t.exit - t.entry) * t.quantity * t.pointValue,
    pnl = round(gross - t.commissions),
    risk = Math.abs(t.entry - t.stop) * t.quantity * t.pointValue;
  return {
    gross,
    pnl,
    risk,
    rr: Math.abs(t.target - t.entry) / Math.abs(t.entry - t.stop),
    r: risk ? pnl / risk : 0,
    duration: (Date.parse(t.exitedAt) - Date.parse(t.enteredAt)) / 60000,
    status: pnl > 0 ? "Win" : pnl < 0 ? "Loss" : "Break Even",
  };
}
const dateFormatters = new Map<string, Intl.DateTimeFormat>();
const timeFormatters = new Map<string, Intl.DateTimeFormat>();
const dateKeys = new Map<string, string>();
export function dateKey(
  value: string | Date = new Date(),
  timezone = defaultPlan.timezone,
) {
  const key =
    timezone + "|" + (typeof value === "string" ? value : value.toISOString());
  const cached = dateKeys.get(key);
  if (cached) return cached;
  let formatter = dateFormatters.get(timezone);
  if (!formatter) {
    formatter = new Intl.DateTimeFormat("en-CA", {
      timeZone: timezone,
      year: "numeric",
      month: "2-digit",
      day: "2-digit",
    });
    if (dateFormatters.size >= 64) dateFormatters.clear();
    dateFormatters.set(timezone, formatter);
  }
  const result = formatter.format(new Date(value));
  if (dateKeys.size >= 10_000) dateKeys.clear();
  dateKeys.set(key, result);
  return result;
}
export function shiftDay(day: string, n: number) {
  const d = new Date(day + "T12:00:00Z");
  d.setUTCDate(d.getUTCDate() + n);
  return d.toISOString().slice(0, 10);
}
export function periodRange(period: string, anchor: string) {
  let start = anchor,
    end = shiftDay(anchor, 1);
  const d = new Date(anchor + "T12:00:00Z");
  if (period === "Week" || period === "weekly") {
    start = shiftDay(anchor, -((d.getUTCDay() + 6) % 7));
    end = shiftDay(start, 7);
  } else if (period === "Month" || period === "monthly") {
    start = anchor.slice(0, 7) + "-01";
    end = new Date(Date.UTC(d.getUTCFullYear(), d.getUTCMonth() + 1, 1))
      .toISOString()
      .slice(0, 10);
  } else if (period === "3M")
    start = new Date(Date.UTC(d.getUTCFullYear(), d.getUTCMonth() - 2, 1))
      .toISOString()
      .slice(0, 10);
  else if (period === "Year") start = anchor.slice(0, 4) + "-01-01";
  else if (period === "All") {
    start = "1900-01-01";
    end = "9999-12-31";
  }
  return { start, end };
}
export function inPeriod(
  trades: Trade[],
  period: string,
  timezone: string,
  anchor = dateKey(new Date(), timezone),
) {
  const { start, end } = periodRange(period, anchor);
  return trades.filter((t) => {
    const day = dateKey(t.exitedAt, timezone);
    return day >= start && day < end;
  });
}
export function stats(trades: Trade[]) {
  const values = [...trades]
      .sort((a, b) => Date.parse(a.exitedAt) - Date.parse(b.exitedAt))
      .map(calc),
    wins = values.filter((x) => x.pnl > 0),
    losses = values.filter((x) => x.pnl < 0),
    gain = wins.reduce((s, t) => s + t.pnl, 0),
    loss = losses.reduce((s, t) => s - t.pnl, 0);
  let streak = 0;
  for (let i = values.length - 1; i >= 0; i--) {
    const s = Math.sign(values[i].pnl);
    if (s === 0 || (streak && Math.sign(streak) !== s)) break;
    streak += s;
  }
  return {
    pnl: round(gain - loss),
    count: trades.length,
    winRate: trades.length ? (wins.length / trades.length) * 100 : 0,
    wins: wins.length,
    losses: losses.length,
    be: values.length - wins.length - losses.length,
    profitFactor: loss ? gain / loss : null,
    avgWin: wins.length ? gain / wins.length : 0,
    avgLoss: losses.length ? -loss / losses.length : 0,
    rr: values.length
      ? values.reduce((s, t) => s + t.rr, 0) / values.length
      : 0,
    r: values.length ? values.reduce((s, t) => s + t.r, 0) / values.length : 0,
    best: values.length ? Math.max(...values.map((t) => t.pnl)) : 0,
    worst: values.length ? Math.min(...values.map((t) => t.pnl)) : 0,
    streak,
  };
}
export function dailyResults(trades: Trade[], timezone: string) {
  const map: Record<string, Trade[]> = {};
  for (const t of trades) (map[dateKey(t.exitedAt, timezone)] ??= []).push(t);
  return Object.entries(map)
    .sort(([a], [b]) => a.localeCompare(b))
    .map(([date, ts]) => ({ date, ...stats(ts), trades: ts }));
}
export function checkPlan(t: Trade, plan: Plan, history: Trade[]) {
  let timeFormatter = timeFormatters.get(plan.timezone);
  if (!timeFormatter) {
    timeFormatter = new Intl.DateTimeFormat("en-GB", {
      timeZone: plan.timezone,
      hour: "2-digit",
      minute: "2-digit",
      hourCycle: "h23",
    });
    if (timeFormatters.size >= 64) timeFormatters.clear();
    timeFormatters.set(plan.timezone, timeFormatter);
  }
  const enteredMs = Date.parse(t.enteredAt);
  const issues: string[] = [],
    m = calc(t),
    time = timeFormatter.format(new Date(t.enteredAt)),
    allowed =
      plan.start === plan.end
        ? true
        : plan.start < plan.end
          ? time >= plan.start && time < plan.end
          : time >= plan.start || time < plan.end,
    day = dateKey(t.enteredAt, plan.timezone),
    prior = history.filter(
      (x) =>
        x.id !== t.id &&
        (Date.parse(x.enteredAt) < enteredMs ||
          (Date.parse(x.enteredAt) === enteredMs && x.id < t.id)) &&
        dateKey(x.enteredAt, plan.timezone) === day,
    ),
    realized = history
      .filter(
        (x) =>
          x.id !== t.id &&
          Date.parse(x.exitedAt) <= enteredMs &&
          dateKey(x.exitedAt, plan.timezone) === day,
      )
      .reduce((s, x) => s + calc(x).pnl, 0);
  if (
    !plan.instruments
      .map((x) => x.toUpperCase())
      .includes(t.instrument.toUpperCase())
  )
    issues.push("Instrumento no permitido");
  if (!allowed) issues.push("Fuera del horario permitido");
  if (m.risk > plan.maxRisk + 1e-8)
    issues.push("Riesgo por operación excedido");
  if (prior.length >= plan.maxTrades)
    issues.push("Máximo de trades diarios excedido");
  if (realized <= -plan.maxDailyLoss)
    issues.push("Pérdida diaria máxima ya alcanzada");
  if (m.rr + 1e-8 < plan.minRR) issues.push("Risk/Reward inferior al mínimo");
  if (!plan.setups.includes(t.setup)) issues.push("Setup no permitido");
  if (t.psychology.adherence < 4)
    issues.push("Incumplimiento de reglas personales declarado");
  return issues;
}
export function goalProgress(g: Goal, data: JournalData) {
  const today = dateKey(new Date(), data.plan.timezone),
    end = g.end < today ? g.end : today,
    days = dailyResults(
      g.kind === "consistency"
        ? data.trades
        : data.trades.map((t) => ({ ...t, exitedAt: t.enteredAt })),
      data.plan.timezone,
    );
  let count = 0,
    streak = 0,
    best = 0;
  for (let day = g.start; day <= end; day = shiftDay(day, 1)) {
    const row = days.find((x) => x.date === day);
    let ok = false;
    if (g.kind === "journal") ok = data.journals.some((j) => j.date === day);
    else if (row) {
      if (g.kind === "plan") ok = row.trades.every((t) => !t.violations.length);
      if (g.kind === "risk")
        ok = row.trades.every((t) => calc(t).risk <= g.threshold);
      if (g.kind === "overtrade")
        ok =
          row.count <= g.threshold &&
          !row.trades.some((t) => t.psychology.overtraded);
      if (g.kind === "consistency") ok = row.pnl >= g.threshold;
    }
    if (ok) {
      count++;
      streak++;
      best = Math.max(best, streak);
    } else if (g.kind === "journal" ? day < today : !!row) streak = 0;
  }
  return {
    count,
    streak,
    best,
    percent: Math.min(100, (count / g.target) * 100),
  };
}
export function emptyData(): JournalData {
  return {
    account: {
      id: "primary",
      name: "Cuenta principal",
      currency: "USD",
      initialBalance: 10000,
      timezone: defaultPlan.timezone,
    },
    trades: [],
    plan: structuredClone(defaultPlan),
    journals: [],
    goals: [],
    reviews: [],
    habits: [],
    screenshots: [],
    settings: {},
  };
}
export function newTrade(
  accountId: string,
  defaults: Partial<Trade> = {},
): Trade {
  const now = new Date().toISOString();
  return {
    id: crypto.randomUUID(),
    accountId,
    enteredAt: now,
    exitedAt: now,
    instrument: "MES",
    direction: "Long",
    entry: 0,
    exit: 0,
    stop: 0,
    target: 0,
    quantity: 1,
    pointValue: 5,
    commissions: 1.24,
    setup: "Pullback",
    timeframe: "5m",
    session: "New York",
    notes: "",
    psychology: psychologySchema.parse({}),
    violations: [],
    ...defaults,
  };
}
export function reconcileTrades(trades: Trade[], fallback: Plan) {
  const plans = new Map<string, Plan>();
  return trades.map((t) => {
    const key = JSON.stringify(t.planSnapshot) ?? "default";
    let plan = plans.get(key);
    if (!plan) {
      const parsed = planSchema.safeParse(t.planSnapshot);
      plan = parsed.success ? parsed.data : fallback;
      plans.set(key, plan);
    }
    return { ...t, planSnapshot: plan, violations: checkPlan(t, plan, trades) };
  });
}
