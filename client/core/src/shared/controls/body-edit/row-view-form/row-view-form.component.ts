import { FormMode } from './../../../models/form-mode.enum';
import { addModelBehaviour } from './../../../../shared/models/control.model';
import { untilDestroy } from './../../../commons/untilDestroy';

import { Observable } from 'rxjs/Rx';
import { ControlComponent } from './../../control.component';

import { Store } from './../../../../core/services/store.service';
import { createSelectorByMap } from './../../../commons/selector';
import { BodyEditColumnComponent } from './../body-edit-column/body-edit-column.component';
import { TbComponentService } from './../../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../../core/services/layout.service';
import { HttpService } from './../../../../core/services/http.service';
import { DocumentService } from './../../../../core/services/document.service';
import { EventDataService } from './../../../../core/services/eventdata.service';
import { BodyEditService } from './../../../../core/services/body-edit.service';

import { Component, OnInit, Input, OnDestroy, ContentChildren, ContentChild, TemplateRef, HostListener, ChangeDetectorRef, ViewChild, AfterContentInit, AfterViewInit, ChangeDetectionStrategy, Directive, ElementRef, ViewEncapsulation } from '@angular/core';
import { Subscription, BehaviorSubject } from '../../../../rxjs.imports';
import { SelectableSettings } from '@progress/kendo-angular-grid/dist/es/selection/selectable-settings';
import { GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import { GridComponent } from '@progress/kendo-angular-grid';
import { RowArgs } from '@progress/kendo-angular-grid/dist/es/rendering/common/row-class';
import * as _ from 'lodash';

const resolvedPromise = Promise.resolve(null); //fancy setTimeout

@Component({

  selector: 'tb-row-view-form',
  templateUrl: './row-view-form.component.html',
  styleUrls: ['./row-view-form.component.scss']
})
export class RowViewFormComponent extends ControlComponent {

  @ContentChild(TemplateRef) rowViewTemplate: TemplateRef<any>;


  constructor(
    public cdr: ChangeDetectorRef,
    public layoutService: LayoutService,
    public tbComponentService: TbComponentService,
    public httpService: HttpService,
    private eventData: EventDataService,
    public changeDetectorRef: ChangeDetectorRef,
    public bodyEditService: BodyEditService
  ) {
    super(layoutService, tbComponentService, cdr);
  }

  closeRowView() {
    this.bodyEditService.setRowViewVisibility(false);
  }

  firstRow() {
    this.bodyEditService.firstRow();
  }

  prevRow() {
    this.bodyEditService.prevRow();
  }

  nextRow() {
    this.bodyEditService.nextRow();
  }

  lastRow() {
    this.bodyEditService.lastRow();
  }

  canFirstRow() {
    return this.bodyEditService.currentDbtRowIdx != 0;
  }

  canPrevRow() {
    return this.bodyEditService.currentDbtRowIdx > 0;
  }
  canNextRow() {
    return this.bodyEditService.currentDbtRowIdx < this.bodyEditService.model.rowCount;
  }

  canLastRow() {
    return this.bodyEditService.currentDbtRowIdx != this.bodyEditService.model.rowCount;
  }
}