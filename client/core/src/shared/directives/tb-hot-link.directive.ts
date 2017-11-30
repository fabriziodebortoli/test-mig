import {
    Directive, Input, ViewContainerRef, ComponentFactoryResolver,
    OnInit, ComponentRef, OnChanges, SimpleChanges
} from '@angular/core';
import { TbHotlinkButtonsComponent } from './../controls/hot-link-buttons/tb-hot-link-buttons.component';
import { ControlComponent } from './../controls/control.component';
import { SimpleChange } from '@angular/core';

export type HlComponent = { slice$: any, model: any };

@Directive({
    selector: '[tbHotLink]'
})
export class TbHotLinkDirective implements OnInit {
    namespace: string;
    name: any;
    model: any;
    private cmp: ComponentRef<TbHotlinkButtonsComponent>;
    private ancestor: HlComponent;

    @Input() set tbHotLink(hl: { namespace: string, name: string, ctx?: any }) {
        this.namespace = hl.namespace;
        this.name = hl.name;
        if (hl.ctx) {
            this.model = hl.ctx;
        }
    }

    constructor(private viewContainer: ViewContainerRef,
        private cfr: ComponentFactoryResolver) {
    }

    monkeyPatch(onAfter: (_source: HlComponent, _dest: TbHotLinkDirective) => void, source: HlComponent, dest: TbHotLinkDirective) {
        if (source) {
            let orig: (changes: {}) => void = (source as any).ngOnChanges;
            (source as any).ngOnChanges = function (changes: {}) {
                if (orig) { orig.apply(source, changes); };
                onAfter(source, dest);
            }
        }
    }

    ngOnInit() {
        console.log(' ===> DIRECTIVE ngOnInit');
        const compFactory = this.cfr.resolveComponentFactory(TbHotlinkButtonsComponent);
        if (!this.model) {
            this.ancestor = (<any>this.viewContainer)._view.component as HlComponent;
            this.monkeyPatch((_ancestor, _me) => {
                if (!_me.model || JSON.stringify(_me.model) !== JSON.stringify(_ancestor.model)) { _me.model = _ancestor.model; }
                if (!_me.cmp.instance.slice$) { _me.cmp.instance.slice$ = _ancestor.slice$; }
                console.log(' ===> Patch running');
            }, this.ancestor, this);
        } else {
            this.cmp.instance.model = this.model;
        }

        this.cmp = this.viewContainer.createComponent(compFactory);
        this.cmp.instance.namespace = this.namespace;
        this.cmp.instance.name = this.name;
    }
}
