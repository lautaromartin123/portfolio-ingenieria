import matplotlib
matplotlib.use("TkAgg")

import matplotlib.pyplot as plt

# parámetros físicos
dt = 0.01
tiempo_total = 8

y = 0          # posición inicial
v = 12         # velocidad inicial
a = -9.8       # gravedad
rebote = 0.85   # pierde energía al rebotar

# preparar gráfico interactivo
plt.ion()
fig, ax = plt.subplots()

ax.set_xlim(0, 1)
ax.set_ylim(0, 9)
ax.set_title("Simulación de rebote")
ax.set_ylabel("Altura")
ax.grid()

pelota, = ax.plot([0.5], [y], 'o')
suelo, = ax.plot([0, 1], [0, 0])

t = 0
contador = 0
while t <= tiempo_total:
    # actualizar física
    v = v + a * dt
    y = y + v * dt

    # rebote contra el suelo
    if y <= 0:
        y = 0
        v = -v * rebote

    # actualizar dibujo
    pelota.set_data([0.5], [y])


    if contador %3 == 0:
     fig.canvas.draw()
     fig.canvas.flush_events()
     plt.pause(0.001)



    contador += 1
    t += dt

plt.ioff()
plt.show()
