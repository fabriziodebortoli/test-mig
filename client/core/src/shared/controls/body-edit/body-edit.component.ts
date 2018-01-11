import { GridComponent } from '@progress/kendo-angular-grid';
import { BodyEditColumnComponent } from './../body-edit-column/body-edit-column.component';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { Component, OnInit, Input, OnDestroy, ContentChildren, ChangeDetectorRef, ViewChild, AfterContentInit, ChangeDetectionStrategy, Directive, ElementRef, ViewEncapsulation } from '@angular/core';
import { Subscription } from '../../../rxjs.imports';

import { ControlComponent } from './../control.component';
import { SelectableSettings } from '@progress/kendo-angular-grid/dist/es/selection/selectable-settings';

const resolvedPromise = Promise.resolve(null); //fancy setTimeout

@Component({

  selector: 'tb-body-edit',
  templateUrl: './body-edit.component.html',
  styleUrls: ['./body-edit.component.scss'],
  exportAs: 'body',
  encapsulation: ViewEncapsulation.None
})
export class BodyEditComponent extends ControlComponent implements AfterContentInit {


  @Input() columns: Array<any>;
  public selectableSettings: SelectableSettings;

  @ContentChildren(BodyEditColumnComponent) be_columns;
  @ViewChild(GridComponent) grid;

  public currentRow: any = undefined;
  constructor(public cdr: ChangeDetectorRef, public layoutService: LayoutService, public tbComponentService: TbComponentService) {
    super(layoutService, tbComponentService, cdr);

    this.selectableSettings = {
      checkboxOnly: false,
      mode: "single"
    };
  }

  ngAfterContentInit() {
    resolvedPromise.then(() => {
      let cols = this.be_columns.toArray();
      let internalColumnComponents = [];
      for (let i = 0; i < cols.length; i++) {
        internalColumnComponents.push(cols[i].columnComponent);
      }
      this.grid.columns.reset(internalColumnComponents);
    });
  }

  ben_row_changed(item) {
    // console.log(item);
    this.currentRow = item.selectedRows[0].dataItem;
  }
}