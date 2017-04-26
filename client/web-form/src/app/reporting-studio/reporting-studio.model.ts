

export interface Message {
  commandType: CommandType;
  message?: string;
}

export enum CommandType { OK, NAMESPACE, INITTEMPLATE, TEMPLATE, ASK, UPDATEASK, DATA, STOP, IMAGE }

export enum AskObjectType { text, radio, check, dropdownlist }

export enum ReportObjectType { textrect, fieldrect, table, graphrect, sqrrect, repeater, cell, link }

export enum LinkType { report, document, url, file, function }

export class link {
  obj: ReportObjectType = ReportObjectType.link;
  type: LinkType;
  ns: string;
  arguments: string;
  runAtServer: boolean = false;
  constructor(jsonObj: any) {
    this.type = jsonObj.type;
    this.ns = jsonObj.ns;
    this.arguments = jsonObj.arguments;
    this.runAtServer = jsonObj.runAtServer ? jsonObj.runAtServer : false;
  }
}

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
    this.rect = new rect(jsonObj.rect);
    this.transparent = jsonObj.transparent;
    this.tooltip = jsonObj.tooltip ? jsonObj.tooltip : '';
    this.shadow_height = jsonObj.shadow_height;
    this.shadow_color = jsonObj.shadow_color;
  };
}

export class baserect extends baseobj {
  borders: borders;
  ratio: number;
  pen: borderpen;
  constructor(jsonObj: any) {
    super(jsonObj.baseobj);
    this.ratio = jsonObj.ratio;
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
  text_align: string;
  vertical_align: string;
  src: string = '';
  constructor(jsonObj: any) {
    super(jsonObj.sqrrect !== undefined ? jsonObj.sqrrect : jsonObj); // if image is constructed from fieldRect the jsonObj, else jsonObj.sqrrect
    this.text_align = jsonObj.text_align;
    this.vertical_align = jsonObj.vertical_align;
    this.value = jsonObj.image ? jsonObj.image : '';
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
  text_align: string;
  vertical_align: string;
  font: font;
  value_is_html: boolean;
  value_is_barcode: boolean;
  constructor(jsonObj: any) {
    super(jsonObj.baserect);
    this.text_align = jsonObj.text_align;
    this.vertical_align = jsonObj.vertical_align;
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
  text_align: string;
  vertical_align: string;
  bkgcolor: string;
  textcolor: string;
  value_is_html: boolean;
  value_is_image: boolean;
  value_is_barcode: boolean;
  link: link = undefined;
  constructor(jsonObj: any) {
    super(jsonObj.baserect);
    this.label = jsonObj.label ? new label(jsonObj.label) : undefined;
    this.font = new font(jsonObj.font);
    this.text_align = jsonObj.text_align;
    this.vertical_align = jsonObj.vertical_align;
    this.bkgcolor = jsonObj.bkgcolor;
    this.textcolor = jsonObj.textcolor;
    this.value_is_html = jsonObj.value_is_html;
    this.value_is_image = jsonObj.value_is_image;
    this.value_is_barcode = jsonObj.value_is_barcode;
    this.link = jsonObj.link ? new link(jsonObj.link) : undefined;
  };
}

export class table extends baseobj {

  obj: ReportObjectType = ReportObjectType.table;
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

export class column {
  id: string;
  hidden: boolean;
  width: number;
  value_is_html: boolean;
  value_is_image: boolean;
  value_is_barcode: boolean;
  title: title;
  //total: column_total;

  constructor(jsonObj: any) {
    this.id = jsonObj.id;
    this.hidden = jsonObj.hidden;
    this.width = jsonObj.width;
    this.value_is_html = jsonObj.value_is_html;
    this.value_is_image = jsonObj.value_is_image;
    this.value_is_barcode = jsonObj.value_is_barcode;
    this.title = jsonObj.title ? new title(jsonObj.title) : undefined;
    // this.total = jsonObj.total ? new column_total(jsonObj.total) : undefined;
  }
}

export class label {
  caption: string;
  textcolor: string;
  font: font;
  text_align: string;
  vertical_align: string;
  constructor(jsonObj: any) {
    this.caption = jsonObj.caption ? jsonObj.caption : '';
    this.textcolor = jsonObj.textcolor;
    this.font = new font(jsonObj.font);
    this.text_align = jsonObj.text_align;
    this.vertical_align = jsonObj.vertical_align;
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
  rect: rect;
  caption: string;
  pen: borderpen;
  borders: borders;
  textcolor: string;
  bkgcolor: string;
  text_align: string;
  vertical_align: string;
  font: font;
  tooltip: string = '';
  constructor(jsonObj: any) {
    this.rect = new rect(jsonObj.rect);
    this.caption = jsonObj.caption;
    this.pen = new borderpen(jsonObj.pen);
    this.borders = new borders(jsonObj.borders);
    this.textcolor = jsonObj.textcolor;
    this.bkgcolor = jsonObj.bkgcolor;
    this.text_align = jsonObj.text_align;
    this.vertical_align = jsonObj.vertical_align;
    this.font = new font(jsonObj.font);
    this.tooltip = jsonObj.tooltip ? jsonObj.tooltip : '';
  }
}

export class cell {
  id: string;
  borders: borders;
  pen: borderpen;
  textcolor: string;
  bkgcolor: string;
  text_align: string;
  vertical_align: string;
  font: font;
  tooltip: string = '';
  value: string = '';
  link: link = undefined;
  constructor(jsonObj: any, id: string) {
    this.id = id;
    this.borders = new borders(jsonObj.borders);
    this.pen = new borderpen(jsonObj.pen);
    this.textcolor = jsonObj.textcolor;
    this.bkgcolor = jsonObj.bkgcolor;
    this.text_align = jsonObj.text_align;
    this.vertical_align = jsonObj.vertical_align;
    this.font = new font(jsonObj.font);
    this.tooltip = jsonObj.tooltip ? jsonObj.tooltip : '';
    this.value = jsonObj.value ? jsonObj.value : '';
    this.link = jsonObj.link ? new link(jsonObj.link) : undefined;
  }
}

export class askGroup {
  caption: string;
  hidden: boolean;
  entries: askObj[] = [];
  constructor(jsonObj: any) {
    this.caption = jsonObj.caption;
    this.hidden = jsonObj.hidden;
    for (let i = 0; i < jsonObj.entries.length; i++) {
      let element = jsonObj.entries[i];
      let obj;

      if (element.text !== undefined) {
        obj = new text(element.text);
      }
      else if (element.check !== undefined) {
        obj = new text(element.check);
      }
      else if (element.radio !== undefined) {
        obj = new text(element.radio);
      }
      else if (element.dropdownlist !== undefined) {
        obj = new text(element.dropdownlist);
      }

      this.entries.push(obj);
    }
  }
}

export class fieldAskObj {
  name: string;
  id: string;
  type: string;
  value: string;
  constructor(jsonObj: any) {
    this.name = jsonObj.name;
    this.id = jsonObj.id;
    this.type = jsonObj.type;
    this.value = jsonObj.value;
  }
}

export class askObj extends fieldAskObj {
  hidden: boolean;
  enabled: boolean;
  caption: string;
  left_aligned: boolean;
  left_text: boolean;
  runatserver: boolean;
  constructor(jsonObj: any) {
    super(jsonObj.field);
    this.hidden = jsonObj.hidden;
    this.enabled = jsonObj.enabled;
    this.caption = jsonObj.caption;
    this.left_aligned = jsonObj.left_aligned;
    this.left_text = jsonObj.left_text;
  }
}

export class text extends askObj {
  obj: AskObjectType = AskObjectType.text;
  constructor(jsonObj: any) {
    super(jsonObj);
  }
}

export class check extends askObj {
  obj: AskObjectType = AskObjectType.check;
  constructor(jsonObj: any) {
    super(jsonObj);
  }
}

export class radio extends askObj {
  obj: AskObjectType = AskObjectType.radio;
  constructor(jsonObj: any) {
    super(jsonObj);
  }
}

export class dropdownlist extends askObj {
  obj: AskObjectType = AskObjectType.dropdownlist;
  list: dropdownListPair[] = [];
  constructor(jsonObj: any) {
    super(jsonObj);
    for (let i = 0; i < jsonObj.list.length; i++) {
      let item = jsonObj.list[i];
      this.list.push(new dropdownListPair(item));
    }
  }
}

export class dropdownListPair {
  value: string;
  caption: string;
  constructor(jsonObj: any) {
    this.value = jsonObj.value;
    this.caption = jsonObj.caption;
  }
}

export class styleArrayElement {
  rowNumber: number;
  public style: cell[] = []
  constructor(row: number) {
    this.rowNumber = row;
  }
}

export class TemplateItem {
  public templateName: string;
  public templateObjects: any[]=[];
  public template: any;

  constructor(tName: string, template: any, tObj: any[]) {
    this.templateName = tName;
    this.templateObjects = tObj;
    this.template = template;
  }
}



