import { RadarComponent } from './radar.component';
import { untilDestroy } from '../../commons/untilDestroy';
import { Observable } from '../../../rxjs.imports';

export default class RadarEventHandler {
    static Attach(radar: RadarComponent): RadarEventHandler {
        return new RadarEventHandler(radar);
    }

    private constructor(r: any) {
        let prevPage = () => r.paginator.currentPage > 0 && r.previousPage();
        let nextPage = () => !r.paginator.noMorePages && r.nextPage();
        let target = () => r.elRef.nativeElement.querySelector('.k-grid-content');

        Observable.fromEvent(r.elRef.nativeElement, 'keydown', { capture: true })
            .pipe(untilDestroy(r))
            // .debounceTime(50)
            .subscribe((e: KeyboardEvent) => {
                switch (e.key) {
                    case 'w': r.selectPrevious(); break
                    case 'ArrowUp': e.ctrlKey && prevPage() || r.selectPrevious(); break;
                    case 's': r.selectNext(); break
                    case 'ArrowDown': e.ctrlKey && nextPage() || r.selectNext(); break;
                    case 'q':
                    case 'PageUp': prevPage(); break;
                    case 'e':
                    case 'PageDown': nextPage(); break;
                    case 'Enter': r.selectAndEdit(r.state.rows[r.state.selectedIndex]); break;
                    case 'Escape': r.m.eventData.openRadar.next(false); r.changeDetectorRef.markForCheck(); break;
                    case 'a': target().scrollLeft -= 10; break;
                    case 'd': target().scrollLeft += 10; break;
                    default: return;
                }
                e.preventDefault();
                e.stopPropagation();
            });

        let click$ = Observable.fromEvent(r.elRef.nativeElement, 'click', { capture: true }); // doubleclick
        click$
            .pipe(untilDestroy(r))
            .filter((e: Event) => target().contains(e.target))
            .buffer(click$.debounceTime(250))
            .filter(arr => arr.length >= 2)
            .subscribe((e: Event[]) => {
                console.log(e[0]['target']);
                r.state.selectedIndex !== -1 && r.selectAndEdit(r.state.rows[r.state.selectedIndex]);
            });
    }
}
