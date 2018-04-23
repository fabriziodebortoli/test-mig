import { untilDestroy } from '../../commons/untilDestroy';
import { Observable } from '../../../rxjs.imports';
import { TbHotlinkButtonsComponent } from './tb-hot-link-buttons.component';
import { OnDestroy } from '@angular/core';

interface ContextMenuPopupPosition {
    top: number;
    left: number;
}

export class TbHotlinkButtonsContextMenuHandler {
    static Attach(hlb: TbHotlinkButtonsComponent): TbHotlinkButtonsContextMenuHandler {
        return new TbHotlinkButtonsContextMenuHandler(hlb);
    }

    private contextMenuTrigger$(hlb: any, elem: HTMLElement): Observable<ContextMenuPopupPosition> {
        return Observable.fromEvent<MouseEvent>(elem, 'contextmenu', { capture: true })
        .pipe(untilDestroy(hlb))
        .do(e => e.preventDefault())
        .do(_ => hlb.popupHandler.onHklExit())
        .map(e => ({top: e.clientY, left: e.clientX}));
    }

    private constructor (hlb: TbHotlinkButtonsComponent) {
        this.contextMenuTrigger$(hlb, hlb.hotLinkButtonTemplate.nativeElement)
        .subscribe(position => {
            hlb.popupHandler.optionOffset = position;
            if (hlb.popupHandler.isOptionsPopupVisible) hlb.popupHandler.closeOptions();
            else hlb.popupHandler.openOptions()
        });
    }
}