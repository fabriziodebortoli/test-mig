import { ElementRef } from '@angular/core';
import { ExplorerComponent } from './explorer.component';
import { Observable, untilDestroy, when } from '../../../rxjs.imports';
import { Logger } from '../../../core/services/logger.service';
import { tryArrayFrom, Dom } from '../../commons/u';
import { Maybe } from '../../commons/monads/maybe';
import { FlexElements } from './flex-elements';

export class ExplorerEventHandler {
    el: any;
    elements: FlexElements;

    static handle(e): ExplorerEventHandler {
        return new ExplorerEventHandler(e);
    }

    private constructor(private cmp: any) {
        this.el = cmp.m.injector.get(ElementRef).nativeElement;
        this.elements = FlexElements.create(this.el, '.explorer .item.selected', '.explorer .item');
        Observable.fromEvent(this.el, 'keyup', { capture: true })
            .pipe(untilDestroy(cmp))
            .subscribe(async (e: KeyboardEvent) => {
                switch (e.key) {
                    case 'ArrowUp': this.up(); Dom.prevent(e); break;
                    case 'ArrowDown': this.down(); Dom.prevent(e); break;
                    case 'ArrowLeft': this.left(); Dom.prevent(e); break;
                    case 'ArrowRight': this.right(); Dom.prevent(e); break;
                    case 'Enter': this.open(); Dom.prevent(e); break;
                    case 'Escape': this.close(); Dom.prevent(e); break;
                    case 'Backspace': !this.cmp.showSearch && this.back(); Dom.prevent(e); break;
                    default: this.isAlphaNumeric(e.keyCode) && await this.find(e.key);
                }
            });

        cmp.itemClick
            .pipe(untilDestroy(cmp))
            .buffer(cmp.itemClick.debounceTime(250))
            .filter(arr => arr.length >= 2)
            .subscribe(item => cmp.select(item[0]));

        Observable.merge(
            Observable.fromEvent(this.el, 'dragover'),
            Observable.fromEvent(this.el, 'drop')
        ).pipe(untilDestroy(cmp))
            .filter(Dom.targetHasOrHasAnchestorWithClass('explorer'))
            .subscribe(Dom.prevent);
    }

    private isAlphaNumeric = (keyCode: number) => keyCode >= 48 && keyCode <= 90;
    private open() {
        if (!this.cmp.selectedItem) return;
        if (this.cmp.selectedItem.level === 3) {
            this.cmp.select(this.cmp.selectedItem);
            return;
        }
        this.cmp.updateItemsInside(this.cmp.selectedItem);
    }
    private back() { this.cmp.currentItem && this.cmp.currentItem.parent && this.cmp.updateItemsInside(this.cmp.currentItem.parent); }
    private async find(key) { await this.cmp.toggleSearch(true, key); }
    private close() { if (this.cmp.showSearch) this.cmp.toggleSearch(); }
    private up() { this.elements.aboveSelected().map(this.elementToSelectedItem); }
    private down() { this.elements.belowSelected().map(this.elementToSelectedItem); }
    private left() { this.elements.leftOfSelected().map(this.elementToSelectedItem); }
    private right() { this.elements.rightOfSelected().map(this.elementToSelectedItem); }
    private elementToSelectedItem = el => this.cmp.selectedItem = this.cmp.filteredItems[el[2]];
}

