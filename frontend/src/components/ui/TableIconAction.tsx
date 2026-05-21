import Tooltip from "@mui/material/Tooltip";
import type { ReactNode } from "react";
import { IconButton } from "./IconButton";

type TableIconActionProps = {
  title: string;
  onClick: () => void;
  children: ReactNode;
  tone?: "default" | "danger";
  disabled?: boolean;
};

export function TableIconAction({
  title,
  onClick,
  children,
  tone = "default",
  disabled = false,
}: TableIconActionProps) {
  return (
    <Tooltip title={title} arrow placement="top">
      <span>
        <IconButton
          size="small"
          onClick={onClick}
          disabled={disabled}
          aria-label={title}
          sx={{
            width: 32,
            height: 32,
            p: 0.5,
            ...(tone === "danger" && {
              color: "error.main",
              "&:hover": {
                borderColor: "error.main",
                bgcolor: "#fef2f2",
              },
            }),
          }}
        >
          {children}
        </IconButton>
      </span>
    </Tooltip>
  );
}
