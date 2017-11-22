import { TbComponentService, LayoutService, ControlComponent, EventDataService, Store } from '@taskbuilder/core';
import { ErpHttpService } from '../../../core/services/erp-http.service';
import { Component, Input, ViewChild, ViewContainerRef, OnInit, ChangeDetectorRef } from '@angular/core';
import { StringUtils } from './../../../core/u/string-utils';

@Component({
    selector: "erp-auto-search-edit",
    templateUrl: './auto-search-edit.component.html',
    styleUrls: ['./auto-search-edit.component.scss']
})
export class AutoSearchEditComponent extends ControlComponent {
    @Input() slice: any;
    @Input() selector: any;

    items: any[] = [];
    filterText = '';

    constructor(public eventData: EventDataService,
        layoutService: LayoutService,
        tbComponentService: TbComponentService,
        changeDetectorRef:ChangeDetectorRef,
        private http: ErpHttpService,
        private store: Store
    ) {
        super(layoutService, tbComponentService, changeDetectorRef);
    }

    formatItem(item) {
        return StringUtils.pad(item.key, 8) + item.value;
    }
}
