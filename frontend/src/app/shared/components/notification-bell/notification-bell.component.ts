import { Component, OnDestroy, OnInit } from '@angular/core';
import { Subscription, interval } from 'rxjs';
import { NotificationDto, NotificationType } from '../../../features/notifications/models/notification.model';
import { NotificationService } from '../../../features/notifications/services/notification.service';
import { AuthService } from '../../../core/auth/auth.service';

const POLL_INTERVAL_MS = 30000;

@Component({
  selector: 'app-notification-bell',
  templateUrl: './notification-bell.component.html',
  standalone: false
})
export class NotificationBellComponent implements OnInit, OnDestroy {
  NotificationType = NotificationType;
  notifications: NotificationDto[] = [];
  open = false;

  private pollSub?: Subscription;
  private authSub?: Subscription;

  constructor(private notificationService: NotificationService, public authService: AuthService) {}

  ngOnInit(): void {
    this.authSub = this.authService.currentUser$.subscribe(user => {
      if (user) {
        this.fetch();
        this.startPolling();
      } else {
        this.stopPolling();
        this.notifications = [];
        this.open = false;
      }
    });
  }

  ngOnDestroy(): void {
    this.authSub?.unsubscribe();
    this.stopPolling();
  }

  get unreadCount(): number {
    return this.notifications.filter(n => !n.isRead).length;
  }

  toggleOpen(): void {
    this.open = !this.open;
  }

  select(notification: NotificationDto): void {
    this.open = false;
    if (notification.isRead) return;
    notification.isRead = true;
    this.notificationService.markRead(notification.id).subscribe();
  }

  markAllRead(): void {
    this.notifications.forEach(n => n.isRead = true);
    this.notificationService.markAllRead().subscribe();
  }

  messageKey(type: NotificationType): string {
    switch (type) {
      case NotificationType.ItemsChangedOnAction: return 'NOTIFICATIONS.ITEMS_CHANGED';
      case NotificationType.ChecklistCompletedWithMissing: return 'NOTIFICATIONS.CHECKLIST_MISSING';
      case NotificationType.ActionCancelled: return 'NOTIFICATIONS.ACTION_CANCELLED';
      case NotificationType.ActionReactivated: return 'NOTIFICATIONS.ACTION_REACTIVATED';
      case NotificationType.AccountRequested: return 'NOTIFICATIONS.ACCOUNT_REQUESTED';
    }
  }

  notificationLink(notification: NotificationDto): string[] {
    return notification.type === NotificationType.AccountRequested
      ? ['/users']
      : ['/checklists', notification.checklistId!];
  }

  private startPolling(): void {
    if (this.pollSub) return;
    this.pollSub = interval(POLL_INTERVAL_MS).subscribe(() => this.fetch());
  }

  private stopPolling(): void {
    this.pollSub?.unsubscribe();
    this.pollSub = undefined;
  }

  private fetch(): void {
    this.notificationService.getMine().subscribe({ next: data => { this.notifications = data; } });
  }
}
