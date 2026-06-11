export type EventType = 'conferencia' | 'taller' | 'concierto';
export type EventStatus = 'activo' | 'cancelado' | 'completado';
export type ReservationStatus = 'pendientePago' | 'confirmada' | 'cancelada' | 'perdida';

export interface Venue {
  id: number;
  name: string;
  capacity: number;
  city: string;
}

export interface EventSummary {
  id: string;
  title: string;
  venueId: number;
  venueName: string | null;
  capacity: number;
  startUtc: string;
  endUtc: string;
  price: number;
  type: EventType;
  status: EventStatus;
}

export interface EventDetail extends EventSummary {
  description: string;
  createdAtUtc: string;
}

export interface CreateEventRequest {
  title: string;
  description: string;
  venueId: number;
  capacity: number;
  startUtc: string;
  endUtc: string;
  price: number;
  type: EventType;
}

export interface Reservation {
  id: string;
  eventId: string;
  quantity: number;
  buyerName: string;
  buyerEmail: string;
  status: ReservationStatus;
  reservationCode: string | null;
  createdAtUtc: string;
  confirmedAtUtc: string | null;
  cancelledAtUtc: string | null;
}

export interface CreateReservationRequest {
  eventId: string;
  quantity: number;
  buyerName: string;
  buyerEmail: string;
}

export interface OccupancyReport {
  eventId: string;
  eventTitle: string;
  capacity: number;
  ticketsSold: number;
  ticketsPending: number;
  ticketsLost: number;
  ticketsAvailable: number;
  occupancyPercentage: number;
  totalRevenue: number;
  status: EventStatus;
}

export interface EventFilters {
  type?: EventType;
  venueId?: number;
  status?: EventStatus;
  title?: string;
  startFromUtc?: string;
  startToUtc?: string;
}

export interface AuthResult {
  token: string;
  expiresAtUtc: string;
  username: string;
  role: string;
}
