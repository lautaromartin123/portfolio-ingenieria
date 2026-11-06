"""Prueba offline del filtro real de SOL; no inicia el bot ni usa servicios externos."""
import ast
import csv
import math
from collections import Counter
from pathlib import Path
from types import SimpleNamespace

root = Path(__file__).resolve().parents[1]
source = ast.parse((root / 'src' / 'sol.py').read_text(encoding='utf-8-sig'))
namespace = {'pd': SimpleNamespace(DataFrame=object), 'np': SimpleNamespace(isnan=math.isnan, array=list, ndarray=object)}
for node in source.body:
    if isinstance(node, ast.Assign):
        try:
            value = ast.literal_eval(node.value)
        except (ValueError, TypeError):
            continue
        for target in node.targets:
            if isinstance(target, ast.Name):
                namespace[target.id] = value
functions = [n for n in source.body if isinstance(n, ast.FunctionDef) and n.name in ('make_signal', 'near_sr')]
exec(compile(ast.Module(body=functions, type_ignores=[]), 'funciones_sol_aisladas', 'exec'), namespace)
blocked_reason = 'Filtro volatilidad: bajo → FLAT'
def signal(row):
    return namespace['make_signal'](SimpleNamespace(iloc=[row], attrs={}))

def bullish_row(percentile):
    return SimpleNamespace(ema_fast=110, ema_slow=100, rsi=60, macd_hist=1,
        macd=2, macd_sig=1, vol_ok=True, bb_pct=percentile, close=110,
        bull_engulf=False, hammer=False, bear_engulf=False, shooting=False)

for percentile, expected in ((0.0, 'FLAT'), (0.39, 'FLAT'), (0.4, 'LONG'), (0.9, 'LONG'), (1.0, 'LONG')):
    result, reasons, _ = signal(bullish_row(percentile))
    assert result == expected, (percentile, result, reasons)
    assert (blocked_reason in reasons) == (percentile < 0.4)
    print('umbral:', percentile, result, 'OK')
# El comportamiento previo para NaN se conserva: percentil sustituto de 50.
result, reasons, _ = signal(bullish_row(math.nan))
assert result == 'LONG' and 'Volatilidad pct=50' in reasons
print('NaN: comportamiento previo conservado OK')

with (root / 'examples' / 'sol_4h.csv').open(encoding='utf-8-sig', newline='') as handle:
    rows = list(csv.DictReader(handle))
valid = []
blocked = 0
counts = Counter()
for record in rows:
    if not record['bb_pct']:
        continue
    row = SimpleNamespace(**{k: (v == 'True' if v in ('True', 'False') else float(v) if v else math.nan) for k, v in record.items() if k != 'ts'})
    result, reasons, _ = signal(row)
    is_blocked = blocked_reason in reasons
    assert is_blocked == (row.bb_pct * 100 < namespace['VOLAT_MIN_PCTL'])
    valid.append(row.bb_pct)
    blocked += is_blocked
    counts[result] += 1
assert len(valid) == 582 and 0 < blocked < len(valid)
print({'filas_csv': len(rows), 'filas_percentil_valido': len(valid),
       'filtradas_por_volatilidad_baja': blocked, 'superan_filtro': len(valid)-blocked,
       'senales_sin_soportes_resistencias': dict(counts)})
