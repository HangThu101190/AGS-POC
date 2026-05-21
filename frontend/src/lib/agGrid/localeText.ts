import type { TFunction } from "i18next";

/** AG Grid UI strings — pass to DataTable / grid `localeText`. */
export function buildAgGridLocaleText(t: TFunction) {
  return {
    sortAscending: t("grid.sortAscending"),
    sortDescending: t("grid.sortDescending"),
    sortUnSort: t("grid.sortUnSort"),
    noSort: t("grid.noSort"),
    filterOoo: t("grid.filterOoo"),
    equals: t("grid.equals"),
    notEqual: t("grid.notEqual"),
    contains: t("grid.contains"),
    notContains: t("grid.notContains"),
    startsWith: t("grid.startsWith"),
    endsWith: t("grid.endsWith"),
    blank: t("grid.blank"),
    notBlank: t("grid.notBlank"),
    applyFilter: t("grid.applyFilter"),
    resetFilter: t("grid.resetFilter"),
    clearFilter: t("grid.clearFilter"),
    pinColumn: t("grid.pinColumn"),
    autosizeThiscolumn: t("grid.autosizeColumn"),
    autosizeAllColumns: t("grid.autosizeAllColumns"),
    resetColumns: t("grid.resetColumns"),
    columns: t("grid.columns"),
    noRowsToShow: t("grid.noRowsToShow"),
    loadingOoo: t("grid.loading"),
    page: t("grid.page"),
    to: t("grid.to"),
    of: t("grid.of"),
    next: t("grid.next"),
    last: t("grid.last"),
    first: t("grid.first"),
    previous: t("grid.previous"),
    pageSizeSelectorLabel: t("grid.pageSize"),
  };
}
