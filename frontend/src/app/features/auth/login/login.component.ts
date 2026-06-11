import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div style="max-width:420px;margin:40px auto">
      <div class="page-head" style="justify-content:center">
        <div style="text-align:center">
          <h1>Acceso administrador</h1>
          <p>Inicia sesión para gestionar eventos, pagos y reportes.</p>
        </div>
      </div>

      <form class="card" [formGroup]="form" (ngSubmit)="submit()">
        <div class="field">
          <label>Usuario *</label>
          <input type="text" formControlName="username" autocomplete="username" />
        </div>
        <div class="field">
          <label>Contraseña *</label>
          <input type="password" formControlName="password" autocomplete="current-password" />
        </div>
        <button type="submit" class="btn btn-primary" style="width:100%" [disabled]="submitting()">
          {{ submitting() ? 'Entrando…' : 'Iniciar sesión' }}
        </button>
        <p class="muted" style="font-size:12.5px;margin:14px 0 0;text-align:center">
          Demo: usuario <strong>admin</strong> · contraseña <strong>Admin123!</strong>
        </p>
      </form>

      <p style="text-align:center;margin-top:16px">
        <a routerLink="/events">← Volver a eventos (acceso público)</a>
      </p>
    </div>
  `
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationService);

  protected readonly submitting = signal(false);

  protected readonly form = this.fb.group({
    username: ['', [Validators.required]],
    password: ['', [Validators.required]]
  });

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const { username, password } = this.form.getRawValue();
    this.submitting.set(true);
    this.auth.login(username!, password!).subscribe({
      next: () => {
        this.notifications.success('Sesión iniciada como administrador.');
        this.router.navigate(['/events']);
      },
      error: () => this.submitting.set(false)
    });
  }
}
