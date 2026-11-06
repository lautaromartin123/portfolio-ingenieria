# Análisis demográfico de Boa Vista

Exploración de la estructura por edades del municipio de Boa Vista (Brasil), usando datos agregados de los censos de 2000, 2010 y 2022 del IBGE.

- [Fuentes y método](datos/boa-vista-censos-2000-2010-2022.md).
- [Código de regresión](scripts/calcular_regresion_ie.py).
- [Resultado](resultados/regresion_ie_2030.json).

```sh
python -m pip install -r requirements.txt
python scripts/calcular_regresion_ie.py
```

La regresión usa tres observaciones, índices redondeados y un grado de libertad residual. La proyección y sus intervalos son exploratorios; no constituyen un modelo predictivo robusto ni una explicación causal. Los datos son agregados públicos, no registros individuales. Fuentes, universo e intervalos temporales están documentados.
