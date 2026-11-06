export async function boundedBody(request: Request, limit: number) {
  if (Number(request.headers.get("content-length")) > limit)
    throw new Error("Solicitud demasiado grande.");
  const reader = request.body?.getReader();
  if (!reader) return new Uint8Array();
  const parts: Uint8Array[] = [];
  let total = 0;
  while (true) {
    const { done, value } = await reader.read();
    if (done) break;
    total += value.length;
    if (total > limit) {
      await reader.cancel();
      throw new Error("Solicitud demasiado grande.");
    }
    parts.push(value);
  }
  const bytes = new Uint8Array(total);
  let offset = 0;
  for (const part of parts) {
    bytes.set(part, offset);
    offset += part.length;
  }
  return bytes;
}
export function imageInfo(
  bytes: Uint8Array,
): { mime: string; width: number; height: number } | null {
  if (bytes.length < 32) return null;
  const dv = new DataView(bytes.buffer, bytes.byteOffset, bytes.byteLength),
    ascii = (start: number, end: number) =>
      String.fromCharCode(...bytes.slice(start, end));
  let mime = "",
    width = 0,
    height = 0;
  if (
    bytes[0] === 137 &&
    ascii(1, 4) === "PNG" &&
    bytes[4] === 13 &&
    bytes[5] === 10 &&
    bytes[6] === 26 &&
    bytes[7] === 10 &&
    ascii(12, 16) === "IHDR" &&
    dv.getUint32(8) === 13 &&
    ascii(bytes.length - 8, bytes.length - 4) === "IEND"
  ) {
    mime = "image/png";
    width = dv.getUint32(16);
    height = dv.getUint32(20);
  } else if (
    bytes[0] === 255 &&
    bytes[1] === 216 &&
    bytes.at(-2) === 255 &&
    bytes.at(-1) === 217
  ) {
    for (let p = 2; p + 8 < bytes.length; ) {
      if (bytes[p] !== 255) {
        p++;
        continue;
      }
      const marker = bytes[p + 1];
      if (marker === 0xd9 || marker === 0xda) break;
      const length = dv.getUint16(p + 2);
      if (length < 2 || p + 2 + length > bytes.length) break;
      if (
        [
          0xc0, 0xc1, 0xc2, 0xc3, 0xc5, 0xc6, 0xc7, 0xc9, 0xca, 0xcb, 0xcd,
          0xce, 0xcf,
        ].includes(marker)
      ) {
        mime = "image/jpeg";
        height = dv.getUint16(p + 5);
        width = dv.getUint16(p + 7);
        break;
      }
      p += 2 + length;
    }
  } else if (
    ascii(0, 4) === "RIFF" &&
    ascii(8, 12) === "WEBP" &&
    dv.getUint32(4, true) + 8 === bytes.length
  ) {
    const type = ascii(12, 16);
    if (type === "VP8X") {
      width = 1 + bytes[24] + (bytes[25] << 8) + (bytes[26] << 16);
      height = 1 + bytes[27] + (bytes[28] << 8) + (bytes[29] << 16);
    } else if (
      type === "VP8 " &&
      bytes[23] === 0x9d &&
      bytes[24] === 1 &&
      bytes[25] === 0x2a
    ) {
      width = dv.getUint16(26, true) & 0x3fff;
      height = dv.getUint16(28, true) & 0x3fff;
    } else if (type === "VP8L" && bytes[20] === 0x2f) {
      const bits = dv.getUint32(21, true);
      width = (bits & 0x3fff) + 1;
      height = ((bits >> 14) & 0x3fff) + 1;
    }
    if (width && height) mime = "image/webp";
  }
  return mime &&
    width > 0 &&
    height > 0 &&
    width <= 16000 &&
    height <= 16000 &&
    width * height <= 60_000_000
    ? { mime, width, height }
    : null;
}
