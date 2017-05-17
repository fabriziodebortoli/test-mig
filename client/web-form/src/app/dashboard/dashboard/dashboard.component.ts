import { Component, OnInit } from '@angular/core';

import { EventDataService } from './../../core/eventdata.service';

@Component({
  selector: 'tb-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss'],
  providers: [EventDataService]
})
export class DashboardComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }

}
