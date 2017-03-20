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

@Directive({ selector: '[tbTileAutofill]' })
export class TileAutofillDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'tile-autofill', true);
    }
}

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
    }
}

@Directive({ selector: '[tbLayoutTypeVbox]' })
export class LayoutTypeVboxDirective {
    constructor(el: ElementRef, renderer: Renderer) {
        renderer.setElementClass(el.nativeElement, 'layoutType-vbox', true);
    }
}



