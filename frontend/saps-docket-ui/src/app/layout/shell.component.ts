import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../core/auth.service';
@Component({ standalone:true, imports:[CommonModule,RouterOutlet,RouterLink,RouterLinkActive], template:`
<div class="app-shell"><aside class="sidebar"><div class="side-brand"><div class="brand-mark">DP</div><div><b>Docket System</b><small>Police Portal</small></div></div><nav>
<a routerLink="/app/dashboard" routerLinkActive="active">Dashboard</a>
<a routerLink="/app/lodge-case" routerLinkActive="active">Open Official Case</a>
<a routerLink="/app/cases" routerLinkActive="active">Cases</a>
<a routerLink="/app/assignments" routerLinkActive="active" *ngIf="show('DetectiveCommander','StationCommander','NationalAdministrator','ProvincialAdministrator')">Assignment Queue</a>
<a routerLink="/app/closures" routerLinkActive="active" *ngIf="show('DetectiveCommander','StationCommander')">Closure Reviews</a>
<a routerLink="/app/tasks" routerLinkActive="active" *ngIf="show('InvestigatingOfficer','DetectiveCommander','StationCommander')">My Tasks</a>
<a routerLink="/app/evidence" routerLinkActive="active" *ngIf="show('EvidenceOfficer','InvestigatingOfficer','DetectiveCommander','StationCommander')">Evidence</a>
<a routerLink="/app/complaints" routerLinkActive="active" *ngIf="show('ComplaintsOfficer','StationCommander','ProvincialAdministrator','NationalAdministrator','Auditor')">Complaints</a>
<a routerLink="/app/admin" routerLinkActive="active" *ngIf="show('NationalAdministrator','ProvincialAdministrator','StationAdministrator','StationCommander')">Administration</a>
<a routerLink="/app/audit" routerLinkActive="active" *ngIf="show('NationalAdministrator','ProvincialAdministrator','StationAdministrator','StationCommander','Auditor')">Audit Trail</a>
</nav><div class="side-user"><small>{{auth.session?.role}}</small><b>{{auth.session?.fullName}}</b><button class="btn ghost full" (click)="auth.logout()">Sign out</button></div></aside><main class="workspace"><router-outlet/></main></div>` })
export class ShellComponent { constructor(public auth:AuthService){} show(...roles:string[]){return this.auth.hasAnyRole(...roles)} }
