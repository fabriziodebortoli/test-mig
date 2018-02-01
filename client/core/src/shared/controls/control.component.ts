import { ControlContainerComponent } from './control-container/control-container.component';
import { EventDataService } from './../../core/services/eventdata.service';
import { TbComponentService } from './../../core/services/tbcomponent.service';
import { Subscription } from '../../rxjs.imports';
import { LayoutService } from './../../core/services/layout.service';
import { Component, Input, ViewEncapsulation, Output, EventEmitter, OnDestroy, AfterContentInit, OnChanges, ChangeDetectorRef, ViewChild, ViewContainerRef } from '@angular/core';
import { TbComponent } from "../components/tb.component";
import { addControlModelBehaviour, createEmptyModel } from './../../shared/models/control.model';
import { ContextMenuItem } from './../models/context-menu-item.model';

@Component({
    template: ''
})
export class ControlComponent extends TbComponent implements OnDestroy/*, OnChanges*/ {
    private _model: any;
    private _width: number;
    private _height: number;

    @Input()
    public caption: string;

    @Input()
    hideCaption : boolean = false;

    @Input() contextMenu: ContextMenuItem[];

    @Input()
    public args: any;
    @Input()
    public validators: Array<any> = [];
    @Input()
    public formatter: string;

    public widthFactor: number = 1;
    public heightFactor: number = 1; 

    @Output('blur') blur: EventEmitter<any> = new EventEmitter();

    subscriptions: Subscription[] = [];

    constructor(
        public layoutService: LayoutService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef
    ) {
        super(tbComponentService, changeDetectorRef);
        this.subscriptions.push(this.layoutService.getWidthFactor().subscribe(wf => { this.widthFactor = wf; }));
        this.subscriptions.push(this.layoutService.getHeightFactor().subscribe(hf => { this.heightFactor = hf }));
    }


    // ngOnChanges() {
    //     //  this.eventData.change.emit(this.cmpId);
    // }

    ngOnDestroy() {
        this.subscriptions.forEach(sub => sub.unsubscribe());
    }
    componentClass() {
       return (!this.model || this.model.visible) ? '' : 'hiddenControl';
    }
    get width(): number {
        return this._width;
    }

    @Input()
    set width(width: number) {
        this._width = width * this.widthFactor + 20;
    }

    get height(): number {
        return this._height;
    }

    @Input()
    set height(height: number) {
        this._height = height * this.heightFactor;
    }

    get model(): any {
        return this._model;
    }

    @Input()
    set model(val: any) {
        if (val === undefined) {
            return;
        }
        this._model = val;
    }

    get value(): any {
        return this._model ? this._model.value : undefined;
    }

    @Input()
    set value(val: any) {
        if (!this._model)
        {
            this.model = createEmptyModel();
        }
        this._model.value = val;
    }

    protected onTranslationsReady() { 
        super.onTranslationsReady();
    }

}