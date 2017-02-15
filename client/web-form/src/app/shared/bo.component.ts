import { DocumentComponent } from 'tb-shared';
import { BOService } from 'tb-core';
import { TbComponent } from './tb.component';
import { Component, OnInit, OnDestroy } from '@angular/core';

@Component({
  selector: 'tb-bo',
  template: '',
  styles: []
})
export abstract class BOComponent extends DocumentComponent implements OnInit, OnDestroy {
  constructor(public bo: BOService) {
    super(bo);
  }

  ngOnInit() {
  }
  ngOnDestroy() {
    this.bo.dispose();
  }

}
