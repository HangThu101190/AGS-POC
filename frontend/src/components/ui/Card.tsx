import MuiCard, { type CardProps as MuiCardProps } from "@mui/material/Card";
import MuiCardContent, { type CardContentProps } from "@mui/material/CardContent";
import Typography from "@mui/material/Typography";

export type CardProps = MuiCardProps & {
  title?: string;
  contentProps?: CardContentProps;
};

/** Page section card with optional uppercase title */
export function Card({ title, children, contentProps, sx, ...props }: CardProps) {
  return (
    <MuiCard
      sx={{
        borderColor: "var(--ags-border)",
        borderRadius: "var(--ags-radius, 8px)",
        boxShadow: "none",
        ...sx,
      }}
      {...props}
    >
      <MuiCardContent {...contentProps}>
        {title ? (
          <Typography
            variant="overline"
            component="h2"
            sx={{ display: "block", mb: 1.5, color: "text.secondary", fontWeight: 600 }}
          >
            {title}
          </Typography>
        ) : null}
        {children}
      </MuiCardContent>
    </MuiCard>
  );
}
