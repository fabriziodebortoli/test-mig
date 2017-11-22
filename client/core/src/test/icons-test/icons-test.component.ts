import { Component, OnInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';
import { Http } from '@angular/http';

import { TestService } from './../test.service';

import { ComponentService } from './../../core/services/component.service';
import { DocumentComponent } from './../../shared/components/document.component';
import { DataService } from './../../core/services/data.service';
import { EventDataService } from './../../core/services/eventdata.service';

@Component({
  selector: 'tb-icons-test',
  templateUrl: './icons-test.component.html',
  styleUrls: ['./icons-test.component.scss'],
  providers: [DataService, EventDataService, TestService]
})
export class IconsTestComponent extends DocumentComponent implements OnInit {

  constructor(
    public eventData: EventDataService, 
    public dataService: DataService, 
    public http: Http, 
    public testService: TestService,
    changeDetectorRef:ChangeDetectorRef) {
    super(testService, eventData, null, changeDetectorRef);
  }

  ngOnInit() {
    this.eventData.model = { 'Title': { 'value': 'Icons Test Page' } };
  }

}

@Component({
  template: ''
})
export class IconsTestFactoryComponent {
  constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
    componentService.createComponent(IconsTestComponent, resolver);
  }
} 