import { ExplorerComponent } from './explorer.component';
import { Observable, untilDestroy, when } from '../../../rxjs.imports';
import { Logger } from '../../../core/services/logger.service';

export default class ExplorerEventHandler {
    static handle(e: ExplorerComponent, enabler?: Observable<boolean>): ExplorerEventHandler {
        return new ExplorerEventHandler(e, enabler);
    }

    private log: Logger;

    private constructor(cmp: any, enabler?: Observable<boolean>) {
        // enabler = enabler || Observable.of(true);
        // const hasClass = (el: any, classes: string) => el.classList && el.classList.value && el.classList.value.includes(classes);

        // Observable.fromEvent(cmp.elRef.nativeElement, 'keyup', { capture: true })
        //     .pipe(untilDestroy(cmp), when(enabler))
        //     .subscribe((e: KeyboardEvent) => {
        //         switch (e.key) {
        //             case 'w':
        //             case 'ArrowUp': this.up(); break;
        //             case 's':
        //             case 'ArrowDown': this.down(); break;
        //             case 'Enter': this.open(); break;
        //             case 'Escape': this.close(); break;
        //             case 'F3': this.find(); break;
        //             case 'a':
        //             case 'ArrowLeft': this.left(); break;
        //             case 'd':
        //             case 'ArrowRight': this.right(); break;
        //             default: return;
        //         }
        //         e.preventDefault();
        //         e.stopPropagation();
        //     });
    }

    open() {

    }

    close() {

    }

    find() {

    }

    left() {

    }

    right() {

    }

    up() {

    }

    down() {

    }
}
