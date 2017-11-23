import { Component, OnInit, ComponentFactoryResolver,ChangeDetectorRef } from '@angular/core';
import { Http } from '@angular/http';

import { TestService } from './../test.service';

import { DocumentComponent } from './../../shared/components/document.component';
import { ComponentService } from './../../core/services/component.service';
import { EventDataService } from './../../core/services/eventdata.service';
import { ExplorerService } from './../../core/services/explorer.service';

@Component({
  selector: 'tb-explorer',
  templateUrl: './explorer.component.html',
  styleUrls: ['./explorer.component.scss'],
  providers: [ExplorerService, EventDataService, TestService]
})
export class ExplorerComponent extends DocumentComponent implements OnInit {

  constructor(
    public eventData: EventDataService, 
    public explorerService: ExplorerService, 
    public http: Http, 
    public testService: TestService,
    changeDetectorRef:ChangeDetectorRef) {
    super(testService, eventData, null,changeDetectorRef);
  }

  ngOnInit() {
    this.eventData.model = { 'Title': { 'value': 'Explorer Component Demo' } };
  }

}

@Component({
  template: ''
})
export class ExplorerFactoryComponent {
  constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
    componentService.createComponent(ExplorerComponent, resolver);
  }
}