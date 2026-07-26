import {
    AfterViewInit,
    ChangeDetectionStrategy, ChangeDetectorRef,
    Component,
    OnDestroy,
    OnInit,
    ViewChild,
    ViewEncapsulation,
} from '@angular/core';
import { TranslocoModule } from '@ngneat/transloco';
import { ActivatedRoute, Router, RouterLink, RouterOutlet } from '@angular/router';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatOptionModule, MatRippleModule } from '@angular/material/core';
import { MatDividerModule } from '@angular/material/divider';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatExpansionModule } from '@angular/material/expansion';
import { AsyncPipe, CommonModule, NgTemplateOutlet } from '@angular/common';
import { MatCardModule } from '@angular/material/card';
import { MatSelectModule } from '@angular/material/select';
import { FormBuilder, FormGroupDirective, ReactiveFormsModule, UntypedFormGroup, Validators } from '@angular/forms';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatInputModule } from '@angular/material/input';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatButtonModule } from '@angular/material/button';
import { fuseAnimations } from '../../../../../@fuse/animations';
import { MatPaginator } from '@angular/material/paginator';
import { Societe } from '../../../../core/Societe/societe.model';
import { catchError, map, Observable, of, ReplaySubject, Subject, takeUntil } from 'rxjs';
import { SocieteService } from '../../../../core/Societe/societe.service';
import { FuseConfirmationService } from '../../../../../@fuse/services/confirmation';
import { SecurefilePipe } from '../../../../core/pipes/securefile.pipe';

@Component({
  selector: 'app-details',
  standalone: true,
    imports: [
        MatButtonModule,
        MatFormFieldModule,
        MatIconModule,
        MatInputModule,
        MatProgressBarModule,
        MatSortModule,
        ReactiveFormsModule,
        MatOptionModule,
        MatSelectModule,
        MatCardModule,
        CommonModule,
        MatExpansionModule,
        MatDatepickerModule,
        MatDividerModule,
        MatRippleModule,
        MatSidenavModule,

        TranslocoModule,
        RouterLink,
        SecurefilePipe,
    ],
  templateUrl: './details.component.html',
  styleUrl: './details.component.scss',
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: fuseAnimations,
})
export class DetailsComponent implements OnInit, OnDestroy, AfterViewInit{
    @ViewChild(MatPaginator) private _paginator: MatPaginator;
    @ViewChild(MatSort) private _sort: MatSort;
    @ViewChild('societeFormDirective') societeFormDirective: FormGroupDirective;
    societeForm: UntypedFormGroup;
    isNewSociete: boolean = false;
    societe$: Observable<Societe>;
    societe: Societe = {} as Societe;
    flashMessage: 'success' | 'error' | null = null;
    isLoading: boolean = false;
    private _unsubscribeAll: Subject<any> = new Subject<any>();
    societeId: string;
    showEmailError = false;
    logoSrc: string | ArrayBuffer | null = null;

    constructor(
        private _activatedRoute: ActivatedRoute,
        private _router: Router,
        private formBuilder: FormBuilder,
        private _societeService: SocieteService,
        private _fuseConfirmationService: FuseConfirmationService,
        private _changeDetectorRef: ChangeDetectorRef
    ) { }

    ngOnInit(): void {

        this.societeForm = this.formBuilder.group({
            societeId: [null],
            logoPath:[null],
            logoData: [null],
            logoExtension: [null],
            nom: ['', Validators.required],
            matriculeFiscal:['', Validators.required],
            rne: ['', Validators.required],
            capital: ['' ],
            dateOverture: ['', Validators.required],
            telephone1: ['' ],
            telephone2: ['' ],
            fax1: ['' ],
            email: ['' ],
            adresse: ['' ],
        });



        this._societeService.societe$
            .pipe(takeUntil(this._unsubscribeAll))
            .subscribe((societe) => {
                if (!societe) {
                    return;
                }
                this.societe = societe;
                this.societeForm.patchValue(societe);

                this._changeDetectorRef.markForCheck();
            });

    }


    onBackdropClicked(): void {
        // Go back to the list
        this._router.navigate(['./'], { relativeTo: this._activatedRoute });

        // Mark for check
        this._changeDetectorRef.markForCheck();
    }


    showFlashMessage(type: 'success' | 'error'): void {
        this.flashMessage = type;
        this._changeDetectorRef.markForCheck();
        setTimeout(() => {
            this.flashMessage = null;
            this._changeDetectorRef.markForCheck();
        }, 8000);
    }

    saveSociete(): void {
        if (this.societeForm.invalid) {
            // Show an error message
            this.showFlashMessage('error');
            return;
        }
        const societe = this.societeForm.getRawValue() as Societe;


        if (!this.societe?.societeId) {
            this._societeService
                .AddSociete(societe)
                .pipe(
                    catchError((error)=>{

                        this.showFlashMessage('error');
                        return of(error)
                    })
                )
                .subscribe(()=>{
                    this.showFlashMessage('success');
                });

            return;
        }

        this._societeService
            .UpdateSociete(societe)
            .subscribe((val)=>{
                if (val){
                    this.showFlashMessage('success');
                    return;
                }

                this.showFlashMessage('error');
            });

    }

    ngOnDestroy(): void {
        this._unsubscribeAll.next(null);
        this._unsubscribeAll.complete();
    }


    ngAfterViewInit(): void {

    }
    toggleEmailError() {
        this.showEmailError = !this.showEmailError; // Toggle error display
    }
    private convertFileToBase64(file: File): Observable<string | ArrayBuffer> {
        const result = new ReplaySubject<string | ArrayBuffer>(1);
        const reader = new FileReader();
        reader.onload = (event) => result.next(event.target.result);
        reader.readAsDataURL(file);
        return result;
    }

    saveImage(event: any) {
        const file = event.target.files[0];

        this.convertFileToBase64(file)
            .pipe(
                map((result) => {
                    console.log(result);
                    this.logoSrc = result;
                    const base64Image = (result as string).replace(
                        /^data:image\/[a-z]+;base64,/,
                        ''
                    );
                    this.societeForm.get('logoData').patchValue(base64Image);
                    this.societeForm
                        .get('logoExtension')
                        .patchValue(file.type?.replace('image/', ''));

                    this._changeDetectorRef.markForCheck();
                })
            )
            .subscribe();
    }
}