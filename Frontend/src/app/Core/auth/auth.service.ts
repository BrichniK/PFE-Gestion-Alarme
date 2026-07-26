import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { AuthUtils } from 'app/core/auth/auth.utils';
import { UserService } from 'app/core/user/user.service';
import { catchError, Observable, of, switchMap, throwError } from 'rxjs';
import { ApiService } from '../common/api.service';
import { User } from '../user/user.types';
import { ApiResponse } from '../common/api-response';


@Injectable({ providedIn: 'root' })
export class AuthService {
    private _authenticated: boolean = false;
    private _httpClient = inject(HttpClient);
    private _userService = inject(UserService);
    private _apiService = inject(ApiService);

    // -----------------------------------------------------------------------------------------------------
    // @ Accessors
    // -----------------------------------------------------------------------------------------------------

    /**
     * Setter & getter for access token
     */
    set accessToken(token: string) {
        localStorage.setItem('accessToken', token);
    }

    get accessToken(): string {
        return localStorage.getItem('accessToken') ?? '';
    }

    set role(role: string) {
        localStorage.setItem('role', role);
    }

    get role(): string {
        return localStorage.getItem('role') ?? '';
    }

    /**
     * setter & getter for UserId
     */

    set utilisateurId(utilisateurId: string) {
        localStorage.setItem('user', utilisateurId);
    }

    get utilisateurId():string{
        return localStorage.getItem('user') ?? '';
    }

    /**
     * setter & getter for SocieteId
     */

    set societeId(societeId: string) {
        localStorage.setItem('societe', societeId);
    }

    get societeId():string{
        return localStorage.getItem('societe') ?? '';
    }


    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Forgot password
     *
     * @param email
     */
    forgotPassword(email: string): Observable<any> {
        return this._httpClient.post('api/auth/forgot-password', email);
    }

    /**
     * Reset password
     *
     * @param password
     */
    resetPassword(password: string): Observable<any> {
        return this._apiService.SilentPost('api/auth/reset-password', password);
    }

    /**
     * Sign in
     *
     * @param credentials
     */
    signIn(credentials: { login: string; password: string }): Observable<any> {
        // Throw error, if the user is already logged in
        if ( this._authenticated )
        {
            throw new Error('User is already logged in.');
        }

        return this._apiService.SilentPost<User>('authentication/v99/login', credentials).pipe(
            switchMap((response) => {

                if (!response || !response.success) {
                    return of(response);
                }

                // Store the access token in the local storage
                this.accessToken = response.data.token;

                this.utilisateurId = response.data.utilisateurId;

                console.log('user:',this.utilisateurId);

                this.societeId = response.data.societeId;


                // Set the authenticated flag to true
                this._authenticated = true;

                // Store the user on the user service
                this._userService.user = response.data;

                // Return a new observable with the response
                return of(response);
            })
        );
    }

    /**
     * Sign in using the access token
     */
    signInUsingToken(): Observable<any> {
        // Sign in using the token
        return this._apiService
            .SilentPost<User>('authentication/v99/login-check', {
                Token: this.accessToken,
            })
            .pipe(
                catchError(() =>
                    // Return false
                    of(false)
                ),
                switchMap((response: ApiResponse<User>) => {
                    if (!response || !response.success) {
                        return of(false);
                    }
                    // Replace the access token with the new one if it's available on
                    // the response object.
                    //
                    // This is an added optional step for better security. Once you sign
                    // in using the token, you should generate a new one on the server
                    // side and attach it to the response object. Then the following
                    // piece of code can replace the token with the refreshed one.

                    this.accessToken = response.data.token;


                    // Set the authenticated flag to true
                    this._authenticated = true;

                    // Store the user on the user service
                    this._userService.user = response.data;

                    // Return true
                    return of(true);
                })
            );
    }

    /**
     * Sign out
     */
    signOut(): Observable<any> {
        // Remove the access token from the local storage
        localStorage.removeItem('accessToken');

        // Set the authenticated flag to false
        this._authenticated = false;

        // Return the observable
        return of(true);
    }

    /**
     * Sign up
     *
     * @param user
     */
    signUp(user: {
        name: string;
        email: string;
        password: string;
        company: string;
    }): Observable<any> {
        return this._httpClient.post('api/auth/sign-up', user);
    }

    /**
     * Unlock session
     *
     * @param credentials
     */
    unlockSession(credentials: {
        email: string;
        password: string;
    }): Observable<any> {
        return this._httpClient.post('api/auth/unlock-session', credentials);
    }

    /**
     * Check the authentication status
     */
    check(): Observable<boolean> {
        // Check if the user is logged in
        if (this._authenticated) {
            return of(true);
        }

        // Check the access token availability
        if (!this.accessToken) {
            return of(false);
        }

        // Check the access token expire date
        // if (AuthUtils.isTokenExpired(this.accessToken)) {
        //     return of(false);
        // }

        // If the access token exists, and it didn't expire, sign in using it
        return this.signInUsingToken();
    }
}
