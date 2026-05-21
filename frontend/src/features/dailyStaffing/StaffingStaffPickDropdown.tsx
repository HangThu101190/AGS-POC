import { useTranslation } from "react-i18next";
import { StaffingCardMenu } from "@/features/dailyStaffing/StaffingCardMenu";

export type StaffPickOption = {
  value: string;
  label: string;
  disabled: boolean;
  /** Already on this staffing line — show checked row (not pickable). */
  assignedOnLine?: boolean;
};

type Props = {
  badgeText: string;
  badgeClassName: string;
  options: StaffPickOption[];
  disabled?: boolean;
  onPick: (employeeId: string) => void;
};

export function StaffingStaffPickDropdown({
  badgeText,
  badgeClassName,
  options,
  disabled,
  onPick,
}: Props) {
  const { t } = useTranslation();

  return (
    <StaffingCardMenu
      wrapClassName="staffing-slot-card__nv-wrap"
      triggerClassName={`staffing-slot-card__nv-btn staffing-slot-card__badge ${badgeClassName}`}
      triggerLabel={
        <>
          <span className="staffing-slot-card__nv-btn-label">{badgeText}</span>
          <span className="staffing-slot-card__nv-btn-caret" aria-hidden>
            ▾
          </span>
        </>
      }
      ariaLabel={t("staffing.hourSlotAssignStaff")}
      disabled={disabled}
    >
      {options.length === 0 ? (
        <p className="staffing-slot-card__menu-empty">{t("phanCongSlot.pickPlaceholder")}</p>
      ) : (
        options.map((o) => (
          <button
            key={o.value}
            type="button"
            role="option"
            aria-selected={o.assignedOnLine}
            disabled={o.disabled}
            className={`staffing-slot-card__menu-item ${o.assignedOnLine ? "staffing-slot-card__menu-item--on" : ""}`}
            onClick={() => {
              if (!o.disabled) onPick(o.value);
            }}
          >
            <span className="staffing-slot-card__menu-item-label">{o.label}</span>
            {o.assignedOnLine ? (
              <span className="staffing-slot-card__menu-item-check" aria-hidden>
                ✓
              </span>
            ) : null}
          </button>
        ))
      )}
    </StaffingCardMenu>
  );
}
