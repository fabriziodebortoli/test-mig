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
        public eventDataService: EventDataService
    ) {
        super(layoutService, tbComponentService);
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
        this.errorMessage = "Uffa!";
    }

}
