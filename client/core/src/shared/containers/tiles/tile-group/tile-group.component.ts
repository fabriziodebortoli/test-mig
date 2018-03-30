import { BOService } from './../../../../core/services/bo.service';
import { TbComponentService } from './../../../../core/services/tbcomponent.service';
import { Component, Input, TemplateRef, ContentChild, ViewChild, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { TbComponent } from '../../../components/tb.component';

@Component({
  selector: 'tb-tile-group',
  templateUrl: './tile-group.component.html',
  styleUrls: ['./tile-group.component.scss']
})
export class TileGroupComponent extends TbComponent implements OnInit, OnDestroy {
  constructor(
    private boService: BOService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);
  }
  ngOnInit() {
    this.boService.activateContainer(this.cmpId, true, true);
  }
  ngOnDestroy() {

    this.boService.activateContainer(this.cmpId, false, true);
  }
}
