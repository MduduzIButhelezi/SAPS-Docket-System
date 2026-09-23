import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../core/auth.service';

@Component({
  standalone: true,
  imports: [FormsModule, CommonModule, RouterLink],
  template: `
    <div class="login-page">
      <div class="login-card">

        <a routerLink="/" class="back">
          ← Public portal
        </a>

        <div class="brand-mark large">
          DP
        </div>

        <h1>Police Staff Login</h1>

        <p class="muted">
          Use your internal staff credentials.
        </p>

        <label>
          Email
          <input
            type="email"
            [(ngModel)]="email"
          />
        </label>

        <label>
          Password
          <input
            type="password"
            [(ngModel)]="password"
            (keyup.enter)="login()"
          />
        </label>

        <div
          class="error"
          *ngIf="error">
          {{ error }}
        </div>

        <button
          class="btn primary full"
          (click)="login()">
          Sign in
        </button>

        <small class="demo">
          Demo: admin&#64;saps.local / Admin123!
        </small>

      </div>
    </div>
  `
})
export class LoginComponent {

  email = 'admin@saps.local';
  password = 'Admin123!';
  error = '';

  constructor(
    private auth: AuthService,
    private router: Router
  ) {}

  login(): void {
    this.error = '';

    this.auth
      .login(this.email, this.password)
      .subscribe({
        next: () => {
          this.router.navigate(['/app/dashboard']);
        },
        error: (e) => {
          this.error =
            e.error?.message ||
            'Login failed';
        }
      });
  }
}