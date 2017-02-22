import { EventDataService, BOService } from 'tb-core';
import { TbComponent } from './tb.component';
import { DocumentComponent } from './document.component';
import { Component, OnInit, OnDestroy } from '@angular/core';

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
