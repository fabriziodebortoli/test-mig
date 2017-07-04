import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';

import { ComponentService } from './../../core/services/component.service';
import { DocumentComponent } from './document.component';
import { EventDataService } from './../../core/services/eventdata.service';
import { BOService } from './../../core/services/bo.service';

@Component({
  selector: 'tb-unsupported',
  template: `
    <p>
      This document interface is not yet supported by the web framework
    </p>
  `,
  styles: [],
  providers: [BOService, EventDataService]
})
export class UnsupportedComponent extends DocumentComponent implements OnInit {

  constructor(boService: BOService, eventData: EventDataService) {
    super(boService, eventData);
  }

  ngOnInit() {
  }

}


@Component({
  template: ''
})
export class UnsupportedFactoryComponent {
  constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
    componentService.createComponent(UnsupportedComponent, resolver);
  }
} 
