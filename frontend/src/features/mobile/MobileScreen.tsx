import type { ReactNode } from "react";
import { mobile } from "./mobileStyles";

type MobileScreenProps = {
  title: string;
  subtitle?: ReactNode;
  children: ReactNode;
};

export function MobileScreen({ title, subtitle, children }: MobileScreenProps) {
  return (
    <div style={mobile.screen}>
      <h1 style={mobile.title}>{title}</h1>
      {subtitle ? <p style={mobile.subtitle}>{subtitle}</p> : null}
      {children}
    </div>
  );
}
