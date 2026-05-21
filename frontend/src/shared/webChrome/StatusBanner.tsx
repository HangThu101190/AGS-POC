import type { CSSProperties, ReactNode } from "react";
import { chrome } from "./styles";

type BannerVariant = "past" | "info" | "warn" | "success";

const styles: Record<BannerVariant, CSSProperties> = {
  past: chrome.pastBanner,
  info: chrome.infoBanner,
  warn: chrome.warnBanner,
  success: chrome.successBanner,
};

type StatusBannerProps = {
  variant: BannerVariant;
  children: ReactNode;
  icon?: string;
};

export function StatusBanner({ variant, children, icon }: StatusBannerProps) {
  return (
    <div style={styles[variant]} role="status">
      {icon ? <span aria-hidden>{icon}</span> : null}
      <span>{children}</span>
    </div>
  );
}
