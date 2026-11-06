"""Menú de las siete simulaciones. No instala paquetes ni cambia el Python global."""
import json
import subprocess
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parent
SIMULATIONS = json.loads((ROOT / "simulaciones.json").read_text(encoding="utf-8"))

def main():
    for number, simulation in enumerate(SIMULATIONS, 1):
        print(f"{number}. {simulation['name']}")
    if len(sys.argv) > 1 and sys.argv[1] == "--list":
        return 0
    choice = sys.argv[1] if len(sys.argv) > 1 else input("Elegí una opción (1-7), o 0 para salir: ").strip()
    if choice == "0":
        return 0
    selected = next((s for i, s in enumerate(SIMULATIONS, 1) if choice in (str(i), s["id"])), None)
    if selected is None:
        print("Opción inválida.")
        return 1
    script = ROOT / selected["id"] / selected["file"]
    return subprocess.run([sys.executable, str(script)], cwd=script.parent, check=False).returncode

if __name__ == "__main__":
    raise SystemExit(main())
