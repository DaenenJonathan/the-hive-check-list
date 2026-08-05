import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';

export interface ImportPreview {
  fileName: string;
  client: string;
  brand: string;
  projectName: string;
  actionType: string;
  eventDate: string | null;
  account: string | null;
  costCode: string | null;
  addressPickUp: string | null;
  addressAction: string | null;
  suggestedChecklistName: string;
  items: ImportPreviewItem[];
  errors: string[];
  isValid: boolean;
}

export interface ImportPreviewItem {
  materialName: string;
  quantityRequested: number;
  category: string | null;
  notes: string | null;
  sortOrder: number;
}

export interface ImportResult {
  brandActionId: string;
  checklistId: string;
}

@Injectable({ providedIn: 'root' })
export class ImportService {
  private readonly base = `${environment.apiUrl}/imports`;

  constructor(private http: HttpClient) {}

  preview(file: File): Observable<ImportPreview> {
    const formData = new FormData();
    formData.append('file', file);
    return this.http.post<ImportPreview>(`${this.base}/preview`, formData);
  }

  confirm(file: File, brandActionId?: string): Observable<ImportResult> {
    const formData = new FormData();
    formData.append('file', file);
    if (brandActionId) formData.append('brandActionId', brandActionId);
    return this.http.post<ImportResult>(`${this.base}/confirm`, formData);
  }
}
