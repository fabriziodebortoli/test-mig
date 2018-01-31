import { Component, Input, AfterContentInit, ChangeDetectorRef, ElementRef, ViewChild, HostListener } from '@angular/core';
import { Http, Headers } from '@angular/http';

import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { InfoService } from './../../../core/services/info.service'
import { EventDataService } from './../../../core/services/eventdata.service';
import { Store } from './../../../core/services/store.service';
import { ControlComponent } from './../../../shared/controls/control.component';
import { ContextMenuItem, FormMode } from './../../../shared/shared.module';
import { Collision } from '@progress/kendo-angular-popup';
import 'rxjs';

@Component({
    selector: 'tb-addressedit',
    templateUrl: './address-edit.component.html',
    styleUrls: ['./address-edit.component.scss']
})

export class AddressEditComponent extends ControlComponent implements AfterContentInit {

    @Input('readonly') readonly = false;
    @Input() slice: any;
    @Input() selector: any;

    @ViewChild('anchor') public anchor: ElementRef;
    @ViewChild('popup', { read: ElementRef }) public popup: ElementRef;

    public addresses = [];
    private ctrlEnabled = false;
    private show = false;
    private collision: Collision = { horizontal: 'flip', vertical: 'fit' };

    addressContextMenu: ContextMenuItem[] = [];
    menuItemSearch = new ContextMenuItem('Search for address', '', true, false, null, this.searchForAddress.bind(this));
    menuItemMap = new ContextMenuItem('Show map', '', true, false, null, this.showMap.bind(this));
    menuItemSatellite = new ContextMenuItem('Show satellite view', '', true, false, null, this.showSatellite.bind(this));

    @HostListener('document:click', ['$event'])
    public documentClick(event: any): void {
        if (!this.contains(event.target)) {
            this.close();
        }
    }

    private contains(target: any): boolean {
        return this.anchor.nativeElement.contains(target) ||
            (this.popup ? this.popup.nativeElement.contains(target) : false);
    }

    constructor(
        public eventData: EventDataService,
        layoutService: LayoutService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef,
        private infoService: InfoService,
        private store: Store,
        private http: Http
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

    subscribeToSelector() {
        if (this.store && this.selector) {
            this.store
                .select(this.selector)
                .selectSlice('formMode', 'address', 'contact')
                .subscribe(
                (v) => this.onFormModeChanged(v)
                );
             
        }
    }

    onFormModeChanged(slice: any) {
        this.ctrlEnabled = slice.formMode === FormMode.FIND || slice.formMode === FormMode.NEW || slice.formMode === FormMode.EDIT;
        this.buildContextMenu();
    }

    buildContextMenu() {
        this.addressContextMenu.splice(0, this.addressContextMenu.length);
        if (this.model.value !== '') {
            if (this.ctrlEnabled) {
                this.addressContextMenu.push(this.menuItemSearch);
            }
            this.addressContextMenu.push(this.menuItemMap);
            this.addressContextMenu.push(this.menuItemSatellite);
        }
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

    async searchForAddress() {
        let slice = await this.store.select(this.selector).take(1).toPromise();

        if (slice) {

            let productInfo = this.infoService.getProductInfo(true).toPromise();

            let address = this.createAddress(slice);
            let region = '';
            if (slice.country.value) {
                region = `&region=${slice.country.value}`;
            }
            let culture = this.infoService.getCulture();
            let language = '';
            if (culture === 'BR') {
                language = '&language=pt-BR';
            } else {
                language = `&language=${culture}`;
            }

            let url = `http://maps.google.com/maps/api/geocode/json?address=${address}&sensor=false${language}${region}`;
            const r = await this.http.post(url, '').toPromise();
            // r.json().results[0].address_components.map(x => x.long_name)
            let result = r.json().results;
            this.addresses = [];
            for (let i = 0; i < result.length; i++) {
                this.addresses.push(result[i]);
            }
            this.show = result.length > 0;
            this.changeDetectorRef.detectChanges();
        }
    }

    async onclick(address: any) {
        this.show = false;
        let slice = await this.store.select(this.selector).take(1).toPromise();

        if (slice.address && address.address_components.filter(x => x.types.find(x => x === 'route')).length > 0) {
            slice.address.value = address.address_components.filter(x => x.types.find(x => x === 'route'))[0].long_name;
        }

        if (slice.streetNo && address.address_components.filter(x => x.types.find(x => x === 'street_number')).length > 0) {
            slice.streetNo.value = address.address_components.filter(x => x.types.find(x => x === 'street_number'))[0].long_name;
        }

        if (slice.city) {
            if (address.address_components.filter(x => x.types.find(x => x === 'locality')).length > 0) {
                slice.city.value = address.address_components.filter(x => x.types.find(x => x === 'locality'))[0].long_name;
            } else {
                if (address.address_components.filter(x => x.types.find(x => x === 'neighborhood')).length > 0) {
                    slice.city.value = address.address_components.filter(x => x.types.find(x => x === 'neighborhood'))[0].long_name;
                }
            }
        }

        if (slice.county && address.address_components.filter(x => x.types.find(x => x === 'administrative_area_level_2')).length > 0) {
            slice.county.value = address.address_components.filter(x => x.types.find(x => x === 'administrative_area_level_2'))[0].short_name;
        }

        if (slice.zipCode && address.address_components.filter(x => x.types.find(x => x === 'postal_code')).length > 0) {
            slice.zipCode.value = address.address_components.filter(x => x.types.find(x => x === 'postal_code'))[0].long_name;
        }
        if (slice.region && address.address_components.filter(x => x.types.find(x => x === 'administrative_area_level_1')).length > 0) {
            slice.region.value = address.address_components.filter(x => x.types.find(x => x === 'administrative_area_level_1'))[0].long_name;
        }

        if (slice.country && address.address_components.filter(x => x.types.find(x => x === 'country')).length > 0) {
            slice.country.value = address.address_components.filter(x => x.types.find(x => x === 'country'))[0].long_name;
        }

        if (slice.isoCode && address.address_components.filter(x => x.types.find(x => x === 'country')).length > 0) {
            slice.isoCode.value = address.address_components.filter(x => x.types.find(x => x === 'country'))[0].short_name;
        }

        if (slice.federal && address.address_components.filter(x => x.types.find(x => x === 'administrative_area_level_1')).length > 0) {
            slice.federal.value = address.address_components.filter(x => x.types.find(x => x === 'administrative_area_level_1'))[0].short_name;
        }

        if (slice.latitude) {
            slice.latitude.value = address.geometry.location.lat;
        }

        if (slice.longitude) {
            slice.longitude.value = address.geometry.location.lng;
        }

        this.changeDetectorRef.detectChanges();
    }

    close() {
        this.show = false;
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
        address = this.addAddressElem(address, slice.address.value);
        address = this.addAddressElem(address, slice.streetNo.value);
        address = this.addAddressElem(address, slice.city.value);
        address = this.addAddressElem(address, slice.county.value);
        address = this.addAddressElem(address, slice.country.value);
        address = this.addAddressElem(address, slice.federal.value);
        address = this.addAddressElem(address, slice.zipCode.value);

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
