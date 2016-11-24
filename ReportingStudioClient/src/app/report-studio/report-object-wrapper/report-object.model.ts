export enum ReportObjectType { RECTANGLE, IMAGE, TEXT, FILE, REPEATER, HYPERLINK_FORM, HYPERLINK_REPORT, TABLE, COLUMN, BARCODE }

export class ReportObject {

  controlType: ReportObjectType;

  styleClass: string;
  transparent: boolean;

  backgroundColor: string;

  borderColor: string;
  borderSize: number;

  shadowColor: string;
  shadowSize: number;

  id?: string;

  tooltip?: string;
  hidden: boolean = false;

  posXpx?: number;
  posYpx?: number;
  posXmm?: number;
  posYmm?: number;

  src?: string;

  constructor() { }

}
