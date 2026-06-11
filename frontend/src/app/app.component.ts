import { Component, inject } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { NotificationService } from './core/services/notification.service';
import { AuthService } from './core/services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive],
  template: `
    <header class="app-header">
      <div class="app-header__inner">
        <a routerLink="/events" class="brand">
          <span class="brand__logo">EV</span>
          EventosVivos
        </a>
        <nav class="nav">
          <a routerLink="/events" routerLinkActive="active" [routerLinkActiveOptions]="{ exact: true }" class="nav__link">Eventos</a>
          @if (auth.isAdmin()) {
            <a routerLink="/events/new" routerLinkActive="active" class="nav__link">Crear evento</a>
            <span style="display:flex;align-items:center;gap:5px;font-size:13px;color:var(--muted);padding:0 6px">👤 {{ auth.username() }}</span>
            <button class="btn btn-ghost btn-sm" (click)="logout()">Salir</button>
          } @else {
            <a routerLink="/login" routerLinkActive="active" class="nav__link">Iniciar sesión</a>
          }
        </nav>
      </div>
    </header>

    <main class="container">
      <router-outlet />
    </main>

    <div class="toasts">
      @for (n of notifications.notifications(); track n.id) {
        <div class="toast toast-{{ n.type }}" (click)="notifications.dismiss(n.id)">{{ n.message }}</div>
      }
    </div>
  `
})
export class AppComponent {
  protected readonly notifications = inject(NotificationService);
  protected readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/events']);
  }
}
