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
    private get compMod(): any {
        return this.cmp.instance.modelComponent.model;
    }

    private getFromEdsModel(path: string): any {
        return _.get(this.eventDataService.model, path);
    }

    private _ancestor : HlComponent;
    private get ancestor() : HlComponent {
        if(!this._ancestor)
            this._ancestor = (this.viewContainer as any)._view.component as HlComponent;
        return this._ancestor;
    }

    @Input() set tbHotLink(hl: HotLinkInfo) {
        this.hotLinkInfo = hl;
        if (hl.ctx) {
            this.model = hl.ctx;
        }
    }

    constructor(private viewContainer: ViewContainerRef,
        private cfr: ComponentFactoryResolver,
        private store: Store,
        private eventDataService: EventDataService) { }

    private getSliceSelector(ancestor: HlComponent): any {
        return createSelector(
            s => this.compMod ? this.compMod.enabled : false,
            s => this.compMod ? this.compMod.value : undefined,
            s => this.getFromEdsModel(ancestor.hotLink.selector),
            s =>  this.compMod ? { value: this.compMod.value, 
                                   enabled: this.compMod.enabled,
                                   selector: this.getFromEdsModel(ancestor.hotLink.selector) }
                   : { value: undefined,
                       enabled: false,
                       selector: this.getFromEdsModel(ancestor.hotLink.selector) }
                );
    }

    ngOnInit() {
        const compFactory = this.cfr.resolveComponentFactory(TbHotlinkButtonsComponent);
        this.cmp = this.viewContainer.createComponent(compFactory);
        if (!this.model) {
            if(this.ancestor) {
                this.cmp.instance.modelComponent = this.ancestor;
                this.cmp.instance.slice$ = this.store.select(this.getSliceSelector(this.ancestor));
            } else this.cmp.instance.slice$ = Observable.of({ value: null, enabled: false, selector: null });
        } else {
            if(this.ancestor) {
                this.cmp.instance.modelComponent = this.ancestor;
            }
            this.cmp.instance.model = this.model;
            this.cmp.instance.slice$ = Observable.of(this.model);
        }

        this.cmp.instance.hotLinkInfo = this.hotLinkInfo; 
    }
}
