import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';
import { Http } from '@angular/http';

import { DocumentComponent } from '@taskbuilder/core';

import { DataService } from '@taskbuilder/core';
import { EventDataService } from '@taskbuilder/core';
import { ComponentService } from '@taskbuilder/core';

@Component({
  selector: 'tb-grid-test',
  templateUrl: './grid-test.component.html',
  styleUrls: ['./grid-test.component.scss'],
  providers: [DataService, EventDataService]
})
export class GridTestComponent extends DocumentComponent implements OnInit {

  constructor(public eventData: EventDataService, private dataService: DataService, private http: Http) {
    super(dataService, eventData);
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