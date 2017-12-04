import { TbComponentService, LayoutService, EventDataService, ControlComponent } from '@taskbuilder/core';
import { Component, Input, OnInit, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import * as u from '../../../core/u/helpers';
import * as _ from 'lodash';

@Component({
  selector: 'erp-chart-of-account',
  templateUrl: './chart-of-account.component.html',
  styleUrls: ['./chart-of-account.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChartOfAccountComponent extends ControlComponent implements OnInit {
  @Input() hotLink: { namespace: string, name: string};
  public errorMessage = '';

  @Input() selector: any;
  @Input() slice$: any;

  constructor(
    public eventData: EventDataService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef) {
      super(layoutService, tbComponentService, changeDetectorRef);
    }

    ngOnInit() {
        console.log('OnInit');
    }

    changeModelValue(value) {
      this.model.value = value;
    }

    onBlur() {
    }
}