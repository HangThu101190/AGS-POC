import { useMemo, useState, type SyntheticEvent } from "react";
import { useTranslation } from "react-i18next";
import Accordion from "@mui/material/Accordion";
import AccordionDetails from "@mui/material/AccordionDetails";
import AccordionSummary from "@mui/material/AccordionSummary";
import Box from "@mui/material/Box";
import Chip from "@mui/material/Chip";
import List from "@mui/material/List";
import ListItemButton from "@mui/material/ListItemButton";
import ListItemIcon from "@mui/material/ListItemIcon";
import ListItemText from "@mui/material/ListItemText";
import Popover from "@mui/material/Popover";
import Typography from "@mui/material/Typography";
import {
  hourSlotFlightNo,
  rosterPickOptionsForLine,
  type HourSlotFlightGroup,
} from "@/features/dailyStaffing/staffingHourSlotFlightView";
import type { StaffingRosterEntry } from "@/shared/api/staffingApi";
import { flightEtaEtdDisplayTime } from "@/shared/planning/flightClock";

function stopBubble(e: SyntheticEvent) {
  e.stopPropagation();
}

type Props = {
  flightGroups: HourSlotFlightGroup[];
  roster?: StaffingRosterEntry[];
  badgeText: string;
  badgeClassName: string;
  disabled?: boolean;
  onToggleFlight: (employeeId: string, flightId: string) => void;
};

const accordionSx = {
  boxShadow: "none",
  "&::before": { display: "none" },
  borderBottom: "1px solid",
  borderColor: "divider",
  "&:last-of-type": { borderBottom: 0 },
} as const;

const summarySx = {
  minHeight: 44,
  px: 1,
  bgcolor: "grey.50",
  "&.Mui-expanded": { minHeight: 44, bgcolor: "primary.50" },
  "& .MuiAccordionSummary-content": { my: 0.75 },
  "& .MuiAccordionSummary-expandIconWrapper": {
    color: "text.secondary",
  },
} as const;

export function StaffingHourSlotHeadDropdown({
  flightGroups,
  roster,
  badgeText,
  badgeClassName,
  disabled,
  onToggleFlight,
}: Props) {
  const { t } = useTranslation();
  const [anchorEl, setAnchorEl] = useState<HTMLElement | null>(null);
  const [expanded, setExpanded] = useState<string | false>(false);

  const open = Boolean(anchorEl) && !disabled;

  const flightRows = useMemo(
    () =>
      flightGroups.map((group) => ({
        group,
        options: rosterPickOptionsForLine(roster, group.line?.id ?? null),
      })),
    [flightGroups, roster],
  );

  const handleAccordionChange =
    (flightId: string) => (_: SyntheticEvent, isExpanded: boolean) => {
      setExpanded(isExpanded ? flightId : false);
    };

  return (
    <Box
      className="staffing-slot-card__nv-wrap"
      onClick={stopBubble}
      onMouseDown={stopBubble}
    >
      <button
        type="button"
        className={`staffing-slot-card__nv-btn staffing-slot-card__badge ${badgeClassName}`}
        disabled={disabled}
        title={t("phanCongSlot.assignFlt")}
        aria-label={t("staffing.hourSlotHeadMenu")}
        aria-expanded={open}
        aria-haspopup="tree"
        onClick={(e) => setAnchorEl(e.currentTarget)}
      >
        <span className="staffing-slot-card__nv-btn-icon" aria-hidden>
          ✈
        </span>
        <span className="staffing-slot-card__nv-btn-label">{badgeText}</span>
        <span className="staffing-slot-card__nv-btn-caret" aria-hidden>
          ▾
        </span>
      </button>

      <Popover
        open={open}
        anchorEl={anchorEl}
        onClose={() => {
          setAnchorEl(null);
          setExpanded(false);
        }}
        anchorOrigin={{ vertical: "bottom", horizontal: "left" }}
        transformOrigin={{ vertical: "top", horizontal: "left" }}
        marginThreshold={8}
        slotProps={{
          paper: {
            elevation: 8,
            onClick: stopBubble,
            onMouseDown: stopBubble,
            sx: {
              mt: 0.5,
              minWidth: Math.max(anchorEl?.offsetWidth ?? 200, 300),
              maxWidth: 360,
              overflow: "hidden",
              display: "flex",
              flexDirection: "column",
              borderRadius: 2,
              border: "1px solid",
              borderColor: "divider",
            },
          },
        }}
        sx={{ zIndex: 10_050 }}
      >
        <Box
          sx={{
            px: 1.5,
            py: 1,
            borderBottom: 1,
            borderColor: "divider",
            bgcolor: "grey.50",
          }}
        >
          <Typography variant="caption" color="text.secondary" sx={{ display: "block" }}>
            {t("staffing.hourSlotHeadMenu")}
          </Typography>
          <Typography variant="caption" color="text.disabled" sx={{ display: "block" }}>
            {t("staffing.hourSlotHeadMenuHint")}
          </Typography>
        </Box>

        <Box sx={{ overflow: "auto", maxHeight: 340 }}>
          {flightRows.length === 0 ? (
            <Typography variant="body2" color="text.secondary" sx={{ p: 2 }}>
              {t("phanCongSlot.noFlightsOverlap")}
            </Typography>
          ) : (
            flightRows.map(({ group, options }) => {
              const { flight, line, need, assigned } = group;
              const time = flightEtaEtdDisplayTime(flight, "etd") || flight.std;
              const panelId = `staffing-flight-${flight.id}`;
              const isExpanded = expanded === flight.id;

              return (
                <Accordion
                  key={flight.id}
                  disableGutters
                  square
                  expanded={isExpanded}
                  onChange={handleAccordionChange(flight.id)}
                  sx={accordionSx}
                >
                  <AccordionSummary
                    expandIcon={
                      <Box component="span" sx={{ fontSize: 11, lineHeight: 1 }}>
                        ▾
                      </Box>
                    }
                    aria-controls={`${panelId}-content`}
                    id={`${panelId}-header`}
                    sx={summarySx}
                  >
                    <Box
                      sx={{
                        display: "flex",
                        alignItems: "center",
                        gap: 0.75,
                        width: "100%",
                        pr: 0.5,
                        flexWrap: "wrap",
                      }}
                    >
                      <Typography
                        variant="body2"
                        color="primary.dark"
                        sx={{ fontWeight: 700 }}
                      >
                        {hourSlotFlightNo(group)}
                      </Typography>
                      <Chip
                        label={time}
                        size="small"
                        variant="outlined"
                        sx={{ height: 20, fontSize: "0.68rem" }}
                      />
                      {line && need > 0 ? (
                        <Chip
                          label={`${assigned}/${need}`}
                          size="small"
                          color={assigned >= need ? "success" : assigned > 0 ? "warning" : "default"}
                          variant={assigned >= need ? "filled" : "outlined"}
                          sx={{ height: 20, fontSize: "0.68rem", ml: "auto" }}
                        />
                      ) : null}
                    </Box>
                  </AccordionSummary>

                  <AccordionDetails sx={{ p: 0, bgcolor: "background.paper" }}>
                    {!line ? (
                      <Typography variant="body2" color="text.secondary" sx={{ px: 2, py: 1.5 }}>
                        {t("staffing.hourSlotNoLineForFlight")}
                      </Typography>
                    ) : options.length === 0 ? (
                      <Typography variant="body2" color="text.secondary" sx={{ px: 2, py: 1.5 }}>
                        {t("phanCongSlot.pickPlaceholder")}
                      </Typography>
                    ) : (
                      <List dense disablePadding role="group" aria-label={hourSlotFlightNo(group)}>
                        {options.map((o) => (
                          <ListItemButton
                            key={o.value}
                            disabled={o.disabled || disabled}
                            selected={o.assignedOnLine}
                            onClick={() => {
                              if (!o.disabled && !disabled) onToggleFlight(o.value, flight.id);
                            }}
                            sx={{
                              py: 0.75,
                              pl: 2.5,
                              borderLeft: "3px solid",
                              borderColor: o.assignedOnLine ? "success.main" : "transparent",
                              "&.Mui-selected": {
                                bgcolor: "success.50",
                                "&:hover": { bgcolor: "success.100" },
                              },
                              "&.Mui-disabled": { opacity: 0.72 },
                            }}
                          >
                            <ListItemIcon sx={{ minWidth: 28 }}>
                              {o.assignedOnLine ? (
                                <Box
                                  component="span"
                                  aria-hidden
                                  sx={{ color: "success.main", fontSize: 14, fontWeight: 700 }}
                                >
                                  ✓
                                </Box>
                              ) : (
                                <Box
                                  component="span"
                                  aria-hidden
                                  sx={{ color: "text.disabled", fontSize: 12 }}
                                >
                                  ○
                                </Box>
                              )}
                            </ListItemIcon>
                            <ListItemText
                              primary={o.name}
                              secondary={
                                [o.code, o.hint].filter(Boolean).join(" · ") || undefined
                              }
                              slotProps={{
                                primary: {
                                  variant: "body2",
                                  noWrap: true,
                                  sx: { fontWeight: o.assignedOnLine ? 600 : 500 },
                                },
                                secondary: { variant: "caption", noWrap: true },
                              }}
                            />
                          </ListItemButton>
                        ))}
                      </List>
                    )}
                  </AccordionDetails>
                </Accordion>
              );
            })
          )}
        </Box>
      </Popover>
    </Box>
  );
}
