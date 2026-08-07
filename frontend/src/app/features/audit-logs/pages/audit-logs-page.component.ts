import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AuditLogDto } from '../models/audit-log.model';
import { AuditLogService } from '../services/audit-log.service';

@Component({
  selector: 'app-audit-logs-page',
  templateUrl: './audit-logs-page.component.html',
  standalone: false
})
export class AuditLogsPageComponent implements OnInit {
  logs: AuditLogDto[] = [];
  loading = false;
  actionName: string | null = null;

  constructor(private route: ActivatedRoute, private auditLogService: AuditLogService) {}

  ngOnInit(): void {
    const actionId = this.route.snapshot.paramMap.get('actionId')!;
    this.actionName = this.route.snapshot.queryParamMap.get('actionName');
    this.load(actionId);
  }

  load(actionId: string): void {
    this.loading = true;
    this.auditLogService.getByAction(actionId).subscribe({
      next: data => { this.logs = data; this.loading = false; },
      error: () => { this.loading = false; }
    });
  }
}
