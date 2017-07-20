import { Component, OnInit, ComponentFactoryResolver, ViewChild, ViewContainerRef } from '@angular/core';
import { Http } from '@angular/http';

import { TestService } from './../test.service';

import { ComponentService } from './../../core/services/component.service';
import { DocumentComponent } from './../../shared/components/document.component';
import { DataService } from './../../core/services/data.service';
import { EventDataService } from './../../core/services/eventdata.service';

import { RadarComponent } from './../../shared/components/radar/radar.component';

@Component({
  selector: 'tb-radar-test',
  templateUrl: './radar-test.component.html',
  styleUrls: ['./radar-test.component.scss'],
  providers: [DataService, EventDataService, TestService]
})
export class RadarTestComponent extends DocumentComponent implements OnInit {

  @ViewChild('radar') radar: RadarComponent;

  constructor(public eventData: EventDataService, private dataService: DataService, private http: Http, private testService: TestService) {
    super(testService, eventData);
  }

  ngOnInit() {
    this.eventData.model = { 'Title': { 'value': 'Radar Test Page' } };
  }

  openRadar() {
    this.radar.toggle();
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