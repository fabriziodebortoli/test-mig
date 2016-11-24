export enum ReportObjectType { RECTANGLE, IMAGE, TEXT, FILE, REPEATER, HYPERLINK_FORM, HYPERLINK_REPORT, TABLE, COLUMN, BARCODE }

export class ReportObject {

  controlType: ReportObjectType;

  styleClass: string;
  transparent: boolean;
  bgColor: string;

  id?: string;

  posX?: string;
  posY?: string;
  src?: string;

  constructor(
    controlType: ReportObjectType,

    styleClass: string,
    transparent: boolean,
    bgColor: string,

    id?: string,

    posX?: string,
    posY?: string,
    src?: string) { }

}
