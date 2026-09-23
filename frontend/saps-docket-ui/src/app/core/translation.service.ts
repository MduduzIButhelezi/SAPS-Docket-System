import { Injectable } from '@angular/core';
import { ApiService } from './api.service';

export interface TranslationResult {
  originalText: string;
  sourceLanguage: string;
  translatedText: string;
  targetLanguage: string;
  provider: string;
  success: boolean;
  errorMessage?: string;
}

@Injectable({ providedIn: 'root' })
export class TranslationService {
  constructor(private api: ApiService) {}

  translateToEnglish(text: string, sourceLanguage: string) {
    return this.api.post<TranslationResult>('/translation/to-english', { text, sourceLanguage });
  }

  translate(text: string, sourceLanguage: string, targetLanguage: string) {
    return this.api.post<TranslationResult>('/translation/translate', { text, sourceLanguage, targetLanguage });
  }
}
