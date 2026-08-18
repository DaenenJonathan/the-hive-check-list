import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { BrandDto, CreateBrandRequest, UpdateBrandRequest } from '../models/brand.model';

@Injectable({ providedIn: 'root' })
export class BrandService {
  private readonly base = `${environment.apiUrl}/brands`;

  constructor(private http: HttpClient) {}

  getAll(agencyId?: string | null): Observable<BrandDto[]> {
    let params = new HttpParams();
    if (agencyId) params = params.set('agencyId', agencyId);
    return this.http.get<BrandDto[]>(this.base, { params });
  }

  create(request: CreateBrandRequest): Observable<string> {
    return this.http.post<string>(this.base, request);
  }

  update(request: UpdateBrandRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${request.id}`, request);
  }

  delete(id: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
