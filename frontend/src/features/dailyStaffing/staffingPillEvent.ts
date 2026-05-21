function escapeHtml(text: string): string {
  return text
    .replace(/&/g, "&amp;")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .replace(/"/g, "&quot;");
}

/** Pill layout for assignment blocks (reference timeline UX). */
export function buildAssignmentPillHtml(employeeName: string, metaLine: string): string {
  const title = escapeHtml(employeeName.trim() || "—");
  const meta = escapeHtml(metaLine.trim());
  return `<div class="staffing-pill-event" role="presentation">
  <div class="staffing-pill-event__title">${title}</div>
  ${meta ? `<div class="staffing-pill-event__meta">${meta}</div>` : ""}
</div>`;
}

export function parseAssignmentEventTitle(title: string): { name: string; meta: string } {
  const lines = title.split("\n").map((s) => s.trim()).filter(Boolean);
  if (lines.length === 0) return { name: "", meta: "" };
  if (lines.length === 1) return { name: lines[0], meta: "" };
  return { name: lines[0], meta: lines.slice(1).join(" · ") };
}
