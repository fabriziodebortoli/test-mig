import { BodyEditColumnComponent } from './../body-edit-column/body-edit-column.component';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { EnumsService } from './../../../core/services/enums.service';
import { ColumnComponent } from '@progress/kendo-angular-grid';
import { LayoutService } from './../../../core/services/layout.service';
import { Component, OnInit, Input, OnDestroy, ContentChildren, ContentChild, TemplateRef, forwardRef, ChangeDetectorRef, ViewChild, AfterContentInit, ViewEncapsulation } from '@angular/core';
import { Subscription } from '../../../rxjs.imports';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-body-edit-checkbox-column',
  templateUrl: './body-edit-checkbox-column.component.html',
  styleUrls: ['./body-edit-checkbox-column.component.scss'],
  providers: [{provide: BodyEditColumnComponent, useExisting: forwardRef(() => BodyEditCheckBoxColumnComponent) }]
})
export class BodyEditCheckBoxColumnComponent extends BodyEditColumnComponent {
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
    changeDetectorRef: ChangeDetectorRef
  ) {
    super(enumsService, layoutService, tbComponentService, changeDetectorRef);
  }
}

