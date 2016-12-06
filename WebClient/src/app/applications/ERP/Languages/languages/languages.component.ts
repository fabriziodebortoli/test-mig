import { ComponentService, DocumentService} from 'tb-core';
import { DocumentComponent, ToolbarComponent, EditComponent } from 'tb-shared';
import { Component, OnInit, ComponentFactoryResolver, } from '@angular/core';

@Component({
  selector: 'tb-languages',
  templateUrl: './languages.component.html',
  styleUrls: ['./languages.component.css'],
  providers:[DocumentService]
})

export class LanguagesComponent extends DocumentComponent implements OnInit {

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
