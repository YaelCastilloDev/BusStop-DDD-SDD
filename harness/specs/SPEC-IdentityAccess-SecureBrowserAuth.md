# SPEC-IdentityAccess-SecureBrowserAuth

## Spec ID
`SPEC-IdentityAccess-SecureBrowserAuth`

## Status
Approved by the user request to replace the current browser authentication with an industry-standard Keycloak/Keycloakify implementation while preserving the existing visual design.

## Bounded Context
**IdentityAccess** — authentication, role claims, policy enforcement.

## Problem
The SPA currently collects user passwords, uses the OAuth password grant, and shares the confidential `busstop-api` client with browser authentication and backend service-account operations. This bypasses the deployed Keycloak login flow, prevents the existing Keycloakify source from protecting the credential boundary, and creates both security and session-initialization defects.

Browser authentication must be owned by Keycloak. The SPA initiates OpenID Connect Authorization Code flow with PKCE through `keycloak-js`; Keycloak renders the existing BusStop login and registration design through the deployed Keycloakify theme. The BusStop API only accepts bearer tokens and never receives account passwords.

## Security Invariants
- BusStop SPA code and BusStop API endpoints never receive or process a user's password.
- The browser uses a dedicated public `busstop-web` client with no client secret.
- `busstop-web` enables only Authorization Code flow with PKCE S256; Direct Access Grants, Implicit Flow, and Service Accounts are disabled.
- `busstop-api` is a resource-server/audience client and cannot initiate browser or service-account login.
- Access and refresh tokens remain in `keycloak-js` memory and are never persisted to local storage, session storage, cookies, or application logs.
- Redirect URIs and web origins are explicit per environment.
- Keycloak enforces email verification, password policy, and brute-force protection.
- Non-development deployments require HTTPS.

## Slice and Layer Impact

### Frontend
- `src/lib/adapters/auth/` — remove password-grant support; initialize `keycloak-js` before the router; keep login, registration, refresh, logout, role checks, and profile parsing behind `IAuthAdapter`/`useAuth`.
- `src/routes/login.tsx` — preserve the route but redirect unauthenticated users to Keycloak login.
- `src/routes/register.tsx` — preserve the route but redirect unauthenticated users to Keycloak registration.
- `src/keycloak-theme/login/` — preserve the BusStop visual design while posting credentials only to Keycloak actions and rendering configured identity providers safely.
- Remove SPA-only login/registration validation and `/auth/signup` client code.

### Keycloak Deployment
- Add a dedicated `busstop-web` public client with Standard Flow and PKCE S256.
- Restrict `busstop-api` to resource-server use.
- Build the Keycloakify theme into the Keycloak container and configure the realm to use it.
- Require external TLS and enable brute-force protection and a production-grade password policy.

### Core / UseCases / Infrastructure / Web
- Remove the anonymous `POST /auth/signup` password-provisioning slice and its Keycloak Admin API service.
- Keep authorized local-profile registration (`POST /auth/register`) and onboarding unchanged.
- Keep API JWT issuer, audience, lifetime, and role validation unchanged.

## Endpoint Impact

| Endpoint | Change |
|---|---|
| `POST /auth/signup` | Removed; self-registration is performed by Keycloak |
| `POST /auth/register` | Unchanged; creates the local profile from verified JWT claims |
| `POST /auth/onboarding` | Unchanged |
| `GET /auth/me` | Unchanged |

Removing `/auth/signup` is an intentional breaking change to an insecure, unauthenticated credential-handling endpoint.

## Event Impact
No event contract changes. `UserRegisteredEvent` remains associated with creation of the local BusStop profile after Keycloak authentication.

## Acceptance Criteria
1. Given an unauthenticated browser at `/login`, when authentication begins, then it redirects to the Keycloak authorization endpoint for `busstop-web` using Authorization Code flow and PKCE S256.
2. Given an unauthenticated browser at `/register`, when registration begins, then Keycloak renders the BusStop Keycloakify registration page and receives the submitted credentials directly.
3. Given a completed Keycloak callback, when the SPA starts, then `keycloak-js` initializes before the router and exposes a parsed authenticated user profile.
4. Given an authenticated user calling the API, when the access token is near expiry, then it is refreshed before the bearer request or the local session is cleared on refresh failure.
5. Given the realm configuration, then `busstop-web` is public with Standard Flow + PKCE S256 and has Direct Access Grants, Implicit Flow, and Service Accounts disabled.
6. Given the realm configuration, then `busstop-api` cannot perform Standard Flow, Direct Access Grants, or Service Account grants and is included only as an access-token audience.
7. Given a fresh Keycloak container build, then the Keycloakify theme JAR is installed and the realm selects the `busstop` login theme.
8. Given repository source and API contracts, then no SPA login/register form or anonymous BusStop endpoint accepts a password.
9. Given invalid login, registration, reset-password, or update-password input, then the Keycloakify pages render Keycloak's server-side validation errors without exposing sensitive details.
10. Given deployment outside local development, then Keycloak requires HTTPS; the development realm still supports the documented localhost workflow.

## Test Strategy
- Frontend unit tests cover auth initialization, redirect methods, refresh failure, and token/profile state via the adapter boundary.
- Frontend route tests verify `/login` and `/register` invoke adapter redirects without rendering credential fields.
- Static realm validation verifies client separation, disabled insecure grants, PKCE, API audience mapping, TLS mode, and selected theme.
- Keycloakify build verifies all custom login pages compile into a deployable JAR.
- Existing backend build, unit, integration, and functional suites verify the remaining profile-registration and authorization flow.

## Rollout
- Existing realms are not overwritten automatically by `--import-realm`; apply the updated realm configuration through the deployment pipeline or recreate the local development realm database.
- Configure production `VITE_KEYCLOAK_URL`, issuer, redirect URIs, web origins, and TLS hostname explicitly.
- Existing Keycloak users remain valid; only the OAuth client used by the browser changes.

## Rollback
- Roll back the frontend and realm/client configuration together. Do not re-enable Direct Access Grants or restore the anonymous password-provisioning endpoint as a partial rollback.

## Decision Log
- **Decision:** Use Keycloak-hosted Keycloakify pages with Authorization Code + PKCE. **Rationale:** Keeps credentials outside BusStop application code and uses Keycloak's supported browser flow. **Impact:** Authentication involves a browser redirect. **Revisit trigger:** Adoption of a backend-for-frontend that owns an HTTP-only session cookie.
- **Decision:** Separate SPA and API clients. **Rationale:** Public browser and resource-server clients have different trust boundaries. **Impact:** Access tokens require an explicit `busstop-api` audience mapper. **Revisit trigger:** API audience or gateway topology changes.
- **Decision:** Remove `/auth/signup`. **Rationale:** Keycloak self-registration already provides validation, verification, required actions, and abuse controls without passing passwords through BusStop. **Impact:** Clients must initiate Keycloak registration. **Revisit trigger:** A reviewed invitation/provisioning use case requiring server-side account creation.
