import type { ReactNode } from "react";
import { WeekPicker } from "@/features/planning/WeekPicker";
import { PageShell } from "./PageShell";

type WeekPageShellProps = {
  children: ReactNode;
  /** When false, page renders its own week picker (e.g. Flights — above day chips). */
  weekInHeader?: boolean;
  /** Stretch page content to fill viewport below app chrome (for full-height tables). */
  fill?: boolean;
};

/** Week-scoped page — toolbar is only the inline week picker (no page title). */
export function WeekPageShell({ children, weekInHeader = true, fill = false }: WeekPageShellProps) {
  return (
    <PageShell
      fill={fill}
      headerEnd={weekInHeader ? <WeekPicker inline /> : undefined}
    >
      {children}
    </PageShell>
  );
}
