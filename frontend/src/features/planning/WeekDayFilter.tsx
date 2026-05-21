import Chip from "@mui/material/Chip";
import Stack from "@mui/material/Stack";
import { useTranslation } from "react-i18next";
import { isPastDay } from "@/shared/planning/weekCalendar";

type WeekDayFilterProps = {
  weekDates: string[];
  todayIdx: number;
  value: number;
  onChange: (dayIdx: number) => void;
};

/** Day-of-week picker with past/today badges (prototype parity). */
export function WeekDayFilter({ weekDates, todayIdx, value, onChange }: WeekDayFilterProps) {
  const { t } = useTranslation();

  return (
    <Stack direction="row" spacing={0.75} sx={{ flexWrap: "wrap", gap: 0.75 }}>
      {weekDates.map((label, dayIdx) => {
        const past = isPastDay(dayIdx, todayIdx);
        const isToday = dayIdx === todayIdx;
        const selected = dayIdx === value;
        return (
          <Chip
            key={dayIdx}
            label={
              past
                ? `${label} · ${t("planning.pastBadge")}`
                : isToday
                  ? `${label} · ${t("planning.todayBadge")}`
                  : label
            }
            size="small"
            variant={selected ? "filled" : "outlined"}
            color={selected ? "primary" : "default"}
            onClick={() => onChange(dayIdx)}
            sx={{
              opacity: past && !selected ? 0.6 : 1,
              maxWidth: "100%",
            }}
          />
        );
      })}
    </Stack>
  );
}
