import { Directive, ElementRef, Renderer2, Input, OnInit } from '@angular/core';

import { Subscription } from '../../rxjs.imports';

import { LayoutService } from './../../core/services/layout.service';

@Directive({ selector: '[tbControl]' })
export class TbControlDirective implements OnInit {

    @Input() marginLeft: number;
    @Input() textAlign: string;

    @Input() staticArea: boolean = true;

    private subscriptions: Subscription[] = [];

    public widthFactor: number = 1;
    public heightFactor: number = 1;

    constructor(
        private el: ElementRef,
        private renderer: Renderer2,
        private layoutService: LayoutService
    ) {
        this.subscriptions.push(this.layoutService.getWidthFactor().subscribe(wf => { this.widthFactor = wf; }));
        this.subscriptions.push(this.layoutService.getHeightFactor().subscribe(hf => { this.heightFactor = hf }));
    }

    ngOnInit() {
        if (this.marginLeft)
            this.renderer.setStyle(this.el.nativeElement, 'margin-left', (this.marginLeft * this.widthFactor) + 'px');

        if (!this.staticArea) 
            this.renderer.addClass(this.el.nativeElement, 'no-static-area');            

        if(this.textAlign)
            this.renderer.addClass(this.el.nativeElement, 'text-align-' + this.textAlign);
    }
}