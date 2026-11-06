"use client";
import { useEffect, useState } from "react";
import {
  Target,
  Plus,
  Save,
  Pencil,
  Trash2,
  Flame,
  ArrowUpRight,
  CheckCircle2,
  ClipboardCheck,
} from "lucide-react";
import { Tabs, TabsList, TabsTrigger } from "@/components/ui/tabs";
import {
  Dialog,
  DialogContent,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";
import { Progress } from "@/components/ui/progress";
import { Button, Choice, Field, Note, Panel, Metric, Empty } from "./ui";
import { api } from "@/lib/api";
import {
  dateKey,
  goalProgress,
  goalSchema,
  inPeriod,
  money,
  periodRange,
  reviewSchema,
  shiftDay,
  signed,
  stats,
  type Goal,
  type Review,
} from "@/lib/domain";
import { toast } from "sonner";
import { type ViewProps, useUnsaved } from "./journals";
export function Reviews(
  props: ViewProps & { selection?: { kind: string; date: string } },
) {
  const [kind, setKind] = useState(props.selection?.kind ?? "weekly"),
    [date, setDate] = useState(
      props.selection?.date ?? dateKey(new Date(), props.data.plan.timezone),
    ),
    [hasChanges, setHasChanges] = useState(false);
  const stage = (fn: () => void) => {
    if (hasChanges)
      props.confirm("Cambiar de período sin guardar las notas", async () => {
        fn();
        setHasChanges(false);
      });
    else fn();
  };
  return (
    <>
      <div className="page-title">
        <div>
          <p className="eyebrow">CERRÁ EL CICLO</p>
          <h1>Reviews</h1>
          <p>Convertí tus registros en decisiones para el próximo período.</p>
        </div>
        <div className="button-row">
          <Tabs value={kind} onValueChange={(v) => stage(() => setKind(v))}>
            <TabsList>
              <TabsTrigger value="daily">Daily</TabsTrigger>
              <TabsTrigger value="weekly">Weekly</TabsTrigger>
              <TabsTrigger value="monthly">Monthly</TabsTrigger>
            </TabsList>
          </Tabs>
          <Field
            label="Fecha de referencia"
            type="date"
            value={date}
            onChange={(v) => v && stage(() => setDate(v))}
          />
        </div>
      </div>
      <ReviewEditor
        key={kind + periodRange(kind, date).start}
        {...props}
        kind={kind as Review["kind"]}
        date={date}
        onDirty={setHasChanges}
      />
    </>
  );
}
function ReviewEditor({
  data,
  refresh,
  demo,
  confirm,
  kind,
  date,
  onDirty,
}: ViewProps & {
  kind: Review["kind"];
  date: string;
  onDirty: (v: boolean) => void;
}) {
  const range = periodRange(kind, date),
    trades = inPeriod(data.trades, kind, data.plan.timezone, date),
    m = stats(trades),
    previousDate = shiftDay(range.start, -1),
    prior = stats(
      inPeriod(data.trades, kind, data.plan.timezone, previousDate),
    ),
    existing = data.reviews.find(
      (r) => r.kind === kind && r.start === range.start,
    ),
    [review, setReview] = useState<Review>(
      () =>
        existing ?? {
          id: crypto.randomUUID(),
          kind,
          start: range.start,
          keep: "",
          improve: "",
          next: "",
        },
    ),
    [saved, setSaved] = useState(JSON.stringify(review)),
    [busy, setBusy] = useState(false);
  useUnsaved(JSON.stringify(review) !== saved);
  useEffect(() => {
    onDirty(JSON.stringify(review) !== saved);
  }, [review, saved, onDirty]);
  const errors: Record<string, number> = {},
    rules: Record<string, number> = {};
  for (const t of trades) {
    if (t.psychology.error.trim())
      errors[t.psychology.error.trim()] =
        (errors[t.psychology.error.trim()] ?? 0) + 1;
    for (const v of t.violations) rules[v] = (rules[v] ?? 0) + 1;
  }
  const topError = Object.entries(errors).sort((a, b) => b[1] - a[1])[0],
    decision = [...trades]
      .filter((t) => t.psychology.lesson || t.notes)
      .sort((a, b) => b.psychology.adherence - a.psychology.adherence)[0];
  return (
    <>
      <div className="review-period">
        <ClipboardCheck size={17} />
        <span>
          {range.start} — {shiftDay(range.end, -1)}
        </span>
        <small>Resumen calculado a partir de tus registros</small>
      </div>
      <div className="stat-strip">
        <Metric
          label="P&L neto"
          value={signed(m.pnl)}
          positive={m.pnl >= 0}
          hint={`${signed(m.pnl - prior.pnl)} frente al período anterior`}
        />
        <Metric
          label="Operaciones"
          value={m.count}
          hint={`${m.count - prior.count >= 0 ? "+" : ""}${m.count - prior.count} frente al anterior`}
        />
        <Metric
          label="Win rate"
          value={m.winRate.toFixed(1) + "%"}
          hint={`${(m.winRate - prior.winRate).toFixed(1)} puntos porcentuales`}
        />
        <Metric
          label="Trades dentro del plan"
          value={
            (trades.length
              ? (trades.filter((t) => !t.violations.length).length /
                  trades.length) *
                100
              : 0
            ).toFixed(0) + "%"
          }
        />
      </div>
      <div className="two-columns">
        <Panel title="Lo que dicen los resultados">
          <div className="panel-body">
            <div className="detail-grid">
              <Metric
                label="Mejor trade"
                value={money(m.best)}
                positive={m.best >= 0}
              />
              <Metric
                label="Peor trade"
                value={money(m.worst)}
                positive={m.worst >= 0}
              />
            </div>
            <div className="review-insight">
              <span>Decisión destacada</span>
              <p>
                {decision
                  ? decision.psychology.lesson || decision.notes
                  : "Agregá notas y lecciones a tus trades para destacar decisiones."}
              </p>
              <small>
                {decision
                  ? `${decision.instrument} · mayor puntuación de respeto del plan (${decision.psychology.adherence}/5)`
                  : "Se selecciona por la evaluación que registraste."}
              </small>
            </div>
            <div className="review-insight">
              <span>Error más frecuente</span>
              <p>
                {topError
                  ? `${topError[0]} · ${topError[1]} veces`
                  : "Sin errores registrados en el período."}
              </p>
            </div>
          </div>
        </Panel>
        <Panel title="Reglas incumplidas">
          <div className="panel-body">
            {Object.keys(rules).length ? (
              Object.entries(rules)
                .sort((a, b) => b[1] - a[1])
                .map(([rule, count]) => (
                  <div className="rule-row" key={rule}>
                    <span>{rule}</span>
                    <b>{count}</b>
                  </div>
                ))
            ) : (
              <Empty
                title={
                  trades.length
                    ? "Todas las operaciones dentro del plan"
                    : "Sin operaciones en este período"
                }
                description="La revisión se actualiza automáticamente al registrar trades."
              />
            )}
          </div>
        </Panel>
      </div>
      <form
        className="section-gap"
        onSubmit={async (e) => {
          e.preventDefault();
          if (demo) return;
          const v = reviewSchema.safeParse(review);
          if (!v.success) {
            toast.error(v.error.issues[0].message);
            return;
          }
          setBusy(true);
          try {
            await api("saveReview", review);
            setSaved(JSON.stringify(review));
            await refresh();
            toast.success("Review guardada");
          } catch (e) {
            toast.error((e as Error).message);
          } finally {
            setBusy(false);
          }
        }}
      >
        <Panel title="Mi próxima versión">
          <div className="panel-body">
            <div className="form-grid three">
              {(["keep", "improve", "next"] as const).map((k, i) => (
                <Note
                  key={k}
                  label={
                    ["Qué mantener", "Qué mejorar", "Objetivo siguiente"][i]
                  }
                  value={review[k]}
                  onChange={(v) => setReview((r) => ({ ...r, [k]: v }))}
                  placeholder={
                    [
                      "¿Qué decisiones voy a repetir?",
                      "¿Qué cambio concreto voy a hacer?",
                      "¿Cuál será mi foco?",
                    ][i]
                  }
                />
              ))}
            </div>
            <div className="form-footer">
              {existing && !demo && (
                <Button
                  type="button"
                  variant="ghost"
                  onClick={() =>
                    confirm("Eliminar las notas de esta review", async () => {
                      await api("deleteReview", existing.id);
                      await refresh();
                      const blank = {
                        ...review,
                        keep: "",
                        improve: "",
                        next: "",
                      };
                      setReview(blank);
                      setSaved(JSON.stringify(blank));
                    })
                  }
                >
                  <Trash2 size={15} />
                  Eliminar notas
                </Button>
              )}
              <Button type="submit" disabled={busy || demo}>
                <Save size={16} />
                {busy ? "Guardando…" : "Guardar review"}
              </Button>
            </div>
          </div>
        </Panel>
      </form>
    </>
  );
}
const kinds = [
  { value: "plan", label: "Días operando dentro del plan" },
  { value: "journal", label: "Días con journal completado" },
  { value: "risk", label: "Días respetando el riesgo máximo" },
  { value: "overtrade", label: "Días sin sobreoperar" },
  { value: "consistency", label: "Días alcanzando un P&L mínimo" },
];
export function Goals({ data, refresh, demo, confirm }: ViewProps) {
  const [editing, setEditing] = useState<Goal | null>(null),
    [showArchived, setShowArchived] = useState(false);
  const today = dateKey(new Date(), data.plan.timezone);
  return (
    <>
      <div className="page-title">
        <div>
          <p className="eyebrow">EL PROGRESO ES UN HÁBITO</p>
          <h1>Objetivos</h1>
          <p>Medí el compromiso con tu proceso.</p>
        </div>
        <Button
          onClick={() =>
            setEditing({
              id: crypto.randomUUID(),
              title: "",
              kind: "plan",
              target: 20,
              threshold: 100,
              start: today,
              end: shiftDay(today, 30),
              archived: false,
            })
          }
        >
          <Plus size={17} />
          Nuevo objetivo
        </Button>
      </div>
      <div className="button-row section-bottom">
        <Button
          variant={!showArchived ? "secondary" : "ghost"}
          onClick={() => setShowArchived(false)}
        >
          Activos
        </Button>
        <Button
          variant={showArchived ? "secondary" : "ghost"}
          onClick={() => setShowArchived(true)}
        >
          Archivados
        </Button>
      </div>
      <div className="goal-grid">
        {data.goals
          .filter((g) => g.archived === showArchived)
          .map((g) => {
            const p = goalProgress(g, data);
            return (
              <Panel key={g.id}>
                <div className="goal-card">
                  <div className="goal-card-top">
                    <span className="focus-icon">
                      {p.percent === 100 ? (
                        <CheckCircle2 size={22} />
                      ) : (
                        <Target size={22} />
                      )}
                    </span>
                    <div className="button-row">
                      <button
                        className="icon-button"
                        aria-label={"Editar " + g.title}
                        onClick={() => setEditing(g)}
                      >
                        <Pencil size={16} />
                      </button>
                      <button
                        className="icon-button"
                        aria-label={"Eliminar " + g.title}
                        disabled={demo}
                        onClick={() =>
                          confirm(
                            "Eliminar el objetivo «" + g.title + "»",
                            async () => {
                              await api("deleteGoal", g.id);
                              await refresh();
                            },
                          )
                        }
                      >
                        <Trash2 size={16} />
                      </button>
                    </div>
                  </div>
                  <h2>{g.title}</h2>
                  <p>
                    {kinds.find((k) => k.value === g.kind)?.label}
                    {["risk", "overtrade", "consistency"].includes(g.kind)
                      ? " · " +
                        (g.kind === "overtrade"
                          ? g.threshold + " trades"
                          : money(g.threshold))
                      : ""}
                  </p>
                  <div className="goal-numbers">
                    <strong>
                      {p.count}
                      <span> / {g.target} días</span>
                    </strong>
                    <b>{p.percent.toFixed(0)}%</b>
                  </div>
                  <Progress value={p.percent} />
                  <div className="goal-streak">
                    <Flame size={15} />
                    <span>Racha: {p.streak} días</span>
                    <span>Mejor: {p.best}</span>
                  </div>
                  <small>
                    {g.start} → {g.end}
                  </small>
                  <Button
                    variant="ghost"
                    disabled={demo}
                    onClick={async () => {
                      try {
                        await api("saveGoal", { ...g, archived: !g.archived });
                        await refresh();
                        toast.success(
                          g.archived
                            ? "Objetivo reactivado"
                            : "Objetivo archivado",
                        );
                      } catch (e) {
                        toast.error((e as Error).message);
                      }
                    }}
                  >
                    {g.archived ? "Reactivar" : "Archivar"}
                  </Button>
                </div>
              </Panel>
            );
          })}
      </div>
      {!data.goals.some((g) => g.archived === showArchived) && (
        <Panel>
          <Empty
            title={
              showArchived
                ? "No hay objetivos archivados"
                : "Elegí en qué querés ser consistente"
            }
            description="Creá un objetivo medible. El progreso se actualiza con tus trades y tu journal."
            action={
              !showArchived && (
                <Button
                  onClick={() =>
                    setEditing({
                      id: crypto.randomUUID(),
                      title: "",
                      kind: "plan",
                      target: 20,
                      threshold: 100,
                      start: today,
                      end: shiftDay(today, 30),
                      archived: false,
                    })
                  }
                >
                  Crear objetivo
                </Button>
              )
            }
          />
        </Panel>
      )}
      <p className="form-hint section-gap">
        Los objetivos cuentan días cumplidos dentro de sus fechas. En trading,
        los días sin operaciones no cuentan ni rompen la racha; riesgo y
        disciplina usan la fecha de entrada, y P&L la de cierre. En journal, se
        cuentan todos los días y el día actual queda pendiente hasta terminar.
      </p>
      {editing && (
        <GoalEditor
          goal={editing}
          close={() => setEditing(null)}
          refresh={refresh}
          demo={demo}
        />
      )}
    </>
  );
}
function GoalEditor({
  goal,
  close,
  refresh,
  demo,
}: {
  goal: Goal;
  close: () => void;
  refresh: () => Promise<void>;
  demo: boolean;
}) {
  const [g, setG] = useState(goal),
    [busy, setBusy] = useState(false);
  return (
    <Dialog open onOpenChange={(v) => !v && !busy && close()}>
      <DialogContent>
        <DialogTitle>
          {goal.title ? "Editar objetivo" : "Nuevo objetivo"}
        </DialogTitle>
        <DialogDescription>
          Elegí una conducta que puedas sostener y medir.
        </DialogDescription>
        <form
          onSubmit={async (e) => {
            e.preventDefault();
            const v = goalSchema.safeParse(g);
            if (!v.success) {
              toast.error(v.error.issues[0].message);
              return;
            }
            setBusy(true);
            try {
              await api("saveGoal", g);
              await refresh();
              toast.success("Objetivo guardado");
              close();
            } catch (e) {
              toast.error((e as Error).message);
            } finally {
              setBusy(false);
            }
          }}
        >
          <div className="stack">
            <Field
              label="Nombre del objetivo"
              value={g.title}
              onChange={(title) => setG({ ...g, title })}
              required
            />
            <Choice
              label="Qué quiero medir"
              value={g.kind}
              onChange={(v) => setG({ ...g, kind: v as Goal["kind"] })}
              options={kinds}
            />
            <Field
              label="Días a cumplir"
              value={g.target}
              onChange={(v) => setG({ ...g, target: +v })}
              type="number"
              min={1}
              required
            />
            {["risk", "overtrade", "consistency"].includes(g.kind) && (
              <Field
                label={
                  g.kind === "overtrade"
                    ? "Máximo de trades diarios"
                    : g.kind === "risk"
                      ? "Riesgo máximo por trade (USD)"
                      : "P&L mínimo del día (USD)"
                }
                value={g.threshold}
                onChange={(v) => setG({ ...g, threshold: +v })}
                type="number"
                min={0}
                step="any"
                required
              />
            )}
            <div className="form-grid">
              <Field
                label="Desde"
                value={g.start}
                onChange={(start) => setG({ ...g, start })}
                type="date"
                required
              />
              <Field
                label="Hasta"
                value={g.end}
                onChange={(end) => setG({ ...g, end })}
                type="date"
                required
              />
            </div>
            <Button type="submit" disabled={busy || demo}>
              {busy ? "Guardando…" : "Guardar objetivo"}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
