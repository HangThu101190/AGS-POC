import type { ButtonHTMLAttributes, ReactNode } from "react";
import styles from "./actionButton.module.css";

type ActionButtonProps = ButtonHTMLAttributes<HTMLButtonElement> & {
  variant?: "primary" | "secondary";
  children: ReactNode;
};

export function ActionButton({
  variant = "secondary",
  children,
  disabled,
  className,
  style,
  ...rest
}: ActionButtonProps) {
  const variantClass = variant === "primary" ? styles.primary : styles.secondary;
  return (
    <button
      type="button"
      disabled={disabled}
      className={[styles.btn, variantClass, className].filter(Boolean).join(" ")}
      style={style}
      {...rest}
    >
      {children}
    </button>
  );
}
