import { useTranslation } from "react-i18next";
import { StaffingCardMenu } from "@/features/dailyStaffing/StaffingCardMenu";
import { flightEtaEtdDisplayTime } from "@/shared/planning/flightClock";
import type { StaffingDay } from "@/shared/api/staffingApi";
import { flightNoLabel } from "@/features/dailyStaffing/staffingHeadcount";

type Props = {
  flights: StaffingDay["flights"];
  selectedFlightIds: string[];
  disabled?: boolean;
  onToggle: (flightId: string) => void;
};

export function StaffingFlightAssignDropdown({
  flights,
  selectedFlightIds,
  disabled,
  onToggle,
}: Props) {
  const { t } = useTranslation();

  return (
    <StaffingCardMenu
      wrapClassName="staffing-slot-card__flt-assign-wrap"
      triggerClassName="staffing-slot-card__flt-assign-btn staffing-slot-card__flt-assign-btn--icon"
      triggerLabel={
        <span className="staffing-slot-card__flt-assign-icon" aria-hidden>
          ✈
        </span>
      }
      title={t("phanCongSlot.assignFlt")}
      ariaLabel={t("phanCongSlot.assignFlt")}
      disabled={disabled}
    >
      {flights.length === 0 ? (
        <p className="staffing-slot-card__menu-empty">{t("phanCongSlot.noFlightsOverlap")}</p>
      ) : (
        flights.map((f) => {
          const selected = selectedFlightIds.includes(f.id);
          const time = flightEtaEtdDisplayTime(f, "etd") || f.std;
          return (
            <button
              key={f.id}
              type="button"
              role="option"
              aria-selected={selected}
              className={`staffing-slot-card__menu-item staffing-slot-card__menu-item--flight ${selected ? "staffing-slot-card__menu-item--on" : ""}`}
              onClick={() => onToggle(f.id)}
            >
              <span className="staffing-slot-card__menu-item-flt">{flightNoLabel(f)}</span>
              <span className="staffing-slot-card__menu-item-time">{time}</span>
              {selected ? (
                <span className="staffing-slot-card__menu-item-check" aria-hidden>
                  ✓
                </span>
              ) : null}
            </button>
          );
        })
      )}
    </StaffingCardMenu>
  );
}
