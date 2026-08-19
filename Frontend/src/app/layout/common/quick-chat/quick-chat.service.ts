
import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { SettingConfigService } from '../../../core/config/setting-config.service';

/**
 * Réponse métier de l'assistant IA.
 */
export interface ChatWithAiResponse {
    message: string;
    deviceMatricule?: string | null;
    deviceName?: string | null;
    riskLevel?: string | null;
    globalTrend?: string | null;
    failureRate?: number | null;
    recommendation?: string | null;
}

/**
 * Réponse API complète.
 *
 * Le backend retourne :
 *
 * {
 *   success: true,
 *   message: "",
 *   statusCode: 200,
 *   validationErrors: null,
 *   data: {
 *      message: "...",
 *      ...
 *   }
 * }
 */
export interface ChatWithAiApiResponse {
    success: boolean;
    message: string;
    statusCode: number;
    validationErrors?: unknown;
    data: ChatWithAiResponse;
}

@Injectable({
    providedIn: 'root'
})
export class QuickChatService {

    private readonly _httpClient = inject(HttpClient);
    private readonly _settingConfigService =
        inject(SettingConfigService);

    sendMessage(
        message: string
    ): Observable<ChatWithAiApiResponse> {

        const baseApi =
            this._settingConfigService.baseApi;

        const url =
            `${baseApi.replace(/\/$/, '')}/ai/chat`;

        console.log('🤖 IA URL:', url);
        console.log('🤖 IA message:', message);

        return this._httpClient.post<ChatWithAiApiResponse>(
            url,
            {
                message
            }
        );
    }
}

