import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AgencyDto, CreateAgencyRequest, UpdateAgencyRequest } from '../models/agency.model';

@Injectable({ providedIn: 'root' })
export class AgencyService {
  private readonly base = `${environment.apiUrl}/agencies`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<AgencyDto[]> {
    return this.http.get<AgencyDto[]>(this.base);
  }

  create(request: CreateAgencyRequest): Observable<string> {
    return this.http.post<string>(this.base, request);
  }

  update(request: UpdateAgencyRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${request.id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
