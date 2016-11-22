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

  private logs: string[] = [''];

  private serverReportId:number;

  private connected: boolean = false;

  private subscription: Subscription;
  private reportNamespace: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private reportService: ReportStudioService) {
  }

  ngOnInit() {
    this.logs.push('Report init');
    this.subscription = this.route.params.subscribe(
      (params: any) => {
        this.reportNamespace = params['namespace'];
      }
    );

    this.logs.push('Richiesta in corso...');
    this.reportService.runReportTest(this.reportNamespace).subscribe(response => {
        this.serverReportId = response.id;

        this.logs.push("serverReportId: " + this.serverReportId);
        this.reportService.connect();
        this.reportService.messages.subscribe((msg: Message) => this.execute(msg));
    });
  }

  execute(msg: Message) {
    let command = msg.command;
    let message = msg.message;

    console.log('execute', command, message);
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
