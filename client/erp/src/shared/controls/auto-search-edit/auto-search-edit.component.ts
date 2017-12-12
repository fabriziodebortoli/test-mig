import { TbComponentService, LayoutService, ControlComponent, EventDataService, Store } from '@taskbuilder/core';
import { LogisticsHttpService } from '../../../core/services/logistics/logistics-http.service';
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
        changeDetectorRef: ChangeDetectorRef,
        private http: LogisticsHttpService,
        private store: Store
    ) {
        super(layoutService, tbComponentService, changeDetectorRef);
    }

    formatItem(item) {
        return StringUtils.pad(item.key, 8) + item.value;
    }

    ngOnChanges(changes) {
        if (changes.slice) {
            this.filterText = changes.slice.filterText;
            this.items = changes.slice.items;
        }
    }

    ngOnInit() {
        this.items = this.model.value;
    }
}
