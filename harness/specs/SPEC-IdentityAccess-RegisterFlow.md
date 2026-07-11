# SPEC-IdentityAccess-RegisterFlow

## Spec ID
`SPEC-IdentityAccess-RegisterFlow`

## Bounded Context
**IdentityAccess** — authentication, role claims, policy enforcement.

## Problem
Users need to register and create a profile. The system uses **Keycloak** as the identity provider. Registration flow:

1. **Signup** — frontend sends email + password to `POST /auth/signup`. Backend calls Keycloak Admin API server-side to create the user with `RegisteredUser` realm role and `VERIFY_EMAIL` required action. Keycloak sends a verification email. Passwords never reach the browser bundle.
2. **Email verification** — user clicks link in verification email. Keycloak marks email as verified.
3. **Login** — user authenticates via Keycloak (direct grant or redirect), receiving a JWT with `RegisteredUser` role claim.
4. **Registration** — frontend calls `POST /auth/register` (authorized, RegisteredUser) to create a local User record linked to the Keycloak identity. Publishes `UserRegisteredEvent`.
5. **Onboarding** — frontend calls `GET /auth/me` to check onboarding status. If username is null, shows onboarding form. User submits username + country via `POST /auth/onboarding`.

Registration via Keycloakify theme is also supported — users created via Keycloak's registration form get `RegisteredUser` via realm `defaultRoles`. They follow steps 4–5 after login.

## Domain Invariants
- Username is unique across all users.
- CountryId must reference an existing country.
- A user must exist (have a KeycloakSub) before onboarding.

## Endpoints

| Endpoint | Auth | Method | Purpose |
|----------|------|--------|---------|
| `POST /auth/signup` | AllowAnonymous | Creates Keycloak user with RegisteredUser role + email verification | Server-side Keycloak Admin API call |
| `POST /auth/register` | `[Authorize]` (RegisteredUser) | Creates local User from JWT claims (sub, email) | Publishes `UserRegisteredEvent` |
| `POST /auth/onboarding` | `[Authorize]` (RegisteredUser) | Adds username + countryId to existing User | Updates domain profile |
| `GET /auth/me` | `[Authorize]` (RegisteredUser) | Returns current user's BusStop profile (or 404) | Checks registration/onboarding status |
| `GET /countries` | AllowAnonymous | Lists countries for onboarding dropdown | Reference data |

## Event Impact
- **Published:** `UserRegisteredEvent` (domain event, Core) → `UserRegisteredIntegrationHandler` (Infrastructure) → RabbitMQ via MassTransit
- **Consumer:** External (deferred — not implemented in this spec)

## Acceptance Criteria
1. Authenticated user can `POST /auth/register` → 201, User record created with KeycloakSub + email from JWT
2. `UserRegisteredEvent` is published to RabbitMQ exchange
3. Authenticated user can `POST /auth/onboarding` with username + countryId → 200, profile updated
4. Duplicate username returns 400
5. Invalid countryId returns 400
6. `GET /countries` returns seeded countries ordered by name
7. Registration before onboarding → user exists but has null username/country

## Layer Changes

### Core
- `User.cs` — remove `PasswordHash`, `IsEmailVerified`; add `CreateFromKeycloak()` factory; add `CompleteOnboarding()` method; make `Username` nullable
- `UserRegisteredEvent` — update to carry only `email` (UserId unavailable pre-save)
- Remove `UserByEmailSpec.cs`, `UserByUsernameSpec.cs` (uniqueness handled by Keycloak)
- `Interfaces/IKeycloakAdminService.cs` — new: server-side Keycloak user creation with role assignment

### UseCases
- `Users/Signup/SignupCommand.cs` — new: AllowAnonymous, delegates to IKeycloakAdminService
- `Users/Signup/SignupHandler.cs` — new: calls IKeycloakAdminService.CreateUserAsync
- `Users/Register/RegisterUserCommand.cs` — implement `IRequireAuthenticatedUser`; remove password
- `Users/Register/RegisterUserHandler.cs` — create user from JWT Sub; remove BCrypt; remove uniqueness checks
- `Users/Register/OnboardingCommand.cs` — new: `IRequireAuthenticatedUser` + username + countryId
- `Users/Register/OnboardingHandler.cs` — new: finds user by Sub, updates profile
- `Users/GetMe/GetMeQuery.cs` — new: returns current user by Sub claim
- `Users/GetMe/GetMeHandler.cs` — new: lookup by GetUserByExternalIdAsync extension
- `Users/Create/` — remove (replaced by register flow)
- `UserResponse.cs` — remove PasswordHash, IsEmailVerified

### Infrastructure
- `UserConfiguration.cs` — remove PasswordHash, IsEmailVerified columns
- Keep MassTransit/RabbitMQ config unchanged
- `Integrations/Keycloak/KeycloakAdminService.cs` — new: implements IKeycloakAdminService via Keycloak Admin REST API

### Web
- `Auth/Signup.cs` — new: AllowAnonymous, delegates to SignupCommand via Mediator
- `Auth/Register.cs` — change to `[Authorize]`, remove password from request/validator
- `Auth/Onboarding.cs` — new endpoint
- `Auth/Me.cs` — new: Authorize(RegisteredUser), delegates to GetMeQuery
- `Users/Create.cs` — remove (replaced by `/auth/register`)
- `MediatorConfig.cs` — update assembly reference from `CreateUserCommand` to `RegisterUserCommand`
- `appsettings.json` — add `Keycloak:Admin` section for server-side admin credentials

### Packages
- Remove `BCrypt.Net-Next` from `Directory.Packages.props` and `BusStop.UseCases.csproj`

## Test Strategy
- Integration: EF config, User factory, Registration handler, Onboarding handler, Seed data

## Rollout Notes
- Requires database migration to drop `PasswordHash`, `IsEmailVerified` columns
- Existing users with `KeycloakSub` continue to work (username/country optional)
