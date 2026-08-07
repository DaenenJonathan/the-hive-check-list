import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AuditLogDto } from '../models/audit-log.model';

@Injectable({ providedIn: 'root' })
export class AuditLogService {
  private readonly base = `${environment.apiUrl}/audit-logs`;

  constructor(private http: HttpClient) {}

  getByAction(actionId: string): Observable<AuditLogDto[]> {
    return this.http.get<AuditLogDto[]>(`${this.base}/action/${actionId}`);
  }
}
