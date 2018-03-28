import { State } from './../../components/customisable-grid/customisable-grid.component';
import { HotLinkInfo } from './../../models/hotLinkInfo.model';

export type HlComponent = { width?: number, model: any, slice$?: any, cmpId: string, isCombo?: boolean, hotLink: HotLinkInfo };
export const DefaultHotLinkSelectionType = 'code';
export const HotLinkComboSelectionType = 'combo';
export const DescriptionHotLinkSelectionType = 'description';

export class HotLinkState extends State {
  readonly selectionColumn: string;
  readonly selectionTypes: any[];
  readonly selectionType: string = DefaultHotLinkSelectionType;

  static new(a?: Partial<HotLinkState>): Readonly<HotLinkState> {
    if (a) return new HotLinkState().with(a);
    return new HotLinkState();
  }
  with(a: (Partial<HotLinkState> | ((s: HotLinkState) => Partial<HotLinkState>))): Readonly<HotLinkState> {
    if (typeof a === 'function')
      a = a(this as any);
    return Object.assign(new HotLinkState(), this, a) as any;
  }
}
