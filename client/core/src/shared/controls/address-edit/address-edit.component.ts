import { Component, Input, AfterContentInit, OnChanges, ChangeDetectorRef } from '@angular/core';

import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { Store } from './../../../core/services/store.service';
import { ControlComponent } from './../../../shared/controls/control.component';
import { ContextMenuItem, FormMode } from './../../../shared/shared.module';
import { AnimationMetadataType } from '@angular/core/src/animation/dsl';

@Component({
    selector: "tb-addressedit",
    templateUrl: './address-edit.component.html',
    styleUrls: ['./address-edit.component.scss']
})

export class AddressEditComponent extends ControlComponent implements AfterContentInit {

    @Input('readonly') readonly = false;
    @Input() slice: any;
    @Input() selector: any;

    private ctrlEnabled = false;

    addressContextMenu: ContextMenuItem[] = [];
    menuItemSearch = new ContextMenuItem('Search for address', '', true, false, null, this.searchForAddress.bind(this));
    menuItemMap = new ContextMenuItem('Show map', '', true, false, null, this.showMap.bind(this));
    menuItemSatellite = new ContextMenuItem('Show satellite view', '', true, false, null, this.showSatellite.bind(this));

    constructor(
        public eventData: EventDataService,
        layoutService: LayoutService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef,
        private store: Store
    ) {
        super(layoutService, tbComponentService, changeDetectorRef);
    }

    getCorrectHeight() {
        return isNaN(this.height) ? this.height.toString() : this.height + 'px';
    }

    getCorrectWidth() {
        return isNaN(this.width) ? this.width.toString() : this.width + 'px';
    }

    ngAfterContentInit() {
        this.subscribeToSelector();
    }

    ngOnChanges(changes) {
    }

    subscribeToSelector() {
        if (this.store && this.selector) {
            this.store
                .select(this.selector)
                .select('formMode')
                .subscribe(
                (v) => this.onFormModeChanged(v)
                );
        }
    }

    onFormModeChanged(formMode: FormMode) {
        this.ctrlEnabled = formMode === FormMode.FIND || formMode === FormMode.NEW || formMode === FormMode.EDIT;
        this.buildContextMenu();
    }

    buildContextMenu() {
        this.addressContextMenu.splice(0, this.addressContextMenu.length);
        if (this.ctrlEnabled){
            this.addressContextMenu.push(this.menuItemSearch);
        }

        this.addressContextMenu.push(this.menuItemMap);
        this.addressContextMenu.push(this.menuItemSatellite);
    }

    searchForAddress() {
        //window.open(link, '_blank');
    }

    async showMap() {
        let slice = await this.store.select(this.selector).take(1).toPromise();

        if (slice) {
            this.openMap(slice, 'm');
        }
    }

    async showSatellite() {
        let slice = await this.store.select(this.selector).take(1).toPromise();

        if (slice) {
            this.openMap(slice, 'k');
        }
    }

    openMap(slice: any, type: string) {
        let mapUrl = this.createWebLink(slice, type);
        window.open(mapUrl, 'blank');
    }

    createWebLink(slice: any, type: string): string {
        let address = this.createAddress(slice);
        let url = `http://maps.google.com/maps?f=q&hl=en&t=${type}&q=${address}`;

        return url;
    }

    createAddress(slice: any): string {
        let address = '';
        address = this.addAddressElem(address, slice.address);
        address = this.addAddressElem(address, slice.streetNo);
        address = this.addAddressElem(address, slice.city);
        address = this.addAddressElem(address, slice.county);
        address = this.addAddressElem(address, slice.country);
        address = this.addAddressElem(address, slice.federal);
        address = this.addAddressElem(address, slice.zipCode);

        return address;
    }

    addAddressElem(address: string, elem: string) {
        if (elem === '') {
            return address;
        } else {
            if (address === '') {
                return elem;
            } else {
                return address + ',+' + elem;
            }
        }
    }
}
