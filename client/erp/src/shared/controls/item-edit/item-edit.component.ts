import { Component, Input, ViewChild, ViewContainerRef, OnInit, OnChanges } from '@angular/core';
import { Store, ContextMenuItem, ControlComponent, TbComponentService, LayoutService } from '@taskbuilder/core';
import { ErpHttpService } from '../../../core/services/erp-http.service';

@Component({
    selector: "erp-item-edit",
    templateUrl: './item-edit.component.html',
    styleUrls: ['./item-edit.component.scss']
})
export class ItemEditComponent extends ControlComponent {
    @Input() slice: any;
    @Input() selector: any;

    itemsAutoNumbering = false;

    constructor(
        public vcr: ViewContainerRef,
        layoutService: LayoutService,
        tbComponentService: TbComponentService,
        private store: Store,
        private http: ErpHttpService
    ) {
        super(layoutService, tbComponentService);
    }

    // funzione(): boolean {
    //     return true;
    // }

    ngOnInit() {
        this.readParams();
    }

    async readParams() {
        let result = await this.http.checkItemsAutoNumbering().toPromise();
        this.itemsAutoNumbering = result.json().itemsAutoNumbering;
        this.itemsAutoNumbering = !this.itemsAutoNumbering;
    }
}

