import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';
import { Http } from '@angular/http';

import { TestService } from './../test.service';

import { DataService } from './../../core/services/data.service';
import { ComponentService } from './../../core/services/component.service';
import { DocumentComponent } from './../../shared/components/document.component';
import { EventDataService } from './../../core/services/eventdata.service';

@Component({
  selector: 'tb-grid-test',
  templateUrl: './grid-test.component.html',
  styleUrls: ['./grid-test.component.scss'],
  providers: [DataService, EventDataService, TestService]
})
export class GridTestComponent extends DocumentComponent implements OnInit {

  constructor(public eventData: EventDataService, private dataService: DataService, private http: Http, private testService: TestService) {
    super(testService, eventData, null);
  }

  ngOnInit() {
    this.eventData.model = { 'Title': { 'value': 'Grid Component Demo' } };
  }

}

@Component({
  template: ''
})
export class GridTestFactoryComponent {
  constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
    componentService.createComponent(GridTestComponent, resolver);
  }
}