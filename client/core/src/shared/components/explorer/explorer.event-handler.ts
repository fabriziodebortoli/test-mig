import { ElementRef } from '@angular/core';
import { ExplorerComponent } from './explorer.component';
import { Observable, untilDestroy, when } from '../../../rxjs.imports';
import { Logger } from '../../../core/services/logger.service';
import { tryArrayFrom } from '../../commons/u';
import { Maybe } from '../../commons/monads/maybe';
import { FlexElements } from './flex-elements';

export class ExplorerEventHandler {
    el: any;
    elements: FlexElements;

    static handle(e): ExplorerEventHandler {
        return new ExplorerEventHandler(e);
    }

    hasClass = (el: any, classes: string) => el.classList && el.classList.value && el.classList.value.includes(classes);

    private constructor(private cmp: any) {
        this.el = cmp.m.injector.get(ElementRef).nativeElement;
        this.elements = FlexElements.create(this.el, '.explorer .item.selected', '.explorer .item');
        const prevent = e => { e.preventDefault(); e.stopPropagation(); };
        Observable.fromEvent(this.el, 'keyup', { capture: true })
            .pipe(untilDestroy(cmp))
            .subscribe(async (e: KeyboardEvent) => {
                switch (e.key) {
                    case 'ArrowUp': this.up(); prevent(e); break;
                    case 'ArrowDown': this.down(); prevent(e); break;
                    case 'ArrowLeft': this.left(); prevent(e); break;
                    case 'ArrowRight': this.right(); prevent(e); break;
                    case 'Enter': this.open(); prevent(e); break;
                    case 'Escape': this.close(); prevent(e); break;
                    case 'Backspace': !this.cmp.showSearch && this.back(); prevent(e); break;
                    default: this.isAlphaNumeric(e.keyCode) && await this.find(e.key);
                }
            });
        cmp.itemClick
            .pipe(untilDestroy(cmp))
            .buffer(cmp.itemClick.debounceTime(250))
            .filter(arr => arr.length >= 2)
            .subscribe(item => cmp.select(item[0]));
    }

    private isAlphaNumeric = (keyCode: number) => keyCode >= 48 && keyCode <= 90;
    private open() {
        if (!this.cmp.selectedItem) return;
        if (this.cmp.selectedItem.level === 3) {
            this.cmp.select(this.cmp.selectedItem.level);
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

