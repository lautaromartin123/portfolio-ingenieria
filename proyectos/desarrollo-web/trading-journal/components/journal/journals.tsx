"use client";
import { useEffect, useState } from "react";
import {
  Save,
  Plus,
  Pencil,
  Trash2,
  CheckCircle2,
  Sun,
  NotebookPen,
  Heart,
  ChevronLeft,
  ChevronRight,
  ShieldCheck,
} from "lucide-react";
import { Button, Choice, Field, Check, Note, Panel, Metric, Empty } from "./ui";
import { api } from "@/lib/api";
import {
  dailySchema,
  defaultPlan,
  dateKey,
  emotions,
  planSchema,
  stats,
  shiftDay,
  type DailyJournal,
  type JournalData,
  type Plan,
} from "@/lib/domain";
import { toast } from "sonner";
import {
  Dialog,
  DialogContent,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";
export type ViewProps = {
  data: JournalData;
  refresh: () => Promise<void>;
  demo: boolean;
  confirm: (title: string, action: () => Promise<void>) => void;
};
export function useUnsaved(dirty: boolean) {
  useEffect(() => {
    window.dispatchEvent(new CustomEvent("journal-dirty", { detail: dirty }));
    const handler = (e: BeforeUnloadEvent) => {
      if (dirty) {
        e.preventDefault();
        e.returnValue = "";
      }
    };
    window.addEventListener("beforeunload", handler);
    return () => {
      window.removeEventListener("beforeunload", handler);
      window.dispatchEvent(new CustomEvent("journal-dirty", { detail: false }));
    };
  }, [dirty]);
}
export function Daily({
  data,
  refresh,
  demo,
  confirm,
  date,
  setDate,
}: ViewProps & { date: string; setDate: (d: string) => void }) {
  const existing = data.journals.find((j) => j.date === date),
    [journal, setJournal] = useState<DailyJournal>(
      () =>
        existing ?? {
          id: crypto.randomUUID(),
          date,
          mood: "Calma",
          energy: 3,
          sleep: 3,
          objective: "",
          well: "",
          improve: "",
          learned: "",
          reflection: "",
          gratitude: "",
          tasks: [],
          habits: [],
        },
    ),
    [saved, setSaved] = useState(JSON.stringify(journal)),
    [busy, setBusy] = useState(false),
    [task, setTask] = useState(""),
    [habit, setHabit] = useState(""),
    [editHabit, setEditHabit] = useState<{ id: string; name: string } | null>(
      null,
    );
  useUnsaved(JSON.stringify(journal) !== saved);
  const update = <K extends keyof DailyJournal>(k: K, v: DailyJournal[K]) =>
    setJournal((j) => ({ ...j, [k]: v }));
  async function save(e: React.FormEvent) {
    e.preventDefault();
    if (demo) {
      toast.info("Salí de la demo para guardar tu journal.");
      return;
    }
    const result = dailySchema.safeParse(journal);
    if (!result.success) {
      toast.error(result.error.issues[0].message);
      return;
    }
    setBusy(true);
    try {
      await api("saveJournal", journal);
      setSaved(JSON.stringify(journal));
      await refresh();
      toast.success("Journal guardado");
    } catch (e) {
      toast.error((e as Error).message);
    } finally {
      setBusy(false);
    }
  }
  return (
    <>
      <div className="page-title">
        <div>
          <p className="eyebrow">MÁS ALLÁ DEL RESULTADO</p>
          <h1>Daily Journal</h1>
          <p>Un espacio para el trading, tus hábitos y tu vida.</p>
        </div>
        <div className="date-navigation">
          <Button
            variant="outline"
            size="icon"
            aria-label="Día anterior"
            onClick={() => setDate(shiftDay(date, -1))}
          >
            <ChevronLeft size={18} />
          </Button>
          <Field
            label="Día del journal"
            value={date}
            type="date"
            onChange={(v) => v && setDate(v)}
          />
          <Button
            variant="outline"
            size="icon"
            aria-label="Día siguiente"
            onClick={() => setDate(shiftDay(date, 1))}
          >
            <ChevronRight size={18} />
          </Button>
        </div>
      </div>
      <form onSubmit={save}>
        <div className="journal-layout">
          <div className="stack">
            <Panel
              title="¿Cómo llegás hoy?"
              action={<Sun size={20} className="muted" />}
            >
              <div className="panel-body">
                <div className="form-grid three">
                  <Choice
                    label="Estado de ánimo"
                    value={journal.mood}
                    onChange={(v) => update("mood", v)}
                    options={emotions}
                  />
                  <Choice
                    label="Energía (1–5)"
                    value={String(journal.energy)}
                    onChange={(v) => update("energy", +v)}
                    options={["1", "2", "3", "4", "5"]}
                  />
                  <Choice
                    label="Calidad del sueño (1–5)"
                    value={String(journal.sleep)}
                    onChange={(v) => update("sleep", +v)}
                    options={["1", "2", "3", "4", "5"]}
                  />
                </div>
                <Note
                  label="Mi objetivo principal"
                  value={journal.objective}
                  onChange={(v) => update("objective", v)}
                  placeholder="¿Qué haría que hoy sea un buen día?"
                />
              </div>
            </Panel>
            <Panel
              title="Mi reflexión"
              action={<NotebookPen size={20} className="muted" />}
            >
              <div className="panel-body">
                <div className="form-grid">
                  <Note
                    label="Qué hice bien"
                    value={journal.well}
                    onChange={(v) => update("well", v)}
                    placeholder="Decisiones que quiero repetir…"
                  />
                  <Note
                    label="Qué hice mal / qué mejorar"
                    value={journal.improve}
                    onChange={(v) => update("improve", v)}
                    placeholder="Algo concreto que puedo cambiar…"
                  />
                </div>
                <Note
                  label="Qué aprendí"
                  value={journal.learned}
                  onChange={(v) => update("learned", v)}
                  placeholder="Una idea para llevarme de hoy…"
                />
                <Note
                  label="Reflexión libre"
                  value={journal.reflection}
                  onChange={(v) => update("reflection", v)}
                  placeholder="Este espacio es tuyo. También en los días sin trading."
                />
                <Note
                  label="Gratitud"
                  value={journal.gratitude}
                  onChange={(v) => update("gratitude", v)}
                  placeholder="Hoy agradezco…"
                />
              </div>
            </Panel>
          </div>
          <div className="stack">
            <Panel title="Tareas importantes">
              <div className="panel-body">
                <div className="inline-add">
                  <input
                    aria-label="Nueva tarea"
                    placeholder="Agregar una tarea"
                    value={task}
                    maxLength={1000}
                    onChange={(e) => setTask(e.target.value)}
                    onKeyDown={(e) => {
                      if (e.key === "Enter") {
                        e.preventDefault();
                        if (task.trim()) {
                          update("tasks", [
                            ...journal.tasks,
                            { text: task.trim(), done: false },
                          ]);
                          setTask("");
                        }
                      }
                    }}
                  />
                  <Button
                    type="button"
                    size="icon"
                    variant="outline"
                    aria-label="Agregar tarea"
                    onClick={() => {
                      if (task.trim()) {
                        update("tasks", [
                          ...journal.tasks,
                          { text: task.trim(), done: false },
                        ]);
                        setTask("");
                      }
                    }}
                  >
                    <Plus size={16} />
                  </Button>
                </div>
                <div className="task-list">
                  {journal.tasks.map((t, i) => (
                    <div key={i}>
                      <Check
                        label={t.text}
                        value={t.done}
                        onChange={(done) =>
                          update(
                            "tasks",
                            journal.tasks.map((x, j) =>
                              i === j ? { ...x, done } : x,
                            ),
                          )
                        }
                      />
                      <button
                        type="button"
                        className="icon-button"
                        aria-label="Eliminar tarea"
                        onClick={() =>
                          update(
                            "tasks",
                            journal.tasks.filter((_, j) => j !== i),
                          )
                        }
                      >
                        <Trash2 size={14} />
                      </button>
                    </div>
                  ))}
                </div>
              </div>
            </Panel>
            <Panel title="Mis hábitos">
              <div className="panel-body">
                <p className="form-hint">
                  Pequeñas acciones, repetidas cada día.
                </p>
                <div className="task-list">
                  {data.habits.map((h) => (
                    <div key={h.id}>
                      <Check
                        label={h.name}
                        value={journal.habits.includes(h.id)}
                        onChange={(v) =>
                          update(
                            "habits",
                            v
                              ? [...journal.habits, h.id]
                              : journal.habits.filter((x) => x !== h.id),
                          )
                        }
                      />
                      <button
                        type="button"
                        className="icon-button"
                        aria-label={"Editar hábito " + h.name}
                        disabled={demo}
                        onClick={() => setEditHabit(h)}
                      >
                        <Pencil size={14} />
                      </button>
                      <button
                        type="button"
                        className="icon-button"
                        aria-label={"Eliminar hábito " + h.name}
                        disabled={demo}
                        onClick={() =>
                          confirm(
                            "Eliminar el hábito «" + h.name + "»",
                            async () => {
                              await api("deleteHabit", h.id);
                              await refresh();
                            },
                          )
                        }
                      >
                        <Trash2 size={14} />
                      </button>
                    </div>
                  ))}
                </div>
                <div className="inline-add">
                  <input
                    aria-label="Nuevo hábito"
                    placeholder="Nuevo hábito"
                    maxLength={100}
                    value={habit}
                    onChange={(e) => setHabit(e.target.value)}
                  />
                  <Button
                    type="button"
                    variant="outline"
                    size="icon"
                    disabled={demo}
                    aria-label="Agregar hábito"
                    onClick={async () => {
                      if (!habit.trim()) return;
                      try {
                        await api("saveHabit", {
                          id: crypto.randomUUID(),
                          name: habit,
                        });
                        setHabit("");
                        await refresh();
                        toast.success("Hábito agregado");
                      } catch (e) {
                        toast.error((e as Error).message);
                      }
                    }}
                  >
                    <Plus size={16} />
                  </Button>
                </div>
              </div>
            </Panel>
            <div className="journal-status">
              <CheckCircle2 size={18} />
              <span>
                {existing
                  ? "Journal completado"
                  : "Todavía no guardaste este día"}
                <small>
                  {JSON.stringify(journal) !== saved
                    ? "Tenés cambios sin guardar"
                    : "Tu reflexión cuenta, con o sin trades."}
                </small>
              </span>
            </div>
            <Button type="submit" disabled={busy || demo}>
              <Save size={17} />
              {busy ? "Guardando…" : "Guardar journal"}
            </Button>
            {existing && !demo && (
              <Button
                type="button"
                variant="ghost"
                onClick={() =>
                  confirm("Eliminar el journal del " + date, async () => {
                    await api("deleteJournal", date);
                    await refresh();
                    setJournal({
                      ...journal,
                      objective: "",
                      well: "",
                      improve: "",
                      learned: "",
                      reflection: "",
                      gratitude: "",
                      tasks: [],
                      habits: [],
                    });
                    setSaved("");
                  })
                }
              >
                <Trash2 size={15} />
                Eliminar entrada
              </Button>
            )}
          </div>
        </div>
      </form>
      {editHabit && (
        <HabitEditor
          habit={editHabit}
          close={() => setEditHabit(null)}
          refresh={refresh}
        />
      )}
    </>
  );
}
export function TradingPlan({ data, refresh, demo }: ViewProps) {
  const [p, setP] = useState<Plan>(data.plan),
    [instruments, setInstruments] = useState(p.instruments.join(", ")),
    [setups, setSetups] = useState(p.setups.join(", ")),
    [saved, setSaved] = useState(JSON.stringify(p)),
    [busy, setBusy] = useState(false);
  const effective = {
    ...p,
    instruments: instruments
      .split(",")
      .map((v) => v.trim().toUpperCase())
      .filter(Boolean),
    setups: setups
      .split(",")
      .map((v) => v.trim())
      .filter(Boolean),
  };
  useUnsaved(JSON.stringify(effective) !== saved);
  const update = <K extends keyof Plan>(k: K, v: Plan[K]) =>
    setP((x) => ({ ...x, [k]: v }));
  async function save(e: React.FormEvent) {
    e.preventDefault();
    const v = planSchema.safeParse(effective);
    if (!v.success) {
      toast.error(v.error.issues[0].message);
      return;
    }
    setBusy(true);
    try {
      await api("savePlan", v.data);
      setSaved(JSON.stringify(effective));
      await refresh();
      toast.success("Plan actualizado");
    } catch (e) {
      toast.error((e as Error).message);
    } finally {
      setBusy(false);
    }
  }
  return (
    <>
      <div className="page-title">
        <div>
          <p className="eyebrow">TU MARCO DE DECISIÓN</p>
          <h1>Plan de trading</h1>
          <p>Definí tus límites antes de entrar al mercado.</p>
        </div>
        <span className="badge good">
          <ShieldCheck size={15} />
          Evaluación automática
        </span>
      </div>
      <form onSubmit={save} className="plan-layout">
        <div className="stack">
          <Panel title="Mercados y horarios">
            <div className="panel-body">
              <Field
                label="Instrumentos permitidos · separados por comas"
                value={instruments}
                onChange={setInstruments}
                required
              />
              <Field
                label="Setups permitidos · separados por comas"
                value={setups}
                onChange={setSetups}
                required
              />
              <div className="form-grid">
                <Field
                  label="Horario desde"
                  value={p.start}
                  onChange={(v) => update("start", v)}
                  type="time"
                  required
                />
                <Field
                  label="Horario hasta"
                  value={p.end}
                  onChange={(v) => update("end", v)}
                  type="time"
                  required
                />
              </div>
              <Choice
                label="Zona horaria del plan y estadísticas"
                value={p.timezone}
                onChange={(v) => update("timezone", v)}
                options={[
                  ...new Set([
                    "America/Argentina/Buenos_Aires",
                    "America/New_York",
                    "America/Chicago",
                    "Europe/London",
                    "Europe/Madrid",
                    "America/Mexico_City",
                    "UTC",
                    p.timezone,
                  ]),
                ]}
              />
              <p className="form-hint">
                El horario final es excluyente. Si el inicio es posterior al
                fin, la sesión cruza medianoche; los límites diarios se
                reinician a medianoche. Horarios iguales permiten todo el día.
              </p>
            </div>
          </Panel>
          <Panel title="Reglas personales">
            <div className="panel-body">
              <Note
                label="Mi compromiso"
                value={p.rules}
                onChange={(v) => update("rules", v)}
                placeholder="Reglas claras, que pueda evaluar después de cada operación."
              />
              <p className="form-hint">
                El texto es tu guía. Para registrar una falta a estas reglas,
                asigná menos de 4/5 en «Respeto del plan» al trade.
              </p>
            </div>
          </Panel>
        </div>
        <div className="stack">
          <Panel title="Gestión del riesgo">
            <div className="panel-body">
              {(["maxRisk", "maxDailyLoss", "maxTrades", "minRR"] as const).map(
                (k, i) => (
                  <Field
                    key={k}
                    label={
                      [
                        "Riesgo máximo por trade (USD)",
                        "Pérdida máxima diaria (USD)",
                        "Máximo de trades por día",
                        "Risk/Reward mínimo",
                      ][i]
                    }
                    value={p[k]}
                    onChange={(v) => update(k, v === "" ? NaN : +v)}
                    type="number"
                    step={k === "maxTrades" ? "1" : "any"}
                    min={0}
                    required
                  />
                ),
              )}
            </div>
          </Panel>
          <div className="info-note">
            <ShieldCheck size={20} />
            <p>
              El plan se comprueba antes de guardar. Cada operación conserva la
              versión del plan utilizada, aunque después cambies tus reglas.
            </p>
          </div>
          <Button type="submit" disabled={busy || demo}>
            <Save size={16} />
            {busy ? "Guardando…" : "Guardar plan"}
          </Button>
        </div>
      </form>
    </>
  );
}

function HabitEditor({
  habit,
  close,
  refresh,
}: {
  habit: { id: string; name: string };
  close: () => void;
  refresh: () => Promise<void>;
}) {
  const [name, setName] = useState(habit.name),
    [busy, setBusy] = useState(false);
  return (
    <Dialog open onOpenChange={(v) => !v && !busy && close()}>
      <DialogContent>
        <DialogTitle>Editar hábito</DialogTitle>
        <DialogDescription>
          Actualizá el nombre. Se conserva el historial de cumplimiento.
        </DialogDescription>
        <form
          onSubmit={async (e) => {
            e.preventDefault();
            setBusy(true);
            try {
              await api("saveHabit", { ...habit, name });
              await refresh();
              close();
              toast.success("Hábito actualizado");
            } catch (e) {
              toast.error((e as Error).message);
            } finally {
              setBusy(false);
            }
          }}
        >
          <Field
            label="Nombre del hábito"
            value={name}
            onChange={setName}
            required
          />
          <div className="form-footer">
            <Button type="submit" disabled={busy}>
              {busy ? "Guardando…" : "Guardar hábito"}
            </Button>
          </div>
        </form>
      </DialogContent>
    </Dialog>
  );
}
