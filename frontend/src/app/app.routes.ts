import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  // Explicit root redirect — no ambiguity
  { path: '', redirectTo: 'home', pathMatch: 'full' },

  // Unified login (public)
  {
    path: 'login',
    loadComponent: () => import('./auth/login/login.component').then((m) => m.LoginComponent),
  },

  // Admin panel (auth required)
  {
    path: 'admin',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./admin/layout/admin-layout.component').then((m) => m.AdminLayoutComponent),
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./admin/dashboard/dashboard.component').then((m) => m.DashboardComponent),
      },
      {
        path: 'users',
        loadComponent: () =>
          import('./admin/users/users.component').then((m) => m.UsersComponent),
      },
      {
        path: 'rsvp',
        loadComponent: () =>
          import('./admin/rsvp/rsvp.component').then((m) => m.RsvpComponent),
      },
      {
        path: 'gifts',
        loadComponent: () =>
          import('./admin/gifts/gifts.component').then((m) => m.GiftsComponent),
      },
      {
        path: 'wedding-info',
        loadComponent: () =>
          import('./admin/wedding-info/wedding-info.component').then(
            (m) => m.WeddingInfoComponent
          ),
      },
      {
        path: 'features',
        loadComponent: () =>
          import('./admin/features/features.component').then((m) => m.FeaturesComponent),
      },
    ],
  },

  // Guest shell — wraps home/rsvp/gifts (home is public, rsvp+gifts need auth)
  {
    path: '',
    loadComponent: () =>
      import('./guest/layout/guest-layout.component').then((m) => m.GuestLayoutComponent),
    children: [
      {
        path: 'home',
        loadComponent: () =>
          import('./guest/home/guest-home.component').then((m) => m.GuestHomeComponent),
      },
      {
        path: 'about',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./guest/about/about.component').then((m) => m.AboutComponent),
      },
      {
        path: 'rsvp',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./guest/rsvp/guest-rsvp.component').then((m) => m.GuestRsvpComponent),
      },
      {
        path: 'gifts',
        canActivate: [authGuard],
        loadComponent: () =>
          import('./guest/gifts/guest-gifts.component').then((m) => m.GuestGiftsComponent),
      },
    ],
  },

  { path: '**', redirectTo: 'home' },
];
