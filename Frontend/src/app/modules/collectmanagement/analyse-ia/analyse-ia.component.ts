import {
    ChangeDetectionStrategy,
    ChangeDetectorRef,
    Component,
    OnInit,
    ViewEncapsulation
} from '@angular/core';

import { ActivatedRoute, Router } from '@angular/router';

import {
    HttpClient,
    HttpClientModule
} from '@angular/common/http';

import { CommonModule } from '@angular/common';

import { MatIconModule } from '@angular/material/icon';

import { MatButtonModule } from '@angular/material/button';

import { TranslocoDirective } from '@ngneat/transloco';


interface SensorMetricAnalysis {

    average: number | null;

    minimum: number | null;

    maximum: number | null;

    recentAverage: number | null;

    historicalAverage: number | null;

    variationPercentage: number | null;

    trend: string;

}


interface SensorAnalysisResponse {

    deviceId: string;

    measurementCount: number;

    failureCount: number;

    failureRate: number;

    temperature: SensorMetricAnalysis;

    vibration: SensorMetricAnalysis;

    pressure: SensorMetricAnalysis;

    humidity: SensorMetricAnalysis;

    globalTrend: string;

    riskLevel: string;

    recommendation: string;

}


interface ApiResponse<T> {

    success: boolean;

    message: string | null;

    statusCode: number;

    validationErrors: any;

    data: T | null;

}


@Component({

    selector: 'app-analyse-ia',

    standalone: true,

    imports: [
        CommonModule,
        HttpClientModule,
        MatIconModule,
        MatButtonModule,
        TranslocoDirective
    ],

    templateUrl: './analyse-ia.component.html',

    styleUrl: './analyse-ia.component.scss',

    encapsulation: ViewEncapsulation.None,

    changeDetection: ChangeDetectionStrategy.OnPush

})


export class AnalyseIaComponent implements OnInit {


    deviceId: string = '';

    analysis: SensorAnalysisResponse | null = null;

    loading = false;

    error = '';


    constructor(

        private readonly _route: ActivatedRoute,

        private readonly _router: Router,

        private readonly _http: HttpClient,

        private readonly _changeDetectorRef: ChangeDetectorRef

    ) {}


    ngOnInit(): void {

        console.log(
            '========== ANALYSE IA COMPONENT =========='
        );


        this.deviceId =
            this._route.snapshot.paramMap.get('deviceId') ?? '';


        console.log(
            'DeviceId récupéré depuis URL:',
            this.deviceId
        );


        if (!this.deviceId) {

            console.error(
                'Aucun deviceId dans l URL'
            );

            this.error =
                'Aucun identifiant de dispositif fourni.';

            this._changeDetectorRef.markForCheck();

            return;
        }


        this.loadAnalysis();

    }


    loadAnalysis(): void {

        this.loading = true;

        this.error = '';

        this.analysis = null;


        const url =
            `http://localhost:6064/cm/sensor-measurement/analysis/${this.deviceId}`;


        console.log(
            '========== APPEL ANALYSE IA =========='
        );

        console.log(
            'URL:',
            url
        );


        this._http
            .get<ApiResponse<SensorAnalysisResponse>>(url)
            .subscribe({

                next: (response) => {

                    console.log(
                        '========== ANALYSE IA API RESPONSE =========='
                    );

                    console.log(
                        'Response complète:',
                        response
                    );


                    console.log(
                        'Response.data:',
                        response?.data
                    );


                    if (
                        response &&
                        response.success &&
                        response.data
                    ) {

                        this.analysis =
                            response.data;


                        console.log(
                            'Analyse reçue:',
                            this.analysis
                        );


                        console.log(
                            'Measurement count:',
                            this.analysis.measurementCount
                        );


                        console.log(
                            'Failure count:',
                            this.analysis.failureCount
                        );


                        console.log(
                            'Temperature:',
                            this.analysis.temperature
                        );


                        console.log(
                            'Vibration:',
                            this.analysis.vibration
                        );


                        console.log(
                            'Pressure:',
                            this.analysis.pressure
                        );


                        console.log(
                            'Humidity:',
                            this.analysis.humidity
                        );

                    } else {

                        this.error =
                            response?.message ??
                            'Impossible de récupérer l analyse.';

                    }


                    this.loading = false;


                    this._changeDetectorRef.markForCheck();

                },


                error: (error) => {

                    console.error(
                        '========== ERREUR ANALYSE IA =========='
                    );

                    console.error(
                        error
                    );


                    this.error =
                        'Erreur lors de la récupération de l analyse.';


                    this.loading = false;


                    this._changeDetectorRef.markForCheck();

                }

            });

    }


    goBack(): void {

        this._router.navigate(
            ['/fichier/device']
        );

    }


    getTrendLabel(trend: string): string {

        switch (trend) {

            case 'Increasing':
                return 'En augmentation';

            case 'Decreasing':
                return 'En diminution';

            case 'Stable':
                return 'Stable';

            default:
                return 'Pas de données';

        }

    }


    getTrendIcon(trend: string): string {

        switch (trend) {

            case 'Increasing':
                return 'heroicons_outline:arrow-trending-up';

            case 'Decreasing':
                return 'heroicons_outline:arrow-trending-down';

            case 'Stable':
                return 'heroicons_outline:minus';

            default:
                return 'heroicons_outline:question-mark-circle';

        }

    }


    getRiskLabel(risk: string): string {

        switch (risk) {

            case 'High':
                return 'Élevé';

            case 'Moderate':
                return 'Modéré';

            case 'Low':
                return 'Faible';

            default:
                return 'Inconnu';

        }

    }


    getRiskClass(risk: string): string {

        switch (risk) {

            case 'High':
                return 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400';

            case 'Moderate':
                return 'bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400';

            case 'Low':
                return 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400';

            default:
                return 'bg-gray-100 text-gray-700 dark:bg-gray-900/30 dark:text-gray-400';

        }

    }


    getTrendClass(trend: string): string {

        switch (trend) {

            case 'Increasing':
                return 'bg-red-100 text-red-700 dark:bg-red-900/30 dark:text-red-400';

            case 'Decreasing':
                return 'bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400';

            case 'Stable':
                return 'bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400';

            default:
                return 'bg-gray-100 text-gray-700 dark:bg-gray-900/30 dark:text-gray-400';

        }

    }

}