export enum NotificationType {
  ItemsChangedOnAction = 0,
  ChecklistCompletedWithMissing = 1,
  ActionCancelled = 2,
  ActionReactivated = 3
}

export interface NotificationDto {
  id: string;
  type: NotificationType;
  brandActionId: string;
  checklistId: string;
  actionName: string;
  checklistName: string;
  isRead: boolean;
  createdAt: string;
}
