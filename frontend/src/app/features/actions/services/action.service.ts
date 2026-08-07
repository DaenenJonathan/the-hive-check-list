import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ActionDto, ActionReturnsDto, CreateActionRequest, UpdateActionRequest } from '../models/action.model';

@Injectable({ providedIn: 'root' })
export class ActionService {
  private readonly base = `${environment.apiUrl}/actions`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ActionDto[]> {
    return this.http.get<ActionDto[]>(this.base);
  }

  create(request: CreateActionRequest): Observable<string> {
    return this.http.post<string>(this.base, request);
  }

  update(request: UpdateActionRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${request.id}`, request);
  }

  markAsSent(actionId: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${actionId}/send`, {});
  }

  getReturns(actionId: string): Observable<ActionReturnsDto> {
    return this.http.get<ActionReturnsDto>(`${this.base}/${actionId}/returns`);
  }

  validateReturns(actionId: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${actionId}/returns/validate`, {});
  }

  cancelAction(actionId: string): Observable<void> {
    return this.http.post<void>(`${this.base}/${actionId}/cancel`, {});
  }

  deleteAction(actionId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${actionId}`);
  }
}
