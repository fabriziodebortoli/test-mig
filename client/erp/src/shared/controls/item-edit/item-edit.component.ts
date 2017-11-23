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

    maxLength = 5;

    itemsAutoNumbering = true;

    constructor(
        public vcr: ViewContainerRef,
        layoutService: LayoutService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef,
        private store: Store,
        private http: ErpHttpService
    ) {
        super(layoutService, tbComponentService, changeDetectorRef);
    }

    ngOnInit() {
        this.readParams();
    }

    readParams() {

        // this.http.getItemsSearchList("producersByCategory").subscribe(result => {
        //     let response = result.json();
        //     console.log(response);
        // })


        this.http.checkItemsAutoNumbering().subscribe(result => {
            this.itemsAutoNumbering = result.json().itemsAutoNumbering;
            this.changeDetectorRef.detectChanges();
        })
    }
}

