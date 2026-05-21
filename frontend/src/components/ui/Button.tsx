import MuiButton, { type ButtonProps as MuiButtonProps } from "@mui/material/Button";

export type ButtonProps = MuiButtonProps;

/**
 * Button usage (AGS SmartShift):
 * - Button / ButtonPrimary — one main CTA per block (Lưu, Phát hành, Xác nhận)
 * - ButtonOutlined / ButtonSecondary — secondary actions (Hủy, Sửa, Hoàn tác)
 * - ButtonText — low emphasis (links in toolbars)
 * - ButtonDanger — destructive (Xóa, Vô hiệu)
 * - ButtonToolbar — compact toolbar (filters, map tools)
 * - ButtonToggle — on/off tool (Vẽ polygon, bật/tắt filter)
 * - CancelButton — dialog cancel (= ButtonOutlined)
 */

/** Primary CTA — contained navy */
export function Button({ variant = "contained", color = "primary", ...props }: ButtonProps) {
  return <MuiButton variant={variant} color={color} {...props} />;
}

export function ButtonPrimary(props: Omit<ButtonProps, "variant" | "color">) {
  return <Button variant="contained" color="primary" {...props} />;
}

/** Secondary — outlined neutral */
export function ButtonOutlined({
  variant = "outlined",
  color = "inherit",
  ...props
}: ButtonProps) {
  return <MuiButton variant={variant} color={color} {...props} />;
}

export const ButtonSecondary = ButtonOutlined;

/** Tertiary — text only */
export function ButtonText({ variant = "text", color = "primary", ...props }: ButtonProps) {
  return <MuiButton variant={variant} color={color} {...props} />;
}

/** Destructive */
export function ButtonDanger({ variant = "outlined", color = "error", ...props }: ButtonProps) {
  return <MuiButton variant={variant} color={color} {...props} />;
}

/** Toolbar / map tools — outlined; default small, pass size="medium" for page toolbars */
export function ButtonToolbar({ size = "small", ...props }: ButtonProps) {
  return <MuiButton variant="outlined" color="inherit" size={size} {...props} />;
}

/** Toggle tool — warning tone when active (e.g. draw mode) */
export function ButtonToggle({
  active = false,
  variant,
  color,
  size = "small",
  ...props
}: ButtonProps & { active?: boolean }) {
  return (
    <MuiButton
      variant={variant ?? (active ? "contained" : "outlined")}
      color={color ?? (active ? "warning" : "inherit")}
      size={size}
      aria-pressed={active}
      {...props}
    />
  );
}

export function CancelButton(props: Omit<ButtonProps, "variant" | "color">) {
  return <ButtonOutlined {...props} />;
}
