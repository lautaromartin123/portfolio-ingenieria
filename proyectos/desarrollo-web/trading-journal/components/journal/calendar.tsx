"use client";
import { useState } from "react";
import {
  ChevronLeft,
  ChevronRight,
  NotebookPen,
  CheckCircle2,
  ArrowUpRight,
} from "lucide-react";
import { Button, Panel, Empty } from "./ui";
import {
  dailyResults,
  dateKey,
  money,
  signed,
  stats,
  type JournalData,
  type Trade,
} from "@/lib/domain";
import { RecentTrades } from "./dashboard";
export default function Calendar({
  data,
  onTrade,
  onJournal,
}: {
  data: JournalData;
  onTrade: (t: Trade) => void;
  onJournal: (d: string) => void;
}) {
  const today = dateKey(new Date(), data.plan.timezone),
    [month, setMonth] = useState(today.slice(0, 7)),
    [selected, setSelected] = useState(today);
  const year = Number(month.slice(0, 4)),
    mon = Number(month.slice(5)),
    first = month + "-01",
    count = new Date(year, mon, 0).getDate(),
    offset = (new Date(first + "T12:00:00Z").getUTCDay() + 6) % 7,
    days = dailyResults(data.trades, data.plan.timezone),
    current = days.filter((d) => d.date.startsWith(month)),
    day = days.find((d) => d.date === selected),
    journal = data.journals.find((j) => j.date === selected);
  function change(n: number) {
    setMonth(
      new Date(Date.UTC(year, mon - 1 + n, 1)).toISOString().slice(0, 7),
    );
  }
  return (
    <>
      <div className="page-title">
        <div>
          <p className="eyebrow">CONSISTENCIA A SIMPLE VISTA</p>
          <h1>Calendario</h1>
          <p>Tus resultados y reflexiones, día por día.</p>
        </div>
        <div className="button-row">
          <Button
            variant="outline"
            size="icon"
            aria-label="Mes anterior"
            onClick={() => change(-1)}
          >
            <ChevronLeft size={17} />
          </Button>
          <b className="month-title">
            {new Date(first + "T12:00:00Z").toLocaleDateString("es-AR", {
              month: "long",
              year: "numeric",
            })}
          </b>
          <Button
            variant="outline"
            size="icon"
            aria-label="Mes siguiente"
            onClick={() => change(1)}
          >
            <ChevronRight size={17} />
          </Button>
          <Button
            variant="outline"
            onClick={() => {
              setMonth(today.slice(0, 7));
              setSelected(today);
            }}
          >
            Hoy
          </Button>
        </div>
      </div>
      <Panel>
        <div className="calendar-summary">
          <span>
            Resultado del mes{" "}
            <b
              className={
                current.reduce((s, d) => s + d.pnl, 0) >= 0
                  ? "positive"
                  : "negative"
              }
            >
              {signed(current.reduce((s, d) => s + d.pnl, 0))}
            </b>
          </span>
          <span>{current.reduce((s, d) => s + d.count, 0)} trades</span>
          <span>
            {current.filter((d) => d.pnl > 0).length} días positivos ·{" "}
            {current.filter((d) => d.pnl < 0).length} negativos
          </span>
        </div>
        <div className="full-calendar">
          {[
            "Lunes",
            "Martes",
            "Miércoles",
            "Jueves",
            "Viernes",
            "Sábado",
            "Domingo",
          ].map((d) => (
            <div key={d} className="weekday">
              <span>{d}</span>
              <small>{d.slice(0, 1)}</small>
            </div>
          ))}
          {Array.from({ length: offset }, (_, i) => (
            <div className="calendar-blank" key={"blank" + i} />
          ))}
          {Array.from({ length: count }, (_, i) => {
            const d = month + "-" + String(i + 1).padStart(2, "0"),
              r = days.find((x) => x.date === d),
              j = data.journals.some((x) => x.date === d);
            return (
              <button
                key={d}
                className={`calendar-cell ${r?.pnl ? (r.pnl > 0 ? "calendar-positive" : "calendar-negative") : ""} ${selected === d ? "selected" : ""} ${today === d ? "today" : ""}`}
                onClick={() => setSelected(d)}
                aria-label={`${d}, ${r ? signed(r.pnl) + ", " + r.count + " trades" : "sin trades"}${j ? ", journal completado" : ""}`}
              >
                <span className="day-number">
                  {i + 1}
                  {j && <NotebookPen size={14} />}
                </span>
                {r && (
                  <>
                    <b>{signed(r.pnl)}</b>
                    <span className="day-stats">
                      {r.count} trades{" "}
                      <small>
                        {r.wins}W · {r.losses}L{r.be ? " · " + r.be + "BE" : ""}
                      </small>
                    </span>
                  </>
                )}
              </button>
            );
          })}
        </div>
        <div className="calendar-legend">
          <span>
            <NotebookPen size={14} />
            Journal completado
          </span>
          <span>Resultados por fecha de cierre · {data.plan.timezone}</span>
        </div>
      </Panel>
      <div className="two-columns section-gap">
        <Panel
          title={"Operaciones · " + selected}
          sub={
            day
              ? `${day.count} trades · ${signed(day.pnl)}`
              : "Día sin operaciones"
          }
        >
          {day ? (
            <RecentTrades trades={day.trades} onTrade={onTrade} />
          ) : (
            <Empty
              title="Un día sin trading también cuenta"
              description="Podés registrar hábitos y reflexiones igualmente."
            />
          )}
        </Panel>
        <Panel
          title="Daily Journal"
          action={
            <Button variant="outline" onClick={() => onJournal(selected)}>
              {journal ? "Editar" : "Escribir"}
              <ArrowUpRight size={15} />
            </Button>
          }
        >
          <div className="panel-body">
            {journal ? (
              <>
                <span className="badge good">
                  <CheckCircle2 size={13} />
                  Completado
                </span>
                <div className="journal-preview">
                  <h3>{journal.objective || "Reflexión del día"}</h3>
                  <p>
                    {journal.reflection ||
                      journal.learned ||
                      "Sin reflexión libre."}
                  </p>
                  <small>
                    {journal.mood} · Energía {journal.energy}/5 · Sueño{" "}
                    {journal.sleep}/5
                  </small>
                </div>
              </>
            ) : (
              <Empty
                title="Reservá un momento para vos"
                description="¿Qué salió bien? ¿Qué aprendiste hoy?"
              />
            )}
          </div>
        </Panel>
      </div>
    </>
  );
}
