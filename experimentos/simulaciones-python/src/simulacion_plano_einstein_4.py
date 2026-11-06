import matplotlib
matplotlib.use("TkAgg")

import numpy as np
import matplotlib.pyplot as plt

# =========================
# PARÁMETROS JUGABLES
# =========================

masa_1 = 4.0
masa_2 = 2.0

x1, y1 = -2, 0
x2, y2 = 2, 0

suavizado = 0.6

# =========================
# FIGURA
# =========================

fig = plt.figure()
ax = fig.add_subplot(111, projection="3d")

fig.patch.set_facecolor("black")
ax.set_facecolor("black")

# =========================
# PLANO
# =========================

x = np.linspace(-6, 6, 120)
y = np.linspace(-6, 6, 120)
X, Y = np.meshgrid(x, y)

R1 = np.sqrt((X - x1)**2 + (Y - y1)**2)
R2 = np.sqrt((X - x2)**2 + (Y - y2)**2)

Z = -masa_1 / (R1 + suavizado) - masa_2 / (R2 + suavizado)

# =========================
# DIBUJO
# =========================

ax.plot_surface(X, Y, Z, cmap="viridis", edgecolor="none")

# las masas ahora también dependen del valor de masa
z1 = -masa_1 / suavizado
z2 = -masa_2 / suavizado

ax.scatter(x1, y1, z1, s=masa_1 * 80, color="red")
ax.scatter(x2, y2, z2, s=masa_2 * 80, color="orange")

ax.set_axis_off()
ax.view_init(elev=35, azim=45)

plt.show()
