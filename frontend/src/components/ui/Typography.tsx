import MuiTypography, { type TypographyProps } from "@mui/material/Typography";

export type { TypographyProps };

export function Typography(props: TypographyProps) {
  return <MuiTypography {...props} />;
}

export function PageTitle({ children, ...props }: TypographyProps) {
  return (
    <MuiTypography
      variant="h4"
      component="h1"
      sx={{
        mt: 0,
        mb: { xs: 1, md: 1.25 },
        fontSize: { xs: "1.25rem", sm: "1.375rem", md: "1.625rem" },
        fontWeight: 700,
        letterSpacing: -0.5,
        color: "var(--ags-text-body)",
        lineHeight: 1.25,
      }}
      {...props}
    >
      {children}
    </MuiTypography>
  );
}
