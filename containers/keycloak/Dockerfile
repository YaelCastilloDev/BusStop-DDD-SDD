FROM node:22-bookworm-slim AS theme-builder

RUN apt-get update \
    && apt-get install --yes --no-install-recommends default-jdk-headless maven \
    && rm -rf /var/lib/apt/lists/*

RUN corepack enable && corepack prepare pnpm@11.19.0 --activate

WORKDIR /workspace
COPY src/BusStop.Frontend/package.json src/BusStop.Frontend/pnpm-lock.yaml src/BusStop.Frontend/pnpm-workspace.yaml ./
RUN pnpm install --frozen-lockfile

COPY src/BusStop.Frontend/ ./
RUN pnpm build-theme

FROM quay.io/keycloak/keycloak:26.5.2

COPY --from=theme-builder /workspace/dist_keycloak/*.jar /opt/keycloak/providers/
