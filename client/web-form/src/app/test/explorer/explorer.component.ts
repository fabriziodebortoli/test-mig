import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';
import { Http } from '@angular/http';

import { DocumentComponent } from '../../shared/document.component';

import { ComponentService, ExplorerService } from '@taskbuilder/core';
import { EventDataService } from '@taskbuilder/core';

@Component({
  selector: 'tb-explorer',
  templateUrl: './explorer.component.html',
  styleUrls: ['./explorer.component.scss'],
  providers: [ExplorerService, EventDataService]
})
export class ExplorerComponent extends DocumentComponent implements OnInit {

  constructor(public eventData: EventDataService, private explorerService: ExplorerService, private http: Http) {
    super(explorerService, eventData);
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