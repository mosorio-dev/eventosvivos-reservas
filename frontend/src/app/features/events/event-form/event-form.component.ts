import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { ApiService } from '../../../core/services/api.service';
import { CreateEventRequest, EventType, Venue } from '../../../core/models/models';
import { NotificationService } from '../../../core/services/notification.service';

@Component({
  selector: 'app-event-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="page-head">
      <div>
        <h1>Crear evento</h1>
        <p>Completa los datos del nuevo evento.</p>
      </div>
      <a class="btn btn-ghost" routerLink="/events">← Volver</a>
    </div>

    <form class="card" [formGroup]="form" (ngSubmit)="submit()" style="max-width:760px">
      <div class="field">
        <label>Título *</label>
        <input type="text" formControlName="title" placeholder="Entre 5 y 100 caracteres" />
        @if (invalid('title')) { <div class="error-text">El título debe tener entre 5 y 100 caracteres.</div> }
      </div>

      <div class="field">
        <label>Descripción *</label>
        <textarea rows="3" formControlName="description" placeholder="Entre 10 y 500 caracteres"></textarea>
        @if (invalid('description')) { <div class="error-text">La descripción debe tener entre 10 y 500 caracteres.</div> }
      </div>

      <div class="grid grid-2">
        <div class="field">
          <label>Venue *</label>
          <select formControlName="venueId">
            <option value="">Selecciona un venue</option>
            @for (v of venues(); track v.id) {
              <option [value]="v.id">{{ v.name }} — cap. {{ v.capacity }} ({{ v.city }})</option>
            }
          </select>
          @if (invalid('venueId')) { <div class="error-text">Selecciona un venue.</div> }
        </div>

        <div class="field">
          <label>Capacidad máxima *</label>
          <input type="number" formControlName="capacity" min="1" />
          @if (invalid('capacity')) { <div class="error-text">Entero positivo, sin superar la capacidad del venue.</div> }
        </div>

        <div class="field">
          <label>Inicio *</label>
          <input type="datetime-local" formControlName="start" />
          @if (invalid('start')) { <div class="error-text">Fecha de inicio obligatoria y futura.</div> }
        </div>

        <div class="field">
          <label>Fin *</label>
          <input type="datetime-local" formControlName="end" />
          @if (invalid('end')) { <div class="error-text">Fecha de fin posterior al inicio.</div> }
        </div>

        <div class="field">
          <label>Precio de entrada (USD) *</label>
          <input type="number" formControlName="price" min="0.01" step="0.01" />
          @if (invalid('price')) { <div class="error-text">El precio debe ser un decimal positivo.</div> }
        </div>

        <div class="field">
          <label>Tipo de evento *</label>
          <select formControlName="type">
            <option value="conferencia">Conferencia</option>
            <option value="taller">Taller</option>
            <option value="concierto">Concierto</option>
          </select>
        </div>
      </div>

      <div class="actions" style="margin-top:8px">
        <button type="submit" class="btn btn-primary" [disabled]="submitting()">
          {{ submitting() ? 'Creando…' : 'Crear evento' }}
        </button>
        <a class="btn btn-ghost" routerLink="/events">Cancelar</a>
      </div>
    </form>
  `
})
export class EventFormComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly fb = inject(FormBuilder);
  private readonly router = inject(Router);
  private readonly notifications = inject(NotificationService);

  protected readonly venues = signal<Venue[]>([]);
  protected readonly submitting = signal(false);

  protected readonly form = this.fb.group({
    title: ['', [Validators.required, Validators.minLength(5), Validators.maxLength(100)]],
    description: ['', [Validators.required, Validators.minLength(10), Validators.maxLength(500)]],
    venueId: ['', [Validators.required]],
    capacity: [100, [Validators.required, Validators.min(1)]],
    start: ['', [Validators.required]],
    end: ['', [Validators.required]],
    price: [50, [Validators.required, Validators.min(0.01)]],
    type: ['conferencia', [Validators.required]]
  });

  ngOnInit(): void {
    this.api.getVenues().subscribe(v => this.venues.set(v));
  }

  protected invalid(control: string): boolean {
    const c = this.form.get(control);
    return !!c && c.invalid && (c.touched || c.dirty);
  }

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const raw = this.form.getRawValue();
    const request: CreateEventRequest = {
      title: raw.title!,
      description: raw.description!,
      venueId: Number(raw.venueId),
      capacity: Number(raw.capacity),
      startUtc: new Date(raw.start!).toISOString(),
      endUtc: new Date(raw.end!).toISOString(),
      price: Number(raw.price),
      type: raw.type as EventType
    };

    this.submitting.set(true);
    this.api.createEvent(request).subscribe({
      next: created => {
        this.notifications.success('Evento creado correctamente.');
        this.router.navigate(['/events', created.id]);
      },
      error: () => this.submitting.set(false)
    });
  }
}
