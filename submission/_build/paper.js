const fs = require('fs')
const path = require('path')
const {
  Document, Packer, Paragraph, TextRun, Table, TableRow, TableCell, ImageRun,
  AlignmentType, HeadingLevel, BorderStyle, WidthType, ShadingType, LevelFormat,
  PageBreak, ExternalHyperlink, TableOfContents, Footer, PageNumber, VerticalAlign,
} = require('docx')

const DOCS = path.resolve(__dirname, '..', '..', 'docs')
const OUT = path.resolve(__dirname, '..', 'paper', 'MovieReviewPlatform_FinalPaper.docx')

// A4 content width with 1" margins = 11906 - 2880 = 9026 DXA. Images sized in px (~96dpi => ~602px).
const IMG_W = 560
const img = (file, srcW, srcH, caption) => {
  const height = Math.round((IMG_W * srcH) / srcW)
  const children = [
    new ImageRun({
      type: 'png',
      data: fs.readFileSync(file),
      transformation: { width: IMG_W, height },
      altText: { title: caption, description: caption, name: path.basename(file) },
    }),
  ]
  const parts = [new Paragraph({ alignment: AlignmentType.CENTER, spacing: { before: 120, after: 40 }, children })]
  if (caption) {
    parts.push(new Paragraph({
      alignment: AlignmentType.CENTER,
      spacing: { after: 200 },
      children: [new TextRun({ text: caption, italics: true, size: 18, color: '5b6472' })],
    }))
  }
  return parts
}

const P = (text, opts = {}) => new Paragraph({
  spacing: { after: opts.after ?? 140, line: 276 },
  alignment: opts.align,
  children: Array.isArray(text) ? text : [new TextRun({ text, size: 22, ...opts.run })],
})

const H1 = (text) => new Paragraph({ heading: HeadingLevel.HEADING_1, spacing: { before: 280, after: 140 }, children: [new TextRun(text)] })
const H2 = (text) => new Paragraph({ heading: HeadingLevel.HEADING_2, spacing: { before: 200, after: 100 }, children: [new TextRun(text)] })

const bullet = (text) => new Paragraph({
  numbering: { reference: 'bullets', level: 0 },
  spacing: { after: 70, line: 270 },
  children: Array.isArray(text) ? text : [new TextRun({ text, size: 22 })],
})
const numbered = (text, ref = 'nums') => new Paragraph({
  numbering: { reference: ref, level: 0 },
  spacing: { after: 70, line: 270 },
  children: Array.isArray(text) ? text : [new TextRun({ text, size: 22 })],
})
const bold = (t) => new TextRun({ text: t, bold: true, size: 22 })
const run = (t) => new TextRun({ text: t, size: 22 })
const mono = (t) => new TextRun({ text: t, font: 'Consolas', size: 20, color: '1d5a4c' })

// ---- Tables ----
const border = { style: BorderStyle.SINGLE, size: 1, color: 'C7CED9' }
const borders = { top: border, bottom: border, left: border, right: border, insideHorizontal: border, insideVertical: border }
const cellMargins = { top: 60, bottom: 60, left: 110, right: 110 }

function table(headers, rows, widths) {
  const total = widths.reduce((a, b) => a + b, 0)
  const headerRow = new TableRow({
    tableHeader: true,
    children: headers.map((h, i) => new TableCell({
      width: { size: widths[i], type: WidthType.DXA },
      shading: { fill: '1F2430', type: ShadingType.CLEAR, color: 'auto' },
      margins: cellMargins,
      verticalAlign: VerticalAlign.CENTER,
      children: [new Paragraph({ children: [new TextRun({ text: h, bold: true, color: 'FFFFFF', size: 20 })] })],
    })),
  })
  const bodyRows = rows.map((cells, r) => new TableRow({
    children: cells.map((c, i) => new TableCell({
      width: { size: widths[i], type: WidthType.DXA },
      shading: { fill: r % 2 ? 'F4F6FA' : 'FFFFFF', type: ShadingType.CLEAR, color: 'auto' },
      margins: cellMargins,
      verticalAlign: VerticalAlign.CENTER,
      children: [new Paragraph({ children: Array.isArray(c) ? c : [new TextRun({ text: String(c), size: 20 })] })],
    })),
  }))
  return new Table({
    width: { size: total, type: WidthType.DXA },
    columnWidths: widths,
    borders,
    rows: [headerRow, ...bodyRows],
  })
}

// =====================================================================
const title = (t, sz, opts = {}) => new Paragraph({
  alignment: AlignmentType.CENTER, spacing: { after: opts.after ?? 80 },
  children: [new TextRun({ text: t, size: sz, bold: opts.bold, color: opts.color ?? '11151F' })],
})

const children = []

// ---------- TITLE PAGE ----------
children.push(
  new Paragraph({ spacing: { before: 1400 } }),
  title('Marquee', 72, { bold: true, after: 40 }),
  title('A Movie & TV Review Platform', 32, { color: 'B07D2B', after: 200 }),
  title('Service Oriented Architecture — Final Project Paper', 24, { color: '5b6472', after: 600 }),
)

const metaRows = [
  ['Student', 'Arb Xhelili'],
  ['Student ID', '131018'],
  ['Course', 'Service Oriented Architecture'],
  ['Institution', 'South East European University'],
  ['Academic Year', '2025 / 2026'],
  ['Programme', '4th Year, Computer Science'],
  ['GitHub Repository', 'https://github.com/Mistik03/MovieReviewPlatform'],
]
children.push(new Table({
  width: { size: 7000, type: WidthType.DXA },
  alignment: AlignmentType.CENTER,
  columnWidths: [2600, 4400],
  borders,
  rows: metaRows.map((r, i) => new TableRow({
    children: [
      new TableCell({
        width: { size: 2600, type: WidthType.DXA },
        shading: { fill: 'D6E0EC', type: ShadingType.CLEAR, color: 'auto' },
        margins: cellMargins,
        children: [new Paragraph({ children: [new TextRun({ text: r[0], bold: true, size: 20, color: '1F3864' })] })],
      }),
      new TableCell({
        width: { size: 4400, type: WidthType.DXA },
        margins: cellMargins,
        children: [new Paragraph({ children: [new TextRun({ text: r[1], size: 20, color: '333333' })] })],
      }),
    ],
  })),
}))
children.push(new Paragraph({ children: [new PageBreak()] }))

// ---------- TABLE OF CONTENTS ----------
children.push(
  new Paragraph({ heading: HeadingLevel.HEADING_1, spacing: { after: 140 }, children: [new TextRun('Table of Contents')] }),
  new TableOfContents('Contents', { hyperlink: true, headingStyleRange: '1-2' }),
  new Paragraph({ children: [new PageBreak()] }),
)

// ---------- ABSTRACT ----------
children.push(
  H1('Abstract'),
  P('Marquee is a full-stack movie and television review platform built to demonstrate a rigorous Service Oriented Architecture. The system lets visitors browse a catalog of real films and series, registered users post exactly one review and one rating per title, and administrators curate the catalog by importing authentic metadata — cast, crew and high-definition artwork — from The Movie Database (TMDB). The backend is an ASP.NET Core Web API (.NET 10) organised into four strictly separated layers — Controllers, Services, Repositories and Entities — communicating only through interfaces registered with the dependency-injection container. Business rules live exclusively in the service layer, authentication uses JWT bearer tokens with BCrypt-hashed passwords and role-based authorization, and data is persisted to a PostgreSQL database (Supabase) through Entity Framework Core. A React single-page application built with Vite, TypeScript, GSAP and Three.js consumes the API and presents a cinematic, fully responsive dark interface. The codebase is verified by 92 automated unit tests spanning controllers, services and repositories, and is delivered through a GitHub Actions CI/CD pipeline that builds, tests and deploys the API to Render and the frontend to Vercel. The result is a clean, testable, end-to-end system that satisfies every functional and architectural requirement of the assignment while extending the original proposal with television support and a real external-service integration.'),
)

// ---------- 1. INTRODUCTION ----------
children.push(H1('1. Introduction'))
children.push(H2('1.1 Background & Motivation'))
children.push(P('Online audiences increasingly decide what to watch based on aggregated opinion. Platforms such as IMDb and Letterboxd have turned film discovery into a social activity, yet building one is an excellent vehicle for studying service-oriented design: the domain is familiar, the relationships are genuinely many-to-many (a title has many people in its cast; a person appears in many titles), and the rules — who may review what, how averages are computed — are concrete enough to test. The motivation for Marquee was to build such a system the right way: not as a single monolithic controller stuffed with logic, but as cleanly separated services that could be reasoned about, tested and deployed independently.'))
children.push(H2('1.2 Problem Statement'))
children.push(P('The project addresses four concrete challenges:'))
children.push(numbered('Architectural discipline — keeping HTTP concerns, business rules and data access in separate layers that depend only on abstractions, so the system stays testable and maintainable as it grows.'))
children.push(numbered('Trustworthy data — populating the catalog with real, verifiable movie and TV information (correct directors, cast and posters) rather than hand-entered or fabricated content.'))
children.push(numbered('Correct, enforceable rules — guaranteeing invariants such as "one review and one rating per user per title" and "a genre in use cannot be deleted" at both the service and database levels.'))
children.push(numbered('Secure, role-aware access — protecting write operations behind authentication and reserving catalog management for administrators.'))
children.push(H2('1.3 Project Objectives'))
children.push(bullet('Implement a fully layered ASP.NET Core Web API following SOA principles with the Repository and Service patterns and Dependency Injection.'))
children.push(bullet('Implement JWT authentication with role-based authorization (Admin, User, Guest) protecting every relevant endpoint.'))
children.push(bullet('Enforce all business rules in the service layer, including complex orchestration of an external service (TMDB import).'))
children.push(bullet('Persist data with Entity Framework Core to a PostgreSQL database, modelling 1:N and M:N relationships.'))
children.push(bullet('Deliver an attractive, responsive React front-end and a full CI/CD pipeline with cloud deployment.'))
children.push(bullet('Achieve meaningful unit-test coverage across controllers, services and repositories.'))

// ---------- 2. RELATED WORK ----------
children.push(H1('2. Related Work'))
children.push(H2('2.1 Existing Solutions'))
children.push(P([bold('IMDb'), run(' is the largest film database, offering exhaustive metadata and a 1–10 rating system. Its strength is breadth; its weakness for our purposes is that it is a closed commercial product with a cluttered interface and no clean public API for cast and crew suitable for a student project.')]))
children.push(P([bold('Letterboxd'), run(' is a social-first film diary with an elegant, opinionated interface and strong review culture. It demonstrates how a focused, well-designed review experience can outshine a feature-heavy competitor — an influence on Marquee’s restrained, cinematic UI — but it is movies-only and proprietary.')]))
children.push(P([bold('The Movie Database (TMDB)'), run(' is a community-maintained, openly licensed metadata service with a free, well-documented REST API covering both movies and television, including full credits and high-resolution imagery. Rather than compete with TMDB, Marquee consumes it as its source of truth, which keeps catalog data accurate and avoids any fabricated content.')]))
children.push(H2('2.2 Novelty of Your Approach'))
children.push(P('Marquee is not intended to out-feature IMDb; its contribution is architectural and pedagogical. Three things distinguish it. First, it treats the external metadata service as a first-class, orchestrated dependency: the admin "import" use case fetches a title’s details and credits, deduplicates genres and people against the local catalog, and persists the title with its cast inside a single database transaction — a genuine piece of complex business logic rather than simple CRUD. Second, it unifies movies and television behind one Title concept distinguished by a MediaType, which keeps the schema small while supporting both. Third, every invariant is enforced twice — once in the service layer with clear error semantics, and once at the database level with unique indexes and a check constraint — so correctness does not depend on the caller behaving well.'))

// ---------- 3. TOOLS & TECHNOLOGIES ----------
children.push(H1('3. Tools and Technologies'))
children.push(table(
  ['Category', 'Technology / Version'],
  [
    ['Backend framework', 'ASP.NET Core Web API (.NET 10 LTS, C#)'],
    ['ORM', 'Entity Framework Core 10 + Npgsql provider'],
    ['Database', 'PostgreSQL (Supabase, managed)'],
    ['Authentication', 'JWT Bearer tokens, BCrypt.Net password hashing'],
    ['API documentation', 'Swagger / Swashbuckle (with JWT authorize)'],
    ['Front-end', 'React 19 + TypeScript, Vite, Tailwind CSS 4'],
    ['Animation / 3D', 'GSAP, Three.js (react-three-fiber, drei)'],
    ['Data fetching', 'TanStack Query, Axios'],
    ['External data', 'The Movie Database (TMDB) REST API'],
    ['Testing', 'xUnit, Moq, FluentAssertions, EF Core SQLite'],
    ['CI/CD', 'GitHub Actions'],
    ['Hosting', 'Render (API, Docker), Vercel (front-end)'],
    ['Version control', 'Git + GitHub'],
    ['IDE', 'Visual Studio 2026, VS Code'],
  ],
  [3000, 6026],
))
children.push(H2('3.1 Architecture Overview'))
children.push(P('The application follows a strict four-layer architecture. A request flows Client → Controller → Service → Repository → Database, and each layer communicates only with the layer directly beneath it, always through an interface. Controllers translate HTTP to method calls and map results to status codes; they contain no business logic. Services own every rule and perform DTO-to-entity mapping so that raw entities never cross the API boundary. Repositories encapsulate all Entity Framework queries. Repositories and services are registered with a Scoped lifetime, giving one instance per HTTP request, and services depend on repository interfaces rather than concrete classes, which is what makes the system unit-testable with mocks. Design patterns applied include the Repository pattern, the Service layer, Dependency Injection, the DTO pattern and a Unit-of-Work abstraction used to make the TMDB import transactional.'))
children.push(...img(path.join(DOCS, 'diagrams', 'architecture.png'), 1180, 720, 'Figure 1 — Four-layer service-oriented architecture and external TMDB integration.'))

// ---------- 4. SYSTEM DESIGN ----------
children.push(H1('4. System Design'))
children.push(H2('4.1 Entity-Relationship Diagram'))
children.push(P('The domain is modelled with eight entities. User, Title, Genre and Person are the principals; TitleGenre and CastMember are join entities realising the two many-to-many relationships; Review and Rating capture user contributions. A Title represents either a movie or a TV show, distinguished by its MediaType. Two composite unique indexes — (UserId, TitleId) on both Review and Rating — enforce the one-per-user-per-title rule in the database, and a check constraint restricts Rating.Score to 1–10. Average rating and counts are computed on demand and never stored.'))
children.push(...img(path.join(DOCS, 'diagrams', 'er-diagram.png'), 1180, 760, 'Figure 2 — Entity-relationship diagram with keys, constraints and relationships.'))
children.push(H2('4.2 UI Mockup'))
children.push(P('The front-end is a cinematic dark interface built around a near-black palette with a single restrained amber accent, a Fraunces display typeface and a film-grain texture. It is fully responsive from 360 px upward, uses SVG icons throughout, and respects the user’s reduced-motion preference. The screenshots below are taken from the running application populated with real TMDB data.'))
children.push(...img(path.join(DOCS, 'screenshots', 'home.png'), 1366, 900, 'Figure 3 — Home: letterboxed hero with a Three.js particle layer and rating-sorted poster rows.'))
children.push(...img(path.join(DOCS, 'screenshots', 'login.png'), 1366, 900, 'Figure 4 — Login screen.'))
children.push(...img(path.join(DOCS, 'screenshots', 'browse.png'), 1366, 900, 'Figure 5 — Browse: type filter, genre chips, search, sort and a responsive poster grid.'))
children.push(...img(path.join(DOCS, 'screenshots', 'detail.png'), 1366, 900, 'Figure 6 — Title detail (primary CRUD view): metadata, cast, rating control and reviews.'))
children.push(...img(path.join(DOCS, 'screenshots', 'admin.png'), 1366, 900, 'Figure 7 — Admin dashboard: TMDB search-and-import, title and genre management.'))

// ---------- 5. IMPLEMENTATION ----------
children.push(H1('5. Implementation'))
children.push(H2('5.1 API Endpoints'))
children.push(table(
  ['Method', 'Route', 'Auth', 'Description'],
  [
    ['POST', '/api/auth/register', 'None', 'Create account (role User), returns JWT'],
    ['POST', '/api/auth/login', 'None', 'Authenticate, returns JWT'],
    ['GET', '/api/titles', 'None', 'Browse catalog (filter, search, sort, page)'],
    ['GET', '/api/titles/{id}', 'None', 'Title detail with cast and stats'],
    ['POST/PUT/DELETE', '/api/titles…', 'Admin', 'Manage catalog'],
    ['GET', '/api/genres', 'None', 'Genres with title counts'],
    ['POST/DELETE', '/api/genres…', 'Admin', 'Manage genres'],
    ['GET', '/api/reviews?titleId=', 'None', 'Reviews for a title'],
    ['POST/PUT/DELETE', '/api/reviews…', 'User', 'Own review (admin may delete any)'],
    ['GET', '/api/ratings?titleId=', 'None', 'Ratings for a title'],
    ['POST/PUT/DELETE', '/api/ratings…', 'User', 'Own rating (1–10)'],
    ['GET', '/api/users/me', 'User', 'Profile with own reviews and ratings'],
    ['GET', '/api/import/tmdb/search', 'Admin', 'Search TMDB for titles'],
    ['POST', '/api/import/tmdb', 'Admin', 'Import a movie/TV show with cast'],
  ],
  [1700, 2700, 1100, 3526],
))
children.push(...img(path.join(DOCS, 'screenshots', 'swagger.png'), 1366, 720, 'Figure 8 — Swagger UI: the full API surface is testable interactively, with JWT authorization.'))
children.push(H2('5.2 Authentication & Authorization'))
children.push(P('On registration a password is hashed with BCrypt and the account is given the User role. Login verifies the password and issues a signed JWT containing the user id, username, email and role claims. The token is returned to the client, stored in localStorage, and attached as a Bearer header by an Axios interceptor on every subsequent request. The API validates the token’s signature, issuer, audience and lifetime on each call. Authorization is role-based: catalog and genre management and the TMDB import are gated with [Authorize(Roles = "Admin")], while review and rating writes require any authenticated user, with ownership checked in the service layer (administrators are exempt). To resist brute-force and account-enumeration attempts, the authentication endpoints are additionally protected by a fixed-window rate limiter, and login returns an identical message for an unknown user and a wrong password.'))
children.push(H2('5.3 Business Logic Highlights'))
children.push(P('The most substantial logic is the TMDB import orchestration. Given a TMDB id and media type, the service first checks that the title has not already been imported, then calls the external API for the title’s details and credits. It deduplicates the returned genres and people against the local catalog by TMDB id — adopting an existing same-named genre where one was created manually — inserts only the genuinely new records, and finally persists the Title together with its top-billed cast. The whole sequence runs inside a Unit-of-Work transaction, so a failure midway leaves no partial data. Beyond the import, the service layer enforces: one review and one rating per user per title (409 Conflict); rating scores constrained to 1–10; blocked deletion of genres still in use and of titles that still carry reviews or ratings; ownership checks on edits and deletes; and uniqueness of a title’s name within a release year and media type. Average ratings and counts are computed by the repository through projection and surfaced on response DTOs.'))
children.push(H2('5.4 Repository Layer'))
children.push(P('Each aggregate has a repository defined by an interface (for example ITitleRepository, IReviewRepository) and an Entity Framework implementation. Services receive these interfaces through constructor injection and never touch the DbContext directly, which both honours separation of concerns and allows the services to be tested against mocked repositories. An IUnitOfWork abstraction exposes a transactional helper used by the import. All implementations are registered in Program.cs with a Scoped lifetime:'))
children.push(new Paragraph({ spacing: { after: 120 }, shading: { fill: 'F4F6FA', type: ShadingType.CLEAR, color: 'auto' }, children: [
  mono('builder.Services.AddScoped<ITitleRepository, TitleRepository>();'),
] }))
children.push(H2('5.5 Unit Testing'))
children.push(P('The solution includes 92 passing tests written with xUnit, Moq and FluentAssertions. Service tests mock the repository interfaces and cover every business rule — duplicate review and rating conflicts, the 1–10 score boundary, genre- and title-deletion blocks, ownership versus admin exemption, password hashing, the identical-error login behaviour, and the TMDB import’s deduplication and transaction logic. Repository tests run against an in-memory SQLite database so that real unique indexes, the rating check constraint and foreign keys are exercised, alongside the statistics projection, filtering, search, sorting and paging. Controller tests verify status codes, CreatedAtAction routes and that the authenticated identity is forwarded correctly. The same test suite runs on every push through GitHub Actions.'))
children.push(...img(path.join(DOCS, 'diagrams', 'tests.png'), 1180, 360, 'Figure 9 — All 92 unit tests passing across the three layers.'))

// ---------- 6. DEPLOYMENT & DEVOPS ----------
children.push(H1('6. Deployment & DevOps'))
children.push(H2('6.1 Cloud Deployment'))
children.push(P('The assignment specifies Azure for cloud hosting. Because an Azure subscription with virtual-machine credit was not available, an equivalent free-tier cloud architecture was used that exercises the same concepts — containerised deployment, a managed database and environment-based configuration. The API is packaged with a multi-stage Dockerfile (the .NET SDK image builds and publishes; the smaller ASP.NET runtime image serves) and deployed as a Render web service defined declaratively by a render.yaml blueprint, with a health check on /api/genres. The database is a managed PostgreSQL instance on Supabase. The React front-end is deployed to Vercel from the same repository, configured with the API base URL as an environment variable and SPA rewrites so client-side routes resolve. Secrets — the connection string, JWT signing key, TMDB key and admin password — are supplied as environment variables in each platform and are never committed to source control (locally they live in .NET user-secrets).'))
children.push(H2('6.2 CI/CD Pipeline'))
children.push(P('Two GitHub Actions workflows automate delivery. The backend workflow triggers on pushes and pull requests that touch the backend: it restores, builds in Release configuration, and runs the full test suite; only when those succeed on the main branch does it call a Render deploy hook, so a red build can never deploy. The frontend workflow installs dependencies and runs a type-checked production build. A representative excerpt:'))
const yaml = [
  'jobs:',
  '  build-and-test:',
  '    runs-on: ubuntu-latest',
  '    steps:',
  '      - uses: actions/checkout@v4',
  '      - uses: actions/setup-dotnet@v4',
  '        with: { dotnet-version: 10.0.x }',
  '      - run: dotnet test backend/MovieReview.slnx -c Release',
]
yaml.forEach((l) => children.push(new Paragraph({ spacing: { after: 0 }, shading: { fill: 'F4F6FA', type: ShadingType.CLEAR, color: 'auto' }, children: [mono(l)] })))
children.push(new Paragraph({ spacing: { after: 140 } }))
children.push(H2('6.3 Live Application'))
children.push(P([bold('Front-end (Vercel): '), run('deployed from the frontend/ directory. '), bold('API (Render): '), run('Swagger served at the service root. '), bold('Note: '), run('the free Render tier sleeps after idle, so the first request after a pause may take 30–60 seconds to wake the container.')]))

// ---------- 7. GITHUB & TEAM ----------
children.push(H1('7. GitHub & Team Collaboration'))
children.push(H2('7.1 Repository Structure'))
children.push(P([run('Repository: '), new TextRun({ text: 'https://github.com/Mistik03/MovieReviewPlatform', size: 22, color: '1155CC', underline: {} })]))
const tree = [
  'backend/',
  '  src/MovieReview.Api/   Controllers · Services · Repositories · Domain · DTOs',
  '                         Data · External/Tmdb · Auth · Middleware',
  '  tests/MovieReview.Tests/   Controllers · Services · Repositories',
  '  Dockerfile',
  'frontend/   src/ (api · auth · components · pages · styles)',
  'docs/       diagrams · screenshots · DEPLOYMENT.md',
  '.github/workflows/   backend-ci.yml · frontend-ci.yml',
  'render.yaml   README.md',
]
tree.forEach((l) => children.push(new Paragraph({ spacing: { after: 0 }, children: [mono(l)] })))
children.push(new Paragraph({ spacing: { after: 140 } }))
children.push(P('The history is composed of small, focused commits with descriptive messages (for example "feat(tmdb): TMDB typed client and import orchestration service" and "test: 89 unit tests across services, repositories and controllers"), following conventional-commit prefixes so that the evolution of the project is easy to follow.'))
children.push(H2('7.2 Team Contribution'))
children.push(table(
  ['Team Member', 'Responsibilities', '% Contribution'],
  [
    ['Arb Xhelili (131018)', 'Architecture, backend API, database, authentication, TMDB integration, front-end, tests, CI/CD, documentation', '100%'],
  ],
  [2600, 5426, 1000],
))
children.push(P('This project was completed individually.', { run: { italics: true, color: '5b6472' } }))

// ---------- 8. DISCUSSION ----------
children.push(H1('8. Discussion'))
children.push(H2('8.1 Does the System Solve the Problem?'))
children.push(P('Each challenge identified in Section 1.2 is addressed. Architectural discipline is realised by the four-layer design with interface-only dependencies, confirmed by the fact that services can be fully unit-tested with mocked repositories. Trustworthy data is guaranteed by sourcing all content from TMDB rather than hand-entering it. Correct rules are enforced redundantly in the service layer (with clear 4xx semantics) and in the database (unique indexes and a check constraint), and are covered by tests. Secure, role-aware access is provided by JWT authentication, role-based authorization on every protected route, ownership checks, and rate-limited auth endpoints.'))
children.push(H2('8.2 Challenges & How They Were Overcome'))
children.push(P('Three challenges stood out. First, the .NET 10 / Swashbuckle stack introduced a new OpenAPI API surface that broke the initial Swagger security configuration; this was resolved by adopting the new document-based security-requirement registration. Second, making the test suite exercise real constraints required running repositories against SQLite rather than the EF in-memory provider, which silently ignores unique indexes and check constraints. Third, an adversarial self-review of the backend surfaced a subtle bug: a title’s name was checked for uniqueness on its raw value but stored trimmed, so a trailing space could bypass the service check and surface as a raw database error; the fix was to normalise before checking, and a regression test now guards it. The same review prompted moving development secrets out of tracked configuration and adding rate limiting.'))

// ---------- 9. CONCLUSION ----------
children.push(H1('9. Conclusion & Future Work'))
children.push(H2('9.1 Conclusion'))
children.push(P('Marquee delivers a complete, working movie-and-TV review platform that meets every requirement of the assignment: a strictly layered service-oriented API, real business logic beyond CRUD, JWT authentication with role-based authorization, a PostgreSQL database modelled with EF Core, an attractive responsive front-end, comprehensive unit tests, and an automated CI/CD pipeline deploying to the cloud. Its principal value is as a demonstration that disciplined separation of concerns produces a system that is easy to test, reason about and extend.'))
children.push(H2('9.2 Limitations'))
children.push(bullet('The free Render tier sleeps when idle, adding cold-start latency to the first request.'))
children.push(bullet('There is no token refresh; sessions end when the JWT expires and the user must sign in again.'))
children.push(bullet('Catalog population is admin-driven; there is no scheduled synchronisation with TMDB.'))
children.push(bullet('Search is a case-insensitive substring match rather than full-text or fuzzy search.'))
children.push(H2('9.3 Future Work'))
children.push(numbered('Add refresh tokens and "remember me" sessions for smoother authentication.'))
children.push(numbered('Introduce watchlists and per-user recommendations derived from ratings.'))
children.push(numbered('Add review helpfulness voting and sorting, plus pagination of reviews.'))
children.push(numbered('Schedule a background job to keep imported titles in sync with TMDB.'))
children.push(numbered('Add integration tests against a containerised PostgreSQL and end-to-end UI tests.'))

// ---------- 10. REFERENCES ----------
children.push(H1('10. References'))
const refs = [
  'Microsoft. (2025). ASP.NET Core documentation. https://learn.microsoft.com/aspnet/core',
  'Microsoft. (2025). Entity Framework Core documentation. https://learn.microsoft.com/ef/core',
  'Microsoft. (2025). Authentication and authorization in ASP.NET Core. https://learn.microsoft.com/aspnet/core/security',
  'The Movie Database. (2025). TMDB API documentation. https://developer.themoviedb.org',
  'Supabase. (2025). Supabase documentation. https://supabase.com/docs',
  'Npgsql. (2025). Npgsql Entity Framework Core provider. https://www.npgsql.org/efcore',
  'React. (2025). React documentation. https://react.dev',
  'Vite. (2025). Vite documentation. https://vite.dev',
  'Tailwind CSS. (2025). Tailwind CSS documentation. https://tailwindcss.com',
  'GreenSock. (2025). GSAP documentation. https://gsap.com/docs',
  'Three.js. (2025). Three.js documentation. https://threejs.org/docs',
  'TanStack. (2025). TanStack Query documentation. https://tanstack.com/query',
  'xUnit.net. (2025). xUnit documentation. https://xunit.net',
  'Render. (2025). Render documentation. https://render.com/docs',
  'Vercel. (2025). Vercel documentation. https://vercel.com/docs',
  'JSON Web Tokens. (2025). Introduction to JSON Web Tokens. https://jwt.io/introduction',
]
refs.forEach((r) => children.push(new Paragraph({ numbering: { reference: 'refs', level: 0 }, spacing: { after: 60, line: 264 }, children: [new TextRun({ text: r, size: 20 })] })))

// ---------- DOCUMENT ----------
const doc = new Document({
  creator: 'Arb Xhelili',
  title: 'Marquee — Movie Review Platform — Final Paper',
  styles: {
    default: { document: { run: { font: 'Calibri', size: 22, color: '222222' } } },
    paragraphStyles: [
      { id: 'Heading1', name: 'Heading 1', basedOn: 'Normal', next: 'Normal', quickFormat: true,
        run: { size: 30, bold: true, color: '11151F', font: 'Calibri' },
        paragraph: { spacing: { before: 280, after: 140 }, outlineLevel: 0, keepNext: true,
          border: { bottom: { style: BorderStyle.SINGLE, size: 6, color: 'B07D2B', space: 4 } } } },
      { id: 'Heading2', name: 'Heading 2', basedOn: 'Normal', next: 'Normal', quickFormat: true,
        run: { size: 25, bold: true, color: '2E3440', font: 'Calibri' },
        paragraph: { spacing: { before: 200, after: 90 }, outlineLevel: 1, keepNext: true } },
    ],
  },
  numbering: {
    config: [
      { reference: 'bullets', levels: [{ level: 0, format: LevelFormat.BULLET, text: '•', alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 520, hanging: 260 } } } }] },
      { reference: 'nums', levels: [{ level: 0, format: LevelFormat.DECIMAL, text: '%1.', alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 520, hanging: 260 } } } }] },
      { reference: 'refs', levels: [{ level: 0, format: LevelFormat.DECIMAL, text: '[%1]', alignment: AlignmentType.LEFT, style: { paragraph: { indent: { left: 560, hanging: 360 } } } }] },
    ],
  },
  features: { updateFields: true },
  sections: [{
    properties: { page: { size: { width: 11906, height: 16838 }, margin: { top: 1440, right: 1440, bottom: 1440, left: 1440 } } },
    footers: {
      default: new Footer({ children: [new Paragraph({ alignment: AlignmentType.CENTER, children: [
        new TextRun({ text: 'Marquee — Arb Xhelili (131018)   ·   Page ', size: 16, color: '999999' }),
        new TextRun({ children: [PageNumber.CURRENT], size: 16, color: '999999' }),
      ] })] }),
    },
    children,
  }],
})

Packer.toBuffer(doc).then((buf) => {
  fs.mkdirSync(path.dirname(OUT), { recursive: true })
  fs.writeFileSync(OUT, buf)
  console.log('Paper written:', OUT, Math.round(buf.length / 1024) + ' KB')
})
