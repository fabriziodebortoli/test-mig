import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { BOService } from './../../../core/services/bo.service';
import { Component, OnInit, Input, Output, ViewEncapsulation, EventEmitter, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { TbComponent } from '../../components/tb.component';


@Component({
  selector: 'tb-panel',
  templateUrl: './panel.component.html',
  styleUrls: ['./panel.component.scss']
})
export class PanelComponent extends TbComponent implements OnInit, OnDestroy {

  _title: string;
  _collapsedTitle: string;
  @Input() isCollapsed: boolean = false;
  @Input() isCollapsible: boolean = false;
  realTitle = ""
  @Output() toggle = new EventEmitter<boolean>();

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

  @Input() public set title(val: any) {
    this._title = val instanceof Object ? val.value : val;
    this.calculateRealTitle();
  }
  public get title() : any {
    return this._title;
  }
  @Input() public set collapsedTitle(val: string) {
    this._collapsedTitle = val;
    this.calculateRealTitle();
  } 
  public get collapsedTitle() : string {
    return this._collapsedTitle;
  }

  toggleCollapse(emit: boolean = true): void {

    if (!this.isCollapsible)
      return;

    this.isCollapsed = !this.isCollapsed;
    this.calculateRealTitle();
    if (emit)
      this.toggle.emit(this.isCollapsed);

  }

  calculateRealTitle() {
    if (this.isCollapsed && this._collapsedTitle) {
      this.realTitle = this._collapsedTitle;
    } else {
      this.realTitle = this._title;
    }

  }
}
