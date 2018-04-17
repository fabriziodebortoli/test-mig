import { TabberService } from './../../../core/services/tabber.service';
import { HotLinkInfo } from './../../models/hotLinkInfo.model';
import { WebSocketService } from './../../../core/services/websocket.service';
import { Subject } from 'rxjs/Subject';
import { HyperLinkService, HyperLinkInfo } from './../../../core/services/hyperlink.service';
import { Observable } from 'rxjs/Rx';
import { Component, OnInit, Input, OnDestroy, QueryList, ContentChildren, HostListener, ChangeDetectorRef, ViewChild, AfterContentInit, AfterViewInit, ChangeDetectionStrategy, Directive, ElementRef, ViewEncapsulation, SkipSelf } from '@angular/core';
import { Subscription, BehaviorSubject } from '../../../rxjs.imports';
import { SelectableSettings } from '@progress/kendo-angular-grid/dist/es/selection/selectable-settings';
import { GridDataResult, PageChangeEvent } from '@progress/kendo-angular-grid';
import { GridComponent } from '@progress/kendo-angular-grid';
import { RowArgs } from '@progress/kendo-angular-grid/dist/es/rendering/common/row-class';
import * as _ from 'lodash';

import { FormMode } from './../../models/form-mode.enum';
import { addModelBehaviour } from './../../../shared/models/control.model';
import { untilDestroy } from './../../commons/untilDestroy';
import { ControlComponent } from './../control.component';
import { Store } from './../../../core/services/store.service';
import { BodyEditColumnComponent } from './body-edit-column/body-edit-column.component';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { HttpService } from './../../../core/services/http.service';
import { DocumentService } from './../../../core/services/document.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { BodyEditService } from './../../../core/services/body-edit.service';

const resolvedPromise = Promise.resolve(null); //fancy setTimeout

@Component({
  selector: 'tb-body-edit',
  templateUrl: './body-edit.component.html',
  styleUrls: ['./body-edit.component.scss'],
  providers: [BodyEditService]
})
export class BodyEditComponent extends ControlComponent implements AfterContentInit, OnDestroy {

  @ContentChildren(BodyEditColumnComponent) be_columns: QueryList<BodyEditColumnComponent>;
  @ViewChild(GridComponent) grid;

  @Input() bodyEditName: string;
  @Input() height: number;
  @Input() columns: Array<any>;
  public selectableSettings: SelectableSettings;

  isRowSelected = (e: RowArgs) => e.index == this.bodyEditService.currentDbtRowIdx;
  fakeRows = [];
  subscriptions = [];

  private numberOfColumns: number = 0;
  private preventLoseFocus: boolean = false;

  public pageSizes = false;
  public previousNext = true;

  public gridView: GridDataResult = { data: [], total: 0 };

  constructor(
    public cdr: ChangeDetectorRef,
    public layoutService: LayoutService,
    public wsService: WebSocketService,
    public tbComponentService: TbComponentService,
    public httpService: HttpService,
    public store: Store,
    private eventData: EventDataService,
    public bodyEditService: BodyEditService,
    public tabberService: TabberService
  ) {
    super(layoutService, tbComponentService, cdr);
    this.selectableSettings = { checkboxOnly: false, mode: "single" };
  }

  //-----------------------------------------------------------------------------------------------
  @HostListener('keydown', ['$event'])
  public keyup(event: KeyboardEvent): void {
    if ((event.shiftKey && event.keyCode == 9) || event.keyCode == 37) {
      this.editPreviousCell();
      return;
    }
    if (event.keyCode === 9) {
      this.editNextCell();
      return;
    }

    if ((event.ctrlKey && event.keyCode == 38) || event.keyCode == 30) {
      this.editAboveCell();
      return;
    }

    if (event.ctrlKey && event.keyCode == 40) {
      this.editBelowCell();
      return;
    }
  }

  //-----------------------------------------------------------------------------------------------
  subscribeToSelector() {
    this.store.select('FormMode.value')
      .subscribe(m => {
        this.bodyEditService.enabled = (m === FormMode.EDIT || m === FormMode.NEW) && (this.model && this.model.enabled);
        this.changeDetectorRef.markForCheck();
      });

    this.subscriptions.push(this.be_columns.changes.subscribe(() => {
      this.resetBodyEditColumns();
    }));

    this.subscriptions.push(this.eventData.activationChanged.subscribe(() => {
      this.changeDetectorRef.detectChanges();
      this.resetBodyEditColumns();
    }));

    let name = this.bodyEditName;
    this.store.select(name)
      .subscribe(m => {
        if (!m)
          return;

        //todoluca, devo solo verificare di non fare subscribe multiple
        if (m.modelChanged) {
          m.modelChanged
            .pipe(untilDestroy(this))
            .subscribe(() => {
              this.updateModel(this.model);
              this.bodyEditService.isLoading = false;
            });
        }
      });
  }

  //-----------------------------------------------------------------------------------------------
  ngOnDestroy() {
  }

  //-----------------------------------------------------------------------------------------------
  ngAfterContentInit() {
    this.bodyEditService.bodyEditName = this.bodyEditName;
    let numberOfVisibleRows = Math.ceil(this.height / this.bodyEditService.rowHeight);
    this.createFakeRows(numberOfVisibleRows);
    this.bodyEditService.pageSize = Math.max(this.bodyEditService.pageSize, numberOfVisibleRows);

    if (this.bodyEditService.skip < 0) {
      this.bodyEditService.skip = 0;
      this.bodyEditService.changeDBTRange(this.bodyEditService.skip, this.bodyEditService.pageSize, 0);
      this.bodyEditService.isLoading = false;
    }

    this.resetBodyEditColumns();

    this.subscribeToSelector();
  }

  resetBodyEditColumns() {
    setTimeout(() => {
      let cols = this.be_columns.toArray();
      let internalColumnComponents = [];
      for (let i = 0; i < cols.length; i++) {
        let currentCol = cols[i];
        if (currentCol.activated && !currentCol.hidden)
          internalColumnComponents.push(currentCol.columnComponent);
      }
      this.grid.columns.reset(internalColumnComponents);
      this.changeDetectorRef.markForCheck();
    }, 1);
  }


  //-----------------------------------------------------------------------------------------------
  ngAfterViewInit() {
    super.ngAfterViewInit();
    this.changeDetectorRef.markForCheck();
  }

  //-----------------------------------------------------------------------------------------------
  public cellClickHandler({ sender, rowIndex, columnIndex, dataItem, isEdited }) {
    if (!isEdited) {
      let colComponent = this.grid.columns.toArray()[columnIndex];
      let colName = colComponent.field;
      let isEnabled = this.bodyEditService.currentRow[colName].enabled && this.bodyEditService.enabled
      if (isEnabled) {
        this.bodyEditService.lastEditedRowIndex = rowIndex;
        this.bodyEditService.lastEditedColumnIndex = columnIndex;
        sender.editCell(rowIndex, columnIndex);
      }
    }
  }

  //-----------------------------------------------------------------------------------------------
  public cellCloseHandler(args: any) {
    if (this.preventLoseFocus) {
      args.preventDefault();
    }
    // this.lastEditedRowIndex = -1;
    // this.lastEditedColumnIndex = -1;
  }

  //-----------------------------------------------------------------------------------------------
  createFakeRows(numberOfVisibleRows: number) {
    for (let index = 0; index < numberOfVisibleRows; index++) {
      this.fakeRows.push({})
    }
  }



  //-----------------------------------------------------------------------------------------------
  addRow() {
    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;

    let tempCount = this.bodyEditService.currentDbtRowIdx + 1;
    let sub = this.httpService.addRowDBTSlaveBuffered(docCmpId, this.bodyEditName, this.bodyEditService.skip, this.bodyEditService.pageSize, tempCount).subscribe((res) => {
      sub.unsubscribe();
    });
  }

  //-----------------------------------------------------------------------------------------------
  removeRow() {
    if (this.bodyEditService.currentDbtRowIdx < 0)
      return;

    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
    let sub = this.httpService.removeRowDBTSlaveBuffered(docCmpId, this.bodyEditName, this.bodyEditService.skip, this.bodyEditService.pageSize, this.bodyEditService.currentDbtRowIdx).subscribe((res) => {
      sub.unsubscribe();
    });
  }

  //-----------------------------------------------------------------------------------------------
  openRowView() {
    this.bodyEditService.setRowViewVisibility(true);
  }

  //-----------------------------------------------------------------------------------------------
  enableMultiselection() {

  }

  //-----------------------------------------------------------------------------------------------
  enhancedView() {

  }

  //-----------------------------------------------------------------------------------------------
  increaseRowHeight() {

    this.bodyEditService.increaseRowHeight();
  }

  //-----------------------------------------------------------------------------------------------
  decreaseRowHeight() {

    this.bodyEditService.decreaseRowHeight();
  }

  //-----------------------------------------------------------------------------------------------
  editAboveCell() {
    if (this.bodyEditService.lastEditedRowIndex < 0 || this.bodyEditService.lastEditedColumnIndex < 0) {
      return;
    }

    if (this.bodyEditService.lastEditedRowIndex > 0) {
      this.bodyEditService.lastEditedRowIndex--;
      this.bodyEditService.prevRow();
      this.grid.editCell(this.bodyEditService.lastEditedRowIndex, this.bodyEditService.lastEditedColumnIndex);
      this.focusCell(this.bodyEditService.lastEditedColumnIndex);
    }
  }

  //-----------------------------------------------------------------------------------------------
  editBelowCell() {
    if (this.bodyEditService.lastEditedRowIndex < 0 || this.bodyEditService.lastEditedColumnIndex < 0) {
      return;
    }

    if (this.bodyEditService.lastEditedRowIndex < this.bodyEditService.rowCount - 1) {
      this.bodyEditService.lastEditedRowIndex++;
      this.bodyEditService.nextRow();
      this.grid.editCell(this.bodyEditService.lastEditedRowIndex, this.bodyEditService.lastEditedColumnIndex);
      this.focusCell(this.bodyEditService.lastEditedColumnIndex);
    }
  }

  //-----------------------------------------------------------------------------------------------
  editNextCell() {
    if (this.bodyEditService.lastEditedRowIndex < 0 || this.bodyEditService.lastEditedColumnIndex < 0) {
      return;
    }

    if (this.bodyEditService.lastEditedColumnIndex < this.grid.columns.length) {
      this.bodyEditService.lastEditedColumnIndex++;
    }
    else {
      this.bodyEditService.lastEditedColumnIndex = 0

      this.bodyEditService.lastEditedRowIndex++;
      this.bodyEditService.nextRow();
    }

    if (!this.bodyEditService.currentActiveControlComponent)
      return;

    //se non ho hotlink, edito la nextcell
    if (!this.bodyEditService.currentActiveControlComponent.hotLink) {
      this.grid.editCell(this.bodyEditService.lastEditedRowIndex, this.bodyEditService.lastEditedColumnIndex);
      this.focusCell(this.bodyEditService.lastEditedColumnIndex);
      return;
    }

    this.preventLoseFocus = true;
    let destroyer = { ngOnDestroy() { } };
    let hls = HyperLinkService.New(this.wsService, this.eventData);
    let elem = document.activeElement;
    let docCmpId = (this.tbComponentService as DocumentService).mainCmpId;
    hls.workingValue = this.bodyEditService.currentActiveControlComponent.model.value;
    const lastTabberIndex = this.tabberService.currentIndex;
    const tempModel = this.bodyEditService.currentActiveControlComponent.model;
    const controlId = this.bodyEditService.currentActiveControlComponent.cmpId;
    //let hlInfo =  as HyperLinkInfo;
    hls.start(destroyer,
      () => { return elem as HTMLElement },
      null,
      { ...(this.bodyEditService.currentActiveControlComponent as any).hotLink, cmpId: docCmpId, controlId: this.bodyEditService.currentActiveControlComponent.cmpId, mustExistData: true, model: this.bodyEditService.currentActiveControlComponent.model },
      Observable.of({ value: hls.workingValue, selector: '', type: 'direct', enabled: tempModel.enabled }),
      (_, __) => {
        console.log("givefocus");
        destroyer.ngOnDestroy();
        hls.stop();
        this.eventData.change.emit(controlId);
        this.preventLoseFocus = false;
      },
      value => {

        this.tabberService.selectTab(lastTabberIndex);
        tempModel.value = value;
        this.eventData.change.emit(controlId);

        this.preventLoseFocus = false;
        this.grid.editCell(this.bodyEditService.lastEditedRowIndex, this.bodyEditService.lastEditedColumnIndex);
        this.focusCell(this.bodyEditService.lastEditedColumnIndex);
        destroyer.ngOnDestroy();
        hls.stop();
        this.changeDetectorRef.detectChanges();

      });
  }

  //-----------------------------------------------------------------------------------------------
  editPreviousCell() {
    if (this.bodyEditService.lastEditedRowIndex < 0 || this.bodyEditService.lastEditedColumnIndex < 0) {
      return;
    }

    if (this.bodyEditService.lastEditedColumnIndex > 0) {
      this.bodyEditService.lastEditedColumnIndex--;

    }
    else {
      this.bodyEditService.lastEditedColumnIndex = this.grid.columns.length;
      this.bodyEditService.lastEditedRowIndex--;
      this.bodyEditService.prevRow();
    }

    this.grid.editCell(this.bodyEditService.lastEditedRowIndex, this.bodyEditService.lastEditedColumnIndex);
    this.focusCell(this.bodyEditService.lastEditedColumnIndex);
  }

  focusCell(index: number) {
    setTimeout(() => {
      var element = <HTMLElement>document.querySelector(`.k-grid-edit-row > td:nth-child(${index + 1}) input`);
      if (element)
        element.focus();
      else {
        element = <HTMLElement>document.querySelector(`.k-grid-edit-row > td:nth-child(${index + 1}) textarea`);
        if (element)
          element.focus();
      }
    });
  }

  //-----------------------------------------------------------------------------------------------
  private updateModel(model: any) {

    if (!model) {
      return;
    }

    this.bodyEditService.setModel(model);

    this.gridView = {
      data: this.bodyEditService.rows,
      total: this.bodyEditService.rowCount
    };

    this.changeDetectorRef.markForCheck();
  }
}