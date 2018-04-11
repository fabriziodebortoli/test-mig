import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { Component, Input, OnInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';

@Component({
    selector: 'elementCard',
    templateUrl: './elementCard-component.html',
    styleUrls: ['./elementCard-component.scss']
})

export class elementCardComponent implements OnInit {
    colorClass: string;

    @Input() backgroundColor: string = "bg-secondary";
    @Input() textColor: string = "text-white";
    @Input() header: string;
    @Input() code: string;
    @Input() description: string;
    @Input() sameHeight: boolean = false;

    ngOnInit() {
        this.colorClass = this.textColor + " " + this.backgroundColor;
    } 
 }

