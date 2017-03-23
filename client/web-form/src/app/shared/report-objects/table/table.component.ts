import { GridModule } from '@progress/kendo-angular-grid';
import { table, column, borders } from './../../../reporting-studio/reporting-studio.model';
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
    try {
      return dataItem[colIndex][colId].value;
    } catch (err) {
      return 'ERROR';
    }
  }


  getTitleStyle(): any {
    let obj = {
      'margin-bottom': '0.5em',
      'align-content': 'center',
      'width': '100%',

      'border-color': this.table.title.pen.color,
      'border-left': this.table.table_title_border.left ? this.table.title.pen.width + 'px' : '0px',
      'border-right': this.table.table_title_border.right ? this.table.title.pen.width + 'px' : '0px',
      'border-bottom': this.table.table_title_border.bottom ? this.table.title.pen.width + 'px' : '0px',
      'border-top': this.table.table_title_border.top ? this.table.title.pen.width + 'px' : '0px',
      'border-style': 'solid',
      'font-family': this.table.title.font.face,
      'font-size': this.table.title.font.size ,
      'font-style': this.table.title.font.italic ? 'italic' : 'normal',
      'font-weight': this.table.title.font.bold ? 'bold' : 'normal',
      'text-decoration': this.table.title.font.underline ? 'underline' : 'none',
      'text-align': 'center'
    };

    return obj;
  }
  getTableStyle(): any {
    let obj = {
      'margin-top': '1em',
      /*'border-color': 'black', //column.title.pen.color,
            'border-left': this.table.table_title_border.left ? column.title.pen.width + 'px' : '0px',
            'border-right': this.table.table_title_border.right ? column.title.pen.width + 'px' : '0px',
            'border-bottom': this.table.table_title_border.bottom ? column.title.pen.width + 'px' : '0px',
            'border-top': this.table.table_title_border.top ? column.title.pen.width + 'px' : '0px',
            'border-style': 'solid'*/
    };

    return obj;
  }

  getColumnHeaderStyle(column: column): any {
    let obj = {
      'font-family': column.title.font.face,
      'font-size': column.title.font.size,
      'font-style': column.title.font.italic ? 'italic' : 'normal',
      'font-weight': column.title.font.bold ? 'bold' : 'normal',
      'text-decoration': column.title.font.underline ? 'underline' : 'none',
      'color': column.title.textcolor,
      'background-color': column.title.bkgcolor,
      'border-color': column.title.pen.color,
      'border-left': this.table.table_title_border.left ? column.title.pen.width + 'px' : '0px',
      'border-right': this.table.table_title_border.right ? column.title.pen.width + 'px' : '0px',
      'border-bottom': this.table.table_title_border.bottom ? column.title.pen.width + 'px' : '0px',
      'border-top': this.table.table_title_border.top ? column.title.pen.width + 'px' : '0px',
      'border-style': 'solid'

    };
    return obj;
  }

  getColumnStyle(column: column): any {
    let obj = {
      'border-color': column.title.pen.color,
      'border-left': column.borders.left ? column.pen.width + 'px' : '0px',
      'border-right': column.borders.right ? column.pen.width + 'px' : '0px',
      'border-bottom': column.borders.bottom ? column.pen.width + 'px' : '0px',
      'border-top': column.borders.top ? column.pen.width + 'px' : '0px',
      'border-style': 'solid'
    };
    return obj;
  }
  getDataStyle(dataItem: any, col: column) {
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