# Frontend Web — Cấu trúc chi tiết

> React 19 · TypeScript · Vite · TanStack Query · SignalR · i18next  
> Phụ thuộc: OpenAPI client từ `../backend/openapi/v1.yaml`

```
production/frontend/
├── package.json
├── vite.config.ts
├── tsconfig.json · tsconfig.app.json
├── index.html
├── .env.example                    # VITE_API_URL, VITE_HUB_URL
│
├── public/
│   └── AGS_Logo.png
│
└── src/
    ├── main.tsx
    ├── app/
    │   ├── App.tsx
    │   ├── router.tsx              # react-router: /flights, /bang-phan-ca, …
    │   ├── providers/
    │   │   ├── QueryProvider.tsx
    │   │   ├── AuthProvider.tsx
    │   │   ├── SignalRProvider.tsx
    │   │   └── I18nProvider.tsx
    │   └── layouts/
    │       ├── AppShell.tsx        # Sidebar + topbar (HR/Sup)
    │       └── AuthLayout.tsx
    │
    ├── shared/
    │   ├── api/
    │   │   ├── client.ts           # axios/fetch + interceptors
    │   │   ├── generated/          # openapi-typescript output
    │   │   └── hooks/              # thin wrappers
    │   ├── auth/
    │   │   ├── tokenStorage.ts
    │   │   └── useAuth.ts
    │   ├── i18n/
    │   │   ├── index.ts
    │   │   └── locales/
    │   │       ├── vi.json
    │   │       └── en.json
    │   ├── signalr/
    │   │   ├── monitoringHub.ts
    │   │   ├── staffHub.ts
    │   │   ├── planningHub.ts
    │   │   └── usePlanningDaySync.ts
    │   ├── ui/                     # Button, Chip, Modal, Table primitives
    │   ├── webChrome/              # PageShell, ActionButton, StatusBanner, MetricCard, WorkflowStages
    │   ├── map/
    │   │   ├── LeafletMap.tsx
    │   │   └── polygonLayer.ts
    │   ├── utils/
    │   │   ├── dateLocale.ts       # vi-VN / en-US BCP 47
    │   │   └── formatShift.ts
    │   └── types/                  # re-export generated + UI-only types
    │
    └── features/
        ├── dashboard/
        │   ├── DashboardPage.tsx
        │   └── components/
        ├── flights/
        │   ├── FlightsPage.tsx
        │   ├── FlightImportDialog.tsx
        │   └── api/useFlights.ts
        ├── dailyStaffing/       # Daily staffing board (PVHK) — /bang-phan-ca (vi), /shift-board (en)
        │   ├── DailyStaffingPage.tsx
        │   ├── DailyStaffingWeekBoard.tsx
        │   ├── StaffingAssignDialog.tsx
        │   └── staffingCalendar.ts
        ├── phanCongSlot/        # Phân công slot (Sup) — /phan-cong-slot
        │   ├── PhanCongSlotPage.tsx
        │   └── FlightAssignDropdown.tsx
        ├── monitoring/
        │   ├── MonitoringPage.tsx
        │   ├── StatsBar.tsx
        │   ├── EmployeeTreeGrid.tsx  # AG Grid tree
        │   ├── MapPanel.tsx
        │   └── api/useMonitoringSnapshot.ts
        ├── reconcile/
        │   ├── ReconcilePage.tsx
        │   └── ReconcileWeekPreviewModal.tsx
        ├── audit/
        ├── people/
        ├── config/
        └── auth/
            ├── LoginPage.tsx
            └── api/useLogin.ts
```

**Quy tắc**: feature chỉ import từ `shared/` hoặc barrel `features/x/index.ts` — không import chéo nội bộ feature khác.
