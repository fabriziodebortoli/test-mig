import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { Component, Input, OnInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';

@Component({
    selector: 'elementCard',
    templateUrl: './elementCard-component.html',
    styleUrls: ['./elementCard-component.scss']
})

export class elementCardComponent implements OnInit {
    @Input() backgroundColor: string = '#929fba';
    @Input() textColor: string = '#ffffff';
    @Input() header: string;
    @Input() code: string;
    @Input() description: string;
    @Input() sameHeight: boolean = false;

    ngOnInit() {
        this.header = this.header + ":";
    } 
 }

