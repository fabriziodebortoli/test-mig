import { GridModule } from '@progress/kendo-angular-grid';
import { table, column } from './../../../reporting-studio/reporting-studio.model';
import { Component, Input } from '@angular/core';
declare var $: any;
@Component({
  selector: 'rs-table',
  templateUrl: './table.component.html',
  styleUrls: ['./table.component.scss']
})
export class ReportTableComponent {

  @Input() table: table;



  constructor() { }

  getValue(dataItem: any, colId: any, colIndex: number): any {
    return dataItem[colIndex][colId].value;
  }

  getTableStyle(): any {
    let obj = {
      
    };

    return obj;
  }

  getColumnStyle(column: column): any {
    let obj = {
      'font-family': column.title.font.face,
      'font-size': column.title.font.size + 'px',
      'font-style': column.title.font.italic ? 'italic' : 'normal',
      'font-weight': column.title.font.bold ? 'bold' : 'normal',
      'text-decoration': column.title.font.underline ? 'underline' : 'none',
    };

    return obj;
  }

  getDataStyle(dataItem: any, colId: any, colIndex: number) {
    let obj = {
    };

    return obj;
  }
}

class ElObj {
  id: string;
  item: any;

  constructor(id: string, item: any) {
    this.id = id;
    this.item = item;
  }
}