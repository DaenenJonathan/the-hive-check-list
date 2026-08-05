export enum ActionStatus {
  Planned = 0, InProgress = 1, Completed = 2, Cancelled = 3
}

export interface ActionDto {
  id: string;
  name: string;
  client: string;
  plannedDate: string;
  plannedDepartureTime: string | null;
  plannedReturnTime: string | null;
  status: ActionStatus;
  description: string | null;
  address: string | null;
  city: string | null;
  createdAt: string;
  checklistCount: number;
  totalItems: number;
  preparedItems: number;
  sent: boolean;
  sentAt: string | null;
  isReadyToSend: boolean;
  returnValidated: boolean;
  returnValidatedAt: string | null;
}

export interface CreateActionRequest {
  name: string;
  client: string;
  plannedDate: string;
  description: string | null;
  plannedDepartureTime: string | null;
  plannedReturnTime: string | null;
}

export interface UpdateActionRequest extends CreateActionRequest {
  id: string;
}

export interface ActionReturnItemDto {
  itemId: string;
  checklistId: string;
  checklistName: string;
  materialName: string;
  category: string | null;
  location: string | null;
  quantityRequested: number;
  quantityPrepared: number;
  quantityReturned: number | null;
}

export interface ActionReturnsDto {
  actionId: string;
  sent: boolean;
  sentAt: string | null;
  returnValidated: boolean;
  returnValidatedAt: string | null;
  items: ActionReturnItemDto[];
}
