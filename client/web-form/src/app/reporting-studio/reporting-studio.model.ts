
export interface Message {
  commandType: CommandType;
  message?: string;
}

export enum CommandType { OK, NAMESPACE, INITTEMPLATE, TEMPLATE, ASK, DATA, STOP }

export enum ReportObjectType { textrect, fieldrect, table, sqrrect, graphrect, repeater }

export class baseobj {

  id: string;
  hidden: boolean;
  transparent: boolean;
  rect: rect;
  tooltip: string;
  shadow_height: number;
  shadow_color: string;

  constructor(jsonObj: any) {

    this.id = jsonObj.id;
    this.hidden = jsonObj.hidden;
    this.transparent = jsonObj.transparent;
    this.rect = new rect(jsonObj.rect);
    this.tooltip = jsonObj.tooltip ? jsonObj.tooltip : '';;
    this.shadow_height = jsonObj.shadow_height;
    this.shadow_color = jsonObj.shadow_color;
  };

}

export class baserect extends baseobj {

  hratio: number;
  vratio: number;
  borders: borders;
  pen: borderpen;

  constructor(jsonObj: any) {
    super(jsonObj.baseobj);

    this.hratio = jsonObj.hratio;
    this.vratio = jsonObj.vratio;
    this.borders = new borders(jsonObj.borders);
    this.pen = new borderpen(jsonObj.pen);
  };
}

export class sqrrect extends baserect {

  obj: ReportObjectType = ReportObjectType.sqrrect;

  bkgcolor: string;

  constructor(jsonObj: any) {
    super(jsonObj.baserect);

    this.bkgcolor = jsonObj.bkgcolor;
  };
}

export class graphrect extends sqrrect {

  obj: ReportObjectType = ReportObjectType.graphrect;

  value: string;
  align: number;

  constructor(jsonObj: any) {
    super(jsonObj.sqrrect);

    this.align = jsonObj.align;
    this.value = jsonObj.value ? jsonObj.value : '';
  };
}

export class repeater extends sqrrect {

  obj: ReportObjectType = ReportObjectType.repeater;

  rows: number;
  columns: number;
  xoffset: number;
  yoffset: number;

  constructor(jsonObj: any) {
    super(jsonObj.sqrrect);

    this.rows = jsonObj.rows;
    this.columns = jsonObj.columns;

    this.xoffset = jsonObj.xoffset;
    this.yoffset = jsonObj.yoffset;
  };
}

export class textrect extends baserect {

  obj: ReportObjectType = ReportObjectType.textrect;
  value: string;
  bkgcolor: string;
  textcolor: string;
  align: number;
  font: font;
  value_is_html: boolean;
  value_is_barcode: boolean;

  constructor(jsonObj: any) {
    super(jsonObj.baserect);
    this.align = jsonObj.align;
    this.value = jsonObj.value ? jsonObj.value : '';
    this.bkgcolor = jsonObj.bkgcolor;
    this.textcolor = jsonObj.textcolor;
    this.font = new font(jsonObj.font);

    this.value_is_html = jsonObj.value_is_html;
    this.value_is_barcode = jsonObj.value_is_barcode;
  };
}

export class fieldrect extends baserect {

  obj: ReportObjectType = ReportObjectType.fieldrect;
  value: string = '';

  label: label;
  font: font;
  align: number;
  bkgcolor: string;
  textcolor: string;

  value_is_html: boolean;
  value_is_image: boolean;
  value_is_barcode: boolean;

  constructor(jsonObj: any) {
    super(jsonObj.baserect);

    this.label = jsonObj.label ? new label(jsonObj.label) : null;

    this.font = new font(jsonObj.font);
    this.align = jsonObj.align;
    this.bkgcolor = jsonObj.bkgcolor;
    this.textcolor = jsonObj.textcolor;

    this.value_is_html = jsonObj.value_is_html;
    this.value_is_image = jsonObj.value_is_image;
    this.value_is_barcode = jsonObj.value_is_barcode;
  };
}

export class table extends baseobj {
  obj: ReportObjectType = ReportObjectType.table;
  column_number: number;
  row_number: number;
  cells_rect: rect;
  table_title_border: borders;
  column_title_border: borders;
  body_border: borders;
  total_border: borders;
  column_title_sep: boolean;
  column_sep: boolean;
  row_sep: boolean;
  row_sep_dynamic: boolean;
  row_sep_pen: borderpen;
  hide_table_title: boolean;
  title: title;
  hide_columns_title: boolean;
  fiscal_end: boolean;

  value: any[] = [];
  columns: column[] = [];

  constructor(jsonObj: any) {
    super(jsonObj.baseobj);
    this.column_number = jsonObj.column_number;
    this.row_number = jsonObj.row_number;
    this.cells_rect = new rect(jsonObj.cells_rect);
    this.table_title_border = new borders(jsonObj.table_borders.table_title);
    this.column_title_border = new borders(jsonObj.table_borders.column_title);
    this.body_border = new borders(jsonObj.table_borders.body);
    this.total_border = new borders(jsonObj.table_borders.total);
    this.column_title_sep = jsonObj.column_title_sep;
    this.column_sep = jsonObj.column_sep;
    this.row_sep = jsonObj.row_sep;
    this.row_sep_dynamic = jsonObj.row_sep_dynamic;
    this.row_sep_pen = jsonObj.row_sep_pen ? new borderpen(jsonObj.row_sep_pen) : jsonObj.row_sep_pen;
    this.hide_table_title = jsonObj.hide_table_title;
    this.title = new title(jsonObj.title);
    this.hide_columns_title = jsonObj.hide_columns_title;
    this.fiscal_end = jsonObj.fiscal_end;

    for (let index = 0; index < jsonObj.columns.length; index++) {
      let element = jsonObj.columns[index];
      let col = new column(element);
      this.columns.push(col);
    }
  }
}

export class column {
  id: string;
  hidden: boolean;
  width: number;
  pen: borderpen;
  borders: borders;
  value_is_html: boolean;
  value_is_image: boolean;
  value_is_barcode: boolean;
  title: column_title;
  total: column_total;
  constructor(jsonObj: any) {
    this.id = jsonObj.id;
    this.width = jsonObj.width;
    this.hidden = jsonObj.hidden;
    this.pen = new borderpen(jsonObj.pen);
    this.borders = new borders(jsonObj.borders);
    this.value_is_html = jsonObj.value_is_html;
    this.value_is_image = jsonObj.value_is_image;
    this.value_is_barcode = jsonObj.value_is_barcode;

    this.title = jsonObj.title === 'undefined' ? null : new column_title(jsonObj.title);
    this.total = jsonObj.total ? new total_cell(jsonObj.total) : null;
  }
}

export class label {
  caption: string;
  textcolor: string;
  font: font;
  align: number;
  constructor(jsonObj: any) {
    this.caption = jsonObj.caption ? jsonObj.caption : '';
    this.textcolor = jsonObj.textcolor;
    this.font = new font(jsonObj.font);
    this.align = jsonObj.align;
  }
}

export class font {
  face: string;
  size: number;
  italic: boolean;
  bold: boolean;
  underline: boolean;
  constructor(jsonObj: any) {
    this.face = jsonObj.face;
    this.size = jsonObj.size;
    this.italic = jsonObj.italic;
    this.bold = jsonObj.bold;
    this.underline = jsonObj.underline;
  }
}

export class borders {
  left: boolean;
  right: boolean;
  top: boolean;
  bottom: boolean;
  constructor(jsonObj: any) {
    this.left = jsonObj.left;
    this.right = jsonObj.right;
    this.top = jsonObj.top;
    this.bottom = jsonObj.bottom;
  }
}

export class borderpen {
  width: number;
  color: string;
  constructor(jsonObj: any) {
    this.width = jsonObj.width;
    this.color = jsonObj.color;
  }
}

export class rect {
  left: number;
  right: number;
  top: number;
  bottom: number;
  constructor(jsonObj: any) {
    this.left = jsonObj.left;
    this.right = jsonObj.right;
    this.top = jsonObj.top;
    this.bottom = jsonObj.bottom;
  };
}

export class title {
  caption: string;
  font: font;
  align: number;
  rect: rect;
  pen: borderpen;

  constructor(jsonObj: any) {
    this.caption = jsonObj.caption;
    this.font = new font(jsonObj.font);
    this.align = jsonObj.align;
    this.rect = new rect(jsonObj.rect);
    this.pen = new borderpen(jsonObj.pen);
  }
}

export class column_title {
  height: number;
  caption: string;
  pen: borderpen;
  textcolor: string;
  bkgcolor: string;
  align: number;
  font: font;
  tooltip: string;
  constructor(jsonObj: any) {
    this.height = jsonObj.height;
    this.caption = jsonObj.caption;
    this.pen = new borderpen(jsonObj.pen);
    this.textcolor = jsonObj.textcolor;
    this.bkgcolor = jsonObj.bkgcolor;
    this.align = jsonObj.align;
    this.font = new font(jsonObj.font);
    this.tooltip = jsonObj.tooltip ? jsonObj.tooltip : '';
  }
}

export class column_total {
  value: string = '';
  textcolor: string;
  bkgcolor: string;
  align: number;
  font: font;
  borders: borders;
  pen: borderpen;
  height: number;
  constructor(jsonObj: any) {
    this.textcolor = jsonObj.textcolor;
    this.bkgcolor = jsonObj.bkgcolor;
    this.align = jsonObj.align;
    this.font = new font(jsonObj.font);  
    this.borders = new borders(jsonObj.borders);
    this.pen = new borderpen(jsonObj.pen);
    this.height = jsonObj.height;
  }
}



