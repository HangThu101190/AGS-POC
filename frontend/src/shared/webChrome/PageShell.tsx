import type { ReactNode } from "react";
import { chrome } from "./styles";
import shellStyles from "./pageShell.module.css";

type PageShellProps = {
  /** Omitted — nav sidebar already labels the page. */
  title?: string;
  subtitle?: ReactNode;
  headerEnd?: ReactNode;
  children: ReactNode;
  fill?: boolean;
};

/** Page chrome — no duplicate page title (saves vertical space). */
export function PageShell({ title, subtitle, headerEnd, children, fill = false }: PageShellProps) {
  const showHeader = Boolean(headerEnd || (title && title.length > 0) || subtitle);
  const shellClass = [shellStyles.shell, fill ? shellStyles.shellFill : ""].filter(Boolean).join(" ");

  return (
    <div className={shellClass}>
      {showHeader ? (
        <div
          className={[
            shellStyles.header,
            !title && !subtitle ? shellStyles.headerToolbarOnly : "",
          ]
            .filter(Boolean)
            .join(" ")}
        >
          <div className={shellStyles.headerRow}>
            {title || subtitle ? (
              <div className={shellStyles.headerMain}>
                {title ? <h1 style={chrome.h1}>{title}</h1> : null}
                {subtitle ? <p style={chrome.subtitle}>{subtitle}</p> : null}
              </div>
            ) : null}
            {headerEnd ? (
              <div
                className={[
                  shellStyles.headerEnd,
                  !title && !subtitle ? shellStyles.headerEndOnly : "",
                ]
                  .filter(Boolean)
                  .join(" ")}
              >
                {headerEnd}
              </div>
            ) : null}
          </div>
        </div>
      ) : null}
      <div className={fill ? shellStyles.bodyFill : undefined}>{children}</div>
    </div>
  );
}
