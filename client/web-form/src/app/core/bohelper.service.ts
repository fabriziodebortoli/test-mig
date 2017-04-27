import { CommandService } from './command.service';
import { MessageDlgArgs } from './websocket.service';
import { Logger } from 'libclient';
import { DialogComponent } from './../shared/containers/dialog/dialog.component';
import { MdDialog } from '@angular/material';
import { Injectable } from '@angular/core';


@Injectable()
export class BOHelperService  {
    
    dialogCloseSubscription: any;
    constructor(
        public dialog: MdDialog,
        public logger: Logger,
        public commandService: CommandService) {

    }
    public messageDialog(args: MessageDlgArgs) {
         const dialogRef = this.dialog.open(DialogComponent, {
                height: '400px',
                width: '600px',
            });
            this.dialogCloseSubscription = dialogRef.afterClosed().subscribe(result => {
                this.dialogCloseSubscription.unsubscribe();
            });
    }
}

