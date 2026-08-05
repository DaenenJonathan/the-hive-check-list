import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { AddChecklistItemRequest, ChecklistDetailDto, ChecklistDto, ChecklistItemStatus, UpdateItemStatusRequest } from '../models/checklist.model';

@Injectable({ providedIn: 'root' })
export class ChecklistService {
  private readonly base = `${environment.apiUrl}/checklists`;
  private readonly itemsBase = `${environment.apiUrl}/checklist-items`;

  constructor(private http: HttpClient) {}

  getAll(brandActionId?: string): Observable<ChecklistDto[]> {
    const params: Record<string, string> = brandActionId ? { brandActionId } : {};
    return this.http.get<ChecklistDto[]>(this.base, { params });
  }

  getById(id: string): Observable<ChecklistDetailDto> {
    return this.http.get<ChecklistDetailDto>(`${this.base}/${id}`);
  }

  create(name: string, brandActionId: string, eventDate?: string | null): Observable<string> {
    return this.http.post<string>(this.base, { name, brandActionId, eventDate: eventDate ?? null });
  }

  update(id: string, name: string): Observable<void> {
    return this.http.put<void>(`${this.base}/${id}`, { id, name });
  }

  updateItemStatus(itemId: string, data: UpdateItemStatusRequest): Observable<void> {
    return this.http.patch<void>(`${this.itemsBase}/${itemId}/status`, data);
  }

  updateItemsStatusBatch(
    checklistId: string,
    items: { itemId: string; status: ChecklistItemStatus; quantityPrepared: number; remark: string | null }[]
  ): Observable<void> {
    return this.http.patch<void>(`${this.base}/${checklistId}/items/status`, items);
  }

  updateItemReturn(itemId: string, quantityReturned: number): Observable<void> {
    return this.http.patch<void>(`${this.itemsBase}/${itemId}/return`, { itemId, quantityReturned });
  }

  addItem(data: AddChecklistItemRequest): Observable<{ id: string }> {
    return this.http.post<{ id: string }>(this.itemsBase, data);
  }

  updateItemDetails(itemId: string, data: AddChecklistItemRequest): Observable<void> {
    return this.http.put<void>(`${this.itemsBase}/${itemId}`, { itemId, ...data });
  }

  deleteItem(itemId: string): Observable<void> {
    return this.http.delete<void>(`${this.itemsBase}/${itemId}`);
  }

  uploadItemImage(itemId: string, file: File): Observable<{ imagePath: string }> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<{ imagePath: string }>(`${this.itemsBase}/${itemId}/image`, formData);
  }

  reorderItems(checklistId: string, items: { itemId: string; sortOrder: number; category: string | null }[]): Observable<void> {
    return this.http.patch<void>(`${this.base}/${checklistId}/reorder`, items);
  }
}
