import { TbComponentService, LayoutService, ControlComponent, EventDataService, Store } from '@taskbuilder/core';
import { LogisticsHttpService } from '../../../core/services/logistics/logistics-http.service';
import { Component, Input, ViewChild, ViewContainerRef, OnInit, ChangeDetectorRef } from '@angular/core';

@Component({
    selector: "erp-strbinedit",
    templateUrl: './str-bin-edit.component.html',
    styleUrls: ['./str-bin-edit.component.scss']
})
export class StrBinEditComponent extends ControlComponent {
    @Input() slice: any;
    @Input() selector: any;

    mask = '';
    errorMessage = '';

    constructor(
        public eventData: EventDataService,
        layoutService: LayoutService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef,
        private http: LogisticsHttpService,
        private store: Store
    ) {
        super(layoutService, tbComponentService, changeDetectorRef);
    }

    async ngOnChanges(changes) {
        if (changes) {
            let r = await this.http.checkBinUsesStructure(changes.slice.zone, changes.slice.storage).toPromise();
            if (r.json().UseBinStructure) {
                this.mask = this.convertMask(changes.slice.formatter, changes.slice.separator, changes.slice.maskChar);
            }
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
