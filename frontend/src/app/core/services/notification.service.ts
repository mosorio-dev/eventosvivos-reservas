import { Injectable, signal } from '@angular/core';

export interface Notification {
  id: number;
  type: 'success' | 'error';
  message: string;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private counter = 0;
  readonly notifications = signal<Notification[]>([]);

  success(message: string): void {
    this.push('success', message);
  }

  error(message: string): void {
    this.push('error', message);
  }

  dismiss(id: number): void {
    this.notifications.update(list => list.filter(n => n.id !== id));
  }

  private push(type: 'success' | 'error', message: string): void {
    const id = ++this.counter;
    this.notifications.update(list => [...list, { id, type, message }]);
    setTimeout(() => this.dismiss(id), 6000);
  }
}
