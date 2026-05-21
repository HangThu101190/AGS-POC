import type { PhanCongSlotFlightDto } from "@/shared/api/phanCongSlotApi";

function parseHhMm(label: string): { mins: number; hasPlus: boolean } | null {
  const s = label.trim();
  if (!s) return null;
  const hasPlus = s.includes("+");
  const core = s.split("+")[0].trim();
  const parts = core.split(":");
  if (parts.length < 2) return null;
  const h = Number(parts[0]);
  const m = Number(parts[1]);
  if (!Number.isFinite(h) || !Number.isFinite(m)) return null;
  return { mins: h * 60 + m, hasPlus };
}

function flightInterval(f: PhanCongSlotFlightDto): [number, number] | null {
  const staLbl = f.sta;
  const stdLbl = f.std;
  const a = parseHhMm(staLbl);
  if (!a) return null;
  const start = a.mins;
  const b = parseHhMm(stdLbl);
  let end: number;
  if (!b) end = start + 60;
  else {
    let endM = b.mins;
    if (b.hasPlus || endM <= start) endM += 24 * 60;
    end = endM;
  }
  if (end <= start) end = start + 30;
  return [start, end];
}

function segmentSpan(seg: string): [number, number] | null {
  const parts = seg.split("-").map((x) => x.trim());
  if (parts.length !== 2) return null;
  const sa = Number(parts[0]);
  const sb = Number(parts[1]);
  if (!Number.isFinite(sa) || !Number.isFinite(sb)) return null;
  let start = sa * 60;
  let end = sb * 60;
  if (sb <= sa) end += 24 * 60;
  return [start, end];
}

function overlap(a0: number, a1: number, b0: number, b1: number) {
  return a0 < b1 && a1 > b0;
}

export function flightOverlapsSlotSegments(f: PhanCongSlotFlightDto, segments: string[]) {
  if (!segments.length) return true;
  const iv = flightInterval(f);
  if (!iv) return true;
  const [fs, fe] = iv;
  return segments.some((seg) => {
    const span = segmentSpan(seg);
    return span ? overlap(fs, fe, span[0], span[1]) : false;
  });
}
