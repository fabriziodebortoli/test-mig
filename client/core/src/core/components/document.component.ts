import { Component, OnInit } from '@angular/core';

import { ViewModeType } from '../models/view-mode-type.model';

import { TbComponent } from './tb.component';

import { DocumentService } from '../services/document.service';
import { EventDataService } from '../services/eventdata.service';

@Component({
  selector: 'tb-document',
  template: '',
  styles: []
})
export abstract class DocumentComponent extends TbComponent implements OnInit {

  viewModeType: ViewModeType;
  title: string;
  args: any;//used tu pass initialization arguments to the component

  constructor(public document: DocumentService, public eventData: EventDataService) {
    super();
  }

  ngOnInit() {
    this.viewModeType = this.document.getViewModeType();
    this.title = this.document.getTitle();
  }

}
