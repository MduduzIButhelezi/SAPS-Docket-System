
import { Routes } from '@angular/router';

import { authGuard } from './core/auth.guard';

import { PublicHomeComponent } from './pages/public-home.component';
import { PublicReportComponent } from './pages/public-report.component';
import { TrackCaseComponent } from './pages/track-case.component';
import { LodgeComplaintComponent } from './pages/lodge-complaint.component';
import { TrackComplaintComponent } from './pages/track-complaint.component';
import { LoginComponent } from './pages/login.component';

import { ShellComponent } from './layout/shell.component';

import { DashboardComponent } from './pages/dashboard.component';
import { AdminComponent } from './pages/admin.component';
import { ReportingComponent } from './pages/reporting.component';
import { CasesComponent } from './pages/cases.component';
import { CaseDetailsComponent } from './pages/case-details.component';
import { AssignmentComponent } from './pages/assignment.component';
import { TasksComponent } from './pages/tasks.component';
import { ClosuresComponent } from './pages/closures.component';
import { EvidenceComponent } from './pages/evidence.component';
import { ComplaintsComponent } from './pages/complaints.component';
import { AuditComponent } from './pages/audit.component';
import { LodgeCaseComponent } from './pages/lodge-case.component';

export const routes: Routes = [

  // ============================================================
  // PUBLIC ROUTES
  // ============================================================

  {
    path: '',
    component: PublicHomeComponent
  },

  {
    path: 'report',
    component: PublicReportComponent
  },

  {
    path: 'track-case',
    component: TrackCaseComponent
  },

  {
    path: 'lodge-complaint',
    component: LodgeComplaintComponent
  },

  {
    path: 'track-complaint',
    component: TrackComplaintComponent
  },

  {
    path: 'login',
    component: LoginComponent
  },

  // ============================================================
  // AUTHENTICATED APPLICATION
  // ============================================================

  {
    path: 'app',
    component: ShellComponent,
    canActivate: [authGuard],

    children: [

      // Dashboard
      {
        path: 'dashboard',
        component: DashboardComponent
      },

      // Administration
      {
        path: 'admin',
        component: AdminComponent
      },

      // Reporting
      {
        path: 'reporting',
        component: ReportingComponent
      },

      // Cases
      {
        path: 'cases',
        component: CasesComponent
      },

      // Case intake form
      {
        path: 'lodge-case',
        component: LodgeCaseComponent
      },

      // Case details
      {
        path: 'cases/:id',
        component: CaseDetailsComponent
      },

      // Assignments
      {
        path: 'assignments',
        component: AssignmentComponent
      },

      // Investigation tasks
      {
        path: 'tasks',
        component: TasksComponent
      },

      // Case closures
      {
        path: 'closures',
        component: ClosuresComponent
      },

      // Evidence
      {
        path: 'evidence',
        component: EvidenceComponent
      },

      // Complaints
      {
        path: 'complaints',
        component: ComplaintsComponent
      },

      // Audit trail
      {
        path: 'audit',
        component: AuditComponent
      },

      // /app → /app/dashboard
      {
        path: '',
        pathMatch: 'full',
        redirectTo: 'dashboard'
      }
    ]
  },

  // ============================================================
  // UNKNOWN ROUTES
  // ============================================================

  {
    path: '**',
    redirectTo: ''
  }
];

