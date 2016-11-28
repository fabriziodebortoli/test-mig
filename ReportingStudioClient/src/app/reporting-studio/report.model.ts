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

export class ReportPage {
  page: number;
  template: string;
  content: ReportObject;
}

export type Report = ReportPage[];


/*
'page_n': 1,
  'template': 'template_abc',
    'value': [
      { id: 'asd1', type: TEXT, backgroundColor: 'red', value: 'xyz' },
      { id: 'asd2', type: TEXT, backgroundColor: 'red', value: 'xyz' },
      { id: 'asd3', type: TEXT, backgroundColor: 'red', value: 'xyz' },
      { id: 'asd4', type: TEXT, backgroundColor: 'red', value: 'xyz' },
      { id: 'asd5', type: TEXT, backgroundColor: 'green', value: 'xyz' }
    ]
},
{
  'page_n': 2,
    'template': 'template_abc',
      'value': [
        { id: 'asd1', type: TEXT, backgroundColor: 'red', value: 'xyz' },
        { id: 'asd2', type: TEXT, backgroundColor: 'red', value: 'xyz' },
        { id: 'asd3', type: TEXT, backgroundColor: 'red', value: 'xyz' },
        { id: 'asd4', type: TEXT, backgroundColor: 'red', value: 'xyz' },
        { id: 'asd5', type: TEXT, backgroundColor: 'green', value: 'xyz' }
      ]
}]
*/
