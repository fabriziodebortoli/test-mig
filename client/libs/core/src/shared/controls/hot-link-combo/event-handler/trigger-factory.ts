import { TriggerData, NewComboF8TriggerData, NewComboF9TriggerData, ValueTriggerDataFactory, NewComboOpeningTriggerData, EventKey,
    EventMode } 
    from './../../hot-link-base/hotLinkTypes';
import { untilDestroy } from '../../../commons/untilDestroy';
import { Observable } from '../../../../rxjs.imports';

export type Merger = <T>(...fts: Observable<T>[]) => Observable<T>;
export const mergeTriggers: Merger = (...fts) => fts.reduce((a, b)=> a.merge(b));
export const mergeFactory: (destroyer, hvtHdr) => (...ftf: FunctionTriggerFactory[]) => Observable<ValueTriggerDataFactory> = 
    (d, h) => (...ftf) => mergeTriggers(...ftf.map(f => f(d,h)));

export type FunctionTriggerFactory = (destroyer: any, eventHander: any) => Observable<ValueTriggerDataFactory>;

export const onF8OrF9$: FunctionTriggerFactory =
    (destroyer, evtHdr) => Observable.fromEvent<KeyboardEvent>(evtHdr.getComboBoxInputElement(), EventKey.KeyUp, EventMode.Capturing)
    .pipe(untilDestroy(destroyer))
    .filter(e => e.key === EventKey.F8 || e.key === EventKey.F9)
    .map(e => e.key === EventKey.F8 ? NewComboF8TriggerData : NewComboF9TriggerData);

export const onClick$: FunctionTriggerFactory =
    (destroyer, evtHdr) => Observable.fromEvent<MouseEvent>(evtHdr.getComboBoxSelectElement(), EventKey.Click, EventMode.Bubbling)
    .pipe(untilDestroy(destroyer))
    .map(e => NewComboOpeningTriggerData);

export const onCtrlDownArrow$: FunctionTriggerFactory =
    (destroyer, evtHdr) => Observable.fromEvent<KeyboardEvent>(evtHdr.getComboBoxInputElement(), EventKey.KeyUp, EventMode.Bubbling)
    .pipe(untilDestroy(destroyer))
    .filter(e => e.key === EventKey.ArrowDown && e.ctrlKey)
    .do(e => e.preventDefault())
    .map(e => NewComboOpeningTriggerData);