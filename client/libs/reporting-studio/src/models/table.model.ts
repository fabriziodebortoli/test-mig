import { styleArrayElement } from './style-array-element.model';
import { cell } from './cell.model';
import { ReportObjectType } from './report-object-type.model';
import { baseobj } from './baseobj.model';
import { title } from './title.model';
import { column } from './column.model';


export class table extends baseobj {
    column_number: number;
    row_number: number;
    row_height: number;
    title: title;
    hide_columns_title: boolean;
    fiscal_end: boolean;
    value: any[] = [];
    columns: column[] = [];
    defaultStyle: styleArrayElement[] = [];

    constructor(jsonObj: any) {
        super(jsonObj.baseobj);
        this.obj = ReportObjectType.table;
        this.column_number = jsonObj.column_number;
        this.row_number = jsonObj.row_number;
        this.row_height = jsonObj.row_height;
        this.title = jsonObj.title ? new title(jsonObj.title) : undefined;
        this.hide_columns_title = jsonObj.hide_columns_title;
        this.fiscal_end = jsonObj.fiscal_end;

        for (let index = 0; index < jsonObj.columns.length; index++) {
            let element = jsonObj.columns[index];
            let col = new column(element);
            this.columns.push(col);
        }

        for (let index = 0; index < jsonObj.rows.length; index++) {

            let elementRow = jsonObj.rows[index];
            let style = new styleArrayElement(index);
            for (let i = 0; i < elementRow.length; i++) {
                try {
                    let colId = jsonObj.columns[i].id;
                    let cellVar = new cell(elementRow[i][colId], colId);
                    style.style.push(cellVar);
                }
                catch (a) {
                    let k = a;
                }
            }
            this.defaultStyle.push(style);

        }
    }
}