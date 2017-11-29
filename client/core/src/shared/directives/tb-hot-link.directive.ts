import { Directive, Input, ViewContainerRef, ComponentFactoryResolver, OnInit, ComponentRef, OnChanges, SimpleChanges } from '@angular/core';
import { TbHotlinkButtonsComponent } from './../controls/hot-link-buttons/tb-hot-link-buttons.component';
import { ControlComponent } from './../controls/control.component';

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

    ngOnInit() {
        const compFactory = this.cfr.resolveComponentFactory(TbHotlinkButtonsComponent);
        if (!this.model) {
            this.ancestor = (<any>this.viewContainer)._view.component as HlComponent;
            this.model = this.ancestor.model;
        } else {
            this.cmp.instance.model = this.model;
        }

        this.cmp = this.viewContainer.createComponent(compFactory);
        this.cmp.instance.namespace = this.namespace;
        this.cmp.instance.name = this.name;
        this.cmp.instance.slice$ = this.ancestor.slice$;
    }
}
