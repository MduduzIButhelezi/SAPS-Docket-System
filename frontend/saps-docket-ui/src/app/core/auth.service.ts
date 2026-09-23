import { Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, tap } from 'rxjs';
import { ApiService } from './api.service';

export interface Session { token: string; userId: number; fullName: string; role: string; policeStationId: number | null; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private key = 'saps_session';
  private subject = new BehaviorSubject<Session | null>(this.read());
  session$ = this.subject.asObservable();
  constructor(private api: ApiService, private router: Router) {}
  get session() { return this.subject.value; }
  get token() { return this.session?.token ?? null; }
  get role() { return this.session?.role ?? ''; }
  isLoggedIn() { return !!this.token; }
  login(email: string, password: string) {
    return this.api.post<Session>('/auth/login', { email, password }).pipe(tap(s => { localStorage.setItem(this.key, JSON.stringify(s)); this.subject.next(s); }));
  }
  logout() { localStorage.removeItem(this.key); this.subject.next(null); this.router.navigate(['/']); }
  hasAnyRole(...roles: string[]) { return roles.includes(this.role); }
  private read(): Session | null { try { return JSON.parse(localStorage.getItem(this.key) || 'null'); } catch { return null; } }
}
