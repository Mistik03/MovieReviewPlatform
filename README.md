# Movie Review Platform

A movie & TV show review platform built as the final project for the **Service Oriented Architecture** course at South East European University.

**Author:** Arb Xhelili (Student ID: 131018)

## Architecture at a glance

| Layer | Technology |
|---|---|
| Backend API | ASP.NET Core Web API (.NET 10), strict 4-layer SOA: Controllers → Services → Repositories → Entities |
| Database | PostgreSQL (Supabase) via Entity Framework Core + Npgsql |
| Authentication | JWT Bearer tokens, role-based authorization (Admin / User / Guest) |
| Frontend | React (Vite + TypeScript), Tailwind CSS, GSAP, Three.js |
| External data | TMDB API — real movie/TV metadata, HD posters, full cast |
| Hosting | API on Render, frontend on Vercel |
| CI/CD | GitHub Actions |

> Full documentation (setup guide, API reference, ER diagram, deployment) is added as the project is built — see `docs/`.

## Repository structure

```
backend/    ASP.NET Core Web API + xUnit tests
frontend/   React SPA (Vite + TypeScript)
docs/       Diagrams, screenshots, project documentation
.github/    CI/CD workflows
```
