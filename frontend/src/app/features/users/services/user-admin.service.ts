import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CreateUserRequest, CreateUserResult, UpdateUserRoleRequest, UserAdminDto } from '../models/user-admin.model';

@Injectable({ providedIn: 'root' })
export class UserAdminService {
  private readonly base = `${environment.apiUrl}/users`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<UserAdminDto[]> {
    return this.http.get<UserAdminDto[]>(this.base);
  }

  updateRole(userId: string, request: UpdateUserRoleRequest): Observable<void> {
    return this.http.put<void>(`${this.base}/${userId}/role`, request);
  }

  create(request: CreateUserRequest): Observable<CreateUserResult> {
    return this.http.post<CreateUserResult>(this.base, request);
  }

  resetPassword(userId: string): Observable<{ temporaryPassword: string }> {
    return this.http.post<{ temporaryPassword: string }>(`${this.base}/${userId}/reset-password`, {});
  }

  deleteUser(userId: string): Observable<void> {
    return this.http.delete<void>(`${this.base}/${userId}`);
  }
}
