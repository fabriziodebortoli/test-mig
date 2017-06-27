import { BOHelperService } from '@taskbuilder/core';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { DocumentComponent } from './document.component';
import { EventDataService } from '@taskbuilder/core';
import { BOService } from '@taskbuilder/core';
import { ControlTypes } from './../shared/controls/control-types.enum';

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
