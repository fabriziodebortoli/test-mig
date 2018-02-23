import { Directive, Input, ViewContainerRef, ComponentFactory, ComponentFactoryResolver, OnInit, ComponentRef } from '@angular/core';
import { HlComponent } from './../controls/hot-link-base/hotLinkTypes';
import { TbHotlinkButtonsComponent } from './../controls/hot-link-buttons/tb-hot-link-buttons.component';
import { TbHotlinkComboComponent } from './../controls/hot-link-combo/tb-hot-link-combo.component';
import { TbHotLinkBaseComponent } from './../controls/hot-link-base/tb-hot-link-base.component';
import { ComboComponent } from './../controls/combo/combo.component';
import { Store } from '../../core/services/store.service';
import { createSelector } from '../../shared/commons/selector';
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
    private cmp: ComponentRef<TbHotlinkButtonsComponent | TbHotlinkComboComponent>
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
                                   type: this.compMod.type ? this.compMod.type : 0,
                                   selector: this.getFromEdsModel(ancestor.hotLink.selector) }
                   : { value: undefined,
                       enabled: false,
                       type: 0,
                       selector: this.getFromEdsModel(ancestor.hotLink.selector) }
                );
    }

    ngOnInit() {
        let compFactory = (this.ancestor && this.ancestor.isCombo) ? this.cfr.resolveComponentFactory(TbHotlinkComboComponent) :
            this.cfr.resolveComponentFactory(TbHotlinkButtonsComponent);
            this.cmp = compFactory ? this.viewContainer.createComponent<TbHotlinkButtonsComponent | TbHotlinkComboComponent>(compFactory) : undefined ;
        if (!this.model) {
            if(this.cmp) {
                this.cmp.instance.modelComponent = this.ancestor;
                this.cmp.instance.slice$ = this.store.select(this.getSliceSelector(this.ancestor));
            } else this.cmp.instance.slice$ = Observable.of({ value: null, enabled: false, selector: null, type: 0 });
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
