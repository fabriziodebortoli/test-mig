import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';

import { DocumentComponent } from './shared/document.component';

import { DocumentService } from './core/document.service';
import { ComponentService } from './core/component.service';
import { EventDataService } from './core/eventdata.service';

@Component({
  selector: 'tb-unsupported',
  template: `
    <p>
      This document interface is not yet supported by the web framework
    </p>
  `,
  styles: [],
  providers: [DocumentService]
})
export class UnsupportedComponent extends DocumentComponent implements OnInit {

  constructor(documentService: DocumentService, eventData: EventDataService) {
    super(documentService, eventData);
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
