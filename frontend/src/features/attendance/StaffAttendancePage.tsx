import { useEffect, useState } from "react";
import { useTranslation } from "react-i18next";
import { useAuth } from "@/shared/auth/AuthContext";
import { fetchMyAttendance } from "@/shared/api/attendanceApi";
import { CheckinFlow } from "@/features/attendance/CheckinFlow";
import { CheckoutFlow } from "@/features/attendance/CheckoutFlow";

/** Web check-in/out for Staff (browser). */
export function StaffAttendancePage() {
  const { t } = useTranslation();
  const { user } = useAuth();
  const [checkedIn, setCheckedIn] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (!user) return;
    let cancelled = false;
    const load = async () => {
      try {
        const att = await fetchMyAttendance(user.id);
        if (!cancelled) setCheckedIn(att.isActive);
      } catch {
        if (!cancelled) setCheckedIn(false);
      } finally {
        if (!cancelled) setLoading(false);
      }
    };
    void load();
    return () => {
      cancelled = true;
    };
  }, [user]);

  if (user?.role !== "staff") {
    return <p style={{ color: "#64748b" }}>{t("attendance.staffOnly")}</p>;
  }

  return (
    <div style={{ maxWidth: 480 }}>
      <h1 style={{ fontSize: 22, fontWeight: 700, margin: "0 0 8px" }}>{t("attendance.title")}</h1>
      <p style={{ fontSize: 13, color: "#64748b", marginBottom: 16 }}>{t("attendance.subtitle")}</p>
      {loading ? (
        <p>{t("common.loading")}</p>
      ) : checkedIn ? (
        <CheckoutFlow onCheckedOut={() => setCheckedIn(false)} />
      ) : (
        <CheckinFlow onCheckedIn={() => setCheckedIn(true)} />
      )}
    </div>
  );
}
