import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

interface AuditLogDto {
  id: string;
  userName: string;
  action: string;
  entityType: string;
  entityId: string;
  oldValue: string | null;
  newValue: string | null;
  occurredAt: string;
}

@Component({
  selector: 'app-audit-logs-page',
  templateUrl: './audit-logs-page.component.html',
  standalone: false
})
export class AuditLogsPageComponent implements OnInit {
  logs: AuditLogDto[] = [];
  loading = false;

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.http.get<AuditLogDto[]>(`${environment.apiUrl}/audit-logs`).subscribe({
      next: data => { this.logs = data; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }
}
