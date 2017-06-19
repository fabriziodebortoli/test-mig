import { BodyEditService } from './../../../core/body-edit.service';
import { Observable } from 'rxjs/Rx';
import { BOService } from './../../../core/bo.service';
import { ControlComponent } from './../control.component';
import { Component, OnInit, Input, OnDestroy, Inject } from '@angular/core';
import { State, process } from '@progress/kendo-data-query';

import {
  GridDataResult,
  DataStateChangeEvent
} from '@progress/kendo-angular-grid';
@Component({
  selector: 'tb-body-edit',
  templateUrl: './body-edit.component.html',
  styleUrls: ['./body-edit.component.scss'],
  providers: [BodyEditService]
})
export class BodyEditComponent extends ControlComponent implements OnInit {
   @Input() columns: Array<any>;
  public view: Observable<GridDataResult>;
  public gridState: State = {
    sort: [],
    skip: 0,
    take: 10
  };


  private editedRowIndex: number;
  private editedProduct: any;

  constructor(private editService: BodyEditService) {
    super();
  }

  public ngOnInit(): void {
    this.view = this.editService.map(data => process(data, this.gridState));

    this.editService.read();
  }

  public onStateChange(state: State) {
    this.gridState = state;

    this.editService.read();
  }

  protected addHandler({ sender }) {
    this.closeEditor(sender);

    sender.addRow({});
  }

  protected editHandler({ sender, rowIndex, dataItem }) {
    this.closeEditor(sender);

    this.editedRowIndex = rowIndex;
    this.editedProduct = Object.assign({}, dataItem);

    sender.editRow(rowIndex);
  }

  protected cancelHandler({ sender, rowIndex }) {
    this.closeEditor(sender, rowIndex);
  }

  private closeEditor(grid, rowIndex = this.editedRowIndex) {
    grid.closeRow(rowIndex);
    this.editService.resetItem(this.editedProduct);
    this.editedRowIndex = undefined;
    this.editedProduct = undefined;
  }

  protected saveHandler({ sender, rowIndex, dataItem, isNew }) {
    this.editService.save(dataItem, isNew);

    sender.closeRow(rowIndex);

    this.editedRowIndex = undefined;
    this.editedProduct = undefined;
  }

  protected removeHandler({ dataItem }) {
    this.editService.remove(dataItem);
  }
}