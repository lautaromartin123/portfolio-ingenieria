import { z } from "zod";
import {
  tradeSchema,
  planSchema,
  dailySchema,
  goalSchema,
  reviewSchema,
} from "./domain";

// The whole file is validated before restoring any batch. Per-request limits
// belong to the server route, not to the size of a user's journal history.
export const backupDataSchema = z.object({
  account: z.object({
    name: z.string().min(1).max(100),
    initialBalance: z.number().finite().min(0).max(1e12),
  }),
  plan: planSchema,
  trades: z.array(tradeSchema),
  journals: z.array(dailySchema),
  goals: z.array(goalSchema),
  reviews: z.array(reviewSchema),
  habits: z.array(
    z.object({
      id: z.string().min(1).max(100),
      name: z.string().min(1).max(100),
    }),
  ),
  settings: z.object({
    widgets: z
      .array(z.enum(["equity", "calendar", "trades", "goals"]))
      .optional(),
    defaults: z
      .object({
        instrument: z.string().max(30),
        pointValue: z.number().finite().positive().max(1e8),
        quantity: z.number().finite().positive().max(1e8),
        commissions: z.number().finite().min(0).max(1e12),
        setup: z.string().max(100),
        timeframe: z.string().max(30),
        session: z.string().max(60),
      })
      .optional(),
  }),
});
