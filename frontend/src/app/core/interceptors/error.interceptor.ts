import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { NotificationService } from '../services/notification.service';
import { AuthService } from '../services/auth.service';

/**
 * Surfaces RFC 7807 ProblemDetails as user-facing notifications and reacts to 401s
 * (expired/invalid session) by clearing auth and routing to login.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notifications = inject(NotificationService);
  const auth = inject(AuthService);
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !req.url.endsWith('/auth/login')) {
        auth.logout();
        router.navigate(['/login']);
      }
      notifications.error(extractMessage(error));
      return throwError(() => error);
    })
  );
};

function extractMessage(error: HttpErrorResponse): string {
  if (error.status === 0) {
    return 'No se pudo conectar con el servidor. ¿Está la API en ejecución?';
  }
  if (error.status === 401) {
    return 'Sesión no válida o credenciales incorrectas.';
  }

  const problem = error.error;
  if (problem?.errors) {
    const messages = Object.values(problem.errors as Record<string, string[]>).flat();
    if (messages.length > 0) return messages.join(' ');
  }
  return problem?.detail ?? 'Ocurrió un error inesperado.';
}
