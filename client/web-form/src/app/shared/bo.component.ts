import { BOHelperService } from 'app/core/bohelper.service';
import { Component, OnInit, OnDestroy } from '@angular/core';
import { DocumentComponent } from './document.component';
import { EventDataService } from './../core/eventdata.service';
import { BOService } from './../core/bo.service';
import { ControlTypes}  from './../shared/controls/control-types.enum';

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
