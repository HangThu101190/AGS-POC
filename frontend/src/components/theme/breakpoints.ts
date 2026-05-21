/** Mobile-first breakpoints — keep in sync with `appShell.module.css` and MUI theme. */
export const agsBreakpoints = {
  xs: 0,
  sm: 600,
  md: 768,
  lg: 1024,
} as const;

export const agsBreakpointCss = {
  "--ags-bp-sm": `${agsBreakpoints.sm}px`,
  "--ags-bp-md": `${agsBreakpoints.md}px`,
  "--ags-bp-lg": `${agsBreakpoints.lg}px`,
  "--ags-sidebar-width": "220px",
  "--ags-header-height": "56px",
  "--ags-shell-header-height": "56px",
  "--ags-touch-min": "44px",
  "--ags-content-px": "0.75rem",
  "--ags-content-py": "0.75rem",
} as const;
