import { WebSocketService } from './web-socket.service';
import { Component, OnInit, AfterViewInit } from '@angular/core';
import { ReportStudioService } from './report-studio.service';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs/Rx';

@Component({
  selector: 'app-report-studio',
  templateUrl: './report-studio.component.html',
  styleUrls: ['./report-studio.component.css']
})
export class ReportStudioComponent implements OnInit, AfterViewInit {

  private wsUrl = 'ws://localhost:5000';

  private logs: string[] = [''];

  private connected: boolean = false;

  private subscription: Subscription;
  private reportNamespace: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private reportService: ReportStudioService,
    private websocketService: WebSocketService) {
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
      if (response.result === 'OK') {
        this.logs.push(response.message);
        this.wsConnect();
      } else {
        console.error('Errore runReport');
      }
    });
  }

  wsConnect() {
    this.websocketService.connect(this.wsUrl);
  }

  ngAfterViewInit() {
  }

}
