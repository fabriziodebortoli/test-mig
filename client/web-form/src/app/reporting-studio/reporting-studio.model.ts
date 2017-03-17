
export interface Message {
  commandType: CommandType;
  message?: string;
}

export enum CommandType { OK, NAMESPACE, TEMPLATE, ASK, TEST, GUID, ERROR, PAGE, PDF, RUN, STOP, NEXTPAGE, PREVPAGE }

export enum ReportObjectType { textrect, fieldrect /*RECTANGLE, IMAGE, TEXT, FILE, REPEATER, HYPERLINK_FORM, HYPERLINK_REPORT, TABLE, COLUMN, BARCODE*/ }

export class baseobj {
  id: number;
  hidden: boolean;
  transparent: boolean;
  left: number;
  right: number;
  top: number;
  bottom: number;
  tooltip: string;
  shadow_height: number;
  shadow_color: string;
  constructor(jsonObj: any) {
    this.id = jsonObj.id;
    this.hidden = jsonObj.hidden;
    this.transparent = jsonObj.transparent;
    this.left = jsonObj.rect.left;
    this.right = jsonObj.rect.right;
    this.top = jsonObj.rect.top;
    this.bottom = jsonObj.rect.bottom;
    this.tooltip = jsonObj.tooltip;
    this.shadow_height = jsonObj.shadow_height;
    this.shadow_color = jsonObj.shadow_color;
  };

}

export class baserect extends baseobj {
  hratio: number;
  vratio: number;
  borders: borders;
  borderpen: borderpen;
  constructor(jsonObj: any) {
    super(jsonObj.baseobj);
    this.hratio = jsonObj.hratio;
    this.vratio = jsonObj.vratio;
    this.borders = new borders(jsonObj.borders);
    this.borderpen = new borderpen(jsonObj.borderpen);
  };
}

export class textrect extends baserect {

  obj: ReportObjectType = ReportObjectType.textrect;
  caption: string;
  bkgcolor: string;
  textcolor: string;
  align: number;
  font: font;
  ishtml: boolean;
  constructor(jsonObj: any) {
    super(jsonObj.baserect);
    this.align = jsonObj.align;
    this.caption = jsonObj.caption ? jsonObj.caption : '';
    this.bkgcolor = jsonObj.bkgcolor;
    this.textcolor = jsonObj.textcolor;
    this.font = new font(jsonObj.font);
    this.ishtml = jsonObj.ishtml;

  };
}

export class fieldrect extends baserect {

  obj: ReportObjectType = ReportObjectType.fieldrect;
  label: label;
  caption: string = '';
  font: font;
  align: number;
  bkgcolor: string;
  textcolor: string;
  constructor(jsonObj: any) {
    super(jsonObj.baserect);
    this.label = new label(jsonObj.label);
    this.font = new font(jsonObj.font);
    this.align = jsonObj.align;
    this.bkgcolor = jsonObj.bkgcolor;
    this.textcolor = jsonObj.textcolor;
  };

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


