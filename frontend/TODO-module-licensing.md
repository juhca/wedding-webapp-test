# TODO: Module Licensing — Frontend Integration

When frontend development starts, integrate with the backend's `GET /api/features` endpoint to show/hide modules based on the active licence.

## Response shape

```json
{ "gifts": true, "rsvp": true, "notifications": false }
```

## Steps

1. **Add `provideHttpClient()`** to `app.config.ts` (currently missing — HttpClient won't work without it)

2. **Create `core/models/modules.model.ts`**
   ```typescript
   export interface Modules {
     gifts: boolean;
     rsvp: boolean;
     notifications: boolean;
   }
   ```

3. **Create `core/services/modules.service.ts`**
   - Use `signal<Modules>` (consistent with existing `signal()` in `app.ts`)
   - Default all flags to `false` to avoid flash-of-content
   - `load()` returns `Promise<void>` for `APP_INITIALIZER`
   - Catch fetch errors silently (fail-open: app boots with all modules hidden)

4. **Wire `APP_INITIALIZER` in `app.config.ts`**
   - Calls `ModulesService.load()` before any component renders
   - Ensures flags are ready before routing kicks in

5. **Create `core/guards/module.guard.ts`**
   - Factory function: `moduleGuard('gifts')` returns a `CanActivateFn`
   - Redirects to `/not-found` if the module is disabled
   - Usage: `{ path: 'gifts', component: GiftsComponent, canActivate: [moduleGuard('gifts')] }`

6. **Conditional nav rendering**
   ```html
   @if (modulesService.modules().gifts) {
     <a routerLink="/gifts">Gifts</a>
   }
   @if (modulesService.modules().notifications) {
     <a routerLink="/notifications">Reminders</a>
   }
   ```

## Notes
- The backend enforces module access too (returns 403 if disabled) — the frontend hiding is UX, not security
- Changing a module flag in `appsettings.json` requires a backend restart
