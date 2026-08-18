export enum NotificationType {
  ItemsChangedOnAction = 0,
  ChecklistCompletedWithMissing = 1,
  ActionCancelled = 2,
  ActionReactivated = 3,
  AccountRequested = 4
}

export interface NotificationDto {
  id: string;
  type: NotificationType;
  brandActionId: string | null;
  checklistId: string | null;
  actionName: string | null;
  checklistName: string | null;
  requesterName: string | null;
  requesterEmail: string | null;
  message: string | null;
  isRead: boolean;
  createdAt: string;
}
