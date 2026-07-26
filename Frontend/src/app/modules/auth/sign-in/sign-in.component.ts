import { Component, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import {
    FormsModule,
    NgForm,
    ReactiveFormsModule,
    UntypedFormBuilder,
    UntypedFormGroup,
    Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { fuseAnimations } from '@fuse/animations';
import { FuseAlertComponent, FuseAlertType } from '@fuse/components/alert';
import { AuthService } from 'app/core/auth/auth.service';
import { ApiResponse } from '../../../core/common/api-response';
import { User } from '../../../core/user/user.types';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
    selector: 'auth-sign-in',
    templateUrl: './sign-in.component.html',
    encapsulation: ViewEncapsulation.None,
    animations: fuseAnimations,
    standalone: true,
    imports: [
        RouterLink,
        FuseAlertComponent,
        FormsModule,
        ReactiveFormsModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        MatIconModule,
        MatCheckboxModule,
        MatProgressSpinnerModule,
    ],
})
export class AuthSignInComponent implements OnInit {
    @ViewChild('signInNgForm') signInNgForm: NgForm;
    signInForm: UntypedFormGroup;
    showAlert: boolean = false;
    isLoading: boolean = false;
    passwordAttempts: number = 0;
    isLocked: boolean = false;
    remainingTime: number = 30; // 30 secondes
    private lockTimer: any;

    alert: { type: FuseAlertType; message: string } = {
        type: 'success',
        message: '',
    };
    // signInForm: UntypedFormGroup;
    // showAlert: boolean = false;

    /**
     * Constructor
     */
    constructor(
        private _activatedRoute: ActivatedRoute,
        private _authService: AuthService,
        private _formBuilder: UntypedFormBuilder,
        private _router: Router
    ) {}

    // -----------------------------------------------------------------------------------------------------
    // @ Lifecycle hooks
    // -----------------------------------------------------------------------------------------------------

    /**
     * On init
     */
    ngOnInit(): void {
        // Create the form
        this.signInForm = this._formBuilder.group({
            login: [
                '',
                [Validators.required],
            ],
            password: ['', Validators.required],
            rememberMe: [''],
        });
    }

    // -----------------------------------------------------------------------------------------------------
    // @ Public methods
    // -----------------------------------------------------------------------------------------------------

    /**
     * Sign in
     */
    // signIn(): void {
    //     // Return if the form is invalid
    //     if (this.signInForm.invalid) {
    //         return;
    //     }
    //
    //     // Disable the form
    //     this.signInForm.disable();
    //
    //     // Hide the alert
    //     this.showAlert = false;
    //
    //     // Sign in
    //     this._authService.signIn(this.signInForm.value).subscribe(
    //         () => {
    //             // Set the redirect url.
    //             // The '/signed-in-redirect' is a dummy url to catch the request and redirect the user
    //             // to the correct page after a successful sign in. This way, that url can be set via
    //             // routing file and we don't have to touch here.
    //             const redirectURL =
    //                 this._activatedRoute.snapshot.queryParamMap.get(
    //                     'redirectURL'
    //                 ) || '/signed-in-redirect';
    //
    //             // Navigate to the redirect url
    //             this._router.navigateByUrl(redirectURL);
    //         },
    //         (response) => {
    //             // Re-enable the form
    //             this.signInForm.enable();
    //
    //             // Reset the form
    //             this.signInNgForm.resetForm();
    //
    //             // Set the alert
    //             this.alert = {
    //                 type: 'error',
    //                 message: 'Wrong email or password',
    //             };
    //
    //             // Show the alert
    //             this.showAlert = true;
    //         }
    //     );
    // }
    signIn(): void {
        if (this.isLocked) return;
        if (this.signInForm.invalid) return;

        this.isLoading = true;
        this.showAlert = false;

        this._authService.signIn(this.signInForm.value).subscribe({
            next: (response) => {
                if (response.success) {
                    this.handleSuccessfulLogin();
                } else {
                    this.handleApiError(response);
                }
            },
            error: (error) => {
                this.handleHttpError(error);
            }
        });
    }

    private handleSuccessfulLogin(): void {
        this.passwordAttempts = 0;
        this.isLoading = false;
        const redirectURL = this._activatedRoute.snapshot.queryParamMap.get('redirectURL') || '/signed-in-redirect';
        this._router.navigateByUrl(redirectURL);
    }

    private handleApiError(response: ApiResponse<User>): void {
        this.isLoading = false;
        this.signInForm.enable();
        this.passwordAttempts++;

        if (this.passwordAttempts >= 3) {
            this.lockAccount();
        } else {
            this.showErrorAlert( 'Utilisateur ou mot de passe incorrect');
        }
    }

    private handleHttpError(error: HttpErrorResponse): void {
        this.isLoading = false;
        this.signInForm.enable();

        let errorMessage = 'Une erreur est survenue. Veuillez réessayer.';
        if (error.status === 0) {
            errorMessage = 'Impossible de se connecter au serveur. Vérifiez votre connexion internet.';
        } else if (error.status >= 500) {
            errorMessage = 'Le serveur rencontre un problème. Veuillez réessayer plus tard.';
        }

        this.showErrorAlert(errorMessage);
    }

    private lockAccount(): void {
        this.isLocked = true;
        this.signInForm.disable();
        this.remainingTime = 30;

        this.showErrorAlert(`Trop de tentatives. Réessayez dans ${this.remainingTime} secondes.`);

        this.lockTimer = setInterval(() => {
            this.remainingTime--;

            // Mettre à jour le message
            this.alert.message = `Trop de tentatives. Réessayez dans ${this.remainingTime} secondes.`;

            // Déverrouiller quand le temps est écoulé
            if (this.remainingTime <= 0) {
                this.unlockAccount();
            }
        }, 1000);
    }

    private unlockAccount(): void {
        clearInterval(this.lockTimer);
        this.isLocked = false;
        this.passwordAttempts = 0;
        this.signInForm.enable();
        this.showAlert = false;
    }

    private showErrorAlert(message: string): void {
        this.alert = {
            type: 'error',
            message: message
        };
        this.showAlert = true;
    }
    ngOnDestroy(): void {
        if (this.lockTimer) {
            clearInterval(this.lockTimer);
        }
    }
}
