"use client";
import { useState } from "react";
import {
  Save,
  Download,
  Upload,
  ShieldCheck,
  Database,
  SlidersHorizontal,
} from "lucide-react";
import { Button, Field, Panel, Check } from "./ui";
import { api } from "@/lib/api";
import { money, stats } from "@/lib/domain";
import { createBackup, restoreBackup } from "@/lib/backup";
import { type ViewProps } from "./journals";
import { toast } from "sonner";
export default function Settings({ data, refresh, demo, confirm }: ViewProps) {
  const [name, setName] = useState(data.account.name),
    [balance, setBalance] = useState(data.account.initialBalance),
    [widgets, setWidgets] = useState<string[]>(
      (data.settings.widgets as string[]) ?? [
        "equity",
        "calendar",
        "trades",
        "goals",
      ],
    ),
    [busy, setBusy] = useState(false);
  return (
    <>
      <div className="page-title">
        <div>
          <p className="eyebrow">A TU MANERA</p>
          <h1>Configuración</h1>
          <p>Tu cuenta, tu dashboard y tus copias de seguridad.</p>
        </div>
      </div>
      <div className="two-columns">
        <div className="stack">
          <Panel title="Cuenta de trading">
            <form
              className="panel-body"
              onSubmit={async (e) => {
                e.preventDefault();
                setBusy(true);
                try {
                  await api("saveAccount", { name, initialBalance: balance });
                  await refresh();
                  toast.success("Cuenta actualizada");
                } catch (e) {
                  toast.error((e as Error).message);
                } finally {
                  setBusy(false);
                }
              }}
            >
              <Field
                label="Nombre de la cuenta"
                value={name}
                onChange={setName}
                required
              />
              <Field
                label="Capital inicial (USD)"
                value={balance}
                onChange={(v) => setBalance(+v)}
                type="number"
                min={0}
                step="any"
                required
              />
              <p className="form-hint">
                Saldo registrado:{" "}
                {money(data.account.initialBalance + stats(data.trades).pnl)}.
                Capital inicial más P&L neto, en USD.
              </p>
              <Button type="submit" disabled={busy || demo}>
                <Save size={16} />
                Guardar cuenta
              </Button>
            </form>
          </Panel>
          <Panel title="Dashboard configurable">
            <div className="panel-body">
              <p className="form-hint">
                Elegí los bloques que querés ver. Las métricas principales
                siempre quedan visibles.
              </p>
              {[
                { id: "equity", label: "Curva de rendimiento" },
                { id: "goals", label: "Enfoque y objetivos" },
                { id: "trades", label: "Últimas operaciones" },
                { id: "calendar", label: "Calendario mensual" },
              ].map((w) => (
                <Check
                  key={w.id}
                  label={w.label}
                  value={widgets.includes(w.id)}
                  onChange={(v) =>
                    setWidgets(
                      v
                        ? [...widgets, w.id]
                        : widgets.filter((x) => x !== w.id),
                    )
                  }
                />
              ))}
              <Button
                disabled={busy || demo}
                onClick={async () => {
                  setBusy(true);
                  try {
                    await api("saveSettings", { ...data.settings, widgets });
                    await refresh();
                    toast.success("Dashboard actualizado");
                  } catch (e) {
                    toast.error((e as Error).message);
                  } finally {
                    setBusy(false);
                  }
                }}
              >
                Guardar preferencias
              </Button>
            </div>
          </Panel>
        </div>
        <div className="stack">
          <Panel
            title="Copias de seguridad"
            action={<Database size={19} className="muted" />}
          >
            <div className="panel-body">
              <p>
                Descargá tus operaciones, capturas, journal, hábitos, objetivos,
                reviews y configuración en un único archivo.
              </p>
              <Button
                variant="outline"
                disabled={busy || demo}
                onClick={async () => {
                  setBusy(true);
                  try {
                    await createBackup(data);
                    toast.success("Copia descargada");
                  } catch (e) {
                    toast.error((e as Error).message);
                  } finally {
                    setBusy(false);
                  }
                }}
              >
                <Download size={16} />
                {busy ? "Procesando…" : "Descargar backup completo"}
              </Button>
              <div className="section-divider" />
              <h3>Restaurar una copia</h3>
              <p className="form-hint">
                La copia se combina con tu cuenta. Los trades ya existentes se
                omiten; el journal y las reviews de fechas coincidentes se
                actualizan. También se restaura el plan y la configuración.
                Hasta 100 MB por copia; los registros se restauran por lotes.
              </p>
              <label
                className={`file-button ${busy || demo ? "disabled" : ""}`}
              >
                <Upload size={16} />
                Seleccionar backup
                <input
                  disabled={busy || demo}
                  type="file"
                  accept=".json,application/json"
                  onChange={(e) => {
                    const file = e.target.files?.[0];
                    e.target.value = "";
                    if (!file) return;
                    confirm(
                      "Restaurar «" +
                        file.name +
                        "» y actualizar las entradas coincidentes, el plan y la configuración",
                      async () => {
                        setBusy(true);
                        try {
                          const r = await restoreBackup(file);
                          await refresh();
                          toast.success("Registros restaurados");
                          if (r.failed)
                            toast.error(
                              `${r.failed} capturas no se pudieron restaurar. Conservá la copia original.`,
                            );
                        } finally {
                          setBusy(false);
                        }
                      },
                    );
                  }}
                />
              </label>
            </div>
          </Panel>
          <div className="info-note">
            <ShieldCheck size={23} />
            <p>
              Tu cuenta se identifica con tu inicio de sesión de ChatGPT. Los
              datos y capturas se guardan en tu espacio privado, disponibles al
              volver a abrir la aplicación.
            </p>
          </div>
        </div>
      </div>
    </>
  );
}
