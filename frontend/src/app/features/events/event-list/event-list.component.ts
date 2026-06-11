import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';
import { AuthService } from '../../../core/services/auth.service';
import { EventFilters, EventSummary, Venue } from '../../../core/models/models';

@Component({
  selector: 'app-event-list',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="page-head">
      <div>
        <h1>Eventos</h1>
        <p>Gestiona eventos, reservas y ocupación en tiempo real.</p>
      </div>
      @if (auth.isAdmin()) {
        <a class="btn btn-primary" routerLink="/events/new">+ Nuevo evento</a>
      }
    </div>

    <form class="card" [formGroup]="filters" (ngSubmit)="search()" style="margin-bottom:20px">
      <div class="grid grid-3">
        <div class="field">
          <label>Buscar por título</label>
          <input type="text" formControlName="title" placeholder="Ej: jazz" />
        </div>
        <div class="field">
          <label>Tipo</label>
          <select formControlName="type">
            <option value="">Todos</option>
            <option value="conferencia">Conferencia</option>
            <option value="taller">Taller</option>
            <option value="concierto">Concierto</option>
          </select>
        </div>
        <div class="field">
          <label>Venue</label>
          <select formControlName="venueId">
            <option value="">Todos</option>
            @for (v of venues(); track v.id) {
              <option [value]="v.id">{{ v.name }}</option>
            }
          </select>
        </div>
        <div class="field">
          <label>Estado</label>
          <select formControlName="status">
            <option value="">Todos</option>
            <option value="activo">Activo</option>
            <option value="completado">Completado</option>
            <option value="cancelado">Cancelado</option>
          </select>
        </div>
        <div class="field">
          <label>Desde (inicio)</label>
          <input type="date" formControlName="startFrom" />
        </div>
        <div class="field">
          <label>Hasta (inicio)</label>
          <input type="date" formControlName="startTo" />
        </div>
      </div>
      <div class="actions">
        <button type="submit" class="btn btn-primary">Filtrar</button>
        <button type="button" class="btn btn-ghost" (click)="reset()">Limpiar</button>
      </div>
    </form>

    @if (loading()) {
      <p class="loading">Cargando eventos…</p>
    } @else if (events().length === 0) {
      <div class="card empty">
        <div class="empty__icon">🗓️</div>
        <p style="margin:0">No hay eventos que coincidan con los filtros.</p>
      </div>
    } @else {
      <div class="events-grid">
        @for (e of events(); track e.id) {
          <a class="event-card" [routerLink]="['/events', e.id]">
            <div class="event-card__top">
              <span class="badge badge-{{ e.type }}">{{ e.type }}</span>
              <span class="badge badge-{{ e.status }}">{{ e.status }}</span>
            </div>
            <div class="event-card__title">{{ e.title }}</div>
            <div class="event-card__meta">
              <span>{{ e.venueName }}</span>
              <span>{{ e.startUtc | date:'medium' }}</span>
            </div>
            <div class="event-card__foot">
              <span class="event-card__price">{{ e.price | currency:'USD' }}</span>
              <span class="muted">Cap. {{ e.capacity }}</span>
            </div>
          </a>
        }
      </div>
    }
  `
})
export class EventListComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly fb = inject(FormBuilder);
  protected readonly auth = inject(AuthService);

  protected readonly events = signal<EventSummary[]>([]);
  protected readonly venues = signal<Venue[]>([]);
  protected readonly loading = signal(false);

  protected readonly filters = this.fb.group({
    title: [''],
    type: [''],
    venueId: [''],
    status: [''],
    startFrom: [''],
    startTo: ['']
  });

  ngOnInit(): void {
    this.api.getVenues().subscribe(v => this.venues.set(v));
    this.search();
  }

  search(): void {
    this.loading.set(true);
    const raw = this.filters.getRawValue();
    const query: EventFilters = {};
    if (raw.title) query.title = raw.title;
    if (raw.type) query.type = raw.type as EventFilters['type'];
    if (raw.venueId) query.venueId = Number(raw.venueId);
    if (raw.status) query.status = raw.status as EventFilters['status'];
    if (raw.startFrom) query.startFromUtc = new Date(raw.startFrom).toISOString();
    if (raw.startTo) query.startToUtc = new Date(raw.startTo).toISOString();

    this.api.getEvents(query).subscribe({
      next: data => { this.events.set(data); this.loading.set(false); },
      error: () => this.loading.set(false)
    });
  }

  reset(): void {
    this.filters.reset({ title: '', type: '', venueId: '', status: '', startFrom: '', startTo: '' });
    this.search();
  }
}
