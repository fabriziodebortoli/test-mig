import { TbComponentService } from './../../../../core/services/tbcomponent.service';
import { Component, Output, EventEmitter, OnInit, OnDestroy, Input, ChangeDetectorRef } from '@angular/core';
import { Subscription } from '../../../../rxjs.imports';

import { LayoutService } from './../../../../core/services/layout.service';
import { TbComponent } from '../../../components/tb.component';
import { TabberComponent } from '../tabber/tabber.component';

@Component({
  selector: 'tb-tab',
  templateUrl: './tab.component.html',
  styleUrls: ['./tab.component.scss']
})
export class TabComponent extends TbComponent implements OnInit, OnDestroy {

  active: boolean;

  @Input() title: string = '';
  @Input() icon: string = '';
  @Input() showCloseButton: boolean = true;

  @Output() close: EventEmitter<any> = new EventEmitter();

  public viewHeightSubscription: Subscription;
  viewHeight: number;

  constructor(
    public layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef) {
    super(tbComponentService, changeDetectorRef);
  }

  ngOnInit() {
    this.viewHeightSubscription = this.layoutService.getViewHeight().subscribe((viewHeight) => this.viewHeight = viewHeight);
  }

  ngOnDestroy() {
    this.viewHeightSubscription.unsubscribe();
  }
}
