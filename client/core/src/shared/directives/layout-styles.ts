import { Directive, ElementRef, Renderer } from '@angular/core';

@Directive({ selector: '[tbLayoutTypeColumn]' })
export class LayoutTypeColumnDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'layoutType-column', true);
    }
}

@Directive({ selector: '[tbLayoutTypeHbox]' })
export class LayoutTypeHboxDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'layoutType-hbox', true);
        renderer.setElementClass(el.nativeElement, 'row', true);
    }
}

@Directive({ selector: '[tbLayoutTypeVbox]' })
export class LayoutTypeVboxDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'layoutType-vbox', true);
    }
}

