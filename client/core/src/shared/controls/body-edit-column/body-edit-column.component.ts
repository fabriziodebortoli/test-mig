import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { EnumsService } from './../../../core/services/enums.service';
import { ColumnComponent } from '@progress/kendo-angular-grid';
import { LayoutService } from './../../../core/services/layout.service';
import { BodyEditService } from './../../../core/services/body-edit.service';
import { Component, OnInit, Input, OnDestroy, ContentChildren, ContentChild, TemplateRef, ChangeDetectorRef, ViewChild, AfterContentInit, ViewEncapsulation } from '@angular/core';
import { Subscription } from '../../../rxjs.imports';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-body-edit-column',
  templateUrl: './body-edit-column.component.html',
  styleUrls: ['./body-edit-column.component.scss']
})
export class BodyEditColumnComponent extends ControlComponent {
  @Input() title: string;

  @Input() columnName: string;

  @Input() formatter: string;
  @Input() readonly = false;

  @Input('rows') rows: number = 0;
  @Input('chars') chars: number = 0;


  @ViewChild(ColumnComponent) columnComponent;
  @ContentChild(TemplateRef) itemTemplate: TemplateRef<any>;
  
  constructor(
    public enumsService: EnumsService,
    public layoutService: LayoutService,
    public tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef,
    public bodyEditService: BodyEditService
  ) {
    super(layoutService, tbComponentService, changeDetectorRef);
  }

  public getWidth() {
    let length;
    if (this.bodyEditService.prototype && this.bodyEditService.prototype[this.columnName].length > 0) {
      length = Math.max(this.title.length, this.bodyEditService.prototype && this.bodyEditService.prototype[this.columnName].length);
    }
    else
      length = this.title.length;
      
    let minChars = this.chars > 0 ? Math.min(length, this.chars) : length;
    return minChars * 10;
  }
}

