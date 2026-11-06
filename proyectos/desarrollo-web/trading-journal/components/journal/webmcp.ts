"use client";
import { useEffect, useRef } from "react";
import { z } from "zod";
import { api } from "@/lib/api";
import {
  calc,
  newTrade,
  stats,
  tradeSchema,
  type JournalData,
} from "@/lib/domain";
type Tool = {
  name: string;
  title: string;
  description: string;
  inputSchema: object;
  annotations: { readOnlyHint: boolean; untrustedContentHint: boolean };
  execute: (input: unknown) => unknown;
};
export function useJournalTools(
  data: JournalData,
  demo: boolean,
  refresh: () => Promise<void>,
  navigate: (s: string) => void,
) {
  const ref = useRef({ data, demo, refresh, navigate });
  ref.current = { data, demo, refresh, navigate };
  useEffect(() => {
    const ctx = (
      document as Document & {
        modelContext?: {
          registerTool: (
            tool: Tool,
            options: { signal: AbortSignal },
          ) => Promise<void> | void;
        };
      }
    ).modelContext;
    if (!ctx?.registerTool) return;
    const lifecycle = new AbortController();
    const register = (tool: Tool) => {
      try {
        Promise.resolve(
          ctx.registerTool(tool, { signal: lifecycle.signal }),
        ).catch(() => {});
      } catch {}
    };
    register({
      name: "get_trading_summary",
      title: "Ver resumen de trading",
      description:
        "Devuelve el rendimiento de las operaciones visibles en la cuenta actual. No modifica datos.",
      inputSchema: {
        type: "object",
        properties: {},
        additionalProperties: false,
      },
      annotations: { readOnlyHint: true, untrustedContentHint: true },
      execute(input) {
        z.object({}).strict().parse(input);
        return { demo: ref.current.demo, ...stats(ref.current.data.trades) };
      },
    });
    register({
      name: "navigate_trading_journal",
      title: "Abrir sección del journal",
      description: "Navega a una sección. No crea ni guarda registros.",
      inputSchema: {
        type: "object",
        properties: {
          section: {
            type: "string",
            enum: [
              "dashboard",
              "trades",
              "daily",
              "plan",
              "analytics",
              "calendar",
              "reviews",
              "goals",
              "settings",
            ],
          },
        },
        required: ["section"],
        additionalProperties: false,
      },
      annotations: { readOnlyHint: false, untrustedContentHint: false },
      execute(input) {
        const { section } = z
          .object({
            section: z.enum([
              "dashboard",
              "trades",
              "daily",
              "plan",
              "analytics",
              "calendar",
              "reviews",
              "goals",
              "settings",
            ]),
          })
          .strict()
          .parse(input);
        ref.current.navigate(section);
        return { section };
      },
    });
    register({
      name: "record_closed_trade",
      title: "Registrar una operación cerrada",
      description:
        "Guarda una operación real, calcula P&L y evalúa el plan. Requiere sesión iniciada. Comisiones totales y valor por punto en USD.",
      inputSchema: {
        type: "object",
        properties: {
          instrument: { type: "string" },
          direction: { type: "string", enum: ["Long", "Short"] },
          entry: { type: "number" },
          exit: { type: "number" },
          stop: { type: "number" },
          target: { type: "number" },
          quantity: { type: "number" },
          pointValue: { type: "number" },
          commissions: { type: "number" },
          enteredAt: { type: "string", format: "date-time" },
          exitedAt: { type: "string", format: "date-time" },
          setup: { type: "string" },
          notes: { type: "string" },
        },
        required: [
          "instrument",
          "direction",
          "entry",
          "exit",
          "stop",
          "target",
          "quantity",
          "pointValue",
          "commissions",
          "enteredAt",
          "exitedAt",
          "setup",
        ],
        additionalProperties: false,
      },
      annotations: { readOnlyHint: false, untrustedContentHint: true },
      async execute(input) {
        if (ref.current.demo)
          throw new Error("Salí de la demo antes de guardar.");
        const args = z
          .object({
            instrument: z.string(),
            direction: z.enum(["Long", "Short"]),
            entry: z.number(),
            exit: z.number(),
            stop: z.number(),
            target: z.number(),
            quantity: z.number(),
            pointValue: z.number(),
            commissions: z.number(),
            enteredAt: z.string(),
            exitedAt: z.string(),
            setup: z.string(),
            notes: z.string().optional(),
          })
          .strict()
          .parse(input);
        const trade = tradeSchema.parse(
          newTrade(ref.current.data.account.id, args),
        );
        const saved = await api<{ trade: typeof trade }>("saveTrade", trade);
        await ref.current.refresh();
        return {
          id: saved.trade.id,
          ...calc(saved.trade),
          violations: saved.trade.violations,
        };
      },
    });
    return () => lifecycle.abort();
  }, []);
}
