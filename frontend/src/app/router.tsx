import { createBrowserRouter, Navigate } from "react-router-dom";
import { AppShell } from "@/app/layouts/AppShell";
import { ProtectedRoute } from "@/shared/auth/ProtectedRoute";
import { RoleRoute } from "@/shared/auth/RoleRoute";
import { DashboardPage } from "@/features/dashboard/DashboardPage";
import { FlightsPage } from "@/features/flights/FlightsPage";
import { DailyStaffingPage } from "@/features/dailyStaffing/DailyStaffingPage";
import { LoginPage } from "@/features/auth/LoginPage";
import { PeoplePage } from "@/features/people/PeoplePage";
import { PhanCongSlotPage } from "@/features/phanCongSlot/PhanCongSlotPage";
import { MonitoringPage } from "@/features/monitoring/MonitoringPage";
import { ReconcilePage } from "@/features/reconcile/ReconcilePage";
import { MobileShell } from "@/features/mobile/MobileShell";
import { MobilePlanPage } from "@/features/mobile/MobilePlanPage";
import { MobileAssignPage } from "@/features/mobile/MobileAssignPage";
import { MobileFlightsPage } from "@/features/mobile/MobileFlightsPage";
import { MobileTodayPage } from "@/features/mobile/MobileTodayPage";
import { MobileSchedulePage } from "@/features/mobile/MobileSchedulePage";
import { MobileInboxPage } from "@/features/mobile/MobileInboxPage";
import { MobileTeamPage } from "@/features/mobile/MobileTeamPage";
import { MobileRequestsPage } from "@/features/mobile/MobileRequestsPage";
import { MobileProfilePage } from "@/features/mobile/MobileProfilePage";
import { MobileIndexRedirect } from "@/features/mobile/MobileIndexRedirect";
import { ProfilePage } from "@/features/account/ProfilePage";
import { SettingsPage } from "@/features/account/SettingsPage";
import { ConfigHubPage } from "@/features/config/ConfigHubPage";
import { StaffAttendancePage } from "@/features/attendance/StaffAttendancePage";
import { localizedChildRoutes } from "@/shared/routing/localizedRoutes";
import {
  LEGACY_ROUTE_REDIRECTS,
  LegacyRouteRedirect,
} from "@/shared/routing/legacyRouteRedirects";
import { AuditPage } from "@/features/audit/AuditPage";

function ConfigPage() {
  return <ConfigHubPage />;
}

const webChildren = [
  ...localizedChildRoutes("dashboard", <DashboardPage />),
  ...localizedChildRoutes("flights", <FlightsPage />),
  ...localizedChildRoutes("dailyStaffing", <DailyStaffingPage />),
  ...LEGACY_ROUTE_REDIRECTS.map(({ path, routeKey }) => ({
    path,
    element: <LegacyRouteRedirect routeKey={routeKey} />,
  })),
  ...localizedChildRoutes("phanCongSlot", <PhanCongSlotPage />),
  ...localizedChildRoutes("monitoring", <MonitoringPage />),
  ...localizedChildRoutes("reconcile", <ReconcilePage />),
  ...localizedChildRoutes("people", <PeoplePage />),
  { path: "attendance", element: <StaffAttendancePage /> },
  ...localizedChildRoutes("audit", <AuditPage />),
  ...localizedChildRoutes("config", <ConfigPage />),
  ...localizedChildRoutes("profile", <ProfilePage />),
  ...localizedChildRoutes("settings", <SettingsPage />),
];

export const router = createBrowserRouter([
  {
    path: "/login",
    element: <LoginPage />,
  },
  {
    path: "/mobile",
    element: (
      <ProtectedRoute>
        <RoleRoute>
          <MobileShell />
        </RoleRoute>
      </ProtectedRoute>
    ),
    children: [
      { index: true, element: <MobileIndexRedirect /> },
      { path: "plan", element: <MobilePlanPage /> },
      { path: "assign", element: <MobileAssignPage /> },
      { path: "flights", element: <MobileFlightsPage /> },
      { path: "today", element: <MobileTodayPage /> },
      { path: "schedule", element: <MobileSchedulePage /> },
      { path: "inbox", element: <MobileInboxPage /> },
      { path: "team", element: <MobileTeamPage /> },
      { path: "requests", element: <MobileRequestsPage /> },
      { path: "profile", element: <MobileProfilePage /> },
      { path: "settings", element: <SettingsPage /> },
    ],
  },
  {
    path: "/",
    element: (
      <ProtectedRoute>
        <RoleRoute>
          <AppShell />
        </RoleRoute>
      </ProtectedRoute>
    ),
    children: webChildren,
  },
  { path: "*", element: <Navigate to="/" replace /> },
]);
