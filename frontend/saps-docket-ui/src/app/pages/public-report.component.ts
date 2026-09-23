import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../core/api.service';
@Component({ standalone: true, imports: [CommonModule, FormsModule, RouterLink], template: `
<div class="public-page"><a routerLink="/" class="back">← Public portal</a><div class="form-card"><h1>Report an Incident</h1><p class="muted">This creates a reporting reference. Police review is still required before an official criminal case number is issued.</p>
<div class="grid two"><label>First name<input [(ngModel)]="m.citizen.firstName"></label><label>Surname<input [(ngModel)]="m.citizen.surname"></label><label>ID / Passport (optional)<input [(ngModel)]="m.citizen.idNumber"></label><label>Phone number<input [(ngModel)]="m.citizen.phoneNumber" placeholder="0821234567"></label><label>Email (optional)<input [(ngModel)]="m.citizen.email"></label><label>Police station<select [(ngModel)]="m.policeStationId"><option [ngValue]="0">Select station</option><option *ngFor="let s of stations" [ngValue]="s.id">{{s.name}}</option></select></label><label>Crime category<input [(ngModel)]="m.crimeCategory" placeholder="e.g. Theft"></label><label>Incident date<input type="datetime-local" [(ngModel)]="m.incidentDate"></label></div>
<label>Incident location<input [(ngModel)]="m.incidentLocation"></label><label>What happened?<textarea rows="6" [(ngModel)]="m.incidentSummary"></textarea></label>
<div class="notice" *ngIf="result">Reference: <b>{{result.reference}}</b><br>{{result.message}}<br>SMS: {{result.smsStatus}}</div><div class="error" *ngIf="error">{{error}}</div><button class="btn primary" (click)="submit()" [disabled]="busy">{{busy?'Submitting...':'Submit Report'}}</button></div></div>` })
export class PublicReportComponent implements OnInit {
  stations:any[]=[]; result:any; error=''; busy=false;
  m:any={citizen:{firstName:'',surname:'',idNumber:'',phoneNumber:'',email:''},policeStationId:0,crimeCategory:'',incidentSummary:'',incidentDate:'',incidentLocation:''};
  constructor(private api:ApiService){} ngOnInit(){this.api.get<any[]>('/public/stations').subscribe(x=>this.stations=x)}
  submit(){this.error='';this.busy=true;this.api.post<any>('/public/reports',this.m).subscribe({next:x=>{this.result=x;this.busy=false},error:e=>{this.error=e.error?.message||'Could not submit report';this.busy=false}})}
}
