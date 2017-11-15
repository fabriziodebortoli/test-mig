import { Component, Input, ViewChild, ViewContainerRef, OnInit, OnChanges, ChangeDetectorRef } from '@angular/core';
import { Store, ContextMenuItem, ControlComponent, TbComponentService, LayoutService } from '@taskbuilder/core';
import { ErpHttpService } from '../../../core/services/erp-http.service';
import { BehaviorSubject } from "../../../rxjs.imports";

@Component({
    selector: "erp-item-edit",
    templateUrl: './item-edit.component.html',
    styleUrls: ['./item-edit.component.scss']
})
export class ItemEditComponent extends ControlComponent {
    @Input() slice: any;
    @Input() selector: any;

    itemsAutoNumbering = true;

    constructor(
        public vcr: ViewContainerRef,
        layoutService: LayoutService,
        tbComponentService: TbComponentService,
        private store: Store,
        private http: ErpHttpService,
        private changeDetectorRef: ChangeDetectorRef
    ) {
        super(layoutService, tbComponentService);
    }

    ngOnInit() {
        this.readParams();
    }

    readParams() {
        this.http.getItemsSearchList("producers").subscribe(result => {
            let response = result;
            console.log(response);
        })

        this.http.checkItemsAutoNumbering().subscribe(result => {
            this.itemsAutoNumbering = result.json().itemsAutoNumbering;
            this.changeDetectorRef.detectChanges();
        })
    }
}

