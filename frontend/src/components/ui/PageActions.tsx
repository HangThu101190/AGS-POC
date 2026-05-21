import Stack, { type StackProps } from "@mui/material/Stack";

/** Full-width stacked actions on xs; inline row from sm. Touch-friendly min height. */
export function PageActions({ sx, ...props }: StackProps) {
  return (
    <Stack
      direction={{ xs: "column", sm: "row" }}
      spacing={1}
      useFlexGap
      sx={{
        width: "100%",
        flexWrap: "wrap",
        "& .MuiButton-root": {
          minHeight: { xs: 44, sm: 36 },
          width: { xs: "100%", sm: "auto" },
        },
        ...sx,
      }}
      {...props}
    />
  );
}
