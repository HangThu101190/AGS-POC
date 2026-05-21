# Mobile — Cấu trúc chi tiết

> v1: **PWA / responsive** trong `frontend/` **hoặc** app riêng tại đây  
> Cùng API + SignalR với Web

## Option A — Route group trong `frontend/` (khuyến nghị v1)

```
frontend/src/
└── features/mobile/
    ├── shell/
    │   ├── MobileShell.tsx
    │   ├── BottomNav.tsx
    │   └── RoleBanner.tsx
    ├── today/
    │   ├── TodayPage.tsx
    │   └── CheckinFlow/           # geofence UX
    │       ├── CheckinFlow.tsx
    │       ├── phases/
    │       └── useGeolocation.ts
    ├── schedule/
    ├── inbox/
    ├── team/                      # Sup only
    ├── flights/                   # Sup: sync ca
    │   ├── FlightsPage.tsx
    │   └── SyncProposalSheet.tsx
    └── profile/
```

Router: `/m/*` hoặc layout `isMobile` breakpoint.

## Option B — App riêng `production/mobile/` (sau này)

```
production/mobile/
├── package.json                   # Vite PWA hoặc Expo/RN
├── src/
│   ├── app/
│   ├── shared/                    # copy hoặc monorepo package @ags/api-client
│   └── features/
│       ├── today/
│       ├── schedule/
│       ├── inbox/
│       └── flights/
└── capacitor.config.ts            # nếu wrap native
```

## Shared với Web

| Module | Mobile dùng chung |
|--------|-------------------|
| `shared/api/generated` | REST types |
| `shared/i18n` | vi/en |
| `shared/auth` | JWT refresh |
| `shared/signalr/staffHub` | ShiftChanged |

## Không port sang production

- `import.meta.env.DEV` geofence simulate buttons  
- `window.storage` — thay bằng API state  

## Tham chiếu prototype

`source-code/combined/AGS_Demo.jsx` → `MobileApp`, `CheckinFlow`, `TodayTab`, `FlightsTab`
