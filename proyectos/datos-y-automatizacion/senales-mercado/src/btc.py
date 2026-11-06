import time
import ccxt
import pandas as pd
import numpy as np

# =======================
# CONFIG
# =======================
SYMBOL = "BTC/USDT"
TIMEFRAME = "15m"      # "15m", "1h", "4h", "1d"
LIMIT = 800

# Indicadores
EMA_FAST = 20
EMA_SLOW = 50

RSI_LEN = 14
RSI_LONG = 53
RSI_SHORT = 47

MACD_FAST = 12
MACD_SLOW = 26
MACD_SIG = 9

VOL_SMA_WIN = 20

ATR_LEN = 14

BB_WIN = 20
VOLAT_MIN_PCTL = 35   # subir = menos señales, bajar = más señales

# Soporte / Resistencia
SR_LOOKBACK = 80
SR_TOL = 0.003        # 0.3%

# =======================
# BINANCE
# =======================
exchange = ccxt.binance({"enableRateLimit": True})


# =======================
# HELPERS
# =======================
def tf_seconds(tf: str) -> int:
    units = {"m": 60, "h": 3600, "d": 86400, "w": 604800}
    n = int(tf[:-1])
    u = tf[-1]
    return n * units[u]


def ema(series, span):
    return series.ewm(span=span, adjust=False).mean()


def fetch_df():
    ohlcv = exchange.fetch_ohlcv(SYMBOL, timeframe=TIMEFRAME, limit=LIMIT)
    df = pd.DataFrame(ohlcv, columns=["ts", "open", "high", "low", "close", "volume"])
    df["ts"] = pd.to_datetime(df["ts"], unit="ms", utc=True).dt.tz_convert("America/Argentina/Buenos_Aires")
    return df


def add_indicators(df: pd.DataFrame) -> pd.DataFrame:
    d = df.copy()

    # EMA
    d["ema_fast"] = ema(d["close"], EMA_FAST)
    d["ema_slow"] = ema(d["close"], EMA_SLOW)

    # RSI
    delta = d["close"].diff()
    gain = delta.clip(lower=0).ewm(alpha=1 / RSI_LEN, adjust=False).mean()
    loss = (-delta.clip(upper=0)).ewm(alpha=1 / RSI_LEN, adjust=False).mean()
    rs = gain / loss.replace(0, np.nan)
    d["rsi"] = 100 - (100 / (1 + rs))

    # MACD
    ef = ema(d["close"], MACD_FAST)
    es = ema(d["close"], MACD_SLOW)
    d["macd"] = ef - es
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
    d["atr"] = tr.ewm(alpha=1 / ATR_LEN, adjust=False).mean()

    # Bollinger width (volatilidad)
    m = d["close"].rolling(BB_WIN).mean()
    s = d["close"].rolling(BB_WIN).std()
    up = m + 2 * s
    dn = m - 2 * s
    d["bb_w"] = (up - dn) / m

    # Percentil del ancho (0..1) y luego lo convertimos a 0..100
    d["bb_pct"] = d["bb_w"].rolling(200).apply(
        lambda x: pd.Series(x).rank(pct=True).iloc[-1]
        if len(pd.Series(x).dropna()) > 0
        else np.nan
    )

    # Soportes y resistencias por pivotes
    d["pivot_low"] = (d["low"].shift(1) < d["low"].shift(2)) & (d["low"].shift(1) < d["low"])
    d["pivot_high"] = (d["high"].shift(1) > d["high"].shift(2)) & (d["high"].shift(1) > d["high"])

    lows = d[d["pivot_low"]].tail(SR_LOOKBACK)["low"].values
    highs = d[d["pivot_high"]].tail(SR_LOOKBACK)["high"].values

    d.attrs["supports"] = list(lows[-10:]) if len(lows) > 0 else []
    d.attrs["resistances"] = list(highs[-10:]) if len(highs) > 0 else []

    # Patrones de velas (básicos)
    o = d["open"]
    c = d["close"]
    h = d["high"]
    l = d["low"]

    body = (c - o).abs()
    lower_shadow = (pd.concat([o, c], axis=1).min(axis=1) - l)
    upper_shadow = (h - pd.concat([o, c], axis=1).max(axis=1))

    d["bull_engulf"] = (
        (c.shift(1) < o.shift(1))
        & (c > o)
        & (c >= o.shift(1))
        & (o <= c.shift(1))
    )

    d["bear_engulf"] = (
        (c.shift(1) > o.shift(1))
        & (c < o)
        & (o >= c.shift(1))
        & (c <= o.shift(1))
    )

    d["hammer"] = (lower_shadow > 2 * body) & (upper_shadow < body)
    d["shooting"] = (upper_shadow > 2 * body) & (lower_shadow < body)

    return d


def near_level(price: float, levels, tol: float) -> bool:
    if levels is None:
        return False
    if len(levels) == 0:
        return False

    levels = np.array(levels, dtype=float)
    return bool(np.any(np.abs(price - levels) / price <= tol))


def make_signal(d: pd.DataFrame):
    row = d.iloc[-1]
    reasons = []

    L = 0.0
    S = 0.0

    # Trend EMA
    if row.ema_fast > row.ema_slow:
        L += 1
        reasons.append("EMA_fast>EMA_slow")
    elif row.ema_fast < row.ema_slow:
        S += 1
        reasons.append("EMA_fast<EMA_slow")

    # RSI
    if row.rsi >= RSI_LONG:
        L += 1
        reasons.append(f"RSI {row.rsi:.1f}≥{RSI_LONG}")
    if row.rsi <= RSI_SHORT:
        S += 1
        reasons.append(f"RSI {row.rsi:.1f}≤{RSI_SHORT}")

    # MACD
    if row.macd > row.macd_sig and row.macd_hist > 0:
        L += 1
        reasons.append("MACD bullish")
    if row.macd < row.macd_sig and row.macd_hist < 0:
        S += 1
        reasons.append("MACD bearish")

    # Volume filter (SIEMPRE convertir a bool)
    # Volume direction with trend context
    vol_ok = bool(row.vol_ok)
    candle_bullish = row.close > row.open
    candle_bearish = row.close < row.open
    trend_bullish = row.ema_fast > row.ema_slow
    trend_bearish = row.ema_fast < row.ema_slow

    if vol_ok and candle_bullish and trend_bullish:
       L += 1
       reasons.append("Volumen alto acompaña tendencia alcista")
    elif vol_ok and candle_bearish and trend_bearish:
        S += 1
        reasons.append("Volumen alto acompaña tendencia bajista")
    elif vol_ok and candle_bullish and trend_bearish:
        L += 0.5
        reasons.append("Volumen alto contra tendencia bajista → posible rebote/squeeze")
    elif vol_ok and candle_bearish and trend_bullish:
        S += 0.5
        reasons.append("Volumen alto contra tendencia alcista → posible rechazo/distribución")
    else:
        reasons.append("Volumen bajo → señal menos confiable")

    # Volatility filter
    pct = float(row.bb_pct) if not np.isnan(row.bb_pct) else 0.5
    pct100 = pct * 100
    reasons.append(f"Volat pct={pct100:.0f}")

    if pct100 < VOLAT_MIN_PCTL:
        return "FLAT", reasons + ["Filtro: volatilidad baja"], row

    # SR filter
    sup = list(d.attrs.get("supports", []))
    res = list(d.attrs.get("resistances", []))

    if near_level(float(row.close), res, SR_TOL) and L > S:
        L -= 1
        reasons.append("Cerca RES → evita LONG")

    if near_level(float(row.close), sup, SR_TOL) and S > L:
        S -= 1
        reasons.append("Cerca SUP → evita SHORT")

    # Candles (solo si hay volumen ok)
    if vol_ok:
        if bool(row.bull_engulf) or bool(row.hammer):
            L += 0.5
            reasons.append("Patrón alcista")
        if bool(row.bear_engulf) or bool(row.shooting):
            S += 0.5
            reasons.append("Patrón bajista")

    # Decisión final
    if L - S > 0.5:
        return "LONG", reasons, row
    if S - L > 0.5:
        return "SHORT", reasons, row
    return "FLAT", reasons, row


# =======================
# MAIN LOOP
# =======================
def main():
    last_ts = None

    while True:
        try:
            df = fetch_df()
            d = add_indicators(df)

            cur_ts = d.iloc[-1].ts

            # Solo imprimir si hay vela nueva
            if last_ts is None or cur_ts != last_ts:
                sig, reasons, row = make_signal(d)

                print(
                    f"[{row.ts}] {SYMBOL} {TIMEFRAME} "
                    f"Close={row.close:.2f} RSI={row.rsi:.1f} "
                    f"EMA{EMA_FAST}={row.ema_fast:.2f} EMA{EMA_SLOW}={row.ema_slow:.2f} "
                    f"MACD={row.macd:.2f}/{row.macd_sig:.2f} ATR={row.atr:.2f} -> {sig}"
                )
                print("Razones:", " | ".join(reasons))
                print("-" * 100)

                last_ts = cur_ts

        except Exception as e:
            import traceback
            traceback.print_exc()

        time.sleep(max(10, tf_seconds(TIMEFRAME) // 4))


if __name__ == "__main__":
    main()
