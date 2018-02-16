import { HttpService } from './http.service';
import { LocalizationService } from './localization.service';
import { Injectable } from '@angular/core';
import { Subject, Observable } from '../../rxjs.imports';
import { InfoService } from './info.service';
import { DiagnosticType } from './../../shared/models/message-dialog.model';

@Injectable()
export class DiagnosticService extends LocalizationService {

    public data: any = {};
    public isVisible: boolean = false;

    public observer: Subject<any>;

    constructor(
        infoService: InfoService,
        httpService: HttpService
    ) {
        super(infoService, httpService);
    }
    showError(message: string) {
        this.showDiagnostic([{ text: message }]);
    }
    showDiagnostic(messages) {
        if (!messages || !messages.length) {
            return;
        }
        this.observer = new Subject<any>();
        this.data.messages = this.adjustMessages(messages);
        this.data.buttons = [{ ok: true, enabled: true, text: this._TB("OK") }];

        this.isVisible = true;
        return this.observer;
    }

    resetDiagnostic() {
        this.data = {};
        this.isVisible = false;

        this.observer.next(true);
        this.observer.complete();
    }

    //compatta i messaggi e li mette nella struttura attesa dal diagnostic component
    adjustMessages(messages) {
        if (!messages || !messages.length)
            return undefined;
            if (messages.length == 1)
            {
                //esiste solo un messaggio, che non ha testo ma solo altri messaggi nidificati: 
                //allora accorpo eliminando un livello
                let msg = messages[0];
                if (typeof (msg) !== 'string'! && !msg.text && msg.messages){
                    return this.adjustMessages(msg.messages);
                }
            }
        for (let i = 0; i < messages.length; i++) {
            let msg = messages[i];
            if (typeof (msg) === 'string') {
                messages[i] = { type: DiagnosticType.Info, text: msg }
            }
            else {
                msg.messages = this.adjustMessages(msg.messages);
            }
        }
        return messages;
    }
}
