import os
import time, json, requests
from datetime import datetime
import ccxt, pandas as pd, numpy as np

# ========= Config =========
SYMBOL = "SOL/USDT"
TIMEFRAME = "4h"
LIMIT = 800

SEND_TELEGRAM = False
TELEGRAM_TOKEN = os.getenv("TELEGRAM_BOT_TOKEN", "")
TELEGRAM_CHAT_ID = os.getenv("TELEGRAM_CHAT_ID", "")

EQUITY_USDT = 1000.0
RISK_PCT = 0.01
ATR_MULT_SL = 1.5

# Umbrales y ventanas
EMA_FAST = 20
EMA_SLOW = 50
RSI_LEN = 14
RSI_LONG = 55
RSI_SHORT = 45
MACD_FAST = 12
MACD_SLOW = 26
MACD_SIG = 9
VOL_SMA_WIN = 20
ATR_LEN = 14
BB_WIN = 20
SR_LOOKBACK = 50          # barras para detectar soportes/resistencias
SR_TOL = 0.003            # 0.3% tolerancia para evitar entrar pegado a SR
VOLAT_MIN_PCTL = 40       # mínimo percentil de volatilidad (BB% o ATR) para operar

# ========= Helpers =========
def telegram(text):
    try:
        url = f"https://api.telegram.org/bot{TELEGRAM_TOKEN}/sendMessage"
        requests.post(url, json={"chat_id": TELEGRAM_CHAT_ID, "text": text}, timeout=10)
    except Exception as e:
        print("Telegram error:", e)

def timeframe_seconds(tf: str) -> int:
    units = {"m":60, "h":3600, "d":86400}
    n = int(tf[:-1]); u = tf[-1]
    return n * units[u]

# ========= Datos e indicadores =========
exchange = ccxt.binance({"enableRateLimit": True})

def fetch_df():
    ohlcv = exchange.fetch_ohlcv(SYMBOL, timeframe=TIMEFRAME, limit=LIMIT)
    df = pd.DataFrame(ohlcv, columns=["ts","open","high","low","close","volume"])
    df["ts"] = pd.to_datetime(df["ts"], unit="ms")
    return df

def ema(series, span): return series.ewm(span=span, adjust=False).mean()

def indicators(df: pd.DataFrame) -> pd.DataFrame:
    d = df.copy()

    # EMA
    d["ema_fast"] = ema(d["close"], EMA_FAST)
    d["ema_slow"] = ema(d["close"], EMA_SLOW)

    # RSI
    delta = d["close"].diff()
    gain = delta.clip(lower=0).ewm(alpha=1/RSI_LEN, adjust=False).mean()
    loss = (-delta.clip(upper=0)).ewm(alpha=1/RSI_LEN, adjust=False).mean()
    rs = gain / loss.replace(0, np.nan)
    d["rsi"] = 100 - (100 / (1 + rs))

    # MACD
    ema_fast = ema(d["close"], MACD_FAST)
    ema_slow = ema(d["close"], MACD_SLOW)
    d["macd"] = ema_fast - ema_slow
    d["macd_sig"] = ema(d["macd"], MACD_SIG)
    d["macd_hist"] = d["macd"] - d["macd_sig"]

    # Volumen
    d["vol_sma"] = d["volume"].rolling(VOL_SMA_WIN).mean()
    d["vol_ok"] = d["volume"] > d["vol_sma"]

    # ATR
    tr1 = d["high"] - d["low"]
    tr2 = (d["high"] - d["close"].shift()).abs()
    tr3 = (d["low"] - d["close"].shift()).abs()
    tr = pd.concat([tr1, tr2, tr3], axis=1).max(axis=1)
    d["atr"] = tr.ewm(alpha=1/ATR_LEN, adjust=False).mean()

    # Bandas de Bollinger (ancho = proxy de volatilidad)
    m = d["close"].rolling(BB_WIN).mean()
    s = d["close"].rolling(BB_WIN).std()
    d["bb_up"] = m + 2*s
    d["bb_dn"] = m - 2*s
    d["bb_w"]  = (d["bb_up"] - d["bb_dn"]) / m
    # Percentil del ancho en la ventana reciente
    d["bb_pct"] = d["bb_w"].rolling(200).apply(lambda x: pd.Series(x).rank(pct=True).iloc[-1] if len(x.dropna())>0 else np.nan, raw=False)

    # Soportes y resistencias por pivotes simples
    # Soporte: mínimo local, Resistencia: máximo local
    d["pivot_low"]  = (d["low"].shift(1)  < d["low"].shift(2)) & (d["low"].shift(1)  < d["low"])
    d["pivot_high"] = (d["high"].shift(1) > d["high"].shift(2)) & (d["high"].shift(1) > d["high"])
    lows  = d[d["pivot_low"]].tail(SR_LOOKBACK)["low"].values
    highs = d[d["pivot_high"]].tail(SR_LOOKBACK)["high"].values
    d.attrs["supports"]    = lows[-10:] if len(lows)>0 else []
    d.attrs["resistances"] = highs[-10:] if len(highs)>0 else []

    # Patrones de velas básicos
    o,c,h,l = d["open"], d["close"], d["high"], d["low"]
    body = (c-o).abs()
    range_ = h-l
    # Engulfing
    d["bull_engulf"] = (c.shift(1) < o.shift(1)) & (c > o) & (c >= o.shift(1)) & (o <= c.shift(1))
    d["bear_engulf"] = (c.shift(1) > o.shift(1)) & (c < o) & (o >= c.shift(1)) & (c <= o.shift(1))
    # Martillo / Shooting star (simplificados)
    lower_shadow = (o.combine(c, min) - l)
    upper_shadow = (h - o.combine(c, max))
    d["hammer"]  = (lower_shadow > 2*body) & (upper_shadow < body)
    d["shooting"]= (upper_shadow > 2*body) & (lower_shadow < body)

    return d

def near_sr(price: float, levels: np.ndarray, tol: float) -> bool:
    if levels is None or len(levels)==0: return False
    return np.any(np.abs(price - levels)/price <= tol)

# ========= Señal con scoring =========
def make_signal(d: pd.DataFrame):
    row = d.iloc[-1]
    reasons = []

    score_long = 0
    score_short = 0

    # Tendencia (EMA)
    if row.ema_fast > row.ema_slow:
        score_long += 1; reasons.append("EMA_fast>EMA_slow (alcista)")
    elif row.ema_fast < row.ema_slow:
        score_short += 1; reasons.append("EMA_fast<EMA_slow (bajista)")

    # Impulso (RSI)
    if row.rsi >= RSI_LONG:
        score_long += 1; reasons.append(f"RSI {row.rsi:.1f}≥{RSI_LONG}")
    if row.rsi <= RSI_SHORT:
        score_short += 1; reasons.append(f"RSI {row.rsi:.1f}≤{RSI_SHORT}")

    # MACD
    if row.macd_hist > 0 and row.macd > row.macd_sig:
        score_long += 1; reasons.append("MACD>Signal & hist>0")
    if row.macd_hist < 0 and row.macd < row.macd_sig:
        score_short += 1; reasons.append("MACD<Signal & hist<0")

    # Volumen filtro
    vol_pass = bool(row.vol_ok)
    reasons.append("Volumen>SMA" if vol_pass else "Volumen<SMA")
    if not vol_pass:
        # penaliza entradas con volumen débil
        score_long -= 0.5; score_short -= 0.5

    # Volatilidad mínima (evitar mercado demasiado apretado)
    volat_pct = row.bb_pct * 100 if not np.isnan(row.bb_pct) else 50
    reasons.append(f"Volatilidad pct={volat_pct:.0f}")
    if volat_pct < VOLAT_MIN_PCTL:
        reasons.append("Filtro volatilidad: bajo → FLAT")
        return "FLAT", reasons, row

    # Soportes/Resistencias cercanos
    supports    = np.array(d.attrs.get("supports", []))
    resistances = np.array(d.attrs.get("resistances", []))
    near_sup = near_sr(row.close, supports, SR_TOL)
    near_res = near_sr(row.close, resistances, SR_TOL)
    if near_res and score_long > score_short:
        reasons.append("Cerca de RESISTENCIA → evito LONG")
        score_long -= 1
    if near_sup and score_short > score_long:
        reasons.append("Cerca de SOPORTE → evito SHORT")
        score_short -= 1

    # Patrones de velas (solo si hay volumen ok)
    if vol_pass:
        if bool(row.bull_engulf) or bool(row.hammer):
            score_long += 0.5; reasons.append("Patrón alcista (engulf/hammer)")
        if bool(row.bear_engulf) or bool(row.shooting):
            score_short += 0.5; reasons.append("Patrón bajista (engulf/shooting)")

    # Decisión
    if score_long - score_short > 0.5:
        return "LONG", reasons, row
    if score_short - score_long > 0.5:
        return "SHORT", reasons, row
    return "FLAT", reasons, row

def position_sizing(price: float, atr: float, side: str):
    if np.isnan(atr) or atr <= 0: return 0.0, None
    sl = price - ATR_MULT_SL*atr if side=="LONG" else price + ATR_MULT_SL*atr
    risk_per_unit = abs(price - sl)
    if risk_per_unit <= 0: return 0.0, None
    qty = (EQUITY_USDT*RISK_PCT) / risk_per_unit
    return float(max(qty,0.0)), float(sl)

# ========= Loop principal =========
def main():
    last_ts = None
    while True:
        try:
            df = fetch_df()
            d  = indicators(df)
            cur_ts = d.iloc[-1].ts
            if last_ts is None or cur_ts != last_ts:
                sig, reasons, row = make_signal(d)
                qty, sl = position_sizing(row.close, row.atr, sig) if sig!="FLAT" else (0.0, None)

                msg = (f"[{row.ts}] {SYMBOL} {TIMEFRAME}  Close={row.close:.4f}  "
                       f"RSI={row.rsi:.1f}  EMA{EMA_FAST}={row.ema_fast:.4f}  EMA{EMA_SLOW}={row.ema_slow:.4f}  "
                       f"MACD={row.macd:.4f}/{row.macd_sig:.4f}  ATR={row.atr:.4f}  → Señal: {sig}\n"
                       f"Razones: " + " | ".join(reasons))
                print(msg)
                d.to_csv(f"datos_{SYMBOL.replace('/','')}_{TIMEFRAME}.csv", index=False)
                if SEND_TELEGRAM: telegram(msg)

                last_ts = cur_ts
        except Exception as e:
            print("Error:", e)

        time.sleep(max(10, timeframe_seconds(TIMEFRAME)//4))

if __name__ == "__main__":
    main()
