import { EventDataService } from './../../core/services/eventdata.service';
import { TbComponentService } from './../../core/services/tbcomponent.service';
import { Subscription } from '../../rxjs.imports';
import { LayoutService } from './../../core/services/layout.service';
import { Component, Input, ViewEncapsulation, Output, EventEmitter, OnDestroy, AfterContentInit, OnChanges, ChangeDetectorRef } from '@angular/core';
import { TbComponent } from "../components/tb.component";

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
    public args: any;
    @Input()
    public validators: Array<any> = [];
    @Input()
    public value: any;
    @Input()
    public format: string;

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
    set model(model: any) {
        if (model == undefined)
            return;

        this._model = model;
        this.value = model.value;
    }
}