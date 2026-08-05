import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from '../auth/auth.service';

export interface ItemUpdatedEvent {
  checklistId: string;
  itemId: string;
}

@Injectable({ providedIn: 'root' })
export class ChecklistRealtimeService {
  private connection: signalR.HubConnection | null = null;

  itemUpdated$ = new Subject<ItemUpdatedEvent>();
  checklistUpdated$ = new Subject<{ checklistId: string }>();

  constructor(private authService: AuthService) {}

  async joinChecklist(checklistId: string): Promise<void> {
    if (!this.connection) {
      this.connection = new signalR.HubConnectionBuilder()
        .withUrl(`${environment.hubUrl}/hubs/checklists`, {
          accessTokenFactory: () => this.authService.getToken() ?? ''
        })
        .withAutomaticReconnect()
        .build();

      this.connection.on('ItemUpdated', (event: ItemUpdatedEvent) => this.itemUpdated$.next(event));
      this.connection.on('ChecklistUpdated', (event: { checklistId: string }) => this.checklistUpdated$.next(event));

      await this.connection.start();
    }
    await this.connection.invoke('JoinChecklistGroup', checklistId);
  }

  async leaveChecklist(checklistId: string): Promise<void> {
    await this.connection?.invoke('LeaveChecklistGroup', checklistId);
  }

  async disconnect(): Promise<void> {
    await this.connection?.stop();
    this.connection = null;
  }
}
