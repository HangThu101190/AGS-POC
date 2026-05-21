import MuiIconButton, { type IconButtonProps as MuiIconButtonProps } from "@mui/material/IconButton";
import { agsTokens } from "@/components/theme/tokens";

export type IconButtonProps = MuiIconButtonProps;

export function IconButton(props: IconButtonProps) {
  return (
    <MuiIconButton
      sx={{
        border: 1,
        borderColor: "divider",
        borderRadius: 1,
        bgcolor: "background.paper",
        "&:hover": {
          borderColor: "primary.main",
          bgcolor: agsTokens.primarySoft,
        },
      }}
      {...props}
    />
  );
}
