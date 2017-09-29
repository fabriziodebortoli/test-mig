import { InfoService } from './../../../core/services/info.service';
import { Component, Inject, forwardRef, OnInit } from '@angular/core';

@Component({
    selector: 'tb-culture-selector',
    templateUrl: './culture-selector.component.html'
})
export class CultureSelectorComponent implements OnInit {

    defaultItem = null;

    constructor(public infoService: InfoService) { }

    ngOnInit() {
        const sub = this.infoService.getDictionaries().subscribe(ret => {
            if (sub) {
                sub.unsubscribe();
            }
            this.setDefaultItem();
        });
    }

    setDefaultItem() {
        this.infoService.dictionaries.some(ci => {
            if (ci.code == this.infoService.culture.value) {
                this.defaultItem = ci;
                return true;
            }

            return false;
        });
    }
    onChange(value) {
        this.setDefaultItem();
        this.infoService.saveCulture();
        location.reload();
    }
}
