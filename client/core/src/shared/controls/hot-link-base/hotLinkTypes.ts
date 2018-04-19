import { State } from './../../components/customisable-grid/customisable-grid.component';
import { HotLinkInfo } from './../../models/hotLinkInfo.model';

export type HlComponent = { width?: number, model: any, slice$?: any, cmpId: string, isCombo?: boolean, hotLink: HotLinkInfo, insideBodyEditDirective?: any };
export interface TriggerData { readonly value: string, readonly selectionType: string };
export type CompleteTriggerDataFactory = (selType: string) => ValueTriggerDataFactory;
export type ValueTriggerDataFactory = (value: string) => TriggerData;
export const CreateTriggerData: CompleteTriggerDataFactory = 
  st => v => ({selectionType: st, value: v ? v : ''});

const DefaultHotLinkSelectionType = 'code';
export const ButtonForKeySelectionType = DefaultHotLinkSelectionType;
export const ButtonForDescriptionSelectionType = 'description';
export const ButtonF8SelectionType = ButtonForKeySelectionType;
export const ButtonF9SelectionType = ButtonForDescriptionSelectionType;
export const ButtonFocusLostSelectionType = 'direct';

export const NewButtonForKeyTriggerData = CreateTriggerData(ButtonForKeySelectionType);
export const NewButtonForDescriptionTriggerData = CreateTriggerData(ButtonForDescriptionSelectionType);
export const NewButtonF8TriggerData = CreateTriggerData(ButtonF8SelectionType);
export const NewButtonF9TriggerData = CreateTriggerData(ButtonF9SelectionType);
export const NewButtonFocusLostTriggerData = CreateTriggerData(ButtonFocusLostSelectionType);

export const ComboOpeningSelectionType = 'combo';
export const ComboFilteringSelectionType = 'code';
export const ComboF8SelectionType = ComboOpeningSelectionType;
export const ComboF9SelectionType = 'description';
export const ComboFocusLostSelectionType = 'direct';

export const NewComboOpeningTriggerData = CreateTriggerData(ComboOpeningSelectionType);
export const NewComboFilteringTriggerData = CreateTriggerData(ComboFilteringSelectionType);
export const NewComboF8TriggerData = CreateTriggerData(ComboF8SelectionType);
export const NewComboF9TriggerData = CreateTriggerData(ComboF9SelectionType);
export const NewComboFocusLostTriggerData = CreateTriggerData(ComboFocusLostSelectionType);

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
