import matplotlib
matplotlib.use("TkAgg")

import matplotlib.pyplot as plt
from mpl_toolkits.mplot3d import Axes3D

# ==========================
# PARÁMETROS FÍSICOS
# ==========================

dt = 0.03
tiempo_total = 8

# posición inicial
x = 0
y = 0
z = 8

# velocidad inicial
vx = 0.5
vy = 0.3
vz = 0

# gravedad
g = -9.8

# rebote contra el plano
rebote = 0.7

# ==========================
# PREPARAR ESCENA 3D
# ==========================

plt.ion()

fig = plt.figure()
ax = fig.add_subplot(111, projection="3d")

ax.set_xlim(-5, 5)
ax.set_ylim(-5, 5)
ax.set_zlim(0, 10)

ax.set_xlabel("Eje X")
ax.set_ylabel("Eje Y")
ax.set_zlabel("Altura Z")

ax.set_title("Simulación 3D: gravedad sobre un plano")

# plano suelo z = 0
px = [-5, 5, 5, -5]
py = [-5, -5, 5, 5]
pz = [0, 0, 0, 0]

ax.plot_trisurf(px, py, pz, alpha=0.3)

# objeto
pelota, = ax.plot([x], [y], [z], "o")

# ==========================
# LOOP DE SIMULACIÓN
# ==========================

t = 0

while t <= tiempo_total:
    # actualizar velocidad por gravedad
    vz = vz + g * dt

    # actualizar posición
    x = x + vx * dt
    y = y + vy * dt
    z = z + vz * dt

    # choque contra el plano
    if z <= 0:
        z = 0
        vz = -vz * rebote

    # actualizar dibujo
    pelota.set_data([x], [y])
    pelota.set_3d_properties([z])

    fig.canvas.draw()
    fig.canvas.flush_events()
    plt.pause(0.001)

    t += dt

plt.ioff()
plt.show()
