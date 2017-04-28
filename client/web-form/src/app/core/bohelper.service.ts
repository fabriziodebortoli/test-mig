import { Injectable } from '@angular/core';

import { MessageDlgArgs, WebSocketService } from './websocket.service';
import { Logger } from './logger.service';

import {
    DialogService,
    DialogRef
} from '@progress/kendo-angular-dialog';

@Injectable()
export class BOHelperService {


    constructor(
        public logger: Logger,
        private dialogService: DialogService,
        private webSocketService: WebSocketService) {

    }
    public messageDialog(mainCmpId: string, args: MessageDlgArgs) {
        const actions = [];
        if (args.ok) {
            actions.push({ text: 'Ok', ok: true });
        }
        if (args.cancel) {
            actions.push({ text: 'Cancel', cancel: true });
        }
        if (args.retry) {
            actions.push({ text: 'Retry', retry: true });
        }
        if (args.continue) {
            actions.push({ text: 'Continue', continue: true });
        }
        if (args.yes) {
            actions.push({ text: 'Yes', yes: true });
        }
        if (args.no) {
            actions.push({ text: 'No', no: true });
        }
        if (args.abort) {
            actions.push({ text: 'Abort', abort: true });
        }
        if (args.ignore) {
            actions.push({ text: 'Ignore', ignore: true });
        }

        const dialog: DialogRef = this.dialogService.open({
            title: 'Please confirm',
            content: args.text,
            actions: actions
        });

        const subs = dialog.result.subscribe((result) => {
            this.webSocketService.doCloseMessageDialog(mainCmpId, result);
            subs.unsubscribe();
        });
    }
}

