import { DocumentService } from 'tb-core';
import { TbComponent } from './tb.component';
import { Component, OnInit, OnDestroy } from '@angular/core';

@Component({
  selector: 'tb-document',
  template: '',
  styles: []
})
export abstract class DocumentComponent extends TbComponent {
  constructor(public document: DocumentService) {
    super();
  }


}
