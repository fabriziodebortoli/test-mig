export enum ReportObjectType { RECTANGLE, IMAGE, TEXT, FILE, REPEATER, HYPERLINK_FORM, HYPERLINK_REPORT, TABLE, COLUMN, BARCODE }


export class ReportObjectStruct {
  controlType: ReportObjectType;
  id?: string;

  styleClass?: string;
  transparent?: boolean;
  backgroundColor?: string;
  borderColor?: string;
  borderSize?: number;
  shadowColor?: string;
  shadowSize?: number;

  tooltip?: string;
  hidden?: boolean = false;

  posXpx?: number;
  posYpx?: number;
  posXmm?: number;
  posYmm?: number;

  src?: string;
}

export class ReportObjectData {
  id: string;
  data: any;
}

export class ReportTemplate {
  template: string;
  content: ReportObjectStruct[];
}

export class ReportData {
  data: ReportObjectData[];
}

export class ReportObject {
  content: ReportTemplate | ReportData;
}

export class ReportPage {
  page: number;
  content: ReportObject[];
}

export type Report = ReportPage[];
