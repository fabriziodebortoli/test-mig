import { ExplorerComponent } from './explorer.component';
import { Observable, untilDestroy, when } from '../../../rxjs.imports';
import { Logger } from '../../../core/services/logger.service';

export default class ExplorerEventHandler {
    static Handle(radar: ExplorerComponent, log: Logger, enabler?: Observable<boolean>): ExplorerEventHandler {
        return new ExplorerEventHandler(radar, log, enabler);
    }

    private constructor(r: any, log: Logger, enabler?: Observable<boolean>) {
        enabler = enabler || Observable.of(true);
        const prevPage = () => r.paginator.currentPage > 0 && r.previousPage();
        const nextPage = () => !r.paginator.noMorePages && r.nextPage();
        const grid = () => r.elRef.nativeElement.querySelector('.k-grid-content');
        const hasClass = (el: any, classes: string) => el.classList && el.classList.value && el.classList.value.includes(classes);
        const keyTarget = (e: Event) => e.target['type'] !== 'text';
        const clickTarget = (e: Event) => grid().contains(e.target) && hasClass(e.target, 'hotLinkGridCell');

        Observable.fromEvent(r.elRef.nativeElement, 'keyup', { capture: true })
            .pipe(untilDestroy(r), when(enabler))
            .filter(keyTarget)
            .subscribe((e: KeyboardEvent) => {
                switch (e.key) {
                    case 'w': r.selectPrevious(); break;
                    case 'ArrowUp': e.ctrlKey && prevPage() || r.selectPrevious(); break;
                    case 's': r.selectNext(); break;
                    case 'ArrowDown': e.ctrlKey && nextPage() || r.selectNext(); break;
                    case 'q':
                    case 'PageUp': prevPage(); break;
                    case 'e':
                    case 'PageDown': nextPage(); break;
                    case 'Enter': r.selectAndEdit(r.state.rows[r.state.selectedIndex]); break;
                    case 'Escape': r.m.eventData.openRadar.next(false); r.changeDetectorRef.markForCheck(); break;
                    case 'a': grid().scrollLeft -= 10; break;
                    case 'd': grid().scrollLeft += 10; break;
                    default: return;
                }
                e.preventDefault();
                e.stopPropagation();
            });

        const click$ = Observable.fromEvent(r.elRef.nativeElement, 'click', { capture: true }); // doubleclick
        click$
            .pipe(untilDestroy(r), when(enabler))
            .filter(clickTarget)
            .buffer(click$.debounceTime(250))
            .filter(arr => arr.length >= 2)
            .subscribe((e: Event[]) => {
                r.state.selectedIndex !== -1 && r.selectAndEdit(r.state.rows[r.state.selectedIndex]);
            });
    }
}
