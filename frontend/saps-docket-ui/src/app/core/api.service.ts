import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class ApiService {

  constructor(private http: HttpClient) {}

  get<T>(path: string) {
    return this.http.get<T>(
      `${environment.apiUrl}${path}`
    );
  }

  post<T>(path: string, body: unknown) {
    return this.http.post<T>(
      `${environment.apiUrl}${path}`,
      body
    );
  }

  patch<T>(path: string, body: unknown = {}) {
    return this.http.patch<T>(
      `${environment.apiUrl}${path}`,
      body
    );
  }

}