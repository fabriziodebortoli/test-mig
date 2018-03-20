import { Directive, ElementRef, Renderer } from '@angular/core';

@Directive({ selector: '[tbNoStaticArea]' })
export class NoStaticAreaDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'no-static-area', true);
    }
}
