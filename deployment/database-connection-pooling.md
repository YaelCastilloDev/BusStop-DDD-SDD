# Database Connection Pooling — Production Deployment Guide

## Overview

BusStop uses **PgBouncer** (transaction mode) to multiplex thousands of client connections
from horizontally-scaled API instances onto a small pool of actual PostgreSQL connections.
This is critical for Cloud Run deployments where each instance opens its own Npgsql connections.

```
 Cloud Run 1 ─┐
 Cloud Run 2 ─┤                          ┌─────────────┐
 Cloud Run N ─┼──► PgBouncer :6432 ────► │ PostgreSQL   │
                  (25 pool)               │ (postgis)   │
                  Pooling=false           └─────────────┘
```

## Connection Strings

| Environment | Connection String |
|---|---|
| Local (docker compose) | `Host=pgbouncer;Port=6432;Database=busstop;Username=busstop;Password=busstop;Pooling=false` |
| Local (dotnet run) | `Host=localhost;Port=6432;Database=busstop;Username=busstop;Password=busstop;Pooling=false` |
| Aspire dev | Injected automatically via `.WithPgBouncer()` |
| Cloud Run (prod) | `Host=<pgbouncer-address>;Port=6432;Database=busstop;Username=<user>;Password=<pass>;Pooling=false` |

**Critical:** `Pooling=false` must be set when behind PgBouncer. Double-pooling (Npgsql + PgBouncer) causes
connection leaks and degraded performance.

## Pool Sizing

### How to size `default_pool_size`

```
pool_size = (total_cores * 1.5) + effective_spindle_count
```

| Concurrent requests (est.) | API instances | default_pool_size | Max PG conns |
|---|---|---|---|
| 100–500 | 2–5 | 25 | 25 |
| 500–2000 | 5–15 | 35 | 35 |
| 2000–5000 | 15–30 | 50 | 50 |
| 5000+ | 30+ | 75 | 75 |

Rule of thumb: start at 25, monitor with `SHOW POOLS`, increase if you see `waiting` > 0 frequently.

### How to size `max_client_conn`

```
max_client_conn = (api_instances * max_pool_size_per_instance) * 1.2
```

Example: 10 Cloud Run instances × 100 Npgsql pooled = 1000 expected clients.
Set `max_client_conn` to 1200 (20% headroom).

### PgBouncer key settings

| Parameter | Default | Purpose |
|---|---|---|
| `pool_mode` | transaction | Returns connection to pool after each tx — mandatory for web apps |
| `default_pool_size` | 25 | Number of actual PostgreSQL connections PgBouncer maintains |
| `max_client_conn` | 1000 | Max incoming app connections PgBouncer accepts |
| `reserve_pool_size` | 5 | Emergency reserved connections when pool is exhausted |
| `server_idle_timeout` | 600s | Idle PG connections are dropped after this |
| `client_idle_timeout` | 0 (disabled) | Set to 300s in production to clean dead client connections |

## Cloud Run Production Checklist

### 1. Deploy PgBouncer

**Option A — Separate Cloud Run service (recommended)**
```
gcloud run deploy pgbouncer \
  --image bitnami/pgbouncer:1.23.1 \
  --port 6432 \
  --vpc-connector <your-vpc-connector> \
  --set-env-vars POSTGRESQL_HOST=<pg-private-ip>,POSTGRESQL_PORT=5432,...
```

Use a VPC connector so PgBouncer can reach the PostgreSQL instance on a private IP.

**Option B — Compute Engine VM**
Deploy PgBouncer on a lightweight VM (e2-micro) in the same VPC as PostgreSQL.
Install via `apt install pgbouncer` and configure `/etc/pgbouncer/pgbouncer.ini`.

### 2. Set environment variable on BusStop API (Cloud Run)

```
ConnectionStrings__PostgresConnection=Host=<pgbouncer-internal-url>;Port=6432;Database=busstop;Username=<user>;Password=<pass>;Pooling=false
```

### 3. Network / firewall

- PostgreSQL must accept connections from PgBouncer's VPC/subnet
- Cloud Run API instances must reach PgBouncer (via VPC connector or internal URL)
- PgBouncer port 6432 must be open between API and PgBouncer

### 4. Authentication

PgBouncer's `userlist.txt` must contain the PostgreSQL credentials the API uses.
The bitnami image auto-generates this from `POSTGRESQL_USERNAME` / `POSTGRESQL_PASSWORD` env vars.

If using a custom PgBouncer deployment, generate the userlist with:
```bash
echo "\"busstop\" \"<password>\"" >> /etc/pgbouncer/userlist.txt
```

### 5. Verify before going live

```bash
# Connect to PgBouncer admin console
psql -h <pgbouncer-host> -p 6432 -U pgbouncer -d pgbouncer

# Check pool status
SHOW POOLS;
SHOW CLIENTS;
SHOW SERVERS;
SHOW STATS;
```

Look for:
- `cl_waiting` column in `SHOW POOLS` — should be 0 most of the time
- `maxwait` — should be low (< 1 second)

## PgBouncer Monitoring Commands

Run via `psql -h <pgbouncer-host> -p 6432 -U pgbouncer -d pgbouncer`:

| Command | What it shows |
|---|---|
| `SHOW POOLS;` | Pool name, active/waiting clients, pool size |
| `SHOW CLIENTS;` | Connected clients, their state, connection age |
| `SHOW SERVERS;` | Actual PostgreSQL connections, idle/active status |
| `SHOW STATS;` | Aggregate statistics (total queries, bytes, time) |
| `SHOW LISTS;` | Database/user/pool configuration |
| `PAUSE <db>;` / `RESUME <db>;` | Gracefully pause/resume traffic to a database |
| `RELOAD;` | Reload configuration without dropping connections |

## Common Issues

| Symptom | Likely cause | Fix |
|---|---|---|
| `cl_waiting` grows over time | `default_pool_size` too low | Increase pool size, reload PgBouncer |
| `no more connections allowed (max_client_conn)` | Client limit reached | Increase `max_client_conn` |
| `server login has been failing` | Auth mismatch or PG unreachable | Check userlist.txt, network, credentials |
| Timeout errors in API | PgBouncer connection timeout too low | Increase `query_timeout` (default 0 = disabled) |
| `FATAL: no pg_hba.conf entry` | PG rejecting PgBouncer's IP | Add PgBouncer's IP/subnet to `pg_hba.conf` |
| Double-pooling connection exhaustion | `Pooling=true` in Npgsql behind PgBouncer | Set `Pooling=false` in connection string |
