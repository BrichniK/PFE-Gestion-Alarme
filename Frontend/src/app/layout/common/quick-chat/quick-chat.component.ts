
import {
    ScrollStrategy,
    ScrollStrategyOptions
} from '@angular/cdk/overlay';

import { TextFieldModule } from '@angular/cdk/text-field';

import {
    DOCUMENT,
    DatePipe,
    NgClass,
    NgTemplateOutlet
} from '@angular/common';

import {
    AfterViewInit,
    Component,
    ElementRef,
    HostBinding,
    HostListener,
    Inject,
    NgZone,
    OnDestroy,
    OnInit,
    Renderer2,
    ViewChild,
    ViewEncapsulation
} from '@angular/core';

import { FormsModule } from '@angular/forms';

import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';

import { FuseScrollbarDirective } from '@fuse/directives/scrollbar';

import {
    ChatWithAiApiResponse,
    ChatWithAiResponse,
    QuickChatService
} from 'app/layout/common/quick-chat/quick-chat.service';

import {
    Subject,
    takeUntil
} from 'rxjs';


interface ChatMessage {

    id: number;

    isMine: boolean;

    value: string;

    createdAt: string;

    loading?: boolean;
}


@Component({

    selector: 'quick-chat',

    templateUrl: './quick-chat.component.html',

    styleUrls: ['./quick-chat.component.scss'],

    encapsulation: ViewEncapsulation.None,

    exportAs: 'quickChat',

    standalone: true,

    imports: [

        NgClass,

        NgTemplateOutlet,

        DatePipe,

        FormsModule,

        MatIconModule,

        MatButtonModule,

        MatFormFieldModule,

        MatInputModule,

        TextFieldModule,

        FuseScrollbarDirective
    ]
})


export class QuickChatComponent
    implements OnInit, AfterViewInit, OnDestroy {


    // ==================================================
    // VIEW
    // ==================================================

    @ViewChild('messageInput')
    messageInput!: ElementRef<HTMLTextAreaElement>;


    // ==================================================
    // CHAT
    // ==================================================

    messages: ChatMessage[] = [];

    messageText = '';

    opened = false;

    loading = false;

    errorMessage = '';

    private messageId = 0;


    // ==================================================
    // OVERLAY
    // ==================================================

    private _mutationObserver!: MutationObserver;

    private _scrollStrategy: ScrollStrategy;

    private _overlay: HTMLElement | null = null;


    // ==================================================
    // UNSUBSCRIBE
    // ==================================================

    private readonly _unsubscribeAll =
        new Subject<void>();


    // ==================================================
    // CONSTRUCTOR
    // ==================================================

    constructor(

        @Inject(DOCUMENT)
        private readonly _document: Document,

        private readonly _elementRef: ElementRef,

        private readonly _renderer2: Renderer2,

        private readonly _ngZone: NgZone,

        private readonly _quickChatService: QuickChatService,

        private readonly _scrollStrategyOptions: ScrollStrategyOptions

    ) {

        this._scrollStrategy =
            this._scrollStrategyOptions.block();
    }


    // ==================================================
    // HOST
    // ==================================================

    @HostBinding('class')

    get classList(): any {

        return {

            'quick-chat-opened':
                this.opened
        };
    }


    // ==================================================
    // RESIZE TEXTAREA
    // ==================================================

    @HostListener('input')

    @HostListener('ngModelChange')

    private _resizeMessageInput(): void {

        if (!this.messageInput) {

            return;
        }

        this._ngZone.runOutsideAngular(() => {

            setTimeout(() => {

                const element =
                    this.messageInput.nativeElement;

                element.style.height = 'auto';

                element.style.height =
                    `${element.scrollHeight}px`;

            });

        });
    }


    // ==================================================
    // INIT
    // ==================================================

    ngOnInit(): void {

        this.addBotMessage(

            'Bonjour 👋\n\n' +

            'Je suis votre assistant IA de maintenance industrielle.\n\n' +

            'Vous pouvez me demander d’analyser un dispositif, ' +

            'son risque de panne, ses tendances ou ses alertes.\n\n' +

            'Exemple :\n' +

            'Est-ce que MACHINE001 présente un risque de panne ?'

        );
    }


    // ==================================================
    // AFTER VIEW INIT
    // ==================================================

    ngAfterViewInit(): void {

        this._mutationObserver =
            new MutationObserver((mutations) => {

                mutations.forEach((mutation) => {

                    const target =
                        mutation.target as HTMLElement;

                    if (
                        mutation.attributeName !== 'class'
                    ) {

                        return;
                    }

                    if (
                        target.classList.contains(
                            'cdk-global-scrollblock'
                        )
                    ) {

                        const top =
                            parseInt(
                                target.style.top,
                                10
                            );

                        this._renderer2.setStyle(

                            this._elementRef.nativeElement,

                            'margin-top',

                            `${Math.abs(top)}px`
                        );

                    } else {

                        this._renderer2.setStyle(

                            this._elementRef.nativeElement,

                            'margin-top',

                            null
                        );
                    }
                });
            });


        this._mutationObserver.observe(

            this._document.documentElement,

            {
                attributes: true,

                attributeFilter: ['class']
            }
        );
    }


    // ==================================================
    // DESTROY
    // ==================================================

    ngOnDestroy(): void {

        if (this._mutationObserver) {

            this._mutationObserver.disconnect();
        }


        this._unsubscribeAll.next();

        this._unsubscribeAll.complete();


        this._scrollStrategy.disable();


        this._hideOverlay();
    }


    // ==================================================
    // OPEN
    // ==================================================

    open(): void {

        if (this.opened) {

            return;
        }

        this._toggleOpened(true);
    }


    // ==================================================
    // CLOSE
    // ==================================================

    close(): void {

        if (!this.opened) {

            return;
        }

        this._toggleOpened(false);
    }


    // ==================================================
    // TOGGLE
    // ==================================================

    toggle(): void {

        if (this.opened) {

            this.close();

        } else {

            this.open();
        }
    }


    // ==================================================
    // SEND MESSAGE
    // ==================================================

    sendMessage(): void {

        if (this.loading) {

            console.log(
                '🤖 Une requête IA est déjà en cours.'
            );

            return;
        }


        const message =
            this.messageText.trim();


        if (!message) {

            return;
        }


        console.log(
            '🤖 Message utilisateur :',
            message
        );


        this.errorMessage = '';


        // ------------------------------------------------
        // USER MESSAGE
        // ------------------------------------------------

        this.addUserMessage(message);


        // ------------------------------------------------
        // CLEAR INPUT
        // ------------------------------------------------

        this.messageText = '';


        if (this.messageInput) {

            const input =
                this.messageInput.nativeElement;

            input.value = '';

            input.style.height = 'auto';
        }


        // ------------------------------------------------
        // LOADING
        // ------------------------------------------------

        this.loading = true;


        const loadingId =
            this.addBotLoadingMessage();


        this.scrollToBottom();


        // ------------------------------------------------
        // BACKEND IA
        // ------------------------------------------------

        console.log(
            '🤖 Envoi vers le backend IA...'
        );


        this._quickChatService

            .sendMessage(message)

            .pipe(
                takeUntil(
                    this._unsubscribeAll
                )
            )

            .subscribe({

                // ==================================================
                // SUCCESS
                // ==================================================

                next: (

                    response: ChatWithAiApiResponse

                ) => {

                    console.log(
                        '🤖 Réponse API complète :',
                        response
                    );


                    console.log(
                        '🤖 success :',
                        response?.success
                    );


                    console.log(
                        '🤖 statusCode :',
                        response?.statusCode
                    );


                    console.log(
                        '🤖 data :',
                        response?.data
                    );


                    console.log(
                        '🤖 message :',
                        response?.message
                    );


                    // ----------------------------------------------
                    // STOP LOADING
                    // ----------------------------------------------

                    this.loading = false;


                    // ----------------------------------------------
                    // REMOVE "ANALYSE EN COURS..."
                    // ----------------------------------------------

                    this.removeMessage(
                        loadingId
                    );


                    // ----------------------------------------------
                    // RESPONSE VALIDATION
                    // ----------------------------------------------

                    if (
                        response &&
                        response.data
                    ) {

                        const aiResponse =
                            response.data;


                        console.log(
                            '🤖 Message IA :',
                            aiResponse.message
                        );


                        // ------------------------------------------
                        // FORMAT RESPONSE
                        // ------------------------------------------

                        const formattedResponse =
                            this.formatAiResponse(
                                aiResponse
                            );


                        console.log(
                            '🤖 Réponse formatée :',
                            formattedResponse
                        );


                        this.addBotMessage(
                            formattedResponse
                        );


                    } else if (

                        response &&
                        response.message

                    ) {

                        // ------------------------------------------
                        // FALLBACK
                        // ------------------------------------------

                        console.warn(
                            '🤖 response.data est vide.'
                        );


                        this.addBotMessage(

                            this.escapeHtml(
                                response.message
                            )

                        );


                    } else {

                        // ------------------------------------------
                        // EMPTY RESPONSE
                        // ------------------------------------------

                        console.warn(
                            '🤖 Réponse API complètement vide.'
                        );


                        this.addBotMessage(

                            'L’assistant IA a retourné une réponse vide.'

                        );
                    }


                    this.scrollToBottom();
                },


                // ==================================================
                // ERROR
                // ==================================================

                error: (

                    error: any

                ) => {

                    console.error(
                        '❌ Erreur chatbot IA :',
                        error
                    );


                    console.error(
                        '❌ Status :',
                        error?.status
                    );


                    console.error(
                        '❌ URL :',
                        error?.url
                    );


                    console.error(
                        '❌ Error body :',
                        error?.error
                    );


                    // ----------------------------------------------
                    // STOP LOADING
                    // ----------------------------------------------

                    this.loading = false;


                    // ----------------------------------------------
                    // REMOVE LOADING MESSAGE
                    // ----------------------------------------------

                    this.removeMessage(
                        loadingId
                    );


                    // ----------------------------------------------
                    // ERROR MESSAGE
                    // ----------------------------------------------

                    if (
                        error?.status === 0
                    ) {

                        this.errorMessage =

                            'Impossible de contacter le serveur IA. ' +

                            'Vérifiez que le backend est démarré.';

                    } else if (

                        error?.status === 401

                    ) {

                        this.errorMessage =

                            'Votre session a expiré. ' +

                            'Veuillez vous reconnecter.';

                    } else if (

                        error?.status === 404

                    ) {

                        this.errorMessage =

                            'Le service IA est introuvable. ' +

                            'Vérifiez l’URL du backend.';

                    } else if (

                        error?.status >= 500

                    ) {

                        this.errorMessage =

                            'Le serveur IA a rencontré une erreur. ' +

                            'Consultez les logs du backend.';

                    } else {

                        this.errorMessage =

                            'Une erreur est survenue lors de la communication avec l’assistant IA.';
                    }


                    this.addBotMessage(
                        this.errorMessage
                    );


                    this.scrollToBottom();
                },


                // ==================================================
                // COMPLETE
                // ==================================================

                complete: () => {

                    console.log(
                        '🤖 Requête IA terminée.'
                    );

                    this.loading = false;
                }
            });
    }


    // ==================================================
    // ENTER
    // ==================================================

    onKeyDown(
        event: KeyboardEvent
    ): void {

        if (

            event.key === 'Enter' &&

            !event.shiftKey

        ) {

            event.preventDefault();

            this.sendMessage();
        }
    }


    // ==================================================
    // USER MESSAGE
    // ==================================================

    private addUserMessage(
        message: string
    ): void {

        this.messages.push({

            id: ++this.messageId,

            isMine: true,

            value:

                this.escapeHtml(message)

                    .replace(
                        /\n/g,
                        '<br>'
                    ),

            createdAt:
                new Date().toISOString()
        });


        this.scrollToBottom();
    }


    // ==================================================
    // BOT MESSAGE
    // ==================================================

    private addBotMessage(
        message: string
    ): void {

        this.messages.push({

            id: ++this.messageId,

            isMine: false,

            value: message,

            createdAt:
                new Date().toISOString()
        });


        this.scrollToBottom();
    }


    // ==================================================
    // LOADING MESSAGE
    // ==================================================

    private addBotLoadingMessage(): number {

        const id =
            ++this.messageId;


        this.messages.push({

            id,

            isMine: false,

            value:
                'Analyse en cours...',

            createdAt:
                new Date().toISOString(),

            loading: true
        });


        return id;
    }


    // ==================================================
    // REMOVE MESSAGE
    // ==================================================

    private removeMessage(
        id: number
    ): void {

        this.messages =

            this.messages.filter(

                message =>
                    message.id !== id
            );
    }


    // ==================================================
    // FORMAT AI RESPONSE
    // ==================================================

    private formatAiResponse(

        response: ChatWithAiResponse

    ): string {

        let result =

            this.escapeHtml(

                response?.message ?? ''

            );


        // ------------------------------------------------
        // DEVICE
        // ------------------------------------------------

        if (
            response?.deviceMatricule
        ) {

            result +=

                `<br><br>` +

                `<strong>Dispositif :</strong> ` +

                `${this.escapeHtml(

                    response.deviceMatricule

                )}`;
        }


        // ------------------------------------------------
        // DEVICE NAME
        // ------------------------------------------------

        if (
            response?.deviceName
        ) {

            result +=

                `<br>` +

                `<strong>Nom :</strong> ` +

                `${this.escapeHtml(

                    response.deviceName

                )}`;
        }


        // ------------------------------------------------
        // RISK LEVEL
        // ------------------------------------------------

        if (
            response?.riskLevel
        ) {

            result +=

                `<br>` +

                `<strong>Niveau de risque :</strong> ` +

                `${this.escapeHtml(

                    response.riskLevel

                )}`;
        }


        // ------------------------------------------------
        // GLOBAL TREND
        // ------------------------------------------------

        if (
            response?.globalTrend
        ) {

            result +=

                `<br>` +

                `<strong>Tendance :</strong> ` +

                `${this.escapeHtml(

                    response.globalTrend

                )}`;
        }


        // ------------------------------------------------
        // FAILURE RATE
        // ------------------------------------------------

        if (

            response?.failureRate !== undefined &&

            response?.failureRate !== null

        ) {

            result +=

                `<br>` +

                `<strong>Taux d’échec :</strong> ` +

                `${Number(

                    response.failureRate

                ).toFixed(2)} %`;
        }


        // ------------------------------------------------
        // RECOMMENDATION
        // ------------------------------------------------

        if (
            response?.recommendation
        ) {

            result +=

                `<br><br>` +

                `<strong>Recommandation :</strong><br>` +

                `${this.escapeHtml(

                    response.recommendation

                ).replace(

                    /\n/g,

                    '<br>'

                )}`;
        }


        // ------------------------------------------------
        // EMPTY MESSAGE FALLBACK
        // ------------------------------------------------

        if (!result.trim()) {

            result =
                'L’assistant IA n’a retourné aucun contenu.';
        }


        return result;
    }


    // ==================================================
    // ESCAPE HTML
    // ==================================================

    private escapeHtml(
        value: string
    ): string {

        if (
            value === null ||
            value === undefined
        ) {

            return '';
        }


        return String(value)

            .replace(
                /&/g,
                '&amp;'
            )

            .replace(
                /</g,
                '&lt;'
            )

            .replace(
                />/g,
                '&gt;'
            )

            .replace(
                /"/g,
                '&quot;'
            )

            .replace(
                /'/g,
                '&#039;'
            );
    }


    // ==================================================
    // SCROLL
    // ==================================================

    private scrollToBottom(): void {

        setTimeout(() => {

            const container =

                this._elementRef
                    .nativeElement
                    .querySelector(
                        '.quick-chat-messages'
                    );


            if (container) {

                container.scrollTop =
                    container.scrollHeight;
            }

        }, 100);
    }


    // ==================================================
    // TRACK
    // ==================================================

    trackByFn(

        index: number,

        item: ChatMessage

    ): number {

        return item.id ?? index;
    }


    // ==================================================
    // OVERLAY
    // ==================================================

    private _showOverlay(): void {

        this._hideOverlay();


        this._overlay =

            this._renderer2.createElement(
                'div'
            );


        if (!this._overlay) {

            return;
        }


        this._overlay.classList.add(
            'quick-chat-overlay'
        );


        this._renderer2.appendChild(

            this._elementRef
                .nativeElement
                .parentElement,

            this._overlay
        );


        this._scrollStrategy.enable();


        this._overlay.addEventListener(

            'click',

            () => this.close()
        );
    }


    // ==================================================
    // HIDE OVERLAY
    // ==================================================

    private _hideOverlay(): void {

        if (!this._overlay) {

            return;
        }


        if (
            this._overlay.parentNode
        ) {

            this._overlay.parentNode.removeChild(
                this._overlay
            );
        }


        this._overlay = null;


        this._scrollStrategy.disable();
    }


    // ==================================================
    // TOGGLE OPENED
    // ==================================================

    private _toggleOpened(
        open: boolean
    ): void {

        this.opened = open;


        if (open) {

            this._showOverlay();


            setTimeout(() => {

                if (this.messageInput) {

                    this.messageInput
                        .nativeElement
                        .focus();
                }

            }, 400);

        } else {

            this._hideOverlay();
        }
    }
}

