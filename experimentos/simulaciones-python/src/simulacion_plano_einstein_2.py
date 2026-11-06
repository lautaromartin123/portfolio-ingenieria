import matplotlib
matplotlib.use("TkAgg")

import numpy as np
import matplotlib.pyplot as plt

fig = plt.figure()
ax = fig.add_subplot(111, projection="3d")

# plano
x = np.linspace(-5, 5, 100)
y = np.linspace(-5, 5, 100)
X, Y = np.meshgrid(x, y)

R = np.sqrt(X**2 + Y**2)

Z = -3 / (R + 0.6)

# superficie SIN grilla
ax.plot_surface(X, Y, Z, cmap='viridis', edgecolor='none')

# masa
ax.scatter(0, 0, -5, s=150, color='red')

ax.set_title("Espacio-tiempo (sin grilla)")
ax.set_xlim(-5, 5)
ax.set_ylim(-5, 5)
ax.set_zlim(-5, 1)

plt.show()
