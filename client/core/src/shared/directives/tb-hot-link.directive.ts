import {
    Directive, Input, ViewContainerRef, ComponentFactoryResolver,
    OnInit, ComponentRef, OnChanges, SimpleChanges
} from '@angular/core';
import { TbHotlinkButtonsComponent, HlComponent } from './../controls/hot-link-buttons/tb-hot-link-buttons.component';
import { ControlComponent } from './../controls/control.component';
import { Store } from '../../core/services/store.service';
import { createSelector, createSelectorByMap } from '../../shared/commons/selector';
import { EventDataService } from '../../core/services/eventdata.service';
import { Observable } from './../../rxjs.imports';
import { HotLinkInfo } from './../models/hotLinkInfo.model';
import * as _ from 'lodash';

@Directive({
    selector: '[tbHotLink]'
})
export class TbHotLinkDirective implements OnInit {
    hotLinkInfo: HotLinkInfo;
    model: any;
    private cmp: ComponentRef<TbHotlinkButtonsComponent>

    @Input() set tbHotLink(hl: HotLinkInfo) {
        this.hotLinkInfo = hl;
        if (hl.ctx) {
            this.model = hl.ctx;
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
        let selTest =  createSelector(s => this.cmp.instance.modelComponent.model ? this.cmp.instance.modelComponent.model.value : '',
        p => this.cmp.instance.modelComponent.model ? this.cmp.instance.modelComponent.model.value : '');
        let selector;
        if (!this.model) {
            let ancestor = (this.viewContainer as any)._view.component as HlComponent;
            if (ancestor) { this.cmp.instance.modelComponent = ancestor; }
            selector = createSelector(
                s => this.cmp.instance.modelComponent.model ? this.cmp.instance.modelComponent.model.enabled : false,
                s => this.cmp.instance.modelComponent.model ? this.cmp.instance.modelComponent.model.value : undefined,
                s => _.get(ancestor.hotLink.selector, this.eventDataService.model) ? 
                            _.get(ancestor.hotLink.selector, this.eventDataService.model) : undefined,
                s => { 
                           if (this.cmp.instance.modelComponent.model){
                           return { value: this.cmp.instance.modelComponent.model.value,
                                    enabled: this.cmp.instance.modelComponent.model.enabled,
                                    selector: _.get(ancestor.hotLink.selector, this.eventDataService.model),
                                     };
                       } else {
                           return { value: undefined, enabled: false, selector: _.get(ancestor.hotLink.selector, this.eventDataService.model) }
                       }
                    });
            this.cmp.instance.slice$ = this.store
            .select(selector)
            .filter(x => !x.value || (x.value !== this.cmp.instance.value));
        } else {
            this.cmp.instance.model = this.model;
            this.cmp.instance.slice$ = Observable.of(this.model);
        }

        this.cmp.instance.hotLinkInfo = this.hotLinkInfo; 
    }
}
