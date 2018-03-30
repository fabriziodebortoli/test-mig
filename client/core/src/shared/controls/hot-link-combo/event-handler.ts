import { untilDestroy } from '../../commons/untilDestroy';
import { Observable } from '../../../rxjs.imports';
import { TbHotlinkComboComponent } from './tb-hot-link-combo.component';
import { findAnchestorByClass } from '../../commons/u';
import { HotLinkComboSelectionType, DescriptionHotLinkSelectionType } from './../hot-link-base/hotLinkTypes';
import { get } from 'lodash';

export class TbHotlinkComboEventHandler {
    static Attach(hlc: TbHotlinkComboComponent): TbHotlinkComboEventHandler {
        return new TbHotlinkComboEventHandler(hlc);
    }

    readonly getHotLinkElement: () => HTMLElement;
    readonly setUppercase: (hlb: any) => void = hlb => {
        let uppercase = get(hlb, 'modelComponent.model.uppercase');
        if(uppercase) {
            let elem = this.getHotLinkElement();
            elem.style.textTransform = 'uppercase';
        }
    };
    readonly setMaxLenght: (hlb: any, maxLenght: number) => void = (hlb, maxLenght) => {
        let elem = this.getHotLinkElement();
        (elem as any).maxLength = maxLenght;
    };
    private constructor (hlb: any) {
        this.getHotLinkElement = () => hlb.combobox.wrapper.getElementsByClassName('k-input')[0] as HTMLElement;
        Observable.fromEvent<KeyboardEvent>(this.getHotLinkElement(), 'keyup',  {capture: true})
        .pipe(untilDestroy(hlb))
        .filter(e => e.keyCode === 119 /*F8*/ || e.keyCode === 120 /*F9*/)
        .map(e => e.keyCode === 119 ? HotLinkComboSelectionType : DescriptionHotLinkSelectionType)
        .subscribe(selectionType => {
            hlb.setSelectionType(selectionType);
            if (!hlb.combobox.isOpen) hlb.combobox.toggle(true);
        });

        hlb.slice$
        .pipe(untilDestroy(hlb)).filter(x => x.uppercase).map(x => x.uppercase).distinctUntilChanged()
        .subscribe(_ => this.setUppercase(hlb));

        hlb.slice$
        .pipe(untilDestroy(hlb)).filter(x => x.length !== undefined).map(x => x.length).distinctUntilChanged()
        .subscribe(maxLenght => this.setMaxLenght(hlb, maxLenght));
    }
}