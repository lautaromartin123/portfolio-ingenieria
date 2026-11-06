"use client";
import { lazy, Suspense, useCallback, useEffect, useState } from "react";
import {
  LayoutDashboard,
  BookOpen,
  NotebookPen,
  ShieldCheck,
  ChartNoAxesCombined,
  CalendarDays,
  ClipboardCheck,
  Target,
  Settings as SettingsIcon,
  Search,
  Plus,
  Sun,
  Moon,
  ChevronDown,
  LogIn,
  LogOut,
  FlaskConical,
  ArrowUpRight,
  Bell,
  RefreshCw,
} from "lucide-react";
import {
  SidebarProvider,
  Sidebar,
  SidebarHeader,
  SidebarContent,
  SidebarFooter,
  SidebarMenu,
  SidebarMenuItem,
  SidebarMenuButton,
  SidebarTrigger,
  useSidebar,
} from "@/components/ui/sidebar";
import {
  AlertDialog,
  AlertDialogContent,
  AlertDialogHeader,
  AlertDialogTitle,
  AlertDialogDescription,
  AlertDialogFooter,
  AlertDialogCancel,
  AlertDialogAction,
} from "@/components/ui/alert-dialog";
import {
  CommandDialog,
  CommandInput,
  CommandList,
  CommandEmpty,
  CommandGroup,
  CommandItem,
} from "@/components/ui/command";
import {
  DropdownMenu,
  DropdownMenuTrigger,
  DropdownMenuContent,
  DropdownMenuLabel,
  DropdownMenuItem,
} from "@/components/ui/dropdown-menu";
import { Skeleton } from "@/components/ui/skeleton";
import { Toaster, toast } from "sonner";
import { Button } from "./ui";
import Dashboard from "./dashboard";
import { TradeForm, TradeDetail } from "./trades";
import { demoData } from "@/lib/demo";
import {
  emptyData,
  dateKey,
  newTrade,
  signed,
  calc,
  type JournalData,
  type Trade,
} from "@/lib/domain";
import { fetchData, api } from "@/lib/api";
import { useJournalTools } from "./webmcp";
const Trades = lazy(() => import("./trades")),
  Daily = lazy(() => import("./journals").then((m) => ({ default: m.Daily }))),
  TradingPlan = lazy(() =>
    import("./journals").then((m) => ({ default: m.TradingPlan })),
  ),
  Analytics = lazy(() => import("./analytics")),
  Calendar = lazy(() => import("./calendar")),
  Reviews = lazy(() =>
    import("./reviews-goals").then((m) => ({ default: m.Reviews })),
  ),
  Goals = lazy(() =>
    import("./reviews-goals").then((m) => ({ default: m.Goals })),
  ),
  Settings = lazy(() => import("./settings"));
export const navigation = [
  { id: "dashboard", label: "Dashboard", icon: LayoutDashboard },
  { id: "trades", label: "Trading Journal", icon: BookOpen },
  { id: "daily", label: "Daily Journal", icon: NotebookPen },
  { id: "plan", label: "Plan de trading", icon: ShieldCheck },
  { id: "analytics", label: "Analytics", icon: ChartNoAxesCombined },
  { id: "calendar", label: "Calendario", icon: CalendarDays },
  { id: "reviews", label: "Reviews", icon: ClipboardCheck },
  { id: "goals", label: "Objetivos", icon: Target },
];
export default function JournalApp() {
  const [realData, setRealData] = useState(emptyData),
    [demo, setDemo] = useState<JournalData | null>(null),
    [reviewSelection, setReviewSelection] = useState<
      { kind: string; date: string } | undefined
    >(),
    [period, setPeriod] = useState("Month"),
    [page, setPage] = useState("dashboard"),
    [dark, setDark] = useState(true),
    [user, setUser] = useState<{ name: string; email: string } | null>(null),
    [loading, setLoading] = useState(true),
    [loadError, setLoadError] = useState(""),
    [editing, setEditing] = useState<Trade | null>(null),
    [detail, setDetail] = useState<string | null>(null),
    [searchOpen, setSearchOpen] = useState(false),
    [dirty, setDirty] = useState(false),
    [date, setDate] = useState(dateKey()),
    [confirmation, setConfirmation] = useState<{
      title: string;
      action: () => Promise<void>;
    } | null>(null),
    [confirmBusy, setConfirmBusy] = useState(false);
  const data = demo ?? realData;
  const refresh = useCallback(async () => {
    try {
      const r = await fetchData();
      setRealData(r.data);
      setUser(r.user);
      setLoadError("");
    } catch (e) {
      setLoadError((e as Error).message);
      throw e;
    } finally {
      setLoading(false);
    }
  }, []);
  useEffect(() => {
    refresh().catch(() => {});
    const stored = localStorage.getItem("tj-theme");
    if (stored) setDark(stored === "dark");
    const onHash = () => {
      const p = location.hash.slice(1);
      if ([...navigation.map((n) => n.id), "settings"].includes(p)) setPage(p);
    };
    onHash();
    const handler = (e: Event) => setDirty((e as CustomEvent).detail);
    const key = (e: KeyboardEvent) => {
      if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === "k") {
        e.preventDefault();
        setSearchOpen((v) => !v);
      }
    };
    window.addEventListener("hashchange", onHash);
    window.addEventListener("journal-dirty", handler);
    window.addEventListener("keydown", key);
    return () => {
      window.removeEventListener("hashchange", onHash);
      window.removeEventListener("journal-dirty", handler);
      window.removeEventListener("keydown", key);
    };
  }, [refresh]);
  useEffect(() => {
    document.documentElement.classList.remove("theme-dark", "theme-light");
    document.documentElement.classList.add(dark ? "theme-dark" : "theme-light");
    localStorage.setItem("tj-theme", dark ? "dark" : "light");
  }, [dark]);
  function confirm(title: string, action: () => Promise<void>) {
    setConfirmation({ title, action });
  }
  function go(next: string) {
    const action = async () => {
      setPage(next);
      history.replaceState(null, "", "#" + next);
      setDirty(false);
    };
    if (dirty && next !== page)
      confirm("Salir sin guardar los cambios de esta sección", action);
    else void action();
  }
  function selectDate(next: string) {
    const action = async () => {
      setDate(next);
      setDirty(false);
    };
    if (dirty) confirm("Cambiar de día sin guardar los cambios", action);
    else void action();
  }
  function addTrade() {
    if (!user && !demo) {
      toast.info("Iniciá sesión para registrar tus operaciones.", {
        action: {
          label: "Iniciar sesión",
          onClick: () => {
            window.location.href = "/signin-with-chatgpt?return_to=/";
          },
        },
      });
      return;
    }
    setEditing(
      newTrade(
        data.account.id,
        (data.settings.defaults ?? {}) as Partial<Trade>,
      ),
    );
  }
  useJournalTools(data, !!demo, refresh, go);
  const active =
      navigation.find((n) => n.id === page)?.label ?? "Configuración",
    selected = data.trades.find((t) => t.id === detail),
    common = { data, refresh, demo: !!demo, confirm };
  const notifications = [
    ...(!data.journals.some(
      (j) => j.date === dateKey(new Date(), data.plan.timezone),
    )
      ? ["Tu Daily Journal de hoy está pendiente."]
      : []),
    ...(data.trades.filter(
      (t) =>
        dateKey(t.enteredAt, data.plan.timezone) ===
          dateKey(new Date(), data.plan.timezone) && t.violations.length,
    ).length
      ? ["Tenés operaciones de hoy fuera del plan. Revisá sus detalles."]
      : []),
  ];
  return (
    <div className={dark ? "theme-dark" : "theme-light"}>
      <SidebarProvider
        style={{ "--sidebar-width": "244px" } as React.CSSProperties}
      >
        <Sidebar className="journal-sidebar">
          <SidebarHeader>
            <a
              href="#dashboard"
              className="brand"
              onClick={(e) => {
                e.preventDefault();
                go("dashboard");
              }}
            >
              <span className="brand-symbol">
                <ChartNoAxesCombined size={22} />
              </span>
              <span>
                Trading<span>Journal</span>
              </span>
            </a>
            <button className="account-picker" onClick={() => go("settings")}>
              <span className="account-avatar">P</span>
              <span>
                <b>{data.account.name}</b>
                <small>Personal · USD</small>
              </span>
              <ChevronDown size={15} />
            </button>
          </SidebarHeader>
          <SidebarContent>
            <p className="nav-label">WORKSPACE</p>
            <SidebarMenu>
              {navigation.map((n) => (
                <SidebarMenuItem key={n.id}>
                  <NavButton active={n.id === page} onClick={() => go(n.id)}>
                    <n.icon size={18} />
                    <span>{n.label}</span>
                  </NavButton>
                </SidebarMenuItem>
              ))}
            </SidebarMenu>
          </SidebarContent>
          <SidebarFooter>
            <div className="sidebar-note">
              <ShieldCheck size={18} />
              <span>
                Tu espacio privado<small>Enfocate en tu proceso.</small>
              </span>
            </div>
            <NavButton
              active={page === "settings"}
              onClick={() => go("settings")}
            >
              <SettingsIcon size={18} />
              Configuración
            </NavButton>
            <a
              className="user-profile"
              href={
                user
                  ? "/signout-with-chatgpt?return_to=/"
                  : "/signin-with-chatgpt?return_to=/"
              }
              target="_top"
            >
              <span className="user-avatar">
                {user?.name.slice(0, 2).toUpperCase() ?? "TJ"}
              </span>
              <span>
                <b>{user?.name ?? "Mi espacio"}</b>
                <small>{user ? "Cerrar sesión" : "Iniciar sesión"}</small>
              </span>
              {user ? <LogOut size={17} /> : <LogIn size={17} />}
            </a>
          </SidebarFooter>
        </Sidebar>
        <main className="workspace">
          <header className="topbar">
            <div className="breadcrumb">
              <SidebarTrigger />
              <span>Workspace</span>
              <span>/</span>
              <b>{active}</b>
            </div>
            <div className="top-actions">
              <button
                className="search-button"
                aria-label="Buscar en todo el journal"
                onClick={() => setSearchOpen(true)}
              >
                <Search size={16} />
                <span>Buscar en tu journal...</span>
                <kbd>Ctrl K</kbd>
              </button>
              <DropdownMenu>
                <DropdownMenuTrigger asChild>
                  <button className="icon-button" aria-label="Notificaciones">
                    <Bell size={18} />
                    {notifications.length > 0 && (
                      <span className="notification-count">
                        {notifications.length}
                      </span>
                    )}
                  </button>
                </DropdownMenuTrigger>
                <DropdownMenuContent align="end" className="notifications-menu">
                  <DropdownMenuLabel>Tu día</DropdownMenuLabel>
                  {notifications.length ? (
                    notifications.map((n) => (
                      <DropdownMenuItem
                        key={n}
                        onSelect={() =>
                          go(n.includes("Journal") ? "daily" : "trades")
                        }
                      >
                        {n}
                      </DropdownMenuItem>
                    ))
                  ) : (
                    <DropdownMenuItem>
                      Todo al día. Buen trabajo con tu proceso.
                    </DropdownMenuItem>
                  )}
                </DropdownMenuContent>
              </DropdownMenu>
              <button
                className="icon-button"
                aria-label="Cambiar tema"
                onClick={() => setDark(!dark)}
              >
                {dark ? <Sun size={18} /> : <Moon size={18} />}
              </button>
              <Button
                aria-label="Nuevo trade"
                disabled={loading && !demo}
                onClick={addTrade}
              >
                <Plus size={17} />
                <span>Nuevo trade</span>
              </Button>
            </div>
          </header>
          <div className="page-content">
            {loadError && !demo && (
              <div className="error-box auth-banner">
                <span>{loadError}</span>
                {!user ? (
                  <a href="/signin-with-chatgpt?return_to=/" target="_top">
                    Iniciar sesión con ChatGPT <ArrowUpRight size={15} />
                  </a>
                ) : (
                  <button onClick={() => refresh().catch(() => {})}>
                    Reintentar
                  </button>
                )}
              </div>
            )}
            {(demo || !data.trades.length) && (
              <div className="demo-banner">
                <FlaskConical size={16} />
                <span>
                  {demo
                    ? "Vista de ejemplo · estos datos no pertenecen a tu cuenta."
                    : "Empezá con tus operaciones o explorá un ejemplo."}
                </span>
                <button
                  onClick={() => {
                    const change = async () => {
                      setDemo(demo ? null : demoData());
                      setDetail(null);
                      setPage("dashboard");
                      setDirty(false);
                    };
                    if (dirty)
                      confirm("Cambiar de espacio sin guardar", change);
                    else void change();
                  }}
                >
                  {demo ? "Salir de demo" : "Explorar demo"}
                  <ArrowUpRight size={14} />
                </button>
              </div>
            )}
            {loading && !demo ? (
              <div className="stack" aria-label="Cargando journal">
                <Skeleton className="h-32 w-full" />
                <Skeleton className="h-80 w-full" />
              </div>
            ) : (
              <Suspense fallback={<Skeleton className="h-80 w-full" />}>
                {page === "dashboard" && (
                  <Dashboard
                    data={data}
                    period={period}
                    setPeriod={setPeriod}
                    onTrade={(t) => setDetail(t.id)}
                    navigate={go}
                    newTrade={addTrade}
                  />
                )}{" "}
                {page === "trades" && (
                  <Trades
                    {...common}
                    onTrade={(t) => setDetail(t.id)}
                    onNew={addTrade}
                  />
                )}{" "}
                {page === "daily" && (
                  <Daily
                    key={date + !!demo}
                    {...common}
                    date={date}
                    setDate={selectDate}
                  />
                )}{" "}
                {page === "plan" && (
                  <TradingPlan key={String(!!demo)} {...common} />
                )}{" "}
                {page === "analytics" && <Analytics data={data} />}{" "}
                {page === "calendar" && (
                  <Calendar
                    data={data}
                    onTrade={(t) => setDetail(t.id)}
                    onJournal={(d) => {
                      setDate(d);
                      go("daily");
                    }}
                  />
                )}{" "}
                {page === "reviews" && (
                  <Reviews
                    key={
                      reviewSelection
                        ? reviewSelection.kind + reviewSelection.date
                        : "default"
                    }
                    {...common}
                    selection={reviewSelection}
                  />
                )}{" "}
                {page === "goals" && <Goals {...common} />}{" "}
                {page === "settings" && <Settings {...common} />}
              </Suspense>
            )}
            <footer className="app-footer">
              <span>Trading Journal</span>
              <span>Construí consistencia, un día a la vez.</span>
            </footer>
          </div>
        </main>
      </SidebarProvider>
      {editing && (
        <TradeForm
          key={editing.id}
          trade={editing}
          data={data}
          onClose={() => setEditing(null)}
          onSaved={refresh}
          demo={!!demo}
        />
      )}{" "}
      {selected && (
        <TradeDetail
          trade={selected}
          data={data}
          onClose={() => setDetail(null)}
          onEdit={() => {
            setDetail(null);
            setEditing(selected);
          }}
          onDelete={() => {
            if (demo) {
              toast.info("Los datos de ejemplo son de solo lectura.");
              return;
            }
            confirm(
              "Eliminar la operación " +
                selected.instrument +
                " y sus capturas",
              async () => {
                await api("deleteTrade", selected.id);
                setDetail(null);
                await refresh();
                toast.success("Operación eliminada");
              },
            );
          }}
          refresh={refresh}
          confirm={confirm}
        />
      )}
      <AlertDialog
        open={!!confirmation}
        onOpenChange={(v) => !v && !confirmBusy && setConfirmation(null)}
      >
        <AlertDialogContent>
          <AlertDialogHeader>
            <AlertDialogTitle>{confirmation?.title}</AlertDialogTitle>
            <AlertDialogDescription>
              Confirmá para continuar. Esta acción puede descartar o modificar
              datos.
            </AlertDialogDescription>
          </AlertDialogHeader>
          <AlertDialogFooter>
            <AlertDialogCancel disabled={confirmBusy}>
              Cancelar
            </AlertDialogCancel>
            <AlertDialogAction
              disabled={confirmBusy}
              onClick={async (e) => {
                e.preventDefault();
                if (!confirmation) return;
                setConfirmBusy(true);
                try {
                  await confirmation.action();
                  setConfirmation(null);
                } catch (err) {
                  toast.error((err as Error).message);
                } finally {
                  setConfirmBusy(false);
                }
              }}
            >
              {confirmBusy ? "Procesando…" : "Confirmar"}
            </AlertDialogAction>
          </AlertDialogFooter>
        </AlertDialogContent>
      </AlertDialog>
      <CommandDialog
        open={searchOpen}
        onOpenChange={setSearchOpen}
        title="Buscador global"
        description="Buscá secciones, operaciones, journal, reviews y objetivos."
      >
        <CommandInput placeholder="Buscar en tu journal…" />
        <CommandList>
          <CommandEmpty>No encontramos coincidencias.</CommandEmpty>
          <CommandGroup heading="Secciones">
            {navigation.map((n) => (
              <CommandItem
                key={n.id}
                onSelect={() => {
                  go(n.id);
                  setSearchOpen(false);
                }}
              >
                <n.icon size={16} />
                {n.label}
              </CommandItem>
            ))}
          </CommandGroup>
          <CommandGroup heading="Operaciones">
            {data.trades
              .slice(-300)
              .reverse()
              .map((t) => (
                <CommandItem
                  key={t.id}
                  value={[
                    t.id,
                    t.instrument,
                    t.setup,
                    t.notes,
                    t.psychology.error,
                    t.enteredAt,
                  ].join(" ")}
                  onSelect={() => {
                    setDetail(t.id);
                    setSearchOpen(false);
                  }}
                >
                  <BookOpen size={15} />
                  {t.instrument} · {t.setup}
                  <span className="command-meta">{signed(calc(t).pnl)}</span>
                </CommandItem>
              ))}
          </CommandGroup>
          <CommandGroup heading="Daily Journal">
            {data.journals.map((j) => (
              <CommandItem
                key={j.id}
                value={[j.date, j.objective, j.reflection, j.learned].join(" ")}
                onSelect={() => {
                  const action = async () => {
                    setDate(j.date);
                    setPage("daily");
                    history.replaceState(null, "", "#daily");
                  };
                  if (dirty)
                    confirm(
                      "Abrir otro journal sin guardar los cambios",
                      action,
                    );
                  else void action();
                  setSearchOpen(false);
                }}
              >
                <NotebookPen size={15} />
                {j.date} · {j.objective || "Mi reflexión"}
              </CommandItem>
            ))}
          </CommandGroup>
          <CommandGroup heading="Objetivos">
            {data.goals.map((g) => (
              <CommandItem
                key={g.id}
                value={g.title}
                onSelect={() => {
                  go("goals");
                  setSearchOpen(false);
                }}
              >
                <Target size={15} />
                {g.title}
              </CommandItem>
            ))}
          </CommandGroup>
          <CommandGroup heading="Reviews">
            {data.reviews.map((r) => (
              <CommandItem
                key={r.id}
                value={[r.start, r.kind, r.keep, r.improve, r.next].join(" ")}
                onSelect={() => {
                  const action = async () => {
                    setReviewSelection({ kind: r.kind, date: r.start });
                    setPage("reviews");
                    history.replaceState(null, "", "#reviews");
                  };
                  if (dirty)
                    confirm(
                      "Abrir otra review sin guardar los cambios",
                      action,
                    );
                  else void action();
                  setSearchOpen(false);
                }}
              >
                <ClipboardCheck size={15} />
                {r.kind} · {r.start}
              </CommandItem>
            ))}
          </CommandGroup>
        </CommandList>
      </CommandDialog>
      <Toaster
        richColors
        position="bottom-right"
        theme={dark ? "dark" : "light"}
      />
    </div>
  );
}
function NavButton({
  active,
  onClick,
  children,
}: {
  active: boolean;
  onClick: () => void;
  children: React.ReactNode;
}) {
  const { setOpenMobile } = useSidebar();
  return (
    <SidebarMenuButton
      isActive={active}
      onClick={() => {
        onClick();
        setOpenMobile(false);
      }}
    >
      {children}
    </SidebarMenuButton>
  );
}
