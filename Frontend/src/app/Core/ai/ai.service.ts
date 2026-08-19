import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
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
    providedIn: 'root'
})
export class AiService {

    private readonly apiUrl = 'http://localhost:6064/cm/ai/chat';

    constructor(private readonly http: HttpClient) {}

    chat(message: string): Observable<ChatWithAiResponse> {
        const request: ChatWithAiRequest = {
            message
        };

        return this.http.post<ChatWithAiResponse>(
            this.apiUrl,
            request
        );
    }
}