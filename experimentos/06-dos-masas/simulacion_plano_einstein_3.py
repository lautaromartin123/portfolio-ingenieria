import matplotlib
matplotlib.use("TkAgg")

import numpy as np
import matplotlib.pyplot as plt

fig = plt.figure()
ax = fig.add_subplot(111, projection="3d")

fig.patch.set_facecolor('black')
ax.set_facecolor('black')

# plano
x = np.linspace(-5, 5, 100)
y = np.linspace(-5, 5, 100)
X, Y = np.meshgrid(x, y)

R = np.sqrt(X**2 + Y**2)
Z = -3/(np.sqrt((X+2)**2 + Y**2)+0.6) - 3/(np.sqrt((X-2)**2 + Y**2)+0.6)

# superficie
ax.plot_surface(X, Y, Z, cmap='viridis', edgecolor='none')

# masa
ax.scatter(0, 0, -5, s=150, color='red')

# ❌ ocultar TODO lo visual extra
ax.set_axis_off()

plt.show()
