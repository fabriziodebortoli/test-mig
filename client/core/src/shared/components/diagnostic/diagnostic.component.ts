import { Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { DiagnosticService } from './../../../core/services/diagnostic.service';
import { TbComponent } from './../../../shared/components/tb.component';
import { TbComponentService } from './../../../core/services/tbcomponent.service';

@Component({
    selector: 'tb-diagnostic',
    templateUrl: './diagnostic.component.html',
    styleUrls: ['./diagnostic.component.scss']
})
export class DiagnosticComponent extends TbComponent  {

    constructor(
        public diagnosticService: DiagnosticService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef
      ) { 
        super(tbComponentService, changeDetectorRef);
        this.enableLocalization();
      }

   

    close() {
        this.diagnosticService.resetDiagnostic();
    }

}
