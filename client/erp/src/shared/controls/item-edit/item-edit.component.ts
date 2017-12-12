import { ChangeDetectionStrategy, Component, Input, ViewChild, ViewContainerRef, OnInit, OnChanges, ChangeDetectorRef } from '@angular/core';
import { Store, ContextMenuItem, ControlComponent, TbComponentService, LayoutService } from '@taskbuilder/core';
import { LogisticsHttpService } from '../../../core/services/logistics/logistics-http.service';
import { BehaviorSubject } from "../../../rxjs.imports";

@Component({
    selector: "erp-item-edit",
    templateUrl: './item-edit.component.html',
    styleUrls: ['./item-edit.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class ItemEditComponent extends ControlComponent {
    @Input() slice: any;
    @Input() selector: any;

    @Input() public hotLink: { namespace: string, name: string };

    maxLength = 5;

    itemsAutoNumbering = true;

    constructor(
        public vcr: ViewContainerRef,
        layoutService: LayoutService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef,
        private store: Store,
        private http: LogisticsHttpService
    ) {
        super(layoutService, tbComponentService, changeDetectorRef);
    }

    ngOnInit() {
        this.readParams();
    }

    readParams() {
        this.http.checkItemsAutoNumbering().subscribe(result => {
            this.itemsAutoNumbering = result.json().itemsAutoNumbering;
            this.changeDetectorRef.detectChanges();
        })
    }
}

