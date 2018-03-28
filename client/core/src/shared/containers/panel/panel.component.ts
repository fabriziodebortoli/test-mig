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

  @Input() title: string;
  @Input() collapsedTitle: string;
  @Input() isCollapsed: boolean = false;
  @Input() isCollapsible: boolean = false;
  realTitle = ""
  @Output() toggle = new EventEmitter<boolean>();

  toggleCollapse(emit: boolean = true): void {

    if (!this.isCollapsible)
      return;

    this.isCollapsed = !this.isCollapsed;
    if (this.isCollapsed) {
      if (this.collapsedTitle) {
        this.realTitle = this.collapsedTitle;
      } else {
        this.realTitle = this.title;
      }
    }
    if (emit)
      this.toggle.emit(this.isCollapsed);

  }
}
