import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { ApiService } from '../core/api.service';
import { TranslationService } from '../core/translation.service';

interface PoliceStation {
  id: number;
  code: string;
  name: string;
  province: string;
  district: string;
}

@Component({
  standalone: true,
  selector: 'app-lodge-case',
  imports: [
    CommonModule,
    FormsModule,
    RouterLink
  ],
  template: `
    <div class="page">

      <!-- HEADER -->
      <div class="page-head">

        <div>
          <span class="eyebrow">CASE INTAKE</span>

          <h1>Lodge Case</h1>

          <p class="muted">
            Capture the incident reported by the complainant.
          </p>
        </div>

        <a
          routerLink="/app/cases"
          class="btn ghost">
          Back to Cases
        </a>

      </div>


      <!-- FORM -->
      <div class="panel">

        <!-- POLICE STATION -->
        <h3>Police Station</h3>

        <label>
          Police Station

          <select
            [(ngModel)]="form.policeStationId"
            name="policeStationId">

            <option [ngValue]="null">
              Select police station
            </option>

            <option
              *ngFor="let station of stations"
              [ngValue]="station.id">

              {{ station.name }}
              ({{ station.code }})

            </option>

          </select>

        </label>


        <hr>


        <!-- COMPLAINANT -->
        <h3>Complainant Details</h3>

        <div class="grid two">

          <label>
            First Name

            <input
              type="text"
              [(ngModel)]="form.firstName"
              name="firstName"
              placeholder="Enter first name">
          </label>


          <label>
            Surname

            <input
              type="text"
              [(ngModel)]="form.surname"
              name="surname"
              placeholder="Enter surname">
          </label>

        </div>


        <div class="grid two">

          <label>
            ID Number

            <input
              type="text"
              [(ngModel)]="form.idNumber"
              name="idNumber"
              placeholder="Enter ID number">
          </label>


          <label>
            Phone Number

            <input
              type="tel"
              [(ngModel)]="form.phoneNumber"
              name="phoneNumber"
              placeholder="Enter phone number">
          </label>

        </div>


        <label>
          Email Address

          <input
            type="email"
            [(ngModel)]="form.email"
            name="email"
            placeholder="Enter email address">
        </label>


        <hr>


        <!-- INCIDENT -->
        <h3>Incident Details</h3>

        <div class="grid two">

          <label>
            Crime Category

            <select
              [(ngModel)]="form.crimeCategory"
              name="crimeCategory">

              <option value="">
                Select crime category
              </option>

              <option value="Theft">
                Theft
              </option>

              <option value="Robbery">
                Robbery
              </option>

              <option value="Assault">
                Assault
              </option>

              <option value="Fraud">
                Fraud
              </option>

              <option value="Housebreaking">
                Housebreaking
              </option>

              <option value="Damage to Property">
                Damage to Property
              </option>

              <option value="Missing Person">
                Missing Person
              </option>

              <option value="Murder">
                Murder
              </option>

              <option value="Sexual Offence">
                Sexual Offence
              </option>

              <option value="Other">
                Other
              </option>
            </select>

          </label>


          <label>
            Priority

            <select
              [(ngModel)]="form.priority"
              name="priority">
              <option value="Low">Low</option>
              <option value="Medium">Medium</option>
              <option value="High">High</option>
              <option value="Critical">Critical</option>
            </select>
          </label>


          <label>
            Incident Date

            <input
              type="datetime-local"
              [(ngModel)]="form.incidentDate"
              name="incidentDate">

          </label>

        </div>


        <label>
          Incident Location

          <input
            type="text"
            [(ngModel)]="form.incidentLocation"
            name="incidentLocation"
            placeholder="Where did the incident happen?">

        </label>


        <label>
          Incident Description

          <textarea
            rows="8"
            [(ngModel)]="form.incidentSummary"
            name="incidentSummary"
            placeholder="Record the complainant's statement in the original language...">
          </textarea>

        </label>

        <div class="grid two">
          <label>
            Statement Language
            <select [(ngModel)]="form.originalLanguage" name="originalLanguage">
              <option value="">Select statement language</option>
              <option *ngFor="let language of statementLanguages" [value]="language.code">{{ language.name }}</option>
            </select>
          </label>
          <div class="translation-note">
            <small>Machine translation — review before saving.</small>
          </div>
        </div>

        <button type="button" class="btn secondary" [disabled]="translating || !form.originalLanguage || !form.incidentSummary" (click)="translateStatement()">
          {{ translating ? 'Translating...' : 'Translate to English' }}
        </button>

        <label>
          English Translation
          <textarea rows="6" [(ngModel)]="form.englishTranslation" name="englishTranslation" placeholder="The English translation will appear here. You may edit it before saving."></textarea>
        </label>

        <div class="notice" *ngIf="translationMessage">{{ translationMessage }}</div>


        <!-- ERROR -->
        <div
          class="error"
          *ngIf="error">

          {{ error }}

        </div>


        <!-- ACTIONS -->
        <div class="form-actions">

          <a
            routerLink="/app/cases"
            class="btn ghost">

            Cancel

          </a>


          <button
            type="button"
            class="btn primary"
            [disabled]="saving"
            (click)="save()">

            {{ saving ? 'Opening...' : 'Open Official Case' }}

          </button>

        </div>

      </div>

    </div>
  `
})
export class LodgeCaseComponent implements OnInit {

  stations: PoliceStation[] = [];

  saving = false;

  error = '';

  form = {

    policeStationId: null as number | null,

    firstName: '',
    surname: '',
    idNumber: '',
    phoneNumber: '',
    email: '',

    crimeCategory: '',
    priority: 'Medium',
    incidentDate: '',
    incidentLocation: '',
    incidentSummary: '',
    originalLanguage: '',
    englishTranslation: '',
    translationProvider: '',
    translationStatus: 'NotTranslated'

  };

  statementLanguages = [
    { code: 'en', name: 'English' },
    { code: 'zu', name: 'isiZulu' },
    { code: 'xh', name: 'isiXhosa' },
    { code: 'af', name: 'Afrikaans' },
    { code: 'st', name: 'Sesotho' },
    { code: 'tn', name: 'Setswana' },
    { code: 'nso', name: 'Sepedi' },
    { code: 'ss', name: 'siSwati' },
    { code: 've', name: 'Tshivenda' },
    { code: 'ts', name: 'Tsonga' },
    { code: 'nr', name: 'isiNdebele' }
  ];
  translating = false;
  translationMessage = '';


  constructor(
    private api: ApiService,
    private translation: TranslationService
  ) {}


  ngOnInit(): void {

    this.loadStations();

  }


  loadStations(): void {

    this.api
      .get<PoliceStation[]>('/public/stations')
      .subscribe({

        next: (stations) => {

          this.stations = stations;

        },

        error: (error) => {

          console.error(
            'Unable to load police stations',
            error
          );

          this.error =
            'Unable to load police stations.';

        }

      });

  }


  save(): void {

    this.error = '';


    // Validate police station
    if (!this.form.policeStationId) {

      this.error =
        'Please select a police station.';

      return;

    }


    // Validate complainant
    if (
      !this.form.firstName ||
      !this.form.surname ||
      !this.form.idNumber ||
      !this.form.phoneNumber
    ) {

      this.error =
        'Please complete the complainant details.';

      return;

    }


    // Validate incident
    if (
      !this.form.crimeCategory ||
      !this.form.originalLanguage ||
      !this.form.incidentDate ||
      !this.form.incidentLocation ||
      !this.form.incidentSummary
    ) {

      this.error =
        'Please complete all incident details.';

      return;

    }


    this.saving = true;


    const request = {

      policeStationId:
        this.form.policeStationId,

      citizen: {

        firstName:
          this.form.firstName,

        surname:
          this.form.surname,

        idNumber:
          this.form.idNumber,

        phoneNumber:
          this.form.phoneNumber,

        email:
          this.form.email

      },

      crimeCategory:
        this.form.crimeCategory,

      priority:
        this.form.priority,

      originalLanguage:
        this.form.originalLanguage,

      englishTranslation:
        this.form.englishTranslation,

      translationProvider:
        this.form.translationProvider,

      translationStatus:
        this.form.translationStatus,

      incidentSummary:
        this.form.incidentSummary,

      incidentDate:
        this.form.incidentDate,

      incidentLocation:
        this.form.incidentLocation

    };


    this.api
      .post<any>(
        '/reporting/open',
        request
      )
      .subscribe({

        next: (response) => {

          this.saving = false;

          alert(
            `Case opened successfully.\n\n` +
            `Case Number: ${response.caseNumber}\n` +
            `Reporting Reference: ${response.reference}`
          );

          console.log(
            'Report created:',
            response
          );

        },


        error: (error) => {

          this.saving = false;

          console.error(
            'Failed to save report:',
            error
          );

          this.error =
            error?.error?.message ||
            'Unable to save the report.';

        }

      });

  }

  translateStatement(): void {
    this.translationMessage = '';
    this.translating = true;
    this.translation.translateToEnglish(this.form.incidentSummary, this.form.originalLanguage).subscribe({
      next: result => {
        this.translating = false;
        this.form.englishTranslation = result.translatedText;
        this.form.translationProvider = result.provider;
        this.form.translationStatus = result.success ? (this.form.originalLanguage === 'en' ? 'NotRequired' : 'Completed') : 'Failed';
        this.translationMessage = result.success ? 'Machine translation completed. Review it before saving.' : (result.errorMessage || 'Translation failed. You can still save the original statement.');
      },
      error: error => {
        this.translating = false;
        this.form.translationStatus = 'Failed';
        this.form.translationProvider = 'Unavailable';
        this.translationMessage = error.error?.errorMessage || error.error?.message || 'Translation failed. You can still save the original statement.';
      }
    });
  }

}