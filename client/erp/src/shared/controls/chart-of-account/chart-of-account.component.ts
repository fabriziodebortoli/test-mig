import { TbComponentService, LayoutService, EventDataService, ControlComponent } from '@taskbuilder/core';
import { Component, Input, OnInit, ChangeDetectionStrategy } from '@angular/core';
import * as u from '../../../core/u/helpers';
import * as _ from 'lodash';

@Component({
  selector: 'erp-chart-of-account',
  templateUrl: './chart-of-account.component.html',
  styleUrls: ['./chart-of-account.component.scss'],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class ChartOfAccountComponent extends ControlComponent implements OnInit {
  @Input() get hotLink(): { namespace: string, name: string} {
    return { namespace: 'ERP.PaymentTerms.Dbl.PaymentTerms',
             name: 'Test'
              };
  }

  public errorMessage = '';

  constructor( public eventData: EventDataService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService) {
      super(layoutService, tbComponentService);
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
