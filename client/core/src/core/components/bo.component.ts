import { Component, OnInit, OnDestroy } from '@angular/core';

import { ControlTypes } from '../models/control-types.enum';
import { EventDataService } from '../services/eventdata.service';
import { BOService } from '../services/bo.service';
import { DocumentComponent } from './document.component';

@Component({
  selector: 'tb-bo',
  template: '',
  styles: []
})
export abstract class BOComponent extends DocumentComponent implements OnInit, OnDestroy {

  controlTypeModel = ControlTypes;
  constructor(public bo: BOService, eventData: EventDataService) {
    super(bo, eventData);
  }

  ngOnInit() {
    super.ngOnInit();
  }
  ngOnDestroy() {
    this.bo.dispose();
  }

}
