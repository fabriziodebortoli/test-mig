import { Component, OnInit, AfterViewInit, Input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs/Rx';

import { ReportStudioService, Message, CommandType } from './report-studio.service';
import { ReportObject } from './report-object-wrapper/report-object.model';

@Component({
  selector: 'app-report-studio',
  templateUrl: './report-studio.component.html',
  styleUrls: ['./report-studio.component.css']
})
export class ReportStudioComponent implements OnInit, AfterViewInit {

  private subscription: Subscription;

  private reportNamespace: string;

  @Input() report: ReportObject[];

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

    console.log('WebSocket, connecting...');
    this.wsConnect();
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
  }

  execute(msg: Message) {
    let commandType = msg.commandType;
    let message = msg.message;

    console.log('Ricevuto messaggio', msg);
    switch (commandType) {
      case CommandType.OK:
        console.log('WebSocket, connected');
        this.reportService.sendNamespace(this.reportNamespace);
        break;

      case CommandType.STRUCT:
        console.log('WebSocket, received Report Structure', message);
        let reportStruct = <ReportObject[]>(JSON.parse(message));
        this.checkReportStruct(reportStruct);
        break;

      case CommandType.DATA:
        console.log('WebSocket, received Report Data', message);
        break;

      case CommandType.ASK:

        break;
    }
  }

  ngAfterViewInit() {
  }

  checkReportStruct(reportStruct: ReportObject[]) {
    // TODO cache struttura pagina
    // TODO join con dati
    this.report = reportStruct;
  }

}
