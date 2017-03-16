export interface Message {
  commandType: CommandType;
  message?: string;
  response?: string;
}

export enum CommandType { OK, NAMESPACE, DATA, TEMPLATE, ASK, TEST, GUID, ERROR, PAGE, PDF, RUN, STOP, NEXTPAGE, PREVPAGE }

export enum ReportObjectType { textrect, fieldrect /*RECTANGLE, IMAGE, TEXT, FILE, REPEATER, HYPERLINK_FORM, HYPERLINK_REPORT, TABLE, COLUMN, BARCODE*/ }

export class baseobj {
  obj: ReportObjectType;
  id: number;
  hidden: boolean;
  transparet: boolean;
  left: number;
  right: number;
  top: number;
  bottom: number;
  tooltip: string;
  shadow_height: number;
  shadow_color: string;
}

export class baserect extends baseobj {
  hratio: number;
  vratio: number;
  borders: borders;
  borderpen: borderpen;
}

export class textrect extends baserect {
  caption: string;
  bkgcolor: string;
  textcolor: string;
  align: number;
  font: font;
  ishtml: boolean;
}

export class fieldrect extends baserect {
  label: label;
  font: font;
  align: number;
  bkgcolor: string;
  textcolor: string;
}

export class label {
  caption: string;
  textcolor: string;
  font: font;
  align: number;
}

export class font {
  face: string;
  size: number;
  italic: boolean;
  vold: boolean;
  underline: false;

}

export class borders {
  left: boolean;
  right: boolean;
  top: boolean;
  bottom: boolean;
}

export class borderpen {
  width: number;
  color: string;
}


