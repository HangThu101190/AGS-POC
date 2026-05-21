import {
  forwardRef,
  useEffect,
  useImperativeHandle,
  useRef,
  useState,
  type ReactNode,
} from "react";
import type { ICellRendererParams } from "ag-grid-community";
import { useTranslation } from "react-i18next";
import type { FlightDto } from "@/shared/api/flightsApi";
import {
  flightEtaEtdDisplayTime,
  resolveFlightDelayMinutes,
} from "@/shared/planning/flightClock";
import { isPastDay } from "@/shared/planning/weekCalendar";
import styles from "./flightGrid.module.css";

function CellFrame({ children }: { children?: ReactNode }) {
  return <div className={styles.cellFrame}>{children}</div>;
}

export function FlightRemarkCell(props: ICellRendererParams<FlightDto>) {
  const remark = props.data?.remark?.trim();
  if (!remark) return <CellFrame />;
  return (
    <CellFrame>
      <span className={styles.remarkText} title={remark}>
        {remark}
      </span>
    </CellFrame>
  );
}

type FlightEtaEtdCellProps = ICellRendererParams<FlightDto> & {
  kind: "eta" | "etd";
  todayIdx: number;
  canEdit: boolean;
  onSave: (
    flightId: string,
    delays: { etaDelayMinutes: number; etdDelayMinutes: number },
  ) => Promise<FlightDto>;
};

/** View: STA/STD + delay → one HH:mm. Click → edit delay minutes; Enter/blur → save. */
export const FlightEtaEtdCell = forwardRef<unknown, FlightEtaEtdCellProps>(
  function FlightEtaEtdCell(props, ref) {
    const { t } = useTranslation();
    const f = props.data;
    const [editing, setEditing] = useState(false);
    const [value, setValue] = useState("");
    const [saving, setSaving] = useState(false);
    const inputRef = useRef<HTMLInputElement>(null);

    useImperativeHandle(ref, () => ({
      refresh: () => true,
    }));

    const delayMinutes = () =>
      f ? resolveFlightDelayMinutes(f, props.kind) : 0;

    const scheduledTime = () =>
      (props.kind === "eta" ? f?.sta : f?.std)?.trim() ?? "";

    const viewTime = () => (f ? flightEtaEtdDisplayTime(f, props.kind) : "");

    useEffect(() => {
      if (!f) return;
      const m = delayMinutes();
      setValue(m > 0 ? String(m) : "");
    }, [
      f?.id,
      f?.eta,
      f?.etd,
      f?.etaDelayMinutes,
      f?.etdDelayMinutes,
      f?.delayMinutes,
      f?.isDelayed,
      props.kind,
    ]);

    useEffect(() => {
      if (editing) {
        inputRef.current?.focus();
        inputRef.current?.select();
      }
    }, [editing]);

    if (!f) return null;

    const isPast = isPastDay(f.dayIdx, props.todayIdx);
    const delayed = delayMinutes() > 0;
    const time = viewTime();
    const sched = scheduledTime();
    const label =
      props.kind === "eta" ? t("flights.delayEtaMin") : t("flights.delayEtdMin");
    const editHint = sched
      ? t("flights.delayMinutesFromScheduled", { scheduled: sched })
      : label;

    const exitEdit = (revert: boolean) => {
      if (revert) {
        const m = delayMinutes();
        setValue(m > 0 ? String(m) : "");
      }
      setEditing(false);
    };

    const commit = async (): Promise<boolean> => {
      const minutes = Math.max(0, Math.min(600, Number.parseInt(value, 10) || 0));
      const eta =
        props.kind === "eta" ? minutes : resolveFlightDelayMinutes(f, "eta");
      const etd =
        props.kind === "etd" ? minutes : resolveFlightDelayMinutes(f, "etd");
      const unchanged =
        eta === resolveFlightDelayMinutes(f, "eta") &&
        etd === resolveFlightDelayMinutes(f, "etd");
      if (unchanged) return true;
      setSaving(true);
      try {
        await props.onSave(f.id, { etaDelayMinutes: eta, etdDelayMinutes: etd });
        return true;
      } finally {
        setSaving(false);
      }
    };

    if (isPast || !props.canEdit) {
      return (
        <CellFrame>
          {time ? (
            <span
              className={[styles.timePlain, delayed ? styles.timeDelayed : ""]
                .filter(Boolean)
                .join(" ")}
              title={time}
            >
              {time}
            </span>
          ) : null}
        </CellFrame>
      );
    }

    if (editing) {
      return (
        <CellFrame>
          <div className={styles.etaEtdEdit}>
            <input
              ref={inputRef}
              type="number"
              min={0}
              max={600}
              className={[styles.delayInput, saving ? styles.delaySaving : ""]
                .filter(Boolean)
                .join(" ")}
              value={value}
              placeholder="0"
              disabled={saving}
              aria-label={label}
              title={editHint}
              onChange={(e) => setValue(e.target.value)}
              onBlur={() => {
                void (async () => {
                  await commit();
                  exitEdit(false);
                })();
              }}
              onKeyDown={(e) => {
                if (e.key === "Escape") {
                  e.preventDefault();
                  e.stopPropagation();
                  exitEdit(true);
                  return;
                }
                if (e.key !== "Enter") return;
                e.preventDefault();
                e.stopPropagation();
                void (async () => {
                  await commit();
                  exitEdit(false);
                })();
              }}
            />
            <span className={styles.delayUnit} aria-hidden>
              &apos;
            </span>
          </div>
        </CellFrame>
      );
    }

    const viewTitle = delayed
      ? t("flights.etaEtdViewDelayed", {
          scheduled: sched,
          minutes: delayMinutes(),
          time,
        })
      : t("flights.etaEtdClickEdit", { scheduled: sched });

    return (
      <CellFrame>
        <button
          type="button"
          className={[
            styles.timeBtn,
            delayed ? styles.timeBtnDelayed : "",
            !time ? styles.timeBtnEmpty : "",
          ]
            .filter(Boolean)
            .join(" ")}
          title={viewTitle}
          onClick={() => setEditing(true)}
        >
          {time ? (
            <span
              className={[styles.timePlain, delayed ? styles.timeDelayed : ""]
                .filter(Boolean)
                .join(" ")}
            >
              {time}
            </span>
          ) : (
            <span className={styles.timeMuted}>—</span>
          )}
        </button>
      </CellFrame>
    );
  },
);
