export enum ReportObjectType { RECTANGLE, IMAGE, TEXT, FILE, REPEATER, HYPERLINK_FORM, HYPERLINK_REPORT, TABLE, COLUMN, BARCODE }

export interface ReportObjectOptions {
  id?: string;
  controlType: ReportObjectType;

  styleClass: string;
  transparent: boolean;
  bgColor: string;

  posX?: number;
  posY?: number;
  src?: string;
}

export class ReportObject {

  constructor(options: ReportObjectOptions) { }

}
