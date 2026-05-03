# ProjectManagement.App - Architecture and UI Standards

## Engineering Standards
- **Validation:** Selalu jalankan `dotnet build` setelah melakukan perubahan kode untuk memastikan tidak ada error sebelum melaporkan ke user atau melanjutkan tugas.
- **UI Components:** Selalu prioritaskan penggunaan komponen **Syncfusion (EJS)** untuk elemen UI yang interaktif (Grid, Dialog, Form, Chart, dll) guna menjaga konsistensi dan estetika high-fidelity.

## UI / UX Design Standards
- **Theme:** High-fidelity, modern Dribbble-style UI.
- **Typography:** Inter font family (`wght@300;400;500;600;700;800`).
- **Colors:** Light grey/white backgrounds (`#f8fafc`), indigo primary accents (`#4f46e5`).
- **Components:**
  - Avoid bulky buttons; prefer clean text buttons, `.e-outline` styles, or soft rounded buttons.
  - Cards should use `.card-custom` (rounded 16px/20px, elegant soft shadows `box-shadow: var(--shadow-elegant)`).
  - Status badges should use pastel soft backgrounds (e.g., `.bg-soft-success`, `.bg-soft-indigo`) with `.soft-badge`.
  - Use `.user-avatar-circle` for user initials.
- **Syncfusion Overrides:**
  - Grid and Kanban components must have custom CSS overrides in `site.css` to remove borders, match header backgrounds (`#fcfdfe`), and implement clean row hover effects that align with the high-fidelity UI.

## Architecture & Code Conventions
- **Data Access:** Keep controllers thin. Do NOT inject `AppDbContext` into controllers. All database queries, including complex filtering for dashboards and activity feeds, must be handled in the Repository layer (e.g., `IProjectRepository`, `IGithubRepository`).
- **HTMX Integration:**
  - Use `hx-on:htmx:after-request` for robust JSON response handling during form submissions (e.g., dialogs).
  - Use global fallback handlers in `site.js` for manual JSON redirects (`window.location.href`). 
  - Use native `Response.Htmx(h => h.Redirect(...))` in controllers for automatic HTMX redirects when possible.
- **GitHub Integration:** Token validation and claim updates (Claim: `GitHubConnected`) are managed globally via `GithubTokenMiddleware`.

## Layout & Navigation
- The main layout uses a sticky navbar and an independent scrolling `.main-wrapper`.
- Do NOT use `overflow: hidden` globally; ensure the body can scroll on smaller resolutions while maintaining a fixed sidebar structure on larger screens.
