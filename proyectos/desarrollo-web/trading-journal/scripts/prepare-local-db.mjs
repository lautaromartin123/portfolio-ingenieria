import { spawnSync } from "node:child_process";
import { fileURLToPath } from "node:url";

const root = fileURLToPath(new URL("../", import.meta.url));
const wrangler = fileURLToPath(
  new URL("../node_modules/wrangler/bin/wrangler.js", import.meta.url),
);

console.log("Preparando la base local del Trading Journal...");
const result = spawnSync(
  process.execPath,
  [
    wrangler,
    "d1", "migrations", "apply", "DB",
    "--local",
    "--config", "wrangler.local.jsonc",
    "--persist-to", ".wrangler/state",
  ],
  {
    cwd: root,
    stdio: "inherit",
    env: {
      ...process.env,
      CI: "true",
      WRANGLER_SEND_METRICS: "false",
      WRANGLER_WRITE_LOGS: "false",
      CLOUDFLARE_CF_FETCH_ENABLED: "false",
    },
  },
);

if (result.error) {
  console.error("No se pudo iniciar la preparación local:", result.error.message);
  process.exitCode = 1;
} else {
  process.exitCode = result.status ?? 1;
}
