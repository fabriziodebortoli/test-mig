import { InfoService } from './../../../core/services/info.service';
import { CookieService } from 'ngx-cookie';
import { HttpService } from './../../../core/services/http.service';
import { EventDataService } from './../../../core/services/eventdata.service';
import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { LayoutService } from './../../../core/services/layout.service';
import { Logger } from './../../../core/services/logger.service';
import { Component, Input } from '@angular/core';

import { ControlComponent } from './../control.component';

@Component({
    selector: 'tb-vat-code',
    templateUrl: './vat-code.component.html',
    styleUrls: ['./vat-code.component.scss']
})
export class VATCodeComponent extends ControlComponent {
    @Input() disabled: boolean;
    @Input() mask: string;

    @Input() params: string;

    errorMessage: string;

    constructor(
        private logger: Logger,
        public layoutService: LayoutService,
        public tbComponentService: TbComponentService,
        public eventDataService: EventDataService,
        public httpService: HttpService,
        public infoService: InfoService,
        public cookieService: CookieService
    ) {
        // super(layoutService, tbComponentService, eventDataService);
        super(layoutService, tbComponentService);

        this.eventDataService.change.subscribe(cmpId => {
            this.logger.info("model modificato", cmpId)
        })
    }

    onBlur(change) {
        this.logger.log(change.target.value);
        this.isValid(change.target.value);
    }

    isValid(fiscalCode: string) {
        let p = this.params.split(',');

        this.logger.log("p", p)
        this.logger.log("model", this.eventDataService.model);
        this.logger.log("model", this.eventDataService.model["Contacts"]["FiscalCode"].value);

        // logica di validazione
        if (p === []) {
            this.errorMessage = "Uffa!";
        } else {
            this.errorMessage = "Valido!";
        }
    }

    onRealCheck(change) {
        // check se valida
        // ...

        // request http al servizio web che verifical'esistenza del vat code nella realta
        let params = { authtoken: this.cookieService.get('authtoken') };
        let url = this.infoService.getDocumentBaseUrl() + 'getProductInfo/';
        let sub = this.httpService.postData(url, params).subscribe(res => {
            console.log(res)
            if (res) {
                this.errorMessage = "HTTP Uffa!";
            } else {
                this.errorMessage = "HTTP Valido!";
            }
        });
    }

}
