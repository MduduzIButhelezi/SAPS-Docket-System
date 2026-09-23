import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
@Component({ standalone: true, imports: [RouterLink], template: `
<div class="public-shell">
  <header class="public-head"><div class="brand-mark">DP</div><div><b>Digital Police Case Management</b><small> Group 8 Project Prototype</small></div><a routerLink="/login" class="btn ghost">Police Login</a></header>
  <main class="hero"><div class="hero-copy"><span class="eyebrow">ACCOUNTABILITY • TRACEABILITY • SERVICE</span><h1>Report, track and follow up without losing the trail.</h1><p>This prototype gives citizens a direct reporting channel and gives police staff a complete digital audit trail from registration to resolution.</p></div>
  <div class="action-grid">
    <a routerLink="/track-case" class="action-card"><span>01</span><h3>Track My Case</h3><p>Check public-safe case progress using your reference and phone number.</p></a>
    <a routerLink="/lodge-complaint" class="action-card warning"><span>02</span><h3>Lodge a Complaint</h3><p>Complain about refusal, poor service, a missing docket or police conduct.</p></a>
    <a routerLink="/track-complaint" class="action-card"><span>03</span><h3>Track Complaint</h3><p>Follow the complaint independently of the officer involved.</p></a>
  </div></main>
</div>` })
export class PublicHomeComponent {}
