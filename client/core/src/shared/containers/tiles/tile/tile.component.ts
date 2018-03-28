import { BOService } from './../../../../core/services/bo.service';
import { TbComponentService } from './../../../../core/services/tbcomponent.service';
import { TbComponent } from '../../../components/tb.component';
import { Component, OnInit, Input, ViewEncapsulation, OnDestroy, ChangeDetectorRef } from '@angular/core';

@Component({
  selector: 'tb-tile',
  templateUrl: './tile.component.html',
  styleUrls: ['./tile.component.scss']
})
export class TileComponent extends TbComponent implements OnInit, OnDestroy{

  constructor(
    private boService: BOService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);
  }
  ngOnInit() {
    this.boService.activateContainer(this.cmpId, true);
  }
  ngOnDestroy() {

    this.boService.activateContainer(this.cmpId, false);
  }
}
