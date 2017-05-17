import { Observable } from 'rxjs/Rx';
import { MessageDlgArgs } from './../../../../shared/containers/message-dialog/message-dialog.component';
import { text } from './../../../../reporting-studio/reporting-studio.model';
import { EventDataService } from './../../../../core/eventdata.service';
import { WebSocketService } from './../../../../core/websocket.service';
import { BOHelperService } from 'app/core/bohelper.service';
import { BOService } from './../../../../core/bo.service';
import { Injectable } from '@angular/core';

@Injectable()
export class IDD_TD_PYMTTERMService extends BOService {
    constructor(
        webSocketService: WebSocketService,
        boHelperService: BOHelperService,
        eventData: EventDataService) {
        super(webSocketService, boHelperService, eventData);
    }

    onCommand(id: string) {
        if (id === 'ID_EXTDOC_SAVE') {
            if (this.eventData.model.PaymentTerms.Description.value.length < 3) {
                const args = new MessageDlgArgs();
                args.text = 'Descrizione un po\' corta, continuo ugualmente?';
                args.yes = true;
                args.no = true;

                this.eventData.openMessageDialog.emit(args);
                return Observable.create(observer => {
                    this.eventData.closeMessageDialog.subscribe(
                        result => {
                            observer.next(result.yes === true);
                            observer.complete();
                        }
                    );

                }
                );
            }
        }
        return true;
    }
}
