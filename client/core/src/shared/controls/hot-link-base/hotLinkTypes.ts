import { State } from './../../components/customisable-grid/customisable-grid.component';
import { HotLinkInfo } from './../../models/hotLinkInfo.model';
import { Record } from './../../commons/mixins/record';


export type HlComponent = { width?: number, model: any, slice$?: any, cmpId: string, isCombo?: boolean, hotLink: HotLinkInfo };

export class HotLinkState extends Record(class extends State {
  readonly selectionColumn: string;
  readonly selectionTypes: any[];
  readonly selectionType: string = 'code';
});