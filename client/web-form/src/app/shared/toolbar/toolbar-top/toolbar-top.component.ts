import { EventDataService } from 'tb-core';
import { Component, OnInit, Input } from '@angular/core';

@Component({
  selector: 'tb-toolbar-top',
  templateUrl: './toolbar-top.component.html',
  styleUrls: ['./toolbar-top.component.scss']
})

export class ToolbarTopComponent implements OnInit {

  constructor(protected eventData: EventDataService) {

  }

  ngOnInit() {
  }



}
