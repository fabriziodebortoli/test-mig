import { TbComponentService, LayoutService, ControlComponent, EventDataService } from '@taskbuilder/core';
import { ErpHttpService } from '../../../core/services/erp-http.service';

import { Component, Input, ViewChild, ViewContainerRef } from '@angular/core';
@Component({
    selector: "erp-strbinedit",
    templateUrl: './str-bin-edit.component.html',
    styleUrls: ['./str-bin-edit.component.scss']
})
export class StrBinEditComponent extends ControlComponent {
    mask = '';
    errorMessage = '';

    constructor(
        public eventData: EventDataService,
        layoutService: LayoutService,
        tbComponentService: TbComponentService,
        private http: ErpHttpService
    ) {
        super(layoutService, tbComponentService);
    }

    async ngOnChanges(changes) {
        let slice: any;

        let r = await this.http.checkBinUsesStructure(slice.zone, slice.storage).toPromise();
        if (r.json()) {
            this.mask = this.convertMask(slice.formatter, slice.separator, slice.maskChar);
        }
        //this.validate();
    }

    private convertMask(mask: string, separator: string, maskChar: string): string {
        if (['0', '9', '#', 'L', 'A', 'a', '&', 'C', '?'].indexOf(separator) > -1)
            mask = mask.replace(new RegExp(separator, 'g'), '\\' + separator);

        return mask
            .replace(new RegExp(maskChar, 'g'), 'A');
    }

    public onBlur() {

    }

    public changeModelValue($event) {

    }
}
