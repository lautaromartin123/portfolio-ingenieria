# Boa Vista: población por grandes grupos de edad

Municipio de Boa Vista (Roraima, Brasil), código IBGE 1400100. Consulta: 30/09/2026.

Población residente de ambos sexos; total municipal, incluyendo áreas urbanas y rurales. Datos censales, no estimaciones anuales.

| Grupo de edad | 2000: personas | 2000: % | 2010: personas | 2010: % | 2022: personas | 2022: % |
|---|---:|---:|---:|---:|---:|---:|
| 0–14 años | 72.448 | 36,12 | 85.021 | 29,90 | 108.649 | 26,28 |
| 15–64 años | 123.048 | 61,35 | 189.914 | 66,80 | 284.596 | 68,83 |
| 65 años o más | 5.072 | 2,53 | 9.378 | 3,30 | 20.241 | 4,90 |
| Total | 200.568 | 100,00 | 284.313 | 100,00 | 413.486 | 100,00 |

## Fuentes oficiales

- Censos 2000 y 2010: [IBGE–SIDRA, tabla 1552](https://sidra.ibge.gov.br/tabela/1552), População residente, por situação do domicílio e sexo, segundo a forma de declaração da idade e a idade.
- [Consulta directa de 2000 y 2010](https://apisidra.ibge.gov.br/values/t/1552/n6/1400100/v/93/p/2000,2010/c1/0/c2/0/c286/0/c287/all).
- Censo 2022: [IBGE–SIDRA, tabla 9514](https://sidra.ibge.gov.br/tabela/9514), População residente, por sexo, idade e forma de declaração da idade.
- [Consulta directa de 2022](https://apisidra.ibge.gov.br/values/t/9514/n6/1400100/v/93/p/2022/c2/6794/c287/all).
- Verificación adicional de población total 2000 y 2010: [tabla 202, consulta directa](https://apisidra.ibge.gov.br/values/t/202/n6/1400100/v/93/p/2000,2010).

## Método y comprobaciones

1. Se seleccionaron el municipio, la población residente y el total de sexo y forma de declaración de edad; en la tabla 1552 también el total de situación del domicilio.
2. Se sumaron intervalos de edad no superpuestos dentro de 0–14, 15–64 y 65 años o más. Se incluyeron las personas de 100 años o más.
3. Se dividió cada suma por la población total del mismo censo y se multiplicó por 100.
4. Para cada año se comprobó que los tres grupos sumaran el total oficial. Se comprobó además, por separado, que las edades simples, incluyendo menores de un año y 100 años o más, sumaran ese total.
5. Las cantidades son datos del IBGE agregados; los porcentajes son cálculos propios. Los porcentajes publicados aquí se redondean a dos decimales. En 2022 suman 100,01 % por redondeo.

El total de 2000 usado es 200.568, coincidente en las tablas 1552 y 202 consultadas. No se debe sustituir por el total de otra publicación sin comprobar que corresponde a la misma versión y universo.

Los datos representan el municipio en cada censo. Esta extracción no reconstruye límites territoriales constantes. Para un análisis causal o territorial detallado, deberá revisarse la comparabilidad de límites y metodología.

Los intervalos temporales son distintos: 2000–2010 abarca 10 años y 2010–2022 abarca 12 años. Las futuras tasas y regresiones deberán usar los años reales.

## Archivos conservados en esta carpeta

- `ibge_1552_boa_vista_2000_2010.json`: respuesta de la API con edades simples, intervalos y total de 2000 y 2010.
- `ibge_9514_boa_vista_2022.json`: respuesta de la API con edades simples, intervalos y total de 2022.
- `boa_vista_resumen_censos_2000_2010_2022.json`: cantidades agrupadas, porcentajes sin redondear, fuentes y comprobaciones.

Los archivos originales incluyen categorías que se superponen (totales, intervalos y edades simples). No deben sumarse todas sus filas juntas.

## Primera lectura descriptiva

La proporción de personas de 65 años o más aumentó de 2,53 % a 3,30 % y luego a 4,90 %. En paralelo, la participación de 0–14 años descendió, aunque su número absoluto aumentó. Estos datos muestran cambios en la estructura por edades; por sí solos no identifican sus causas ni prueban consecuencias económicas.

## Texto de fuente para futuras tablas

Fuente: elaboración propia a partir del IBGE, Censos Demográficos 2000, 2010 y 2022, tablas SIDRA 1552 y 9514. Municipio de Boa Vista (RR), población residente de ambos sexos. Consulta: 30 de septiembre de 2026.
