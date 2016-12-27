import { DocumentComponent } from 'tb-shared';
import { DocumentService, ComponentService } from 'tb-core';
import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';

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

  constructor(documentService: DocumentService) {
    super(documentService);
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
