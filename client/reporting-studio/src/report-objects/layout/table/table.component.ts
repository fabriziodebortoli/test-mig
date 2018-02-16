import { cell } from './../../../models/cell.model';
import { column } from './../../../models/column.model';
import { table } from './../../../models/table.model';
import { UtilsService, InfoService } from '@taskbuilder/core';

import { Component, Input, ViewEncapsulation } from '@angular/core';

@Component({
  selector: 'rs-table',
  templateUrl: './table.component.html',
  encapsulation: ViewEncapsulation.None,
  styleUrls: ['./table.component.scss']
})
export class ReportTableComponent {

  @Input() table: table;
  src: string
  constructor(public utils: UtilsService, public infoService: InfoService) { }

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

    let rgbaBkgColor = this.utils.hexToRgba(this.table.title.bkgcolor);
    let backgroundColor = 'rgba(' + rgbaBkgColor.r + ',' + rgbaBkgColor.g + ',' +rgbaBkgColor.b + ',' + 1 + ')';
    let rgbaTextColor = this.utils.hexToRgba(this.table.title.textcolor);
    let textColor = 'rgba(' + rgbaTextColor.r + ',' + rgbaTextColor.g + ',' +rgbaTextColor.b + ',' + 1 + ')';
  
    let obj = {
      'height': this.table.title.rect.bottom - this.table.title.rect.top + 'px',
      'border-left': this.table.title.borders.left ? this.table.title.pen.width + 'px' : '0px',
      'border-right': this.table.title.borders.right ? this.table.title.pen.width + 'px' : '0px',
      'border-bottom': this.table.title.borders.bottom ? this.table.title.pen.width + 'px' : '0px',
      'border-top': this.table.title.borders.top ? this.table.title.pen.width + 'px' : '0px',
      'border-color': this.table.title.pen.color,
      'border-style': 'solid',
      'font-family': this.table.title.font.face,
      'font-size': this.table.title.font.size + 'px',
      'font-style': this.table.title.font.italic ? 'italic' : 'normal',
      'font-weight': this.table.title.font.bold ? 'bold' : 'normal',
      'text-decoration': this.table.title.font.underline ? 'underline' : 'none',
      'text-align': this.table.title.text_align,
      'vertical-align': this.table.title.vertical_align,
      'background-color': backgroundColor,
      'color': textColor
    };

    return obj;
  }

  // -----------------------------------------------------
  getTableStyle(): any {
    let obj = {
      'position': 'absolute',
      'top': this.table.rect.top + 'px',
      'left': this.table.rect.left + 'px',
      'width': this.getColumWidth(this.table.columns) + 2 +'px'
    };
    return obj;
  }

  getColumWidth(columns : column[]){
    let widthTotal = 0;
    for(let index = 0; index < columns.length; index++){
      if(!columns[index].hidden)
        widthTotal += columns[index].width;
    }
    return widthTotal;
  }

  // -----------------------------------------------------
  getColumnHeaderStyle(column: column): any {
    if (column.hidden)
        return {};
    let obj = {
      'text-decoration': column.title.font.underline ? 'underline' : 'none',
      'color': column.title.textcolor,
      'border-left': column.title.borders.left ? column.title.pen.width + 'px' : '0px',
      'border-right': column.title.borders.right ? column.title.pen.width + 'px' : '0px',
      'border-bottom': column.title.borders.bottom ? column.title.pen.width + 1 + 'px' : '0px',
      'border-top': column.title.borders.top ? column.title.pen.width + 'px' : '0px',
      'border-color': column.title.pen.color,
      'background-color': column.title.bkgcolor,
      'border-style': 'solid',
      'height': (column.title.rect.bottom - column.title.rect.top) + 'px',
      'padding': '0px',
      'text-align': column.title.text_align,
      'vertical-align': column.title.vertical_align
    };
    return obj;
  }

  // -----------------------------------------------------
  getColumnHeaderFont(column: column): any {
    if (column.hidden)
        return {};
    let obj = {
      'font-family': column.title.font.face,
      'font-size': column.title.font.size + 'px',
      'font-style': column.title.font.italic ? 'italic' : 'normal',
      'font-weight': column.title.font.bold ? 'bold' : 'normal',
    };
    return obj;
  }

  // -----------------------------------------------------
  getCellsStyle(column: column): any {
    let obj = {
      'padding': '0px',
      'border': '0px',
      'height': this.table.row_height + 'px',
    };
    return obj;
  }

  // -----------------------------------------------------
  getSingleCellStyle(dataItem: any, rowIndex: number, column: column): any {
    const defStyle: cell = this.findDefaultStyle(column.id, rowIndex);
    const specStyle: any = dataItem[column.id];

    const bk = specStyle.bkgcolor === undefined ? defStyle.bkgcolor : specStyle.bkgcolor;
    const rgba = this.utils.hexToRgba(bk);
    rgba.a = this.table.transparent ? 0 : 1;
    const backgroundCol = 'rgba(' + rgba.r + ',' + rgba.g + ',' + rgba.b + ',' + rgba.a + ')';

    let obj = {
      'height': this.table.row_height + 'px',
      'background-color': backgroundCol,
      'border-left': specStyle.borders !== undefined ? (specStyle.borders.left ? defStyle.pen.width + 'px' : '0px') : (defStyle.borders.left ? defStyle.pen.width + 'px' : '0px'),
      'border-right': specStyle.borders !== undefined ? (specStyle.borders.right ? defStyle.pen.width + 'px' : '0px') : (defStyle.borders.right ? defStyle.pen.width + 'px' : '0px'),
      'border-bottom': specStyle.borders !== undefined ? (specStyle.borders.bottom ? defStyle.pen.width + 'px' : '0px') : (defStyle.borders.bottom ? defStyle.pen.width + 'px' : '0px'),
      'border-top': specStyle.borders !== undefined ? (specStyle.borders.top ? defStyle.pen.width + 'px' : '0px') : (defStyle.borders.top ? defStyle.pen.width + 'px' : '0px'),
      'border-color': defStyle.pen.color,
      'border-style': 'solid',
      'vertical-align': defStyle.vertical_align,
      'text-align': defStyle.text_align,
      'color': specStyle.textcolor === undefined ? 
               specStyle.font !== undefined && specStyle.font.fontcolor !== undefined ? specStyle.font.fontcolor : defStyle.textcolor
               :
               specStyle.textcolor,
      'text-decoration': specStyle.font === undefined ? (defStyle.font.underline ? 'underline' : 'none') : (specStyle.font.underline ? 'underline' : 'none'),
      'padding': '0px'
    };

    if (column.value_is_image) {
      //this.image.src = 'http://www.jqueryscript.net/images/Simplest-Responsive-jQuery-Image-Lightbox-Plugin-simple-lightbox.jpg';
      dataItem[column.id].src = this.infoService.getBaseUrl() + '/rs/image/' + dataItem[column.id].value;
    }
    return obj;
  }

  // -----------------------------------------------------
  getSingleCellFont(dataItem: any, rowIndex: number, column: column){
    const defStyle: cell = this.findDefaultStyle(column.id, rowIndex);
    const specStyle: any = dataItem[column.id];
    let obj = {
      'font-family': specStyle.font === undefined ? defStyle.font.face : specStyle.font.face,
      'font-size': specStyle.font === undefined ? (defStyle.font.size + 'px') : (specStyle.font.size + 'px'),
      'font-style': specStyle.font === undefined ? (defStyle.font.italic ? 'italic' : 'normal') : (specStyle.font.italic ? 'italic' : 'normal'),
      'font-weight': specStyle.font === undefined ? (defStyle.font.bold ? 'bold' : 'normal') : (specStyle.font.bold ? 'bold' : 'normal'),
    };

    return obj;

  }

  // -----------------------------------------------------
  getImageStyle() {
    let obj = {
      'max-width': ' 100%',
      'max-height': '100%'
    }
    return obj;
  }

  // -----------------------------------------------------
  public findDefaultStyle(id: string, rowIndex: number): cell {
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

