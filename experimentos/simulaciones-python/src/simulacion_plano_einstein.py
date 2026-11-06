import matplotlib
matplotlib.use("TkAgg")

import numpy as np
import matplotlib.pyplot as plt

fig = plt.figure()
ax = fig.add_subplot(111, projection="3d")

# grilla del plano
x = np.linspace(-5, 5, 80)
y = np.linspace(-5, 5, 80)
X, Y = np.meshgrid(x, y)

# distancia al centro
R = np.sqrt(X**2 + Y**2)

# deformación gravitatoria visual
Z = -3 / (R + 0.6)

# dibujar malla
ax.plot_wireframe(X, Y, Z, rstride=3, cstride=3, linewidth=0.5)

# masa central
ax.scatter(0, 0, -5, s=150)

ax.set_title("Plano espacio-tiempo deformado por una masa")
ax.set_xlabel("Espacio X")
ax.set_ylabel("Espacio Y")
ax.set_zlabel("Curvatura")

ax.set_xlim(-5, 5)
ax.set_ylim(-5, 5)
ax.set_zlim(-5, 1)

plt.show()
