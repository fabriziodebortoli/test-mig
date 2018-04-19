import { untilDestroy } from '../../commons/untilDestroy';
import { Observable } from '../../../rxjs.imports';
import { TbHotlinkButtonsComponent } from './tb-hot-link-buttons.component';
import { findAnchestorByClass } from '../../commons/u';
import { ButtonF8SelectionType, ButtonF9SelectionType } from './../hot-link-base/hotLinkTypes';

export class TbHotlinkButtonsEventHandler {
    static Attach(hlb: TbHotlinkButtonsComponent): TbHotlinkButtonsEventHandler {
        return new TbHotlinkButtonsEventHandler(hlb);
    }

    readonly getHotLinkElement: () => HTMLElement;
    private constructor (hlb: any) {
        this.getHotLinkElement = () => (hlb.vcr.element.nativeElement.parentNode.getElementsByClassName('k-textbox') as HTMLCollection).item(0) as HTMLElement;
        Observable.fromEvent<KeyboardEvent>(document, 'keyup', { capture: true })
        .pipe(untilDestroy(hlb))
        .filter(e => e.keyCode === 27 && hlb.popupHandler.isTablePopupVisible) // Esc
        .subscribe(e => hlb.popupHandler.closePopups());

        Observable.fromEvent<KeyboardEvent>(this.getHotLinkElement(), 'keyup',  {capture: true})
        .pipe(untilDestroy(hlb))
        .filter(e => e.key === 'F8' || e.key === 'F9')
        .map(e => e.key === 'F8' ? ButtonF8SelectionType : ButtonF9SelectionType)
        .subscribe(selectionType =>{
            hlb.setHotLinkIcon(selectionType);
            hlb.selectionTypeChanged(selectionType);
        });

        Observable.fromEvent<KeyboardEvent>(this.getHotLinkElement(), 'blur',  {capture: true})
        .pipe(untilDestroy(hlb))
        .subscribe(_ => hlb.emitModelChange());

        Observable.fromEvent<MouseEvent>(document, 'click', { capture: true }).pipe(untilDestroy(hlb))
        .filter(e => ((hlb.popupHandler.tablePopupRef && !hlb.popupHandler.tablePopupRef.popupElement.contains(e.toElement) 
            && !findAnchestorByClass(e['target'], 'customisable-grid-filter')
            && !findAnchestorByClass(e['target'], 'k-calendar-view')
            && !findAnchestorByClass(e['target'], 'k-popup'))
            || (hlb.popupHandler.optionsPopupRef 
            && !hlb.popupHandler.optionsPopupRef.popupElement.contains(e.toElement)))
            && (hlb.popupHandler.isTablePopupVisible || hlb.popupHandler.isOptionsPopupVisible))
        .subscribe(_ => setTimeout(() => hlb.popupHandler.closePopups()));
    }
}