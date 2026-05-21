import styles from "./tableSkeleton.module.css";

type TableSkeletonProps = {
  rows?: number;
  columns?: number;
  headerHeight?: number;
  rowHeight?: number;
};

export function TableSkeleton({
  rows = 10,
  columns = 6,
  headerHeight = 42,
  rowHeight = 42,
}: TableSkeletonProps) {
  const widths = ["barWide", "barMid", "barNarrow", "barMid", "barWide", "barNarrow"] as const;

  return (
    <div className={styles.overlay} role="status" aria-busy="true" aria-label="Loading">
      <div className={styles.header} style={{ height: headerHeight }}>
        {Array.from({ length: columns }, (_, i) => (
          <div key={`h-${i}`} className={styles.headerCell}>
            <div className={`${styles.bar} ${styles[widths[i % widths.length]]}`} />
          </div>
        ))}
      </div>
      <div className={styles.body}>
        {Array.from({ length: rows }, (_, r) => (
          <div key={`r-${r}`} className={styles.row} style={{ height: rowHeight }}>
            {Array.from({ length: columns }, (_, c) => (
              <div key={`c-${c}`} className={styles.cell}>
                <div
                  className={`${styles.bar} ${styles[widths[(r + c) % widths.length]]}`}
                />
              </div>
            ))}
          </div>
        ))}
      </div>
    </div>
  );
}
