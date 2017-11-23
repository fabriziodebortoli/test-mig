import { Component, OnInit, ComponentFactoryResolver, ViewChild, ViewContainerRef, ChangeDetectorRef } from '@angular/core';
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

  constructor(
    eventData: EventDataService, 
    public dataService: DataService, 
    testService: TestService,
    changeDetectorRef: ChangeDetectorRef, 
    public http: Http) {
    super(testService, eventData, null, changeDetectorRef);
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