import { Component, OnInit, AfterViewInit, Input } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs/Rx';

import { ReportingStudioService } from './reporting-studio.service';
import { ReportObject } from './reporting-studio.model';

@Component({
  selector: 'app-reporting-studio',
  templateUrl: './reporting-studio.component.html',
  styleUrls: ['./reporting-studio.component.scss']
})
export class ReportingStudioComponent implements OnInit, AfterViewInit {

  private subscription: Subscription;

  private reportNamespace: string;
  private reportParams: any = '';

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private reportService: ReportingStudioService) {
  }

  ngOnInit() {
    this.subscription = this.route.params.subscribe(
      (params: any) => {
        this.reportNamespace = params['namespace'];
        // this.reportParams = params['params'];
      }
    );

    console.log('Reporting Studio Component Init ', this.reportNamespace, this.reportParams);
  }

  ngAfterViewInit() { }

}
