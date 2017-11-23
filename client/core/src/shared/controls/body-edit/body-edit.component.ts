import { GridComponent } from '@progress/kendo-angular-grid';
import { BodyEditColumnComponent } from './../body-edit-column/body-edit-column.component';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { Component, OnInit, Input, OnDestroy, ContentChildren, ChangeDetectorRef, ViewChild, AfterContentInit } from '@angular/core';
import { Subscription } from '../../../rxjs.imports';

import { ControlComponent } from './../control.component';

const resolvedPromise = Promise.resolve(null); //fancy setTimeout


@Component({
  selector: 'tb-body-edit',
  templateUrl: './body-edit.component.html',
  styleUrls: ['./body-edit.component.scss']
})
export class BodyEditComponent extends ControlComponent implements AfterContentInit {
  @Input() columns: Array<any>;

  @ContentChildren(BodyEditColumnComponent) be_columns;
  @ViewChild(GridComponent) grid;

  constructor(public cdr: ChangeDetectorRef, public layoutService: LayoutService, public tbComponentService: TbComponentService) {
    super(layoutService, tbComponentService, cdr);
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
}



