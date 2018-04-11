import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { Component, Input, OnInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';

@Component({
    selector: 'elementCard-component',
    templateUrl: './elementCard-component.html',
    styleUrls: ['./elementCard-component.scss']
})

export class elementCardComponent implements OnInit {
    colorClass: string;

    @Input() backgroundColor: string;
    @Input() textColor: string;
    @Input() header: string;
    @Input() code: string;
    @Input() descrtiption: string;
    
    ngOnInit() {
        this.colorClass = this.textColor + " " + this.backgroundColor;
    }
 }

