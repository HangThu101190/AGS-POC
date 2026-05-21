import type { ColDef } from "ag-grid-community";
import type { TFunction } from "i18next";

/**
 * AG Grid column: `headerTooltip` when label is shorter than the full meaning (abbrev / code).
 */
export function gridCol<T>(
  t: TFunction,
  headerKey: string,
  tipKey: string | undefined,
  def: ColDef<T>,
): ColDef<T> {
  const headerName = t(headerKey);
  const tip = tipKey ? t(tipKey) : undefined;
  return {
    ...def,
    headerName,
    ...(tip && tip !== headerName ? { headerTooltip: tip } : {}),
  };
}
