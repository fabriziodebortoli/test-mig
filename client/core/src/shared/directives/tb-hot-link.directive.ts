import { Directive, Input, ViewContainerRef, ComponentFactoryResolver, OnInit } from '@angular/core';
import { TbHotlinkButtonsComponent } from './../controls/hot-link-buttons/tb-hot-link-buttons.component';
import { ControlComponent } from './../controls/control.component';

@Directive({
    selector: '[tbHotLink]'
})
export class TbHotLinkDirective implements OnInit {
    namespace: string;
    name: any;
    model: any;

    @Input() set tbHotLink(hl: {namespace: string, name: string, ctx?: any }) {
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
            let anchestor = (<any>this.viewContainer)._view.component as ControlComponent;
            this.model = anchestor.model;
        }
        let comp = this.viewContainer.createComponent(compFactory);
        comp.instance.model = this.model;
        comp.instance.namespace = this.namespace;
        comp.instance.name = this.name;
    }
}
