import { InfoService } from './../../../core/services/info.service';
import { Component, Inject, forwardRef, OnInit, ChangeDetectorRef } from '@angular/core';
import { TbComponent } from './../../../shared/components/tb.component';
import { TbComponentService } from './../../../core/services/tbcomponent.service';

@Component({
    selector: 'tb-culture-selector',
    templateUrl: './culture-selector.component.html',
    styleUrls: ['./culture-selector.component.scss']
})
export class CultureSelectorComponent extends TbComponent implements OnInit {

    defaultItem = null;

    constructor(
        public infoService: InfoService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef) {
        super(tbComponentService, changeDetectorRef);
        this.enableLocalization();
    }

    ngOnInit() {
        super.ngOnInit();
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
