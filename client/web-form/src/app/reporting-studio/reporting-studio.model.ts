export interface Message {
  commandType: CommandType;
  message?: string;
  response?: string;
}

export enum CommandType { OK, NAMESPACE, DATA, TEMPLATE, ASK, TEST, GUID, ERROR, PAGE, PDF, RUN, PAUSE, STOP }

export enum ReportObjectType { RECTANGLE, IMAGE, TEXT, FILE, REPEATER, HYPERLINK_FORM, HYPERLINK_REPORT, TABLE, COLUMN, BARCODE }

export class ReportObject {
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