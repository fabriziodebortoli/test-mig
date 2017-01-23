import { DocumentService } from 'tb-core';
import { TbComponent } from './tb.component';
import { Component, OnInit, OnDestroy } from '@angular/core';

@Component({
  selector: 'tb-document',
  template: '',
  styles: []
})
export abstract class DocumentComponent extends TbComponent implements OnInit, OnDestroy {
  constructor(public document: DocumentService) {
    super();
  }

  ngOnInit() {
  }
  ngOnDestroy() {
    this.document.dispose();
  }

}
