import { Http } from '@angular/http';
import { DataService } from '@taskbuilder/core';
import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';

import { EventDataService } from '@taskbuilder/core';
import { ComponentService } from '@taskbuilder/core';
import { DocumentComponent } from "@taskbuilder/core";

@Component({
  selector: 'tb-icons-test',
  templateUrl: './icons-test.component.html',
  styleUrls: ['./icons-test.component.scss'],
  providers: [DataService, EventDataService]
})
export class IconsTestComponent extends DocumentComponent implements OnInit {

  constructor(public eventData: EventDataService, private dataService: DataService, private http: Http) {
    super(dataService, eventData);
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