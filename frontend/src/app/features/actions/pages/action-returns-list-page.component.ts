import { Component, OnInit } from '@angular/core';
import { ActionDto } from '../models/action.model';
import { ActionService } from '../services/action.service';

@Component({
  selector: 'app-action-returns-list-page',
  templateUrl: './action-returns-list-page.component.html',
  standalone: false
})
export class ActionReturnsListPageComponent implements OnInit {
  actions: ActionDto[] = [];
  loading = false;

  constructor(private actionService: ActionService) {}

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
}
