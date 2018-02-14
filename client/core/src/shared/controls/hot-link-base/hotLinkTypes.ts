import { State } from './../../components/radar/custom-grid.component';
import { HotLinkInfo } from './../../models/hotLinkInfo.model';

export type HlComponent = { width?: number, model: any, slice$?: any, cmpId: string, isCombo?: boolean, hotLink: HotLinkInfo };

export class HotLinkState extends State {
  readonly selectionColumn: string;
  readonly selectionTypes: any[];
  readonly selectionType: string = 'code';
 }