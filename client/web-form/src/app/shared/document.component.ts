import { DocumentService } from 'tb-core';
import { TbComponent } from './tb.component';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { ViewModeType } from './';

@Component({
  selector: 'tb-document',
  template: '',
  styles: []
})
export abstract class DocumentComponent extends TbComponent implements OnInit, OnDestroy {

  viewModeType: ViewModeType;
  title: string;

  constructor(public document: DocumentService) {
    super();
  }

  ngOnInit() {
    this.viewModeType = this.document.getViewModeType();
    this.title = this.document.getTitle();
  }
  ngOnDestroy() {
    this.document.dispose();
  }

}
