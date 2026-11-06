import test from "node:test";
import assert from "node:assert/strict";
import fs from "node:fs";
import ts from "typescript";
import { newTrade, calc } from "../lib/domain.ts";

// Execute the production CSV module without a browser or framework loader.
const source = fs.readFileSync(
  new URL("../lib/csv.ts", import.meta.url),
  "utf8",
);
const compiled = ts
  .transpileModule(source, {
    compilerOptions: {
      target: ts.ScriptTarget.ES2022,
      module: ts.ModuleKind.ES2022,
    },
  })
  .outputText.replace(
    '"./domain"',
    JSON.stringify(new URL("../lib/domain.ts", import.meta.url).href),
  );
const { exportCSV, importCSV, parseRows } = await import(
  "data:text/javascript;base64," + Buffer.from(compiled).toString("base64")
);
const trade = newTrade("test", {
  id: "csv-test",
  entry: 100,
  exit: 94,
  stop: 95,
  target: 115,
  quantity: 2,
  pointValue: 1,
  commissions: 3,
  notes: 'Paciencia, "confirmación"\nSegunda línea: emoción ✓',
});
test("CSV preserva comillas, saltos de línea, Unicode y pérdida neta", async () => {
  const { trades, errors } = await importCSV(exportCSV([trade]), "restored");
  assert.deepEqual(errors, []);
  assert.equal(trades[0].notes, trade.notes);
  assert.equal(trades[0].accountId, "restored");
  assert.equal(calc(trades[0]).pnl, -15);
  assert.deepEqual(trades[0].psychology, trade.psychology);
});
test("CSV neutraliza fórmulas y conserva el texto al importar", async () => {
  const formula = { ...trade, notes: "=1+1", setup: "+señal" };
  const csv = exportCSV([formula]);
  assert.ok(csv.includes("'=1+1"));
  const { trades, errors } = await importCSV(csv, "test");
  assert.deepEqual(errors, []);
  assert.equal(trades[0].notes, "=1+1");
  assert.equal(trades[0].setup, "+señal");
});
test("CSV rechaza número obligatorio vacío", async () => {
  const rows = parseRows(exportCSV([trade]));
  rows[1][rows[0].indexOf("entry")] = "";
  const csv = rows
    .map((row) => row.map((v) => JSON.stringify(v)).join(","))
    .join("\n");
  const result = await importCSV(csv, "test");
  assert.equal(result.trades.length, 0);
  assert.equal(result.errors.length, 1);
});
test("CSV sin ID produce una identidad estable", async () => {
  const csv = exportCSV([{ ...trade, id: "" }]);
  const a = await importCSV(csv, "test"),
    b = await importCSV(csv, "test");
  assert.deepEqual(a.errors, []);
  assert.equal(a.trades[0].id, b.trades[0].id);
});
test("CSV rechaza comillas sin cerrar y columnas faltantes", async () => {
  assert.throws(() => parseRows('a,b\n"sin cerrar,2'));
  await assert.rejects(
    importCSV("instrument,entry\nMES,100", "test"),
    /Faltan columnas/,
  );
});
test("CSV preserva apóstrofos literales e IDs de longitud máxima", async () => {
  const original = { ...trade, id: "-" + "x".repeat(99), notes: "'=SUM(1,2)" };
  const { trades, errors } = await importCSV(exportCSV([original]), "test");
  assert.deepEqual(errors, []);
  assert.equal(trades[0].id, original.id);
  assert.equal(trades[0].notes, original.notes);
});
