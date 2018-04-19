import { untilDestroy } from '../../commons/untilDestroy';
import { Observable } from '../../../rxjs.imports';
import { TbHotlinkComboComponent } from './tb-hot-link-combo.component';
import { findAnchestorByClass } from '../../commons/u';
import { TriggerData, NewComboF8TriggerData, NewComboF9TriggerData, ValueTriggerDataFactory, NewComboOpeningTriggerData } from './../hot-link-base/hotLinkTypes';
import { get } from 'lodash';

type FunctionTriggerFactory = (destroyer: any, elem: HTMLElement) => Observable<ValueTriggerDataFactory>;


const onF8OrF9$: FunctionTriggerFactory =
    (destroyer, elem) => Observable.fromEvent<KeyboardEvent>(elem, 'keyup',  {capture: true})
    .pipe(untilDestroy(destroyer))
    .filter(e => e.key === 'F8' || e.key === 'F9')
    .map(e => e.key === 'F8' ? NewComboF8TriggerData : NewComboF9TriggerData);

const onClick$: FunctionTriggerFactory =
    (destroyer, elem) => Observable.fromEvent<MouseEvent>(elem, 'click',  {capture: false})
    .pipe(untilDestroy(destroyer))
    .map(e => NewComboOpeningTriggerData);

const onCtrlDownArrow$: FunctionTriggerFactory =
    (destroyer, elem) => Observable.fromEvent<KeyboardEvent>(elem, 'keyup',  {capture: false})
    .pipe(untilDestroy(destroyer))
    .filter(e => e.key === 'ArrowDown' && e.ctrlKey)
    .do(console.log)
    .do(e => e.preventDefault())
    .do(console.log)
    .map(e => NewComboOpeningTriggerData);

const merge: <T>(...fts: Observable<T>[]) => Observable<T> = (...fts) => fts.reduce((a, b)=> a.merge(b));
    

export class TbHotlinkComboEventHandler {
    static Attach(hlc: TbHotlinkComboComponent): TbHotlinkComboEventHandler {
        return new TbHotlinkComboEventHandler(hlc);
    }

    readonly getHotLinkElement: () => HTMLElement;
    readonly getHotLinkSelectElement: () => HTMLElement;
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
        this.getHotLinkSelectElement = () => hlb.combobox.wrapper.getElementsByClassName('k-select')[0] as HTMLElement;
        
        merge(onF8OrF9$(hlb, this.getHotLinkElement()),
            onClick$(hlb, this.getHotLinkSelectElement()),
            onCtrlDownArrow$(hlb, this.getHotLinkElement()))
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