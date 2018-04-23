import { TbHotlinkComboComponent } from './../tb-hot-link-combo.component';
import { findAnchestorByClass } from '../../../commons/u';
import { mergeFactory, onF8OrF9$, onClick$, onCtrlDownArrow$, mergeTriggers } from './trigger-factory';
import { untilDestroy } from '../../../commons/untilDestroy';
import { get } from 'lodash';
import { HTMLElementResolver, resolveFirstChild } from '../../hot-link-base/hotLinkTypes';

export class TbHotlinkComboEventHandler {
    static Attach(hlc: TbHotlinkComboComponent): TbHotlinkComboEventHandler {
        return new TbHotlinkComboEventHandler(hlc);
    }

    readonly getComboBoxInputElement: HTMLElementResolver;
    readonly getComboBoxSelectElement: HTMLElementResolver;
    readonly setUppercase: (hlb: any) => void = hlb => {
        let uppercase = get(hlb, 'modelComponent.model.uppercase');
        if(uppercase) {
            let elem = this.getComboBoxInputElement();
            elem.style.textTransform = 'uppercase';
        }
    };

    readonly setMaxLenght: (hlb: any, maxLenght: number) => void = (hlb, maxLenght) => {
        let elem = this.getComboBoxSelectElement();
        (elem as any).maxLength = maxLenght;
    };

    private constructor (hlb: any) {
        let fromComboResolve = resolveFirstChild(hlb.combobox.wrapper);
        this.getComboBoxInputElement = () => fromComboResolve('k-input');
        this.getComboBoxSelectElement = () => fromComboResolve('k-select');
        let merge = mergeFactory(hlb, this);
        merge(onF8OrF9$, onClick$, onCtrlDownArrow$)
        .subscribe(triggerDataFactory => {
            hlb.start();
            hlb.combobox.toggle(true);
            hlb.emitQueryTrigger(triggerDataFactory)
        });

        hlb.slice$
        .pipe(untilDestroy(hlb)).filter(x => x.uppercase).map(x => x.uppercase).distinctUntilChanged()
        .subscribe(_ => this.setUppercase(hlb));

        hlb.slice$
        .pipe(untilDestroy(hlb)).filter(x => x.length !== undefined).map(x => x.length).distinctUntilChanged()
        .subscribe(maxLenght => this.setMaxLenght(hlb, maxLenght));
    }
}