import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';
import { Http } from '@angular/http';

import { ComponentService } from './../../core/services/component.service';
import { DocumentComponent } from './../../shared/components/document.component';
import { DataService } from './../../core/services/data.service';
import { EventDataService } from './../../core/services/eventdata.service';

@Component({
  selector: 'tb-radar-test',
  templateUrl: './radar-test.component.html',
  styleUrls: ['./radar-test.component.scss'],
  providers: [DataService, EventDataService]
})
export class RadarTestComponent extends DocumentComponent implements OnInit {

  constructor(public eventData: EventDataService, private dataService: DataService, private http: Http) {
    super(dataService, eventData);
  }

  ngOnInit() {
    this.eventData.model = { 'Title': { 'value': 'Radar Test Page' } };
  }

}

@Component({
  template: ''
})
export class RadarTestFactoryComponent {
  constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
    componentService.createComponent(RadarTestComponent, resolver);
  }
} 