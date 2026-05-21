import TextField from "@mui/material/TextField";
import { useTranslation } from "react-i18next";
import { ButtonPrimary } from "@/components/ui";
import styles from "@/features/dailyStaffing/staffingDayExcelBoard.module.css";
import type { StaffingBioHeader } from "@/shared/api/staffingApi";

type Props = {
  bio?: StaffingBioHeader;
  radioNote: string;
  onRadioChange: (value: string) => void;
  canEdit: boolean;
  onSave: () => void;
  saving: boolean;
};

export function StaffingDayBioTable({
  bio,
  radioNote,
  onRadioChange,
  canEdit,
  onSave,
  saving,
}: Props) {
  const { t } = useTranslation();

  return (
    <table className={styles.bioTable}>
      <tbody>
        <tr>
          <td className={styles.bioLabel} rowSpan={2}>
            {t("dailyStaffing.bioResponsible")}
          </td>
          <td className={styles.bioRole}>HC-S</td>
          <td colSpan={2}>{t("dailyStaffing.bioMorningShift")}</td>
          <td>{bio?.morningSupName || "—"}</td>
          <td className={styles.bioLabel}>{t("dailyStaffing.bioRadioCheckStart")}</td>
          <td colSpan={2}>{t("dailyStaffing.bioSegmentLeads")}</td>
        </tr>
        <tr>
          <td className={styles.bioRole}>C-Đ</td>
          <td colSpan={2}>{t("dailyStaffing.bioEveningShift")}</td>
          <td>{bio?.eveningSupName || "—"}</td>
          <td className={styles.bioLabel}>{t("dailyStaffing.bioRadioCheckEnd")}</td>
          <td colSpan={2} />
        </tr>
        <tr>
          <td colSpan={2} />
          <td className={styles.bioLabel}>{t("dailyStaffing.bioPrepSu")}</td>
          <td colSpan={2} />
          <td className={styles.bioInput}>
            <TextField
              size="small"
              fullWidth
              placeholder={t("dailyStaffing.bioRadio")}
              value={radioNote || bio?.radioNote || ""}
              onChange={(e) => onRadioChange(e.target.value)}
              disabled={!canEdit}
              variant="standard"
              slotProps={{ input: { sx: { fontSize: 11 } } }}
            />
          </td>
          <td>
            {canEdit ? (
              <ButtonPrimary size="small" onClick={onSave} disabled={saving}>
                {t("common.save")}
              </ButtonPrimary>
            ) : null}
          </td>
        </tr>
      </tbody>
    </table>
  );
}
