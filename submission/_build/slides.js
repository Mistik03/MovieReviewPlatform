const path = require('path')
const fs = require('fs')
const PptxGenJS = require('pptxgenjs')

const DOCS = path.resolve(__dirname, '..', '..', 'docs')
const OUT = path.resolve(__dirname, '..', 'slides', 'MovieReviewPlatform_Presentation.pptx')

// Screening-room palette (matches the app)
const INK = '0B0B0D'
const PANEL = '15151A'
const LINE = '2A2A31'
const PAPER = 'EDEAE4'
const MUTED = '8E8B85'
const AMBER = 'D9A648'
const GREEN = '4EC97C'

const pptx = new PptxGenJS()
pptx.defineLayout({ name: 'WIDE', width: 10, height: 5.625 })
pptx.layout = 'WIDE'
pptx.author = 'Arb Xhelili'
pptx.title = 'Marquee — Movie Review Platform'

const HEAD = 'Georgia'
const BODY = 'Calibri'

function base(slide, { footer = true } = {}) {
  slide.background = { color: INK }
  if (footer) {
    slide.addText('Marquee — Service Oriented Architecture · Arb Xhelili (131018) · SEEU', {
      x: 0.5, y: 5.32, w: 7.5, h: 0.25, fontFace: BODY, fontSize: 8.5, color: '55534E', align: 'left', margin: 0,
    })
  }
}

function eyebrow(slide, text, x = 0.5, y = 0.42) {
  slide.addText(text.toUpperCase(), {
    x, y, w: 6.5, h: 0.3, fontFace: BODY, fontSize: 10.5, color: AMBER, charSpacing: 4, bold: true, margin: 0,
  })
}

function titleText(slide, text, opts = {}) {
  slide.addText(text, {
    x: opts.x ?? 0.5, y: opts.y ?? 0.72, w: opts.w ?? 9, h: opts.h ?? 0.85,
    fontFace: HEAD, fontSize: opts.size ?? 30, color: PAPER, bold: true, margin: 0,
  })
}

// Icon chip: small amber-tinted rounded square with a glyph
function chip(slide, x, y, glyph) {
  slide.addShape('roundRect', {
    x, y, w: 0.34, h: 0.34, rectRadius: 0.07,
    fill: { color: '2A2415' }, line: { color: AMBER, width: 0.75 },
  })
  slide.addText(glyph, {
    x: x - 0.06, y: y - 0.02, w: 0.46, h: 0.38, align: 'center', valign: 'middle',
    fontFace: BODY, fontSize: 14, color: AMBER, bold: true, margin: 0,
  })
}

function card(slide, x, y, w, h) {
  slide.addShape('roundRect', {
    x, y, w, h, rectRadius: 0.08,
    fill: { color: PANEL }, line: { color: LINE, width: 1 },
  })
}

// Image inside a white frame card (for light-background diagrams)
function framedImage(slide, file, x, y, w, h) {
  slide.addShape('roundRect', { x, y, w, h, rectRadius: 0.06, fill: { color: 'FFFFFF' }, line: { color: LINE, width: 1 } })
  const pad = 0.08
  slide.addImage({ path: file, x: x + pad, y: y + pad, w: w - 2 * pad, h: h - 2 * pad })
}

// ---------- 1. TITLE ----------
{
  const s = pptx.addSlide()
  base(s, { footer: false })
  // marquee dots motif
  for (let i = 0; i < 14; i++) {
    s.addShape('ellipse', { x: 0.55 + i * 0.66, y: 0.55, w: 0.07, h: 0.07, fill: { color: i % 2 ? '3A3220' : AMBER } })
    s.addShape('ellipse', { x: 0.55 + i * 0.66, y: 4.95, w: 0.07, h: 0.07, fill: { color: i % 2 ? AMBER : '3A3220' } })
  }
  s.addText('MARQUEE', {
    x: 0.5, y: 1.55, w: 9, h: 1.15, align: 'center', fontFace: HEAD, fontSize: 60, color: PAPER, bold: true, charSpacing: 6, margin: 0,
  })
  s.addText('A movie & TV review platform built on a strict Service Oriented Architecture', {
    x: 1.2, y: 2.72, w: 7.6, h: 0.5, align: 'center', fontFace: BODY, fontSize: 15, color: AMBER, italic: true, margin: 0,
  })
  s.addText([
    { text: 'Arb Xhelili · 131018\n', options: { fontSize: 14, color: PAPER, bold: true } },
    { text: 'Service Oriented Architecture — Final Project\n', options: { fontSize: 11.5, color: MUTED } },
    { text: 'South East European University · 2025/2026', options: { fontSize: 11.5, color: MUTED } },
  ], { x: 1.5, y: 3.55, w: 7, h: 0.95, align: 'center', fontFace: BODY, lineSpacing: 17, margin: 0 })
  s.addText('github.com/Mistik03/MovieReviewPlatform', {
    x: 2.5, y: 4.5, w: 5, h: 0.3, align: 'center', fontFace: 'Consolas', fontSize: 10.5, color: '6B675F', margin: 0,
  })
}

// ---------- 2. PROBLEM ----------
{
  const s = pptx.addSlide()
  base(s)
  eyebrow(s, 'The problem')
  titleText(s, 'Building opinion platforms the right way is hard')
  const items = [
    ['1', 'Architectural discipline', 'Logic leaks into controllers; systems become untestable monoliths.'],
    ['2', 'Trustworthy data', 'Hand-entered catalogs drift into wrong directors, fake casts, broken posters.'],
    ['3', 'Enforceable rules', '"One review per user" must survive race conditions and misbehaving clients.'],
    ['4', 'Role-aware security', 'Catalog management, user content and public browsing need different gates.'],
  ]
  items.forEach(([n, h, d], i) => {
    const x = 0.5 + (i % 2) * 4.62, y = 1.85 + Math.floor(i / 2) * 1.62
    card(s, x, y, 4.38, 1.42)
    s.addText(n, { x: x + 0.18, y: y + 0.16, w: 0.5, h: 0.6, fontFace: HEAD, fontSize: 26, color: AMBER, bold: true, margin: 0 })
    s.addText(h, { x: x + 0.68, y: y + 0.16, w: 3.55, h: 0.32, fontFace: BODY, fontSize: 14.5, color: PAPER, bold: true, margin: 0 })
    s.addText(d, { x: x + 0.68, y: y + 0.5, w: 3.55, h: 0.8, fontFace: BODY, fontSize: 11, color: MUTED, lineSpacing: 14, margin: 0 })
  })
}

// ---------- 3. SOLUTION ----------
{
  const s = pptx.addSlide()
  base(s)
  eyebrow(s, 'The solution')
  titleText(s, 'Marquee — every film deserves a verdict', { w: 5.1, size: 26, h: 1.3 })
  const points = [
    ['Browse', 'Real films & series with HD artwork, filters, search and four sort orders.'],
    ['Review & rate', 'One review and one 1–10 score per user per title — enforced, not suggested.'],
    ['Curate', 'Admins import titles from TMDB: metadata, genres and top-billed cast in one click.'],
    ['Real data only', 'Every fact in the catalog comes from The Movie Database — nothing invented.'],
  ]
  points.forEach(([h, d], i) => {
    const y = 2.0 + i * 0.82
    chip(s, 0.5, y, '◆')
    s.addText(h, { x: 1.0, y: y - 0.04, w: 3.6, h: 0.3, fontFace: BODY, fontSize: 13.5, color: PAPER, bold: true, margin: 0 })
    s.addText(d, { x: 1.0, y: y + 0.26, w: 3.7, h: 0.55, fontFace: BODY, fontSize: 10.5, color: MUTED, lineSpacing: 13, margin: 0 })
  })
  // home screenshot right half (1366x900)
  s.addImage({ path: path.join(DOCS, 'screenshots', 'home.png'), x: 5.05, y: 1.15, w: 4.45, h: 2.93 })
  s.addShape('roundRect', { x: 5.05, y: 1.15, w: 4.45, h: 2.93, rectRadius: 0.04, fill: { type: 'none' }, line: { color: LINE, width: 1.25 } })
  s.addText('The live home page — Three.js particle hero, GSAP reveals, rating-sorted rows', {
    x: 5.05, y: 4.14, w: 4.45, h: 0.3, fontFace: BODY, fontSize: 9, color: '6B675F', italic: true, align: 'center', margin: 0,
  })
}

// ---------- 4. ARCHITECTURE ----------
{
  const s = pptx.addSlide()
  base(s)
  eyebrow(s, 'Design — architecture')
  titleText(s, 'Four layers, interfaces only, one direction', { size: 26 })
  framedImage(s, path.join(DOCS, 'diagrams', 'architecture.png'), 0.5, 1.62, 5.55, 3.5)
  const notes = [
    ['Controllers', 'HTTP in, status codes out — zero logic'],
    ['Services', 'Every business rule + DTO mapping'],
    ['Repositories', 'All EF Core queries behind interfaces'],
    ['Scoped DI', 'One instance per request, mock-friendly'],
  ]
  notes.forEach(([h, d], i) => {
    const y = 1.72 + i * 0.88
    s.addText(h, { x: 6.35, y, w: 3.1, h: 0.3, fontFace: BODY, fontSize: 13.5, color: AMBER, bold: true, margin: 0 })
    s.addText(d, { x: 6.35, y: y + 0.3, w: 3.15, h: 0.5, fontFace: BODY, fontSize: 10.5, color: MUTED, lineSpacing: 13, margin: 0 })
  })
}

// ---------- 5. DATA MODEL ----------
{
  const s = pptx.addSlide()
  base(s)
  eyebrow(s, 'Design — data model')
  titleText(s, '8 entities, two M:N bridges, rules in the schema', { size: 26 })
  framedImage(s, path.join(DOCS, 'diagrams', 'er-diagram.png'), 0.5, 1.6, 6.1, 3.55)
  const facts = [
    ['(UserId, TitleId)', 'composite unique on Review and Rating — one each, per user, per title'],
    ['CHECK 1–10', 'score constraint lives in PostgreSQL, not just in code'],
    ['Computed stats', 'average rating & counts projected at query time, never stored'],
  ]
  facts.forEach(([h, d], i) => {
    const y = 1.85 + i * 1.06
    card(s, 6.85, y, 2.65, 0.92)
    s.addText(h, { x: 7.0, y: y + 0.1, w: 2.4, h: 0.26, fontFace: 'Consolas', fontSize: 10.5, color: GREEN, bold: true, margin: 0 })
    s.addText(d, { x: 7.0, y: y + 0.37, w: 2.4, h: 0.52, fontFace: BODY, fontSize: 9, color: MUTED, lineSpacing: 11, margin: 0 })
  })
}

// ---------- 6. COMPLEX LOGIC: TMDB IMPORT ----------
{
  const s = pptx.addSlide()
  base(s)
  eyebrow(s, 'Beyond CRUD')
  titleText(s, 'TMDB import — orchestrating an external service', { size: 26 })
  const steps = [
    ['1', 'Guard', 'Reject if this TMDB id was already imported (409)'],
    ['2', 'Fetch', 'Call TMDB for details + full credits over a typed HttpClient'],
    ['3', 'Dedupe', 'Match genres & people by TMDB id; adopt same-name manual genres'],
    ['4', 'Persist', 'Title + genres + top-12 cast written in ONE transaction (Unit of Work)'],
  ]
  steps.forEach(([n, h, d], i) => {
    const x = 0.5 + i * 2.38
    card(s, x, 1.95, 2.18, 2.0)
    s.addText(n, { x: x + 0.16, y: 2.12, w: 0.6, h: 0.55, fontFace: HEAD, fontSize: 30, color: AMBER, bold: true, margin: 0 })
    s.addText(h, { x: x + 0.16, y: 2.72, w: 1.86, h: 0.3, fontFace: BODY, fontSize: 14, color: PAPER, bold: true, margin: 0 })
    s.addText(d, { x: x + 0.16, y: 3.04, w: 1.86, h: 0.85, fontFace: BODY, fontSize: 10, color: MUTED, lineSpacing: 13, margin: 0 })
    if (i < 3) s.addText('→', { x: x + 2.16, y: 2.7, w: 0.26, h: 0.4, fontFace: BODY, fontSize: 18, color: AMBER, align: 'center', margin: 0 })
  })
  s.addText([
    { text: 'Also in the service layer:  ', options: { color: PAPER, bold: true } },
    { text: 'blocked deletes for in-use genres and reviewed titles · ownership checks with admin exemption · unique title per (name, year, type) · identical login error for unknown user vs wrong password', options: { color: MUTED } },
  ], { x: 0.5, y: 4.35, w: 9, h: 0.7, fontFace: BODY, fontSize: 11, lineSpacing: 15, margin: 0 })
}

// ---------- 7. SECURITY ----------
{
  const s = pptx.addSlide()
  base(s)
  eyebrow(s, 'Authentication & authorization')
  titleText(s, 'Three roles, one token, defense in depth', { size: 26 })
  const rows = [
    ['Guest', 'browse titles, genres, reviews, ratings — no account needed', '37352F'],
    ['User', 'register/login · write, edit and delete OWN reviews & ratings', '2A2415'],
    ['Admin', 'manage catalog & genres · TMDB import · moderate any content', '241A10'],
  ]
  rows.forEach(([role, desc, bg], i) => {
    const y = 1.8 + i * 0.78
    s.addShape('roundRect', { x: 0.5, y, w: 5.6, h: 0.62, rectRadius: 0.07, fill: { color: bg }, line: { color: LINE, width: 0.75 } })
    s.addText(role, { x: 0.72, y: y + 0.08, w: 1.0, h: 0.42, fontFace: HEAD, fontSize: 14, color: AMBER, bold: true, valign: 'middle', margin: 0 })
    s.addText(desc, { x: 1.8, y: y + 0.08, w: 4.2, h: 0.46, fontFace: BODY, fontSize: 10.5, color: PAPER, valign: 'middle', lineSpacing: 12, margin: 0 })
  })
  const mech = [
    'JWT bearer (HMAC-SHA256) — id, name, email, role claims · validated on every call',
    'BCrypt password hashing — plaintext never stored',
    'Rate-limited /api/auth endpoints — brute-force resistant',
    'Global middleware → RFC 7807 ProblemDetails (404 / 409 / 403 / 401 / 400)',
  ]
  s.addText('How it is enforced', { x: 6.4, y: 1.8, w: 3.1, h: 0.3, fontFace: BODY, fontSize: 13, color: PAPER, bold: true, margin: 0 })
  mech.forEach((m, i) => {
    s.addText('▪', { x: 6.4, y: 2.2 + i * 0.62, w: 0.22, h: 0.3, fontFace: BODY, fontSize: 11, color: AMBER, margin: 0 })
    s.addText(m, { x: 6.66, y: 2.18 + i * 0.62, w: 2.9, h: 0.6, fontFace: BODY, fontSize: 9.5, color: MUTED, lineSpacing: 12, margin: 0 })
  })
}

// ---------- 8. TESTING ----------
{
  const s = pptx.addSlide()
  base(s)
  eyebrow(s, 'Quality')
  titleText(s, '92 tests across all three layers', { size: 26 })
  s.addImage({ path: path.join(DOCS, 'diagrams', 'tests.png'), x: 0.7, y: 1.7, w: 8.6, h: 2.63 })
  s.addText([
    { text: 'Services mocked with Moq · repositories run on real SQLite constraints (unique indexes, CHECK) · controllers verify status codes and identity forwarding. ', options: { color: MUTED } },
    { text: 'The suite gates every deploy in CI.', options: { color: AMBER, bold: true } },
  ], { x: 0.7, y: 4.5, w: 8.6, h: 0.55, fontFace: BODY, fontSize: 11.5, lineSpacing: 15, margin: 0 })
}

// ---------- 9. FRONTEND ----------
{
  const s = pptx.addSlide()
  base(s)
  eyebrow(s, 'Front-end')
  titleText(s, 'A screening room, not a spreadsheet', { size: 26 })
  s.addImage({ path: path.join(DOCS, 'screenshots', 'browse.png'), x: 0.5, y: 1.62, w: 4.5, h: 2.96 })
  s.addShape('roundRect', { x: 0.5, y: 1.62, w: 4.5, h: 2.96, rectRadius: 0.04, fill: { type: 'none' }, line: { color: LINE, width: 1.25 } })
  s.addImage({ path: path.join(DOCS, 'screenshots', 'detail.png'), x: 5.2, y: 1.62, w: 4.5, h: 2.96 })
  s.addShape('roundRect', { x: 5.2, y: 1.62, w: 4.5, h: 2.96, rectRadius: 0.04, fill: { type: 'none' }, line: { color: LINE, width: 1.25 } })
  s.addText('React 19 + TypeScript · Tailwind 4 · GSAP scroll reveals · Three.js hero · responsive from 360 px · reduced-motion aware · SVG icons only', {
    x: 0.5, y: 4.68, w: 9.2, h: 0.35, fontFace: BODY, fontSize: 10.5, color: MUTED, align: 'center', margin: 0,
  })
}

// ---------- 10. DEVOPS ----------
{
  const s = pptx.addSlide()
  base(s)
  eyebrow(s, 'Deployment & CI/CD')
  titleText(s, 'A red build can never reach production', { size: 26 })
  const flow = [
    ['git push', 'small, conventional commits', 'Consolas'],
    ['GitHub Actions', 'restore · build · 92 tests', BODY],
    ['Render', 'Docker API + Supabase PostgreSQL', BODY],
    ['Vercel', 'React SPA, env-configured API URL', BODY],
  ]
  flow.forEach(([h, d, f], i) => {
    const x = 0.5 + i * 2.38
    card(s, x, 2.0, 2.18, 1.55)
    s.addText(h, { x: x + 0.16, y: 2.2, w: 1.9, h: 0.34, fontFace: f, fontSize: 13.5, color: i === 1 ? GREEN : AMBER, bold: true, margin: 0 })
    s.addText(d, { x: x + 0.16, y: 2.6, w: 1.86, h: 0.8, fontFace: BODY, fontSize: 10, color: MUTED, lineSpacing: 13, margin: 0 })
    if (i < 3) s.addText('→', { x: x + 2.16, y: 2.55, w: 0.26, h: 0.4, fontFace: BODY, fontSize: 18, color: AMBER, align: 'center', margin: 0 })
  })
  s.addText([
    { text: 'Cloud note:  ', options: { color: PAPER, bold: true } },
    { text: 'the rubric names Azure; without VM credit we deployed the same concepts — containers, managed DB, env-based secrets — on free tiers (Render + Supabase + Vercel). The deploy hook only fires after tests pass on main.', options: { color: MUTED } },
  ], { x: 0.5, y: 3.95, w: 9, h: 0.8, fontFace: BODY, fontSize: 11, lineSpacing: 15, margin: 0 })
}

// ---------- 11. LIVE DEMO ----------
{
  const s = pptx.addSlide()
  base(s)
  eyebrow(s, 'Live demonstration')
  s.addText('Demo', { x: 0.5, y: 0.72, w: 9, h: 0.85, fontFace: HEAD, fontSize: 30, color: PAPER, bold: true, margin: 0 })
  const script = [
    ['1', 'Browse as a guest — filters, search, a title page with real cast'],
    ['2', 'Register, then rate and review a film (and hit the one-review rule)'],
    ['3', 'Sign in as admin — import a brand-new movie live from TMDB'],
    ['4', 'Swagger — call a protected endpoint with and without the JWT'],
  ]
  script.forEach(([n, t], i) => {
    const y = 1.85 + i * 0.78
    s.addShape('ellipse', { x: 0.55, y, w: 0.5, h: 0.5, fill: { color: '2A2415' }, line: { color: AMBER, width: 1 } })
    s.addText(n, { x: 0.55, y, w: 0.5, h: 0.5, align: 'center', valign: 'middle', fontFace: HEAD, fontSize: 16, color: AMBER, bold: true, margin: 0 })
    s.addText(t, { x: 1.25, y: y + 0.05, w: 5.4, h: 0.45, fontFace: BODY, fontSize: 13, color: PAPER, valign: 'middle', lineSpacing: 15, margin: 0 })
  })
  s.addImage({ path: path.join(DOCS, 'screenshots', 'admin.png'), x: 6.9, y: 1.85, w: 2.6, h: 1.71 })
  s.addShape('roundRect', { x: 6.9, y: 1.85, w: 2.6, h: 1.71, rectRadius: 0.04, fill: { type: 'none' }, line: { color: LINE, width: 1 } })
  s.addText('Admin import — the live step 3', { x: 6.9, y: 3.6, w: 2.6, h: 0.26, fontFace: BODY, fontSize: 8.5, color: '6B675F', italic: true, align: 'center', margin: 0 })
  s.addText('Backup: if conference Wi-Fi fails, the same flow runs on localhost with the seeded catalog.', {
    x: 6.9, y: 3.95, w: 2.6, h: 0.85, fontFace: BODY, fontSize: 9, color: MUTED, lineSpacing: 12, margin: 0,
  })
}

// ---------- 12. CLOSING / Q&A ----------
{
  const s = pptx.addSlide()
  base(s, { footer: false })
  for (let i = 0; i < 14; i++) {
    s.addShape('ellipse', { x: 0.55 + i * 0.66, y: 0.55, w: 0.07, h: 0.07, fill: { color: i % 2 ? AMBER : '3A3220' } })
    s.addShape('ellipse', { x: 0.55 + i * 0.66, y: 4.95, w: 0.07, h: 0.07, fill: { color: i % 2 ? '3A3220' : AMBER } })
  }
  s.addText('Questions', { x: 0.5, y: 1.5, w: 9, h: 0.9, align: 'center', fontFace: HEAD, fontSize: 44, color: PAPER, bold: true, margin: 0 })
  const stats = [
    ['4', 'layers, interface-only'],
    ['8', 'entities, 2 M:N bridges'],
    ['92', 'tests gating deploys'],
    ['30+', 'real titles via TMDB'],
  ]
  stats.forEach(([n, l], i) => {
    const x = 1.05 + i * 2.05
    s.addText(n, { x, y: 2.6, w: 1.85, h: 0.6, align: 'center', fontFace: HEAD, fontSize: 34, color: AMBER, bold: true, margin: 0 })
    s.addText(l, { x, y: 3.2, w: 1.85, h: 0.5, align: 'center', fontFace: BODY, fontSize: 10.5, color: MUTED, lineSpacing: 12, margin: 0 })
  })
  s.addText([
    { text: 'Arb Xhelili · 131018   ·   ', options: { color: MUTED } },
    { text: 'github.com/Mistik03/MovieReviewPlatform', options: { color: AMBER } },
  ], { x: 1.5, y: 4.15, w: 7, h: 0.35, align: 'center', fontFace: 'Consolas', fontSize: 11, margin: 0 })
}

fs.mkdirSync(path.dirname(OUT), { recursive: true })
pptx.writeFile({ fileName: OUT }).then(() => {
  const kb = Math.round(fs.statSync(OUT).size / 1024)
  console.log('Slides written:', OUT, kb + ' KB')
})
