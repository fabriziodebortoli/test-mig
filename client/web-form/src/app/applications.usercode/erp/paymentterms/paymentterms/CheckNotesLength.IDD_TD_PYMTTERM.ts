import { Observable } from 'rxjs/Rx';
import { MessageDlgArgs } from './../../../../shared/containers/message-dialog/message-dialog.component';
import { BOService, BOClient } from '@taskbuilder/core';

export class CheckNotesLength extends BOClient {
    constructor(
        boService: BOService) {
        super(boService);
    }

    onCommand(id: string): Observable<boolean> {
        return Observable.create(observer => {
            if (id === 'ID_EXTDOC_SAVE') {
                if (this.boService.eventData.model.PaymentTerms.Notes.value.length < 5) {
                    const args = new MessageDlgArgs();
                    args.text = 'Notes is too short, go on anyway?';
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
