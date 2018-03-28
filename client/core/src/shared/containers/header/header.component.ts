import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { BOService } from './../../../core/services/bo.service';
import { Component, Input, TemplateRef, ContentChild, OnInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { TbComponent } from '../../components/tb.component';

@Component({
  selector: 'tb-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss']
})
export class TbHeaderComponent extends TbComponent implements OnInit, OnDestroy 
{ 
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
