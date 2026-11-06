"""Regresión exploratoria del IE usando sus valores publicados a dos decimales."""
from pathlib import Path
import json
import numpy as np

root = Path(__file__).resolve().parents[1]
out = root / 'resultados'
out.mkdir(exist_ok=True)
x = np.array([0., 10., 22.])
y = np.array([7., 11.03, 18.63])
a, b = np.linalg.lstsq(np.column_stack([np.ones(3), x]), y, rcond=None)[0]
fitted = a+b*x
residuals = y-fitted
sse = np.sum(residuals**2)
df = len(x)-2
s = np.sqrt(sse/df)
sxx = np.sum((x-x.mean())**2)
# Student t with one degree of freedom is a standard Cauchy distribution.
assert df == 1
t = 1/np.tan(np.pi*.025)
pred = a+b*30
h = 1/len(x)+(30-x.mean())**2/sxx
ci = [float(pred-t*s*np.sqrt(h)), float(pred+t*s*np.sqrt(h))]
pi = [float(pred-t*s*np.sqrt(1+h)), float(pred+t*s*np.sqrt(1+h))]
assert np.isclose(b, .5320879120879123)
assert np.isclose(pred, 22.50703296703297)
result = dict(years=[2000,2010,2022], index=y.tolist(), input_note='IE redondeado a dos decimales; coeficientes y cálculos posteriores sin redondear.', intercept=float(a), slope=float(b), r_squared=float(1-sse/np.sum((y-y.mean())**2)), fitted=fitted.tolist(), residuals=residuals.tolist(), residual_sum_of_squares=float(sse), residual_standard_error=float(s), degrees_of_freedom=df, t_critical=float(t), projection_2030=float(pred), confidence_interval_95=ci, prediction_interval_95=pi)
(out/'regresion_ie_2030.json').write_text(json.dumps(result,indent=2,ensure_ascii=False),encoding='utf-8')
print(json.dumps(result,ensure_ascii=False))
