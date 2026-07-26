import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class TelegramService {

  constructor(private _http:HttpClient) { }
    async sendTelegramMessage(token: string, channel: string, message: string): Promise<any> {
        try {
            // Construct the Telegram API endpoint for sending a message
            const url = `https://api.telegram.org/${token}/sendMessage?chat_id=${channel}&text=${message}`;

            // Send GET request using HttpClient
            const response = await this._http.get(url).toPromise();

            // Return the response object
            return response;
        } catch (error) {
            // Handle errors by logging them to the console
            console.error('Error:', error);
            throw error; // Optionally rethrow the error for handling elsewhere
        }
    }
}
