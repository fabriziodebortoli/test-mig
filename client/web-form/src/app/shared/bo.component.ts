import { Component, OnInit, OnDestroy } from '@angular/core';

import { DocumentComponent } from './document.component';

import { EventDataService } from './../core/eventdata.service';
import { BOService } from './../core/bo.service';

@Component({
  selector: 'tb-bo',
  template: '',
  styles: []
})
export abstract class BOComponent extends DocumentComponent implements OnInit, OnDestroy {
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
