import { Component, OnInit, AfterViewInit } from '@angular/core';
import { ReportStudioService, Message, CommandType } from './report-studio.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs/Rx';

@Component({
    selector: 'app-report-studio',
    templateUrl: './report-studio.component.html',
    styleUrls: ['./report-studio.component.css']
})
export class ReportStudioComponent implements OnInit, AfterViewInit {

    private guid: string;

    private subscription: Subscription;
    private reportNamespace: string;

    constructor(
        private route: ActivatedRoute,
        private router: Router,
        private reportService: ReportStudioService) {
    }

    ngOnInit() {
        console.log('Report init');
        this.subscription = this.route.params.subscribe(
            (params: any) => {
                this.reportNamespace = params['namespace'];
            }
        );

        console.log('Richiesta GUID in corso...');
        this.reportService.runReport(this.reportNamespace).subscribe(
            (message: Message) => {

                if (message.commandType === CommandType.GUID) {
                    this.guid = message.message;
                    console.log('GUID: ' + this.guid);
                    this.wsConnect();
                } else {
                    console.log(message);
                }

            },
            (error: any) => {
                console.error('ERROR', error);
            });
    }

    /**
     * Connessione al WebSocket, invio del GUID ricevuto dall'API e sottoscrizione ai messaggi
     */
    wsConnect() {
        this.reportService.connect();

        this.reportService.messages.subscribe(
            (msg: Message) => this.execute(msg),
            (error) => console.log('WS_ERROR', error),
            () => console.log('WS_CLOSED')
        );

        this.sendGuid()
    }

    sendGuid() {
        this.reportService.sendGUID(this.guid);
    }

    execute(msg: Message) {
        let commandType = msg.commandType;
        let message = msg.message;

        console.log('Ricevuto messaggio', msg);
        switch (commandType) {
            case CommandType.STRUCT:

                break;
            case CommandType.DATA:

                break;
            case CommandType.ASK:

                break;
        }
    }

    ngAfterViewInit() {
    }

    // send message to server
    sendMessage() {
        let message = {
            commandType: CommandType.TEST,
            message: 'yeah'
        };
        this.reportService.messages.next(message);
    }

}
