# SPEC-IdentityAccess-LoginOnboardingFrontend

## Spec ID
`SPEC-IdentityAccess-LoginOnboardingFrontend`

## Bounded Context
**IdentityAccess** — authentication, role claims, policy enforcement.

## Problem
The backend registration flow (`SPEC-IdentityAccess-RegisterFlow`) is implemented, but the frontend is incomplete and insecure:

1. The register page calls the Keycloak Admin API **directly from the browser** using admin credentials shipped in `VITE_KEYCLOAK_ADMIN_*` env vars, and collects firstName/lastName/username that the backend discards.
2. There is no API client for the BusStop backend; only the axios Bearer-token interceptor exists.
3. There is no email-verification gate after signup (Keycloak sets `VERIFY_EMAIL` required action; `email_verified` claim is available in tokens).
4. There is no onboarding UI: after login, users with a null username must complete username + country via a blocking, centered modal over the main page.

This spec delivers the frontend login, refactored register, email-verification gate, and blocking onboarding modal, consuming the existing backend endpoints.

## Domain Invariants
- Passwords and Keycloak admin credentials never reach the browser bundle. Signup goes through `POST /auth/signup`.
- A user must authenticate via Keycloak before any local profile operation.
- A user must have a verified email (`email_verified` token claim) before entering the app.
- Onboarding is mandatory exactly once: a profile with null `username` cannot use the app until onboarding completes.
- Username: 3–50 characters (mirrors backend `OnboardingValidator`); countryId must reference an existing country (selected from `GET /countries`, never free input).

## Use-Case Slice Path
Frontend-only feature slice: `src/BusStop.Frontend/src/features/auth/` plus route pages and auth adapter refactor. Consumes existing endpoints; no new backend endpoints.

## Layer File Checklist

### Frontend — auth adapter (`src/lib/adapters/auth/`)
- `types.ts` — add `emailVerified: boolean` to `UserProfile`; remove `DirectRegisterRequest`.
- `IAuthAdapter.ts` — remove `directRegister`; add `discardSession()`.
- `KeycloakAdapter.ts` — map `email_verified` claim; remove `directRegister` (browser admin flow); add `discardSession()`.
- `keycloak-http-client.ts` — remove `createUser`, `usersUrl`, `parseUserCreationError`.
- `useAuth.ts`, `index.ts` — updated exports/surface accordingly.

### Frontend — API client (`src/lib/api/`, new)
- `types.ts` — `BusStopUser`, `Country` response types mirroring backend contracts.
- `auth-api.ts` — `signup`, `registerUser`, `getMe` (404 → `null`), `completeOnboarding`; FastEndpoints error-shape parsing helper.
- `countries-api.ts` — `listCountries`.

### Frontend — routes & shared components
- `routes/register.tsx` — react-hook-form + zod; email/password/confirm only; calls `POST /auth/signup`; success → `VerifyEmailNotice`.
- `routes/login.tsx` — react-hook-form + zod; email + password; unverified-email handling → `VerifyEmailNotice`; 401 → inline error.
- `components/auth/verify-email-notice.tsx` — new shared notice component.
- `features/auth/schemas/auth-schemas.ts` — zod schemas (login, register, onboarding).
- `features/auth/hooks/use-countries.ts` — react-query countries query.
- `features/auth/hooks/use-my-profile.ts` — `['me']` query; on 404 → `registerUser()` once → refetch.
- `features/auth/components/onboarding-gate.tsx` — blocking, non-dismissible Dialog (z-100 per design system, blurred/dimmed overlay) rendered when profile username is null.
- `features/auth/components/onboarding-form.tsx` — RHF + zod form: username + country Select; inline field errors; duplicate-username server error mapped to username field.
- `features/map/map-page.tsx` — mount `<OnboardingGate />`.
- `main.tsx` — set `axios.defaults.baseURL` from `VITE_API_URL`.
- `.env.example` — remove `VITE_KEYCLOAK_ADMIN_*`; add `VITE_API_URL`.

### Backend
- `src/BusStop.Web/appsettings.Development.json` — add `Cors:AllowedOrigins: ["http://localhost:5173"]` for the Vite dev server. No C# changes.

### Tests
- `features/auth/schemas/auth-schemas.test.ts` — schema validation unit tests.
- `lib/api/auth-api.test.ts` — `getMe` 404→null mapping and error-shape parsing (mocked axios).

## Endpoint Impact (consumption only)

| Endpoint | Usage |
|----------|-------|
| `POST /auth/signup` | Register page (anonymous) |
| `POST /auth/register` | `use-my-profile` when `GET /auth/me` returns 404 |
| `GET /auth/me` | Onboarding gate: null username → open modal |
| `POST /auth/onboarding` | Onboarding form submit |
| `GET /countries` | Onboarding country Select |
| Keycloak token endpoint (direct grant) | Login page; `email_verified` claim inspected post-login |

## Event Impact
None new. `UserRegisteredEvent` publication remains owned by backend `POST /auth/register`.

## Acceptance Criteria
1. Given no account, when a user submits valid email + password + matching confirmation on `/register`, then `POST /auth/signup` is called and a verify-email notice is shown.
2. Given a duplicate email, when signup is submitted, then an inline error "A user with this email already exists." is shown.
3. Given invalid email format, password < 8 chars, or mismatched confirmation, when the register form is submitted, then per-field validation messages are shown and no request is sent.
4. Given a registered user with verified email, when they sign in on `/login`, then they land on the main page.
5. Given a registered user with unverified email, when they sign in, then the verify-email notice is shown and they are not authenticated in the app.
6. Given wrong credentials, when sign-in is submitted, then "Invalid username or password." is shown inline.
7. Given an authenticated user with no local profile, when the main page loads, then `POST /auth/register` runs once and the onboarding modal opens.
8. Given an authenticated user whose profile username is null, when the main page loads, then a centered, non-dismissible onboarding modal appears over a dimmed/blurred background; it cannot be closed by click-outside, Escape, or a close button.
9. Given the onboarding modal, when username is empty/< 3/> 50 chars or no country is selected, then per-field errors are shown and submit is blocked.
10. Given a duplicate username, when onboarding is submitted, then the error is surfaced on the username field and the modal stays open.
11. Given valid username + country, when onboarding is submitted, then the modal closes, the profile cache is updated, and the app is usable.
12. Given an authenticated user with a completed profile, when the main page loads, then no modal appears.

## Rollout and Rollback
- Requires `VITE_API_URL` env var (dev default `https://localhost:57679`) and backend dev CORS origin `http://localhost:5173`.
- Local dev requires trusting the ASP.NET dev certificate (`dotnet dev-certs https --trust`) for axios calls to the https API.
- **Risk:** `nginx/nginx.conf` routes `/auth` to Keycloak while the API also owns `/auth/*` — docker deployments would misroute these endpoints. Deferred: introduce an `/api` prefix or fix nginx paths in a follow-up spec.
- **Deferred:** resend-verification-email action (requires backend endpoint); auth-page i18n keys (pages currently hardcode English); backend username-uniqueness enforcement (existing TODO in `OnboardingHandler`).
- Rollback: revert frontend changes; backend config change is additive and harmless.
