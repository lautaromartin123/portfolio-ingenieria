import {
  sqliteTable,
  text,
  real,
  integer,
  index,
  uniqueIndex,
  check,
} from "drizzle-orm/sqlite-core";
import { sql } from "drizzle-orm";
export const users = sqliteTable("users", {
  id: text("id").primaryKey(),
  email: text("email").notNull(),
  name: text("name").notNull(),
});
export const accounts = sqliteTable(
  "trading_accounts",
  {
    id: text("id").primaryKey(),
    userId: text("user_id")
      .notNull()
      .references(() => users.id, { onDelete: "cascade" }),
    name: text("name").notNull(),
    currency: text("currency").notNull().default("USD"),
    initialBalance: real("initial_balance").notNull().default(10000),
    timezone: text("timezone").notNull(),
  },
  (t) => [
    index("idx_accounts_user").on(t.userId),
    check("balance_nonnegative", sql`${t.initialBalance} >= 0`),
  ],
);
export const setups = sqliteTable(
  "setups",
  {
    id: text("id").primaryKey(),
    userId: text("user_id")
      .notNull()
      .references(() => users.id, { onDelete: "cascade" }),
    accountId: text("account_id")
      .notNull()
      .references(() => accounts.id, { onDelete: "cascade" }),
    name: text("name").notNull(),
  },
  (t) => [uniqueIndex("idx_setup_account_name").on(t.accountId, t.name)],
);
export const plans = sqliteTable(
  "trading_plans",
  {
    id: text("id").primaryKey(),
    userId: text("user_id")
      .notNull()
      .references(() => users.id, { onDelete: "cascade" }),
    accountId: text("account_id")
      .notNull()
      .references(() => accounts.id, { onDelete: "cascade" }),
    payload: text("payload").notNull(),
    updatedAt: text("updated_at").notNull(),
  },
  (t) => [uniqueIndex("idx_plan_account").on(t.accountId)],
);
export const trades = sqliteTable(
  "trades",
  {
    id: text("id").primaryKey(),
    userId: text("user_id")
      .notNull()
      .references(() => users.id, { onDelete: "cascade" }),
    accountId: text("account_id")
      .notNull()
      .references(() => accounts.id, { onDelete: "cascade" }),
    setupId: text("setup_id").references(() => setups.id, {
      onDelete: "set null",
    }),
    enteredAt: text("entered_at").notNull(),
    exitedAt: text("exited_at").notNull(),
    instrument: text("instrument").notNull(),
    direction: text("direction").notNull(),
    quantity: real("quantity").notNull(),
    payload: text("payload").notNull(),
  },
  (t) => [
    index("idx_trades_user_exit").on(t.userId, t.exitedAt),
    index("idx_trades_account_entry").on(t.accountId, t.enteredAt),
    check("trade_direction", sql`${t.direction} IN ('Long','Short')`),
    check("trade_quantity_positive", sql`${t.quantity} > 0`),
  ],
);
export const psychology = sqliteTable("trade_psychology", {
  tradeId: text("trade_id")
    .primaryKey()
    .references(() => trades.id, { onDelete: "cascade" }),
  userId: text("user_id")
    .notNull()
    .references(() => users.id, { onDelete: "cascade" }),
  payload: text("payload").notNull(),
});
export const journals = sqliteTable(
  "daily_journals",
  {
    id: text("id").primaryKey(),
    userId: text("user_id")
      .notNull()
      .references(() => users.id, { onDelete: "cascade" }),
    date: text("date").notNull(),
    payload: text("payload").notNull(),
  },
  (t) => [uniqueIndex("idx_journal_user_date").on(t.userId, t.date)],
);
export const habits = sqliteTable(
  "habits",
  {
    id: text("id").primaryKey(),
    userId: text("user_id")
      .notNull()
      .references(() => users.id, { onDelete: "cascade" }),
    name: text("name").notNull(),
  },
  (t) => [index("idx_habit_user").on(t.userId)],
);
export const journalHabits = sqliteTable(
  "journal_habits",
  {
    id: text("id").primaryKey(),
    journalId: text("journal_id")
      .notNull()
      .references(() => journals.id, { onDelete: "cascade" }),
    habitId: text("habit_id")
      .notNull()
      .references(() => habits.id, { onDelete: "cascade" }),
  },
  (t) => [uniqueIndex("idx_journal_habit").on(t.journalId, t.habitId)],
);
export const goals = sqliteTable(
  "goals",
  {
    id: text("id").primaryKey(),
    userId: text("user_id")
      .notNull()
      .references(() => users.id, { onDelete: "cascade" }),
    accountId: text("account_id")
      .notNull()
      .references(() => accounts.id, { onDelete: "cascade" }),
    payload: text("payload").notNull(),
  },
  (t) => [index("idx_goal_user").on(t.userId)],
);
export const screenshots = sqliteTable(
  "screenshots",
  {
    id: text("id").primaryKey(),
    userId: text("user_id")
      .notNull()
      .references(() => users.id, { onDelete: "cascade" }),
    tradeId: text("trade_id")
      .notNull()
      .references(() => trades.id, { onDelete: "cascade" }),
    kind: text("kind").notNull(),
    name: text("name").notNull(),
    mime: text("mime").notNull(),
    size: integer("size").notNull(),
    objectKey: text("object_key").notNull(),
  },
  (t) => [
    index("idx_screenshot_user").on(t.userId),
    index("idx_screenshot_trade").on(t.tradeId),
    check("screenshot_kind", sql`${t.kind} IN ('before','after')`),
  ],
);
export const reviews = sqliteTable(
  "reviews",
  {
    id: text("id").primaryKey(),
    userId: text("user_id")
      .notNull()
      .references(() => users.id, { onDelete: "cascade" }),
    kind: text("kind").notNull(),
    start: text("start").notNull(),
    payload: text("payload").notNull(),
  },
  (t) => [uniqueIndex("idx_review_user_period").on(t.userId, t.kind, t.start)],
);
export const settings = sqliteTable("user_settings", {
  userId: text("user_id")
    .primaryKey()
    .references(() => users.id, { onDelete: "cascade" }),
  payload: text("payload").notNull(),
});
export const storageDeletions = sqliteTable(
  "storage_deletions",
  {
    objectKey: text("object_key").primaryKey(),
    userId: text("user_id")
      .notNull()
      .references(() => users.id, { onDelete: "cascade" }),
  },
  (t) => [index("idx_cleanup_user").on(t.userId)],
);
