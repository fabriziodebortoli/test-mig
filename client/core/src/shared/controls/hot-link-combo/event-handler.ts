import { untilDestroy } from '../../commons/untilDestroy';
import { Observable } from '../../../rxjs.imports';
import { TbHotlinkComboComponent } from './tb-hot-link-combo.component';
import { findAnchestorByClass } from '../../commons/u';
import { TriggerData, NewComboF8TriggerData, NewComboF9TriggerData } from './../hot-link-base/hotLinkTypes';
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
        this.getHotLinkElement = () => {
            return hlb.combobox.wrapper.getElementsByClassName('k-input')[0] as HTMLElement;
        }
        Observable.fromEvent<KeyboardEvent>(this.getHotLinkElement(), 'keyup',  {capture: true})
        .pipe(untilDestroy(hlb))
        .filter(e => e.key === 'F8' || e.key === 'F9')
        .map(e => e.key === 'F8' ? NewComboF8TriggerData : NewComboF9TriggerData)
        .subscribe(triggerDataFactory => {
            hlb.emitQueryTrigger(triggerDataFactory)
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