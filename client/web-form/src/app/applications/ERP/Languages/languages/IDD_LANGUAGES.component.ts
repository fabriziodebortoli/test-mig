import { ComponentService, DocumentService } from 'tb-core';
import { DocumentComponent } from 'tb-shared';
import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';

@Component({
  selector: 'tb-languages',
  templateUrl: './IDD_LANGUAGES.component.html',
  providers: [DocumentService]
})

export class LanguagesComponent extends DocumentComponent implements OnInit {

  private document = {
    id: 'IDD_LANGUAGES',
    name: 'Languages'
  }

  constructor(documentService: DocumentService) {
    super(documentService);
  }

  ngOnInit() {
  }

}


@Component({
  template: ''
})
export class LanguagesFactoryComponent {
  constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
    componentService.createComponent(LanguagesComponent, resolver);
  }
} 
