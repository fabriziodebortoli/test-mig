
import { GridModule } from '@progress/kendo-angular-grid';
import { table, column, cell } from './../../../reporting-studio/reporting-studio.model';
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

  // -----------------------------------------------------
  getValue(dataItem: any, colId: any, colIndex: number): any {
    try {
      return dataItem[colIndex][colId].value;
    } catch (err) {
      return 'ERROR';
    }
  }

  // -----------------------------------------------------
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
      'font-size': this.table.title.font.size + 'px',
      'font-style': this.table.title.font.italic ? 'italic' : 'normal',
      'font-weight': this.table.title.font.bold ? 'bold' : 'normal',
      'text-decoration': this.table.title.font.underline ? 'underline' : 'none',
      'text-align': this.table.title.vertical_align
    };

    return obj;
  }

  // -----------------------------------------------------
  getWidth(): any {
    return this.table.rect.right - this.table.rect.left;
  }
  // -----------------------------------------------------
  getTableStyle(): any {
    let obj = {
      'margin': '1em',
      'position': 'absolute',
      'top': this.table.rect.top + 'px',
      'left': this.table.rect.left + 'px',
      'width': this.table.rect.right - this.table.rect.left + 'px',
      // 'height': this.table.rect.bottom - this.table.rect.top + 'px',

      /*'border-color': 'black', //column.title.pen.color,
            'border-left': this.table.table_title_border.left ? column.title.pen.width + 'px' : '0px',
            'border-right': this.table.table_title_border.right ? column.title.pen.width + 'px' : '0px',
            'border-bottom': this.table.table_title_border.bottom ? column.title.pen.width + 'px' : '0px',
            'border-top': this.table.table_title_border.top ? column.title.pen.width + 'px' : '0px',
            'border-style': 'solid'*/
    };

    return obj;
  }

  // -----------------------------------------------------
  getColumnHeaderStyle(column: column): any {
    let obj = {
      'font-family': column.title.font.face,
      'font-size': column.title.font.size + 'px',
      'font-style': column.title.font.italic ? 'italic' : 'normal',
      'font-weight': column.title.font.bold ? 'bold' : 'normal',
      'text-decoration': column.title.font.underline ? 'underline' : 'none',
      'color': column.title.textcolor,
      'background-color': column.title.bkgcolor,
      'border-color': column.title.pen.color,
      'border-left': column.title.borders.left ? column.title.pen.width + 'px' : '0px',
      'border-right': column.title.borders.right ? column.title.pen.width + 'px' : '0px',
      'border-bottom': column.title.borders.bottom ? column.title.pen.width + 'px' : '0px',
      'border-top': column.title.borders.top ? column.title.pen.width + 'px' : '0px',
      'border-style': 'solid',
      'height': (column.title.rect.bottom - column.title.rect.top) + 'px',
      'padding': '0px',
      'text-align': column.title.text_align,
      'vertical-align': column.title.vertical_align
    };
    return obj;
  }

  getCellsStyle(): any {
    let obj = {
      'text-align': 'center',
      'padding': '0px',
      'height': this.table.row_height + 'px',
      'width': '100%'
    };

    return obj;
  }

  // -----------------------------------------------------
  getBackGroundSingleCellStyle(dataItem: any, rowIndex: number, colId: string): any {
    let defStyle: cell = this.findDefaultStyle(colId, rowIndex);
    let specStyle: any = dataItem[colId];

    let obj = {
      'height': '100%',
      'width': '100%',
      'background-color': specStyle.bkgcolor === undefined ? defStyle.bkgcolor : specStyle.bkgcolor,
      'border-color': defStyle.pen.color,
      'border-left': specStyle.borders !== undefined ? (specStyle.borders.left ? defStyle.pen.width + 'px' : '0px') : (defStyle.borders.left ? defStyle.pen.width + 'px' : '0px'),
      'border-right': specStyle.borders !== undefined ? (specStyle.borders.right ? defStyle.pen.width + 'px' : '0px') : (defStyle.borders.right ? defStyle.pen.width + 'px' : '0px'),
      'border-bottom': specStyle.borders !== undefined ? (specStyle.borders.bottom ? defStyle.pen.width + 'px' : '0px') : (defStyle.borders.bottom ? defStyle.pen.width + 'px' : '0px'),
      'border-top': specStyle.borders !== undefined ? (specStyle.borders.top ? defStyle.pen.width + 'px' : '0px') : (defStyle.borders.top ? defStyle.pen.width + 'px' : '0px'),
      'border-style': 'solid',
      'vertical-align': defStyle.vertical_align,
      'padding-left': '2px',
    };
    return obj;
  }

  // -----------------------------------------------------
  getDataStyle(dataItem: any, rowIndex: number, colId: string): any {
    let defStyle: cell = this.findDefaultStyle(colId, rowIndex);
    let specStyle: any = dataItem[colId];

    let obj = {
      'text-align': defStyle.text_align,
      'color': specStyle.textcolor === undefined ? defStyle.textcolor : specStyle.textcolor,
      'background-color': specStyle.bkgcolor === undefined ? defStyle.bkgcolor : specStyle.bkgcolor,
      'font-family': specStyle.font === undefined ? defStyle.font.face : specStyle.font.face,
      'font-size': specStyle.font === undefined ? (defStyle.font.size + 'px') : (specStyle.font.size + 'px'),
      'font-style': specStyle.font === undefined ? (defStyle.font.italic ? 'italic' : 'normal') : (specStyle.font.italic ? 'italic' : 'normal'),
      'font-weight': specStyle.font === undefined ? (defStyle.font.bold ? 'bold' : 'normal') : (specStyle.font.bold ? 'bold' : 'normal'),
      'text-decoration': specStyle.font === undefined ? (defStyle.font.underline ? 'underline' : 'none') : (specStyle.font.underline ? 'underline' : 'none'),

    };
    return obj;
  }

  // -----------------------------------------------------
  private findDefaultStyle(id: string, rowIndex: number): cell {
    for (let index = 0; index < this.table.defaultStyle.length; index++) {
      let element = this.table.defaultStyle[index];
      if (element.rowNumber === rowIndex) {
        for (let i = 0; i < element.style.length; i++) {
          let elementCell = element.style[i];
          if (elementCell.id === id) {
            return elementCell;
          }
        }
      }
    }
    return undefined;
  }
}

