import { TbComponentService } from './../../../../core/services/tbcomponent.service';
import { BOService } from './../../../../core/services/bo.service';
import { Component, Input, Output, EventEmitter, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { TbComponent } from '../../../components/tb.component';

@Component({
  selector: 'tb-filter-group',
  templateUrl: './filter-group.component.html',
  styleUrls: ['./filter-group.component.scss']
})
export class FilterGroupComponent extends TbComponent implements OnInit, OnDestroy {

  @Input() title: string;
  @Input() isCollapsed: boolean = false;
  @Input() isCollapsible: boolean = false;

  constructor(
    private boService: BOService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(tbComponentService, changeDetectorRef);
  }

  ngOnInit() {
    this.boService.activateContainer(this.cmpId, true, false);
  }

  ngOnDestroy() {
    this.boService.activateContainer(this.cmpId, false, false);
  }

}
