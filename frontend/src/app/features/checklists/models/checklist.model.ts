export enum ChecklistStatus {
  Draft = 0, Active = 1, InProgress = 2, Completed = 3, Archived = 4
}

export enum BrandActionStatus {
  Planned = 0, InProgress = 1, Completed = 2, Cancelled = 3
}

export enum ChecklistItemStatus {
  ToPrepare = 0, Prepared = 1, Missing = 2, PartiallyPrepared = 3,
  Loaded = 4, Cancelled = 5, Replaced = 6
}

export interface ChecklistDto {
  id: string;
  name: string;
  version: number;
  status: ChecklistStatus;
  importedAt: string | null;
  sourceFileName: string | null;
  eventDate: string | null;
  brandActionId: string;
  brandActionName: string | null;
  brandActionAddress: string | null;
  brandActionCity: string | null;
  brandActionStatus: BrandActionStatus;
  brandActionPlannedDepartureTime: string | null;
  brandActionPlannedReturnTime: string | null;
  createdAt: string;
  totalItems: number;
  preparedItems: number;
}

export interface ChecklistDetailDto extends ChecklistDto {
  items: ChecklistItemDto[];
}

export interface ChecklistItemDto {
  id: string;
  materialName: string;
  quantityRequested: number;
  quantityPrepared: number;
  quantityReturned: number | null;
  location: string | null;
  notes: string | null;
  imagePath: string | null;
  remark: string | null;
  status: ChecklistItemStatus;
  category: string | null;
  sortOrder: number;
  updatedAt: string | null;
  updatedBy: string | null;
}

export interface UpdateItemStatusRequest {
  itemId: string;
  status: ChecklistItemStatus;
  quantityPrepared: number;
  remark: string | null;
}

export interface AddChecklistItemRequest {
  checklistId: string;
  materialName: string;
  quantityRequested: number;
  category: string | null;
  notes: string | null;
  location: string | null;
}
