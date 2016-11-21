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

  private subscription: Subscription;
  private reportNamespace: string;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private reportService: ReportStudioService) {
  }

  ngOnInit() {
    this.subscription = this.route.params.subscribe(
      (params: any) => {
        this.reportNamespace = params['namespace'];
      }
    );
  }

  ngAfterViewInit() {
  }

}
