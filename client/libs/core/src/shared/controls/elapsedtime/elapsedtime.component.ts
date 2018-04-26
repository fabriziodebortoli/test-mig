import {Component, Input, OnChanges, ChangeDetectorRef, ViewChild, OnInit, SimpleChanges } from "@angular/core";

import { ControlContainerComponent } from './../control-container/control-container.component';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { FormattersService } from './../../../core/services/formatters.service';
import { Store } from './../../../core/services/store.service';
import { ControlComponent } from './../control.component';

@Component({
  selector: "tb-elapsedtime",
  templateUrl: "./elapsedtime.component.html",
  styleUrls: ["./elapsedtime.component.scss"]
})
export class ElapsedTimeComponent extends ControlComponent implements OnChanges, OnInit {
  formatterProps: any;

  constructor(
    public eventData: EventDataService,
    private formattersService: FormattersService,
    layoutService: LayoutService,
    tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  ngOnInit() {
    this.formattersService.loadFormattersTable();
    this.formatterProps = this.formattersService.getFormatter("ElapsedTime");
  }

  ngOnChanges(changes: SimpleChanges) {
    if (!changes.model || !this.model || !this.model.length) return;
    
  }

  getmask() {
    // switch (this.formatterProps.FormatType) {
    //   case 'TIME_DHMSF': return '(d:h:m:s.f)';
    //   case 'TIME_DHM'S: return '(d:h:m:s)';
    //   case 'TIME_DHM': return '(d:h:m)';
    //   case 'TIME_DH': return '(d:h)';
    //   case 'TIME_D': return '00000';
    //   case 'TIME_HMSF': return '(h:m:s.f)';
    //   case 'TIME_HMS': return '(h:m:s)';
    //   case 'TIME_HM': return '(h:m)';
    //   case 'TIME_H': return '000000';
    //   case 'TIME_MSF': return '(m:s.f)';
    //   case 'TIME_MSEC': return '(m:s)';
    //   case 'TIME_M': return '00000000';
    //   case 'TIME_SF': return '(s.f)';
    //   case 'TIME_S': return '(s)';
    //   case 'TIME_DHMCM': return '(d:h:m.f)';
    //   case 'TIME_DHCH': return '(d:h.f)';
    //   case 'TIME_HMCM': return '(h:m.f)';
    //   case 'TIME_MCM': return '(m.f)';
    //   case 'TIME_CH': return '(hc.f)';  //ora centesimale
    // }
    // if (this.modelValid()) {
    //   this.onUpdateNgModel(this.model.value);
    // }
  }

  onBlur(e): any {
    // switch (this.formatter) {
    //   case 'Integer':
    //   case 'Long':
    //   case 'Money':
    //   case 'Percent':
    //     this.constraint = new RegExp('\\d');
    //     break;
    //   case 'Double':
    //     this.constraint = new RegExp('[-+]?[0-9]*\.?[0-9]+');
    //     break;

    //   default: break;
    // }

    // if (!this.constraint.test(this.model.value)) {
    //   this.cc.errorMessage = 'Input not in correct form';
    //   this.showError = 'inputError';
    // }
    // else {
    //   this.cc.errorMessage = '';
    //   this.showError = '';
    // }
    this.eventData.change.emit(this.cmpId);
    this.blur.emit(this);
  }
}
