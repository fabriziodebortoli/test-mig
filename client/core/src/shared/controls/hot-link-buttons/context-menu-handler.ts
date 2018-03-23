import { untilDestroy } from '../../commons/untilDestroy';
import { Observable } from '../../../rxjs.imports';
import { TbHotlinkButtonsComponent } from './tb-hot-link-buttons.component';

export class TbHotlinkButtonsContextMenuHandler {
    static Attach(hlb: TbHotlinkButtonsComponent): TbHotlinkButtonsContextMenuHandler {
        return new TbHotlinkButtonsContextMenuHandler(hlb);
    }

    private constructor (hlb: TbHotlinkButtonsComponent) {
        Observable.fromEvent<MouseEvent>(hlb.hotLinkButtonTemplate.nativeElement, 'contextmenu', { capture: true })
        .pipe(untilDestroy(hlb))
        .do(e => e.preventDefault()).do(_ => hlb.popupHandler.closeTable())
        .subscribe(e => {
            hlb.popupHandler.optionOffset = {top: e.clientY, left: e.clientX};
            if (hlb.popupHandler.isOptionsPopupVisible) hlb.popupHandler.closeOptions();
            else hlb.popupHandler.openOptions()
        });
    }
}