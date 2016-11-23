import { Component, OnInit, AfterViewInit } from '@angular/core';
import { ReportStudioService, Message, Command } from './report-studio.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs/Rx';

@Component({
  selector: 'app-report-studio',
  templateUrl: './report-studio.component.html',
  styleUrls: ['./report-studio.component.css']
})
export class ReportStudioComponent implements OnInit, AfterViewInit {

  private guid: number;

  private wsConnectionState: number = 3;

  private subscription: Subscription;
  private reportNamespace: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private reportService: ReportStudioService) {
  }

  ngOnInit() {
    console.debug('Report init');
    this.subscription = this.route.params.subscribe(
      (params: any) => {
        this.reportNamespace = params['namespace'];
      }
    );

    console.debug('Richiesta in corso...');
    this.reportService.runReport(this.reportNamespace).subscribe(response => {
      console.log(response);
      this.guid = response.id;

      console.info("guid: " + this.guid);
    });
  }

  wsConnect() {
    this.reportService.connect();

    this.wsConnectionState = this.reportService.wsConnectionState
    // this.reportService.wsConnectionState.subscribe((wsConnectionState: number) => this.wsConnectionState = wsConnectionState);

    this.reportService.messages.subscribe((msg: Message) => this.execute(msg));

    let message = {
      command: Command.GUID,
      message: '${this.guid}'
    };
    this.reportService.messages.next(message);
  }

  execute(msg: Message) {
    let command = msg.command;
    let message = msg.message;

    console.info('execute', command, message);
    switch (command) {
      case Command.STRUCT:

        break;
      case Command.DATA:

        break;
      case Command.ASK:

        break;
    }
  }

  ngAfterViewInit() {
  }

  // send message to server
  sendMessage() {
    let message = {
      command: Command.TEST,
      message: 'yeah'
    };
    this.reportService.messages.next(message);
  }

}
