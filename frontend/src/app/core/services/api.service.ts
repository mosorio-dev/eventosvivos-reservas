import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CreateEventRequest,
  CreateReservationRequest,
  EventDetail,
  EventFilters,
  EventSummary,
  OccupancyReport,
  Reservation,
  Venue
} from '../models/models';

@Injectable({ providedIn: 'root' })
export class ApiService {
  private readonly http = inject(HttpClient);
  private readonly base = environment.apiBaseUrl;

  getVenues(): Observable<Venue[]> {
    return this.http.get<Venue[]>(`${this.base}/venues`);
  }

  getEvents(filters: EventFilters = {}): Observable<EventSummary[]> {
    let params = new HttpParams();
    if (filters.type) params = params.set('type', filters.type);
    if (filters.venueId != null) params = params.set('venueId', filters.venueId);
    if (filters.status) params = params.set('status', filters.status);
    if (filters.title) params = params.set('title', filters.title);
    if (filters.startFromUtc) params = params.set('startFromUtc', filters.startFromUtc);
    if (filters.startToUtc) params = params.set('startToUtc', filters.startToUtc);
    return this.http.get<EventSummary[]>(`${this.base}/events`, { params });
  }

  getEvent(id: string): Observable<EventDetail> {
    return this.http.get<EventDetail>(`${this.base}/events/${id}`);
  }

  createEvent(request: CreateEventRequest): Observable<EventDetail> {
    return this.http.post<EventDetail>(`${this.base}/events`, request);
  }

  getReport(eventId: string): Observable<OccupancyReport> {
    return this.http.get<OccupancyReport>(`${this.base}/events/${eventId}/report`);
  }

  getReservations(eventId: string): Observable<Reservation[]> {
    return this.http.get<Reservation[]>(`${this.base}/events/${eventId}/reservations`);
  }

  createReservation(request: CreateReservationRequest): Observable<Reservation> {
    return this.http.post<Reservation>(`${this.base}/reservations`, request);
  }

  confirmReservation(id: string): Observable<Reservation> {
    return this.http.post<Reservation>(`${this.base}/reservations/${id}/confirm`, {});
  }

  cancelReservation(id: string): Observable<Reservation> {
    return this.http.post<Reservation>(`${this.base}/reservations/${id}/cancel`, {});
  }
}
