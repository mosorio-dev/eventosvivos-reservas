import { Injectable, computed, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResult } from '../models/models';

interface StoredAuth {
  token: string;
  username: string;
  role: string;
  expiresAtUtc: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl;
  private readonly storageKey = 'eventosvivos.auth';

  private readonly auth = signal<StoredAuth | null>(this.restore());

  readonly username = computed(() => this.auth()?.username ?? null);
  readonly isAuthenticated = computed(() => this.auth() !== null && !this.isExpired());
  readonly isAdmin = computed(() => this.isAuthenticated() && this.auth()?.role === 'Admin');

  login(username: string, password: string): Observable<AuthResult> {
    return this.http.post<AuthResult>(`${this.base}/auth/login`, { username, password }).pipe(
      tap(res => {
        const stored: StoredAuth = {
          token: res.token,
          username: res.username,
          role: res.role,
          expiresAtUtc: res.expiresAtUtc
        };
        localStorage.setItem(this.storageKey, JSON.stringify(stored));
        this.auth.set(stored);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(this.storageKey);
    this.auth.set(null);
  }

  getToken(): string | null {
    return this.isAuthenticated() ? this.auth()!.token : null;
  }

  private isExpired(): boolean {
    const current = this.auth();
    if (!current) return true;
    return new Date(current.expiresAtUtc).getTime() <= Date.now();
  }

  private restore(): StoredAuth | null {
    try {
      const raw = localStorage.getItem(this.storageKey);
      return raw ? (JSON.parse(raw) as StoredAuth) : null;
    } catch {
      return null;
    }
  }
}
