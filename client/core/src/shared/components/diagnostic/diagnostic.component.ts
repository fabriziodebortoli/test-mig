import { LocalizationService } from './../../../core/services/localization.service';
import { Component, OnInit } from '@angular/core';
import { DiagnosticService } from './../../../core/services/diagnostic.service';

@Component({
    selector: 'tb-diagnostic',
    templateUrl: './diagnostic.component.html',
    styleUrls: ['./diagnostic.component.scss']
})
export class DiagnosticComponent implements OnInit {

    constructor(
        public diagnosticService: DiagnosticService,
        public localizationService: LocalizationService
    ) {
        
     }

    ngOnInit() {
    }

    close() {
        this.diagnosticService.resetDiagnostic();
    }

}
