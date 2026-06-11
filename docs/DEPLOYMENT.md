# Deployment Guide

The platform runs on three free-tier cloud services. Everything below is a one-time setup; afterwards every push to `main` flows through CI automatically.

```
GitHub push ──▶ GitHub Actions (build + 89 tests)
                      │ green?
                      ▼
              Render deploy hook ──▶ Render (Docker, ASP.NET API)
                                          │
Vercel Git integration ──▶ Vercel (React SPA)   Supabase (PostgreSQL)
```

## 1. Database — Supabase (done)

Project `MovieReviewPlatform` (ref `aoyrqroobgcrnjywsayb`, region `eu-central-1`, free tier).

- Schema is applied through EF Core migrations (the API also runs `Database.Migrate()` at startup).
- Row Level Security is enabled on all tables with no policies: the auto-generated Supabase REST API is fully locked, while the backend's direct Postgres connection (table owner) is unaffected.
- Connection string format (IPv4-friendly session pooler):

```
Host=aws-1-eu-central-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.aoyrqroobgcrnjywsayb;Password=<DB PASSWORD>;SSL Mode=Require;Trust Server Certificate=true
```

## 2. API — Render

1. Sign up at [render.com](https://render.com) (free, GitHub login).
2. **New → Blueprint**, select the `Mistik03/MovieReviewPlatform` repository — Render reads [render.yaml](../render.yaml) and proposes the `moviereview-api` service.
3. When prompted for the secret env vars, set:

| Variable | Value |
|---|---|
| `ConnectionStrings__Default` | the Supabase connection string above |
| `Jwt__Key` | a long random string (64+ chars) — generate with `[Convert]::ToBase64String((1..48 \| %{Get-Random -Max 256}))` in PowerShell |
| `Tmdb__ApiKey` | your TMDB API key or v4 read token |
| `Admin__Password` | the production admin password (login: `admin`) |
| `Cors__Origins__1` | your Vercel URL once known, e.g. `https://moviereview.vercel.app` |

4. After the first deploy, copy the service URL (e.g. `https://moviereview-api.onrender.com`) — Swagger lives at its root.
5. **Settings → Deploy Hook**: copy the hook URL for CI (next section).

Free-tier note: the service sleeps after ~15 minutes idle; the first request afterwards takes ~30–60 s. Mention this in the demo, or open the URL a minute before presenting.

## 3. CI/CD — GitHub Actions

Pipelines live in [.github/workflows](../.github/workflows) and run automatically. One secret connects CI to Render:

1. GitHub repo → **Settings → Secrets and variables → Actions → New repository secret**
2. Name: `RENDER_DEPLOY_HOOK_URL`, value: the deploy hook from Render.

From then on: every push to `main` touching `backend/` builds, runs all 89 tests, and only on green triggers the Render deployment. Pull requests get the same build+test gate without deploying.

## 4. Frontend — Vercel

1. Sign in at [vercel.com](https://vercel.com) with GitHub.
2. **Add New → Project**, import `Mistik03/MovieReviewPlatform`.
3. Set **Root Directory** to `frontend` (Framework preset: Vite — detected automatically).
4. Add environment variable `VITE_API_BASE_URL` = your Render URL (no trailing slash).
5. Deploy. Afterwards, put the production URL into Render's `Cors__Origins__1` so the API accepts browser requests from it.

SPA routing is handled by [frontend/vercel.json](../frontend/vercel.json) (all paths rewrite to `index.html`).

## 5. Post-deploy smoke test

1. `https://<render-url>/api/titles` returns the catalog JSON.
2. Open the Vercel site: home page shows the catalog, posters load.
3. Register a user, post a rating and a review.
4. Log in as `admin`, import a new movie via Admin → TMDB Import.
