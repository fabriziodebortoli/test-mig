import { ChangeDetectionStrategy, Component, Input, ViewChild, ViewContainerRef, OnInit, OnChanges, ChangeDetectorRef } from '@angular/core';
import { Store, ContextMenuItem, ControlComponent, TbComponentService, LayoutService} from '@taskbuilder/core';
import { ItemsHttpService } from '../../../core/services/items/items-http.service';
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

    maxLength = -1;

    itemsAutoNumbering = true;

    constructor(
        public vcr: ViewContainerRef,
        layoutService: LayoutService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef,
        private store: Store,
        private http: ItemsHttpService
    ) {
        super(layoutService, tbComponentService, changeDetectorRef);
    }

    ngOnInit() {
        this.readParams();
    }

    async readParams() {
        this.maxLength = await this.http.getItemInfo_CodeLength();
    }
}

