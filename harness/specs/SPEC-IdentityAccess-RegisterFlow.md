# SPEC-IdentityAccess-RegisterFlow

## Spec ID
`SPEC-IdentityAccess-RegisterFlow`

## Bounded Context
**IdentityAccess** — authentication, role claims, policy enforcement.

## Problem
Users need to register and create a profile. The system uses **Keycloak** (via authorization_code + PKCE / Keycloakify) as the identity provider. The frontend registers users directly with Keycloak; our API never sees passwords or manages authentication.

After getting a JWT from Keycloak, the user calls our API in two steps:
1. **Registration** (security/infra) — creates a local User record linked to the Keycloak identity. Publishes `UserRegisteredEvent` via MassTransit/RabbitMQ for downstream email workflows.
2. **Onboarding** (domain) — adds profile data: username, country.

## Domain Invariants
- Username is unique across all users.
- CountryId must reference an existing country.
- A user must exist (have a KeycloakSub) before onboarding.

## Endpoints

| Endpoint | Auth | Method | Purpose |
|----------|------|--------|---------|
| `POST /auth/register` | `[Authorize]` (RegisteredUser) | Creates local User from JWT claims (sub, email) | Publishes `UserRegisteredEvent` |
| `POST /auth/onboarding` | `[Authorize]` (RegisteredUser) | Adds username + countryId to existing User | Updates domain profile |
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

### UseCases
- `Users/Register/RegisterUserCommand.cs` — implement `IRequireAuthenticatedUser`; remove password
- `Users/Register/RegisterUserHandler.cs` — create user from JWT Sub; remove BCrypt; remove uniqueness checks
- `Users/Register/OnboardingCommand.cs` — new: `IRequireAuthenticatedUser` + username + countryId
- `Users/Register/OnboardingHandler.cs` — new: finds user by Sub, updates profile
- `Users/Create/` — remove (replaced by register flow)
- `UserResponse.cs` — remove PasswordHash, IsEmailVerified

### Infrastructure
- `UserConfiguration.cs` — remove PasswordHash, IsEmailVerified columns
- Keep MassTransit/RabbitMQ config unchanged

### Web
- `Auth/Register.cs` — change to `[Authorize]`, remove password from request/validator
- `Auth/Onboarding.cs` — new endpoint
- `Users/Create.cs` — remove (replaced by `/auth/register`)
- `MediatorConfig.cs` — update assembly reference from `CreateUserCommand` to `RegisterUserCommand`

### Packages
- Remove `BCrypt.Net-Next` from `Directory.Packages.props` and `BusStop.UseCases.csproj`

## Test Strategy
- Integration: EF config, User factory, Registration handler, Onboarding handler, Seed data

## Rollout Notes
- Requires database migration to drop `PasswordHash`, `IsEmailVerified` columns
- Existing users with `KeycloakSub` continue to work (username/country optional)
