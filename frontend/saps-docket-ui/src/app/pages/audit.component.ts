import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService } from '../core/api.service';
@Component({ standalone:true, imports:[CommonModule], template:`<div class="page"><div class="page-head"><div><span class="eyebrow">IMMUTABLE ACTIVITY HISTORY</span><h1>Audit Trail</h1></div></div><div class="panel"><div class="table-wrap"><table><thead><tr><th>Date</th><th>Actor</th><th>Action</th><th>Entity</th><th>ID</th><th>Details</th></tr></thead><tbody><tr *ngFor="let a of rows"><td>{{a.dateTime|date:'medium'}}</td><td>{{a.actor}}</td><td>{{a.action}}</td><td>{{a.entityType}}</td><td>{{a.entityId}}</td><td>{{a.details}}</td></tr></tbody></table></div></div></div>` })
export class AuditComponent implements OnInit {rows:any[]=[];constructor(private api:ApiService){}ngOnInit(){this.api.get<any[]>('/system/audit').subscribe(x=>this.rows=x)}}
