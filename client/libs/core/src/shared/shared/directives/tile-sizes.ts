import { Directive, ElementRef, Renderer } from '@angular/core';

@Directive({ selector: '[tbTileMicro]' })
export class TileMicroDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'tile-micro', true);
    }
}

@Directive({ selector: '[tbTileMini]' })
export class TileMiniDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'tile-mini', true);
    }
}

@Directive({ selector: '[tbTileStandard]' })
export class TileStandardDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'tile-standard', true);
    }
}

@Directive({ selector: '[tbTileWide]' })
export class TileWideDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'tile-wide', true);
    }
}

@Directive({ selector: '[tbTileAutoFill]' })
export class TileAutofillDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'tile-autofill', true);
    }
}



