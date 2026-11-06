import test from "node:test";
import assert from "node:assert/strict";
import {
  calc,
  newTrade,
  stats,
  tradeSchema,
  checkPlan,
  defaultPlan,
  dateKey,
  periodRange,
  goalProgress,
  emptyData,
  round,
  reconcileTrades,
} from "../lib/domain.ts";
import { imageInfo } from "../lib/server-validation.ts";
const base = newTrade("test", {
  entry: 100,
  exit: 110,
  stop: 95,
  target: 115,
  quantity: 2,
  pointValue: 1,
  commissions: 3,
});
test("Long: P&L neto, riesgo y R", () => {
  const m = calc(base);
  assert.equal(m.pnl, 17);
  assert.equal(m.risk, 10);
  assert.equal(m.r, 1.7);
});
const short = {
  ...base,
  direction: "Short",
  entry: 200,
  exit: 190,
  stop: 205,
  target: 185,
  quantity: 3,
  pointValue: 5,
  commissions: 10,
};
test("Short: dirección y multiplicador", () => {
  assert.equal(calc(short).pnl, 140);
  assert.equal(calc(short).risk, 75);
});
const loss = { ...base, exit: 94, commissions: 2 },
  be = { ...base, exit: 102, quantity: 1, commissions: 2 };
test("Deslizamiento y break even neto", () => {
  assert.equal(calc(loss).pnl, -14);
  assert.equal(calc(loss).r, -1.4);
  assert.equal(calc(be).status, "Break Even");
});
test("Estadísticas agregadas", () => {
  const s = stats([base, short, loss, be]);
  assert.equal(s.pnl, 143);
  assert.equal(s.winRate, 50);
  assert.ok(Math.abs(s.profitFactor - 157 / 14) < 1e-8);
});
test("Estadísticas vacías sin divisiones indefinidas", () => {
  const s = stats([]);
  assert.equal(s.pnl, 0);
  assert.equal(s.profitFactor, null);
  assert.equal(s.count, 0);
});
test("Stop del lado incorrecto", () =>
  assert.equal(tradeSchema.safeParse({ ...base, stop: 105 }).success, false));
test("Salida anterior a entrada", () =>
  assert.equal(
    tradeSchema.safeParse({ ...base, exitedAt: "2000-01-01T00:00:00.000Z" })
      .success,
    false,
  ));
test("NaN y tamaño cero", () => {
  assert.equal(tradeSchema.safeParse({ ...base, entry: NaN }).success, false);
  assert.equal(tradeSchema.safeParse({ ...base, quantity: 0 }).success, false);
});
test("Comisiones negativas", () =>
  assert.equal(
    tradeSchema.safeParse({ ...base, commissions: -1 }).success,
    false,
  ));
test("Medianoche en zona de Buenos Aires", () =>
  assert.equal(
    dateKey("2026-09-11T01:30:00Z", defaultPlan.timezone),
    "2026-09-10",
  ));
test("Semana desde lunes", () =>
  assert.deepEqual(periodRange("Week", "2026-09-10"), {
    start: "2026-09-07",
    end: "2026-09-14",
  }));
test("Mes bisiesto", () =>
  assert.deepEqual(periodRange("Month", "2024-02-29"), {
    start: "2024-02-01",
    end: "2024-03-01",
  }));
const p = {
    ...defaultPlan,
    instruments: ["MES"],
    maxRisk: 100,
    minRR: 2,
    start: "09:30",
    end: "16:00",
  },
  good = {
    ...base,
    id: "z-test",
    instrument: "MES",
    enteredAt: "2026-09-10T12:30:00.000Z",
    exitedAt: "2026-09-10T13:00:00.000Z",
  };
test("Trade permitido", () => assert.deepEqual(checkPlan(good, p, []), []));
test("Fin del horario excluyente", () =>
  assert.ok(
    checkPlan(
      { ...good, enteredAt: "2026-09-10T19:00:00.000Z" },
      p,
      [],
    ).includes("Fuera del horario permitido"),
  ));
test("Riesgo y cantidad máximos", () => {
  assert.ok(
    checkPlan({ ...good, quantity: 100 }, p, []).includes(
      "Riesgo por operación excedido",
    ),
  );
  assert.ok(
    checkPlan(good, { ...p, maxTrades: 1 }, [
      { ...good, id: "a-test" },
    ]).includes("Máximo de trades diarios excedido"),
  );
});
test("Editar excluye el trade del recuento previo", () =>
  assert.deepEqual(checkPlan(good, { ...p, maxTrades: 1 }, [good]), []));
test("Precisión de timestamps no cambia el orden temporal", () => {
  const earlier = { ...loss, exitedAt: "2026-09-10T13:00:00Z" },
    later = { ...base, exitedAt: "2026-09-10T13:00:00.500Z" };
  assert.equal(stats([later, earlier]).streak, 1);
});
test("Riesgo infinito rechazado", () =>
  assert.equal(
    tradeSchema.safeParse({
      ...base,
      entry: 1e308,
      exit: 1e308,
      stop: 1,
      target: 1.1e308,
      quantity: 1e8,
      pointValue: 1e8,
    }).success,
    false,
  ));
test("Redondeo simétrico", () => assert.equal(round(-1.005), -round(1.005)));
test("Planes iniciales independientes", () => {
  const a = emptyData(),
    b = emptyData();
  a.plan.instruments.push("TEST");
  assert.ok(!b.plan.instruments.includes("TEST"));
});
test("Cambiar historia recalcula el plan aplicado", () => {
  const low = { ...p, maxDailyLoss: 10 },
    prior = {
      ...loss,
      id: "a",
      enteredAt: "2026-09-10T12:00:00Z",
      exitedAt: "2026-09-10T12:30:00Z",
      planSnapshot: low,
    },
    next = {
      ...good,
      id: "b",
      enteredAt: "2026-09-10T12:30:00.500Z",
      planSnapshot: low,
    };
  const result = reconcileTrades([next, prior], p);
  assert.ok(
    result[0].violations.includes("Pérdida diaria máxima ya alcanzada"),
  );
});
test("Imagen truncada rechazada", () =>
  assert.equal(imageInfo(new Uint8Array([137, 80, 78, 71])), null));
test("PNG válido reconocido", () => {
  const png = Buffer.from(
    "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAwMCAO+aFQAAAABJRU5ErkJggg==",
    "base64",
  );
  assert.deepEqual(imageInfo(png), { mime: "image/png", width: 1, height: 1 });
});
