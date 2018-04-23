import {
    TbComponentService, LayoutService, ControlComponent, EventDataService,
    Store, ControlContainerComponent, Selector, createSelector, FormMode
} from '@taskbuilder/core';
import { WmsHttpService } from '../../../core/services/wms/wms-http.service';
import { Component, Input, ViewChild, ViewContainerRef, OnInit, ChangeDetectorRef } from '@angular/core';

@Component({
    selector: "erp-strbinedit",
    templateUrl: './str-bin-edit.component.html',
    styleUrls: ['./str-bin-edit.component.scss']
})
export class StrBinEditComponent extends ControlComponent {
    @Input() slice: any;
    @Input() selector: Selector<any, any>;

    @ViewChild(ControlContainerComponent) cc: ControlContainerComponent;

    mask = '';

    constructor(
        public eventData: EventDataService,
        layoutService: LayoutService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef,
        private http: WmsHttpService,
        private store: Store
    ) {
        super(layoutService, tbComponentService, changeDetectorRef);
    }

    ngOnInit() {
        this.store.select({
            'zone': 'WMBin.Zone.value',
            'storage': 'WMBin.Storage.value',
            'formatter': 'Formatter.value',
            'separator': 'Separator.value',
            'maskChar': 'GenericMaskChar.value',
            'formMode': 'FormMode.value'
        }).subscribe(
            s => {
                if (s.formMode === FormMode.NEW || s.formMode === FormMode.EDIT) {
                    this.setComponentMask(s);
                }
            }
        );
    }

    async setComponentMask(s: any) {
        let mask = '';
        let r = await this.http.checkBinUsesStructure(s.zone, s.storage).toPromise();
        if (r.json().UseBinStructure) {
            mask = this.convertMask(s.formatter, s.separator, s.maskChar);
        }

        if (this.mask != mask) {
            this.mask = mask;
        }
    }

    private convertMask(mask: string, separator: string, maskChar: string): string {
        if (['0', '9', '#', 'L', 'A', 'a', '&', 'C', '?'].indexOf(separator) > -1)
            mask = mask.replace(new RegExp(separator, 'g'), '\\' + separator);

        return mask
            .replace(new RegExp(maskChar, 'g'), 'A');
    }

    changeModelValue(value) {
        this.model.value = value;
    }

    onKeyDown($event) {
        if (($event.keyCode === 63) || ($event.keyCode === 32)) {
            $event.preventDefault();
        }
    }
}
