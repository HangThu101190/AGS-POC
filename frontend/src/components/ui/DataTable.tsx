import Menu from "@mui/material/Menu";
import MenuItem from "@mui/material/MenuItem";
import {
  forwardRef,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ComponentRef,
  type CSSProperties,
  type MouseEvent,
  type Ref,
} from "react";
import { useTranslation } from "react-i18next";
import { AgGridReact } from "ag-grid-react";
import type {
  CellContextMenuEvent,
  ColDef,
  GridApi,
  GridOptions,
  GridReadyEvent,
  IDatasource,
  IGetRowsParams,
} from "ag-grid-community";
import { agsGridTheme } from "@/lib/agGrid/agsTheme";
import { buildAgGridLocaleText } from "@/lib/agGrid/localeText";
import { useIsMdUp } from "@/shared/hooks/useMediaQuery";
import type { PagedList, TableListParams } from "@/shared/types/tableList";
import { TableSkeleton } from "./TableSkeleton";
import styles from "./dataTable.module.css";

export type DataTableFetchRows<TData> = (
  params: TableListParams,
  signal?: AbortSignal,
) => Promise<PagedList<TData>>;

export type DataTableProps<TData = unknown> = {
  columnDefs: ColDef<TData>[];
  /** Server-side lazy load — required for every DataTable. */
  fetchRows: DataTableFetchRows<TData>;
  height?: number | string;
  fillHeight?: boolean;
  autoFitColumns?: boolean;
  enableFilter?: boolean;
  enableFloatingFilter?: boolean;
  showRowCount?: boolean;
  /** Rows per infinite-scroll block (and API page size). */
  cacheBlockSize?: number;
  className?: string;
  style?: CSSProperties;
  headerHeight?: number;
  floatingFiltersHeight?: number;
  rowHeight?: number;
  defaultColDef?: ColDef<TData>;
  gridOptions?: GridOptions<TData>;
  onGridReady?: (event: GridReadyEvent<TData>) => void;
  domLayout?: "normal" | "autoHeight" | "print";
};

type ContextMenuState<TData> = {
  mouseX: number;
  mouseY: number;
  row: TData | undefined;
  value: string | null | undefined;
};

function fitColumns(api: GridApi | undefined) {
  if (!api) return;
  try {
    api.sizeColumnsToFit();
  } catch {
    /* destroyed */
  }
}

function getRowsParamsToTableList(
  params: IGetRowsParams,
  blockSize: number,
): TableListParams {
  const sort = params.sortModel[0];
  return {
    page: Math.floor(params.startRow / blockSize),
    pageSize: blockSize,
    sortBy: sort?.colId,
    sortDir: sort?.sort === "desc" ? "desc" : sort?.sort === "asc" ? "asc" : undefined,
    filters: params.filterModel as Record<string, unknown>,
  };
}

function DataTableInner<TData>(
  {
    columnDefs,
    fetchRows,
    height = 360,
    fillHeight = false,
    autoFitColumns = false,
    enableFilter = true,
    enableFloatingFilter = false,
    showRowCount = false,
    cacheBlockSize = 20,
    className = "",
    style,
    headerHeight,
    floatingFiltersHeight = 36,
    rowHeight = 42,
    defaultColDef,
    gridOptions,
    onGridReady,
    domLayout = "normal",
  }: DataTableProps<TData>,
  ref: Ref<ComponentRef<typeof AgGridReact<TData>>>,
) {
  const { t, i18n } = useTranslation();
  const isMdUp = useIsMdUp();
  const apiRef = useRef<GridApi<TData> | null>(null);
  const fetchRowsRef = useRef(fetchRows);
  const [contextMenu, setContextMenu] = useState<ContextMenuState<TData> | null>(null);
  const [totalCount, setTotalCount] = useState<number | null>(null);
  const [initialLoading, setInitialLoading] = useState(true);
  const [refreshing, setRefreshing] = useState(false);
  const hasLoadedOnceRef = useRef(false);
  const inFlightRef = useRef(0);

  useEffect(() => {
    fetchRowsRef.current = fetchRows;
  }, [fetchRows]);

  const localeText = useMemo(() => buildAgGridLocaleText(t), [t, i18n.language]);

  const resolvedHeaderHeight = headerHeight ?? (enableFloatingFilter ? 44 : isMdUp ? 42 : 40);
  const resolvedRowHeight = rowHeight === 42 && !isMdUp ? 46 : rowHeight;

  const mergedDefaultColDef = useMemo<ColDef<TData>>(
    () => ({
      sortable: true,
      resizable: true,
      filter: enableFilter ? "agTextColumnFilter" : false,
      floatingFilter: enableFilter && enableFloatingFilter,
      suppressHeaderFilterButton: !enableFilter,
      suppressHeaderMenuButton: true,
      filterParams: enableFilter ? { debounceMs: 400 } : undefined,
      flex: 1,
      minWidth: 100,
      ...defaultColDef,
    }),
    [defaultColDef, enableFilter, enableFloatingFilter],
  );

  const mergedGridOptions = useMemo<GridOptions<TData>>(
    () => ({
      animateRows: false,
      suppressColumnMoveAnimation: true,
      enableCellTextSelection: true,
      ensureDomOrder: true,
      suppressMenuHide: false,
      preventDefaultOnContextMenu: true,
      rowModelType: "infinite",
      cacheBlockSize,
      maxBlocksInCache: 10,
      infiniteInitialRowCount: cacheBlockSize,
      ...gridOptions,
    }),
    [cacheBlockSize, gridOptions],
  );

  const shellStyle = useMemo<CSSProperties>(() => {
    if (fillHeight) {
      return { width: "100%", height: "100%", minHeight: 0, ...style };
    }
    const resolvedHeight =
      typeof height === "number"
        ? height
        : height ?? (isMdUp ? 360 : "clamp(260px, 52vh, 420px)");
    return { width: "100%", height: resolvedHeight, minWidth: 0, ...style };
  }, [fillHeight, height, isMdUp, style]);

  const datasource = useMemo<IDatasource>(
    () => ({
      getRows: (params: IGetRowsParams) => {
        const isInitial = params.startRow === 0;
        if (isInitial) {
          if (hasLoadedOnceRef.current) {
            setRefreshing(true);
          } else {
            setInitialLoading(true);
          }
        }
        inFlightRef.current += 1;
        void fetchRowsRef
          .current(getRowsParamsToTableList(params, cacheBlockSize))
          .then((page) => {
            setTotalCount(page.totalCount);
            params.successCallback(page.items, page.totalCount);
            hasLoadedOnceRef.current = true;
          })
          .catch(() => {
            setTotalCount(0);
            params.successCallback([], 0);
          })
          .finally(() => {
            inFlightRef.current -= 1;
            if (inFlightRef.current <= 0) {
              inFlightRef.current = 0;
              setInitialLoading(false);
              setRefreshing(false);
            }
          });
      },
    }),
    [cacheBlockSize],
  );

  useEffect(() => {
    apiRef.current?.refreshInfiniteCache();
  }, [fetchRows, datasource]);

  const refreshFromServer = useCallback(() => {
    apiRef.current?.refreshInfiniteCache();
  }, []);

  const handleGridReady = useCallback(
    (params: GridReadyEvent<TData>) => {
      apiRef.current = params.api;
      params.api.setGridOption("datasource", datasource);
      if (autoFitColumns) fitColumns(params.api);
      onGridReady?.(params);
    },
    [autoFitColumns, datasource, onGridReady],
  );

  const handleSortOrFilterChanged = useCallback(() => {
    refreshFromServer();
  }, [refreshFromServer]);

  const closeContextMenu = () => setContextMenu(null);

  const handleCellContextMenu = useCallback((event: CellContextMenuEvent<TData>) => {
    const e = event.event;
    if (e instanceof MouseEvent) {
      e.preventDefault();
      e.stopPropagation();
    }
    if (!(e instanceof MouseEvent)) return;
    setContextMenu({
      mouseX: e.clientX,
      mouseY: e.clientY,
      row: event.data,
      value: event.value != null ? String(event.value) : "",
    });
  }, []);

  const blockBrowserContextMenu = useCallback((e: MouseEvent) => {
    e.preventDefault();
  }, []);

  const copyText = useCallback(async (text: string) => {
    try {
      await navigator.clipboard.writeText(text);
    } catch {
      /* ignore */
    }
    closeContextMenu();
  }, []);

  const copyRow = useCallback(() => {
    const row = contextMenu?.row;
    if (row == null) return;
    const fields = columnDefs.map((c) => c.field).filter(Boolean) as string[];
    const line = fields.map((f) => String((row as Record<string, unknown>)[f] ?? "")).join("\t");
    void copyText(line);
  }, [columnDefs, contextMenu?.row, copyText]);

  const resetTableState = useCallback(() => {
    const api = apiRef.current;
    if (!api) return;
    api.setFilterModel(null);
    api.applyColumnState({ defaultState: { sort: null } });
    refreshFromServer();
    closeContextMenu();
  }, [refreshFromServer]);

  const exportCsv = useCallback(() => {
    apiRef.current?.exportDataAsCsv({ fileName: "ags-export.csv" });
    closeContextMenu();
  }, []);

  const autoSizeAll = useCallback(() => {
    apiRef.current?.autoSizeAllColumns();
    closeContextMenu();
  }, []);

  const wrapperClass = [
    styles.wrapper,
    fillHeight ? styles.wrapperFill : "",
    className,
  ]
    .filter(Boolean)
    .join(" ");

  const rowCount = totalCount ?? 0;
  const skeletonRows = useMemo(() => {
    const h =
      typeof shellStyle.height === "number"
        ? shellStyle.height
        : typeof height === "number"
          ? height
          : 400;
    return Math.max(6, Math.floor((h - resolvedHeaderHeight) / resolvedRowHeight));
  }, [height, resolvedHeaderHeight, resolvedRowHeight, shellStyle.height]);

  const gridShellClass = [
    styles.gridShell,
    initialLoading ? styles.gridInitialLoading : "",
    refreshing ? styles.gridRefreshing : "",
  ]
    .filter(Boolean)
    .join(" ");

  return (
    <div className={wrapperClass} style={shellStyle}>
      {showRowCount && (
        <div className={styles.toolbar}>
          <span className={styles.rowCount}>
            {t("grid.rowCount", { count: rowCount })}
          </span>
        </div>
      )}

      <div className={`${gridShellClass} ags-scroll-x`} onContextMenu={blockBrowserContextMenu}>
        {initialLoading ? (
          <TableSkeleton
            rows={skeletonRows}
            columns={Math.min(columnDefs.length, 8)}
            headerHeight={resolvedHeaderHeight}
            rowHeight={resolvedRowHeight}
          />
        ) : null}
        <div className={styles.gridInner}>
          <AgGridReact<TData>
          ref={ref}
          theme={agsGridTheme}
          columnDefs={columnDefs}
          defaultColDef={mergedDefaultColDef}
          localeText={localeText}
          domLayout={domLayout}
          headerHeight={resolvedHeaderHeight}
          floatingFiltersHeight={enableFloatingFilter ? floatingFiltersHeight : undefined}
          rowHeight={resolvedRowHeight}
          onGridReady={handleGridReady}
          onSortChanged={handleSortOrFilterChanged}
          onFilterChanged={handleSortOrFilterChanged}
          onCellContextMenu={handleCellContextMenu}
          preventDefaultOnContextMenu
          suppressCellFocus
          gridOptions={mergedGridOptions}
        />
        </div>
      </div>

      <Menu
        open={contextMenu != null}
        onClose={closeContextMenu}
        anchorReference="anchorPosition"
        anchorPosition={
          contextMenu != null
            ? { top: contextMenu.mouseY, left: contextMenu.mouseX }
            : undefined
        }
      >
        <MenuItem
          disabled={!contextMenu?.value}
          onClick={() => contextMenu?.value && void copyText(contextMenu.value)}
        >
          {t("grid.ctxCopyCell")}
        </MenuItem>
        <MenuItem disabled={contextMenu?.row == null} onClick={copyRow}>
          {t("grid.ctxCopyRow")}
        </MenuItem>
        <MenuItem onClick={autoSizeAll}>{t("grid.ctxAutoSize")}</MenuItem>
        <MenuItem onClick={resetTableState}>{t("grid.ctxResetFilters")}</MenuItem>
        <MenuItem onClick={exportCsv}>{t("grid.ctxExportCsv")}</MenuItem>
      </Menu>
    </div>
  );
}

/** Server-side AG Grid table — infinite row model, column filter/sort on API. */
export const DataTable = forwardRef(DataTableInner) as <TData>(
  props: DataTableProps<TData> & { ref?: Ref<ComponentRef<typeof AgGridReact<TData>>> },
) => ReturnType<typeof DataTableInner>;
