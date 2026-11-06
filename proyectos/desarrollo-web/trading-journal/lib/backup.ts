import type { JournalData } from "./domain";
import { downloadFile } from "./csv";
import { uploadScreenshot } from "./api";
import { backupDataSchema } from "./backup-schema";
import { z } from "zod";
const maxBackupBytes = 100 * 1024 * 1024;
export async function createBackup(data: JournalData) {
  backupDataSchema.parse(data);
  if (
    new Blob([JSON.stringify(data)]).size +
      data.screenshots.reduce(
        (n, s) => n + Math.ceil(s.size / 3) * 4 + 500,
        0,
      ) >
    maxBackupBytes
  )
    throw new Error(
      "La copia completa supera 100 MB. Reducí el tamaño de las capturas antes de exportar.",
    );
  const images = [];
  for (const s of data.screenshots) {
    const r = await fetch("/api/screenshots?id=" + s.id);
    if (!r.ok) throw new Error("No se pudo incluir la captura " + s.name);
    const bytes = new Uint8Array(await r.arrayBuffer());
    let binary = "";
    for (let i = 0; i < bytes.length; i += 8192)
      binary += String.fromCharCode(...bytes.slice(i, i + 8192));
    images.push({ ...s, base64: btoa(binary) });
  }
  const content = JSON.stringify({
    format: "trading-journal",
    version: 1,
    exportedAt: new Date().toISOString(),
    data,
    images,
  });
  if (new Blob([content]).size > maxBackupBytes)
    throw new Error(
      "La copia completa supera 100 MB. Reducí el tamaño de las capturas antes de exportar.",
    );
  downloadFile(
    content,
    `trading-journal-backup-${new Date().toISOString().slice(0, 10)}.json`,
  );
}
export async function restoreBackup(file: File) {
  if (file.size > maxBackupBytes)
    throw new Error("La copia supera los 100 MB.");
  const backup = JSON.parse(await file.text());
  if (
    backup.format !== "trading-journal" ||
    backup.version !== 1 ||
    !backup.data ||
    !Array.isArray(backup.images)
  )
    throw new Error("No es una copia compatible de Trading Journal.");
  const data = backupDataSchema.parse(backup.data);
  const images = z
    .array(
      z.object({
        tradeId: z.string().min(1).max(100),
        name: z.string().min(1).max(255),
        kind: z.enum(["before", "after"]),
        size: z
          .number()
          .int()
          .min(1)
          .max(5 * 1024 * 1024),
        mime: z.enum(["image/png", "image/jpeg", "image/webp"]),
        base64: z
          .string()
          .max(7_100_000)
          .regex(
            /^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$/,
          ),
      }),
    )
    .parse(backup.images);
  const result: {
    idMap: Record<string, string>;
    existingImages: {
      tradeId: string;
      name: string;
      kind: string;
      size: number;
    }[];
  } = { idMap: {}, existingImages: [] };
  const empty = {
    ...data,
    trades: [],
    journals: [],
    goals: [],
    reviews: [],
    habits: [],
  };
  let completed = 0;
  const sendBatch = async (batch: typeof data) => {
    try {
      const r = await fetch("/api/backup", {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          "X-Journal-Request": "1",
        },
        body: JSON.stringify(batch),
      });
      const part = (await r.json()) as typeof result & { error?: string };
      if (!r.ok)
        throw new Error(part.error ?? "No se pudo restaurar la copia.");
      Object.assign(result.idMap, part.idMap);
      result.existingImages = part.existingImages;
      completed++;
    } catch (e) {
      throw new Error(
        (e as Error).message +
          (completed
            ? " Parte de la copia ya se guardó. Podés volver a seleccionar el mismo archivo para continuar sin duplicar registros."
            : ""),
      );
    }
  };
  // Restore habits first so journal references can be resolved across batches.
  for (let i = 0; i < data.habits.length; i += 50)
    await sendBatch({ ...empty, habits: data.habits.slice(i, i + 50) });
  const count = Math.max(
    data.trades.length,
    data.journals.length,
    data.goals.length,
    data.reviews.length,
    1,
  );
  for (let i = 0; i < count; i += 50)
    await sendBatch({
      ...empty,
      trades: data.trades.slice(i, i + 50),
      journals: data.journals.slice(i, i + 50),
      goals: data.goals.slice(i, i + 50),
      reviews: data.reviews.slice(i, i + 50),
    });
  let failed = 0;
  for (const image of images) {
    if (
      !result.idMap[image.tradeId] ||
      result.existingImages.some(
        (s) =>
          s.tradeId === result.idMap[image.tradeId] &&
          s.name === image.name &&
          s.kind === image.kind &&
          s.size === image.size,
      )
    )
      continue;
    try {
      const binary = atob(image.base64),
        bytes = Uint8Array.from(binary, (c) => c.charCodeAt(0));
      await uploadScreenshot(
        result.idMap[image.tradeId],
        image.kind,
        new File([bytes], image.name, { type: image.mime }),
      );
    } catch {
      failed++;
    }
  }
  return { failed };
}
