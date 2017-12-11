import {
    Directive, Input, ViewContainerRef, ComponentFactoryResolver,
    OnInit, ComponentRef, OnChanges, SimpleChanges
} from '@angular/core';
import { TbHotlinkButtonsComponent, HlComponent } from './../controls/hot-link-buttons/tb-hot-link-buttons.component';
import { ControlComponent } from './../controls/control.component';
import { Store } from '../../core/services/store.service';
import { createSelector, createSelectorByMap } from '../../shared/commons/selector';

import { EventDataService } from '../../core/services/eventdata.service';

export type HlDefinition = { namespace: string, name: string, ctx?: any };
export type BadHlDefinition = { name: HlDefinition }

@Directive({
    selector: '[tbHotLink]'
})
export class TbHotLinkDirective implements OnInit {
    namespace: string;
    name: any;
    model: any;
    private cmp: ComponentRef<TbHotlinkButtonsComponent>

    @Input() set tbHotLink(hl: any) {
        let goodHl: any;
        try {
            goodHl = JSON.parse(hl.name.replace (/\'/g, '"'));
        } catch (e) {
            goodHl = hl;
        }

        this.namespace = goodHl.namespace;
        this.name = goodHl.name;
        if (goodHl.ctx) {
            this.model = goodHl.ctx;
        }
    }

    constructor(private viewContainer: ViewContainerRef,
        private cfr: ComponentFactoryResolver,
        private store: Store,
    private eventDataService: EventDataService) {
    }

    ngOnInit() {
        const compFactory = this.cfr.resolveComponentFactory(TbHotlinkButtonsComponent);
        this.cmp = this.viewContainer.createComponent(compFactory);
        if (!this.model) {
            let ancestor = (this.viewContainer as any)._view.component as HlComponent;
            if (ancestor) { this.cmp.instance.modelComponent = ancestor; }
        } else { this.cmp.instance.model = this.model; }

        this.cmp.instance.namespace = this.namespace;
        this.cmp.instance.name = this.name;

        let selector = createSelector(
            s => this.cmp.instance.modelComponent.model ? this.cmp.instance.modelComponent.model.enabled : false,
            s => this.cmp.instance.modelComponent.model ? this.cmp.instance.modelComponent.model.value : undefined,
            s => this.cmp.instance.modelComponent.model ? { value: this.cmp.instance.modelComponent.model.value,
                                                            enabled: this.cmp.instance.modelComponent.model.enabled } :
                                                          { value: undefined, enabled: false });
        this.cmp.instance.slice$ = this.store.select(selector).startWith( { value: undefined,  enabled: false });
    }
}
