# KDS Client

Kitchen Display System — Angular 17 frontend for the .NET backend.

## Quick start

```bash
cd kds-client
npm install
npm start
```

Open http://localhost:4200

## Backend

The app calls the .NET API at `http://localhost:5050/api` (configured in
`src/environments/environment.ts`). Make sure the backend is running.

## Project structure

```
kds-client/
├── src/
│   ├── environments/
│   │   └── environment.ts          # API + hub URLs
│   ├── index.html
│   ├── main.ts                     # standalone bootstrap
│   ├── styles.scss                 # global resets
│   └── app/
│       ├── app.component.ts        # root (just <router-outlet>)
│       ├── app.config.ts           # providers (router, http, interceptor)
│       ├── app.routes.ts           # routes (lazy standalone components)
│       ├── core/
│       │   ├── models.ts           # LoginRequest, LoginResponse, UserInfo, ROLES
│       │   ├── auth.service.ts     # login, session, logout
│       │   ├── auth.guard.ts       # authGuard (functional)
│       │   └── auth.interceptor.ts # JWT attach + 401 handling (functional)
│       └── pages/
│           ├── login/              # login page (.ts/.html/.scss)
│           └── home/               # role-based home (.ts/.html/.scss)
├── angular.json
├── package.json
├── tsconfig.json
└── tsconfig.app.json
```

## Notes

- **Standalone components** — no NgModules. Each component is self-contained.
- **Functional guards & interceptors** — modern Angular 17 patterns.
- **Role-based home** — after login every user lands on `/home`. The
  `HomeComponent` shows a different view for Admin / Cashier / Cook.
