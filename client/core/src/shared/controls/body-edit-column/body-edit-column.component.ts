import { ColumnComponent } from '@progress/kendo-angular-grid';
import { LayoutService } from './../../../core/services/layout.service';
import { Component, OnInit, Input, OnDestroy, ContentChildren, ChangeDetectorRef, ViewChild, AfterContentInit, ViewEncapsulation } from '@angular/core';
import { Subscription } from 'rxjs';

import { ControlComponent } from './../control.component';

@Component({
  selector: 'tb-body-edit-column',
  templateUrl: './body-edit-column.component.html',
  styleUrls: ['./body-edit-column.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class BodyEditColumnComponent extends ControlComponent {
  @Input() title: string;

  @Input() columnName: string;
  @Input() columnType: string;

  //queste sono duplicazioni della combocomponent...dovrei riuscire a creare un componente differenziato a seconda del type
  @Input() public itemSource: any = undefined;
  @Input() public hotLink: any = undefined;
  @Input() formatter: string;
  @Input() readonly = false;

  @ViewChild(ColumnComponent) columnComponent;
}

