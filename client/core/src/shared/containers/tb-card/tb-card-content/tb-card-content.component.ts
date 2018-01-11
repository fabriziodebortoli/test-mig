import { Component, ChangeDetectorRef, ChangeDetectionStrategy } from '@angular/core';

import { TbComponentService } from './../../../../core/services/tbcomponent.service';

import { TbComponent } from "../../../components/tb.component";

@Component({
  selector: 'tb-card-content',
  templateUrl: './tb-card-content.component.html',
  styleUrls: ['./tb-card-content.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class TbCardContentComponent extends TbComponent {
  
  constructor(
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);
  }

}
