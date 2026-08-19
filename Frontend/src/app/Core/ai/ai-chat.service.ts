import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

export interface ChatWithAiRequest {
    message: string;
}

export interface ChatWithAiResponse {
    message: string;
    deviceMatricule?: string;
    deviceName?: string;
    riskLevel?: string;
    globalTrend?: string;
    failureRate?: number;
    recommendation?: string;
}

@Injectable({
    providedIn: 'root',
})
export class QuickChatService {

    private readonly apiUrl =
        'http://localhost:6064/cm/ai/chat';

    constructor(private _httpClient: HttpClient) {}

    /**
     * Envoie une question au chatbot IA.
     */
    sendMessage(
        message: string
    ): Observable<ChatWithAiResponse> {

        const request: ChatWithAiRequest = {
            message: message,
        };

        return this._httpClient.post<ChatWithAiResponse>(
            this.apiUrl,
            request
        );
    }
}