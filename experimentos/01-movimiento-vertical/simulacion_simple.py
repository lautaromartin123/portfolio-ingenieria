import numpy as np
import matplotlib.pyplot as plt

# parámetros
dt = 0.1
tiempo_total = 10

# variables físicas
y = 0          # posición inicial
v = 10         # velocidad inicial
a = -9.8       # gravedad

# listas para guardar resultados
tiempos = []
posiciones = []

t = 0

while t <= tiempo_total:
    tiempos.append(t)
    posiciones.append(y)

    # física (motor)
    v = v + a * dt
    y = y + v * dt

    t += dt

# graficar
plt.plot(tiempos, posiciones)
plt.xlabel("Tiempo")
plt.ylabel("Altura")
plt.title("Simulación física paso a paso")
plt.grid()
plt.show()
