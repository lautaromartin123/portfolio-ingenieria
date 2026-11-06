import {
  checkPlan,
  dateKey,
  emptyData,
  newTrade,
  shiftDay,
  type JournalData,
} from "./domain";
export function demoData(): JournalData {
  const data = emptyData();
  data.account.name = "Cuenta demo";
  const today = dateKey(new Date(), data.plan.timezone);
  data.habits = [
    { id: "demo-h1", name: "Preparar la sesión" },
    { id: "demo-h2", name: "Hacer ejercicio" },
    { id: "demo-h3", name: "Meditar 10 minutos" },
  ];
  for (let i = 40; i >= 0; i--) {
    const day = shiftDay(today, -i);
    if ([0, 6].includes(new Date(day + "T12:00:00Z").getUTCDay())) continue;
    for (let j = 0; j < 2 + (i % 2); j++) {
      const win = (i + j) % 4 !== 0,
        direction = (i + j) % 3 === 0 ? "Short" : "Long",
        entry = 6500 + i * 2,
        t = newTrade("primary", {
          id: `demo-${i}-${j}`,
          enteredAt: day + `T${String(13 + j).padStart(2, "0")}:30:00.000Z`,
          exitedAt: day + `T${String(13 + j).padStart(2, "0")}:55:00.000Z`,
          entry,
          exit:
            entry + (direction === "Long" ? 1 : -1) * (win ? 8 + (i % 5) : -5),
          stop: entry + (direction === "Long" ? -5 : 5),
          target: entry + (direction === "Long" ? 12 : -12),
          quantity: 2,
          direction,
          setup: ["Pullback", "Breakout", "Reversal"][i % 3],
          notes: win
            ? "Esperé la confirmación y ejecuté el setup."
            : "Revisar el contexto antes de entrar.",
        });
      t.psychology.confidence = 3 + (i % 3);
      t.psychology.quality = 3 + (j % 3);
      t.psychology.impulsive = !win && i % 3 === 0;
      t.psychology.adherence = t.psychology.impulsive ? 2 : 5;
      t.psychology.error = !win ? "Entrada anticipada" : "";
      t.psychology.lesson = "Esperar el cierre de la vela de confirmación.";
      t.violations = checkPlan(t, data.plan, data.trades);
      if (Date.parse(t.exitedAt) <= Date.now()) data.trades.push(t);
    }
    if (i % 3 !== 0)
      data.journals.push({
        id: `demo-j-${i}`,
        date: day,
        mood: "Calma",
        energy: 4,
        sleep: 4,
        objective: "Priorizar buenas decisiones.",
        well: "Respeté el riesgo inicial.",
        improve: "Reducir distracciones.",
        learned: "La paciencia mejora mis entradas.",
        reflection: "Una sesión ordenada. El proceso es mi prioridad.",
        gratitude: "Tiempo para aprender.",
        tasks: [],
        habits: ["demo-h1", "demo-h2"],
      });
  }
  data.goals = [
    {
      id: "demo-g-1",
      title: "Operar dentro del plan",
      kind: "plan",
      target: 20,
      threshold: 0,
      start: shiftDay(today, -30),
      end: shiftDay(today, 15),
      archived: false,
    },
    {
      id: "demo-g-2",
      title: "Escribir todos los días",
      kind: "journal",
      target: 30,
      threshold: 0,
      start: shiftDay(today, -30),
      end: shiftDay(today, 15),
      archived: false,
    },
  ];
  return data;
}
