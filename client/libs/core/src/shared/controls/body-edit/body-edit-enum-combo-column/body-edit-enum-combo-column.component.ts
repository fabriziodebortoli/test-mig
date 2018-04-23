import { BodyEditColumnComponent } from './../body-edit-column/body-edit-column.component';
import { TbComponentService } from './../../../../core/services/tbcomponent.service';
import { EnumsService } from './../../../../core/services/enums.service';
import { ColumnComponent } from '@progress/kendo-angular-grid';
import { LayoutService } from './../../../../core/services/layout.service';
import { BodyEditService } from './../../../../core/services/body-edit.service';
import { Component, OnInit, Input, OnDestroy, ContentChildren, ContentChild, TemplateRef, forwardRef, ChangeDetectorRef, ViewChild, AfterContentInit, ViewEncapsulation, AfterViewInit } from '@angular/core';
import { Subscription } from '../../../../rxjs.imports';

import { ControlComponent } from './../../control.component';

@Component({
  selector: 'tb-body-edit-enum-combo-column',
  templateUrl: './body-edit-enum-combo-column.component.html',
  styleUrls: ['./body-edit-enum-combo-column.component.scss'],
  providers: [{ provide: BodyEditColumnComponent, useExisting: forwardRef(() => BodyEditEnumComboColumnComponent) }]
})
export class BodyEditEnumComboColumnComponent extends BodyEditColumnComponent  {
  @Input() title: string;

  @Input() columnName: string;

  @Input() formatter: string;
  @Input() readonly = false;

  @ViewChild(ColumnComponent) columnComponent;
  @ContentChild(TemplateRef) itemTemplate: TemplateRef<any>;
  constructor(
    public enumsService: EnumsService,
    public layoutService: LayoutService,
    public tbComponentService: TbComponentService,
    changeDetectorRef: ChangeDetectorRef,
    public bodyEditService: BodyEditService
  ) {
    super(enumsService, layoutService, tbComponentService, changeDetectorRef, bodyEditService);
  }

  getWidth() {
    let length = this.bodyEditService.getColumnLength(this.columnName, this.title);
    return length * 10;
  }
}

