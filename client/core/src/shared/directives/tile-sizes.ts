import { Directive, ElementRef, Renderer } from '@angular/core';

@Directive({ selector: '[tbTileMicro]' })
export class TileMicroDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'tile-micro', true);
        renderer.setElementClass(el.nativeElement, 'col-xs-12', true);
        renderer.setElementClass(el.nativeElement, 'col-lg-2', true);
    }
}

@Directive({ selector: '[tbTileMini]' })
export class TileMiniDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'tile-mini', true);
        renderer.setElementClass(el.nativeElement, 'col-xs-12', true);
        renderer.setElementClass(el.nativeElement, 'col-lg-3', true);
    }
}

@Directive({ selector: '[tbTileStandard]' })
export class TileStandardDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'tile-standard', true);
        renderer.setElementClass(el.nativeElement, 'col-xs-12', true);
        renderer.setElementClass(el.nativeElement, 'col-lg-6', true);
    }
}

@Directive({ selector: '[tbTileWide]' })
export class TileWideDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'tile-wide', true);
        renderer.setElementClass(el.nativeElement, 'col-xs-12', true);
        renderer.setElementClass(el.nativeElement, 'col-lg-12', true);
    }
}

@Directive({ selector: '[tbTileAutofill]' })
export class TileAutofillDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'tile-autofill', true);
        renderer.setElementClass(el.nativeElement, 'col-xs-12', true);
        renderer.setElementClass(el.nativeElement, 'col-lg-12', true);
    }
}



