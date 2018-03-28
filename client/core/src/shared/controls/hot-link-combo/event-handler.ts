import { untilDestroy } from '../../commons/untilDestroy';
import { Observable } from '../../../rxjs.imports';
import { TbHotlinkComboComponent } from './tb-hot-link-combo.component';
import { findAnchestorByClass } from '../../commons/u';
import { HotLinkComboSelectionType, DescriptionHotLinkSelectionType } from './../hot-link-base/hotLinkTypes';

export class TbHotlinkComboEventHandler {
    static Attach(hlc: TbHotlinkComboComponent): TbHotlinkComboEventHandler {
        return new TbHotlinkComboEventHandler(hlc);
    }

    readonly getHotLinkElement: () => HTMLElement;
    private constructor (hlb: any) {
        this.getHotLinkElement = () => hlb.combobox.wrapper as HTMLElement;

        Observable.fromEvent<KeyboardEvent>(this.getHotLinkElement(), 'keyup',  {capture: true})
        .pipe(untilDestroy(hlb))
        .filter(e => e.keyCode === 119 /*F8*/ || e.keyCode === 120 /*F9*/)
        .map(e => e.keyCode === 119 ? HotLinkComboSelectionType : DescriptionHotLinkSelectionType)
        .subscribe(selectionType => { hlb.setSelectionType(selectionType);
            if (!hlb.combobox.isOpen) hlb.combobox.toggle(true);
        });
    }
}