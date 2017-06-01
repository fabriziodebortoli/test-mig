﻿import { Observable } from 'rxjs/Rx';
import { MessageDlgArgs } from './../../../../shared/containers/message-dialog/message-dialog.component';
import { BOService, BOClient } from './../../../../core/bo.service';

export class CheckDescriptionLength extends BOClient {
    constructor(
        private boService: BOService) {
        super();
    }

    onCommand(id: string): Observable<boolean> {
        return Observable.create(observer => {
            if (id === 'ID_EXTDOC_SAVE') {
                if (this.boService.eventData.model.PaymentTerms.Description.value.length < 3) {
                    const args = new MessageDlgArgs();
                    args.text = 'Description is too short, go on anyway?';
                    args.yes = true;
                    args.no = true;

                    this.boService.eventData.openMessageDialog.emit(args);
                    const subs = this.boService.eventData.closeMessageDialog.subscribe(
                        result => {
                            observer.next(result.yes === true);
                            observer.complete();
                            subs.unsubscribe();
                        });
                    return;
                }
            }
            observer.next(true);
            observer.complete();

        });
    }
}
