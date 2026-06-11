import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { EventDetail, OccupancyReport, Reservation } from '../../../core/models/models';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-event-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <a class="btn btn-ghost" routerLink="/events" style="margin-bottom:18px">← Volver a eventos</a>

    @if (loading()) {
      <p class="loading">Cargando…</p>
    } @else {
      @if (event(); as e) {
        <div class="card">
          <div class="row-between" style="align-items:flex-start">
            <div>
              <div class="actions" style="margin-bottom:8px">
                <span class="badge badge-{{ e.type }}">{{ e.type }}</span>
                <span class="badge badge-{{ e.status }}">{{ e.status }}</span>
              </div>
              <h1>{{ e.title }}</h1>
            </div>
            <div class="event-card__price">{{ e.price | currency:'USD' }}</div>
          </div>
          <p class="text-soft" style="margin:12px 0 18px">{{ e.description }}</p>
          <div class="stats">
            <div class="stat"><div class="stat__label">Venue</div><div class="stat__value" style="font-size:16px">{{ e.venueName }}</div></div>
            <div class="stat"><div class="stat__label">Inicio</div><div class="stat__value" style="font-size:15px">{{ e.startUtc | date:'medium' }}</div></div>
            <div class="stat"><div class="stat__label">Fin</div><div class="stat__value" style="font-size:15px">{{ e.endUtc | date:'medium' }}</div></div>
          </div>
        </div>

        @if (auth.isAdmin()) {
          @if (report(); as r) {
            <div class="card">
              <h2 class="card__title">Ocupación</h2>
              <div class="stats" style="margin-bottom:16px">
                <div class="stat"><div class="stat__label">Vendidas</div><div class="stat__value">{{ r.ticketsSold }}</div></div>
                <div class="stat"><div class="stat__label">Disponibles</div><div class="stat__value">{{ r.ticketsAvailable }}</div></div>
                <div class="stat"><div class="stat__label">Ingresos</div><div class="stat__value">{{ r.totalRevenue | currency:'USD' }}</div></div>
              </div>
              <div class="row-between" style="margin-bottom:8px">
                <span class="muted" style="font-size:13px">Pendientes: {{ r.ticketsPending }} · perdidas: {{ r.ticketsLost }}</span>
                <strong>{{ r.occupancyPercentage }}%</strong>
              </div>
              <div class="progress"><div class="progress__bar" [style.width.%]="r.occupancyPercentage"></div></div>
            </div>
          }
        }

        <div [class]="auth.isAdmin() ? 'grid grid-2' : ''">
          <div class="card" [style.maxWidth]="auth.isAdmin() ? null : '480px'">
            <h2 class="card__title">Reservar entradas</h2>
            <form [formGroup]="reserveForm" (ngSubmit)="reserve()">
              <div class="field">
                <label>Cantidad *</label>
                <input type="number" formControlName="quantity" min="1" />
              </div>
              <div class="field">
                <label>Nombre del comprador *</label>
                <input type="text" formControlName="buyerName" />
              </div>
              <div class="field">
                <label>Email del comprador *</label>
                <input type="email" formControlName="buyerEmail" />
                @if (reserveForm.controls.buyerEmail.touched && reserveForm.controls.buyerEmail.invalid) {
                  <div class="error-text">Ingresa un email válido.</div>
                }
              </div>
              <button type="submit" class="btn btn-primary" [disabled]="reserving()">
                {{ reserving() ? 'Reservando…' : 'Reservar' }}
              </button>
            </form>
          </div>

          @if (auth.isAdmin()) {
            <div class="card">
              <h2 class="card__title">Reservas</h2>
              @if (reservations().length === 0) {
                <div class="empty"><div class="empty__icon">🎟️</div><p style="margin:0">Aún no hay reservas.</p></div>
              } @else {
                <div class="table-wrap">
                  <table>
                    <thead>
                      <tr><th>Comprador</th><th>Cant.</th><th>Estado</th><th>Código</th><th></th></tr>
                    </thead>
                    <tbody>
                      @for (res of reservations(); track res.id) {
                        <tr>
                          <td class="stack"><strong>{{ res.buyerName }}</strong><span class="muted">{{ res.buyerEmail }}</span></td>
                          <td>{{ res.quantity }}</td>
                          <td><span class="badge badge-{{ res.status }}">{{ res.status }}</span></td>
                          <td>{{ res.reservationCode ?? '—' }}</td>
                          <td style="white-space:nowrap">
                            <div class="actions">
                              @if (res.status === 'pendientePago') {
                                <button class="btn btn-success btn-sm" (click)="confirm(res)">Confirmar</button>
                              }
                              @if (res.status === 'pendientePago' || res.status === 'confirmada') {
                                <button class="btn btn-danger btn-sm" (click)="cancel(res)">Cancelar</button>
                              }
                            </div>
                          </td>
                        </tr>
                      }
                    </tbody>
                  </table>
                </div>
              }
            </div>
          }
        </div>
      } @else {
        <div class="card empty"><div class="empty__icon">🔍</div><p style="margin:0">No se encontró el evento.</p></div>
      }
    }
  `
})
export class EventDetailComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly route = inject(ActivatedRoute);
  private readonly fb = inject(FormBuilder);
  private readonly notifications = inject(NotificationService);
  protected readonly auth = inject(AuthService);

  protected readonly event = signal<EventDetail | null>(null);
  protected readonly report = signal<OccupancyReport | null>(null);
  protected readonly reservations = signal<Reservation[]>([]);
  protected readonly loading = signal(true);
  protected readonly reserving = signal(false);

  private eventId = '';

  protected readonly reserveForm = this.fb.group({
    quantity: [1, [Validators.required, Validators.min(1)]],
    buyerName: ['', [Validators.required]],
    buyerEmail: ['', [Validators.required, Validators.email]]
  });

  ngOnInit(): void {
    this.eventId = this.route.snapshot.paramMap.get('id') ?? '';
    this.api.getEvent(this.eventId).subscribe({
      next: e => { this.event.set(e); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
    this.refreshAdminData();
  }

  /** Report and reservations are admin-only endpoints; only load them for admins. */
  private refreshAdminData(): void {
    if (!this.auth.isAdmin()) return;
    this.api.getReport(this.eventId).subscribe(r => this.report.set(r));
    this.api.getReservations(this.eventId).subscribe(list => this.reservations.set(list));
  }

  reserve(): void {
    if (this.reserveForm.invalid) {
      this.reserveForm.markAllAsTouched();
      return;
    }
    const raw = this.reserveForm.getRawValue();
    this.reserving.set(true);
    this.api.createReservation({
      eventId: this.eventId,
      quantity: Number(raw.quantity),
      buyerName: raw.buyerName!,
      buyerEmail: raw.buyerEmail!
    }).subscribe({
      next: () => {
        this.notifications.success('Reserva creada (pendiente de pago).');
        this.reserveForm.reset({ quantity: 1, buyerName: '', buyerEmail: '' });
        this.reserving.set(false);
        this.refreshAdminData();
      },
      error: () => this.reserving.set(false)
    });
  }

  confirm(reservation: Reservation): void {
    this.api.confirmReservation(reservation.id).subscribe(() => {
      this.notifications.success('Pago confirmado.');
      this.refreshAdminData();
    });
  }

  cancel(reservation: Reservation): void {
    this.api.cancelReservation(reservation.id).subscribe(() => {
      this.notifications.success('Reserva cancelada.');
      this.refreshAdminData();
    });
  }
}
