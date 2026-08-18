import { Component, OnInit } from '@angular/core';
import { ActionDto } from '../models/action.model';
import { ActionService } from '../services/action.service';
import { AuthService } from '../../../core/auth/auth.service';

@Component({
  selector: 'app-action-returns-list-page',
  templateUrl: './action-returns-list-page.component.html',
  standalone: false
})
export class ActionReturnsListPageComponent implements OnInit {
  actions: ActionDto[] = [];
  loading = false;
  confirmingId: string | null = null;

  constructor(private actionService: ActionService, public authService: AuthService) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.actionService.getAll().subscribe({
      next: data => {
        this.actions = data
          .filter(a => a.sent && !a.returnValidated)
          .sort((a, b) => new Date(a.plannedDate).getTime() - new Date(b.plannedDate).getTime());
        this.loading = false;
      },
      error: () => { this.loading = false; }
    });
  }

  confirmReturn(action: ActionDto): void {
    this.confirmingId = action.id;
    this.actionService.validateReturns(action.id).subscribe({
      next: () => { this.confirmingId = null; this.load(); },
      error: () => { this.confirmingId = null; }
    });
  }
}
