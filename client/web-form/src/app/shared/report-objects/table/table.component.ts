
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
      'border-left': this.table.title.borders.left ? this.table.title.pen.width + 'px' : '0px',
      'border-right': this.table.title.borders.right ? this.table.title.pen.width + 'px' : '0px',
      'border-bottom': this.table.title.borders.bottom ? this.table.title.pen.width + 'px' : '0px',
      'border-top': this.table.title.borders.top ? this.table.title.pen.width + 'px' : '0px',
      'border-style': 'solid',
      'font-family': this.table.title.font.face,
      'font-size': this.table.title.font.size,
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
      'border-left': this.table.title.borders.left ? column.title.pen.width + 'px' : '0px',
      'border-right': this.table.title.borders.right ? column.title.pen.width + 'px' : '0px',
      'border-bottom': this.table.title.borders.bottom ? column.title.pen.width + 'px' : '0px',
      'border-top': this.table.title.borders.top ? column.title.pen.width + 'px' : '0px',
      'border-style': 'solid',
      'height': (column.title.rect.bottom - column.title.rect.top) + 'px',
      

    };
    return obj;
  }

  getColumnStyle(column: column): any {
    let obj = {
      'border-color': column.title.pen.color,
      'border-left': column.title.borders.left ? column.title.pen.width + 'px' : '0px',
      'border-right': column.title.borders.right ? column.title.pen.width + 'px' : '0px',
      'border-bottom': column.title.borders.bottom ? column.title.pen.width + 'px' : '0px',
      'border-top': column.title.borders.top ? column.title.pen.width + 'px' : '0px',
      'border-style': 'solid'
    };
    return obj;
  }
  getDataStyle(dataItem: any): any {
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