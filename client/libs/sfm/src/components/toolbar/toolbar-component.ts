import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { Component, Input, OnInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';

@Component({
    selector: 'toolbar',
    templateUrl: './toolbar-component.html',
    styleUrls: ['./toolbar-component.scss']
})

export class toolbarComponent implements OnInit {

    @Input() workerName: string;
    @Input() workerImage: string;
    
    today: Date = new Date();
    
    constructor(
        private route: ActivatedRoute,
        private router: Router,
    ) {}

    ngOnInit() {
    }

    onWorkCenter() {
        this.router.navigate(['/workCenters']);
    }
    
    onOperation() {
        this.router.navigate(['/operations']);
    }
    
    onMORoutingStep() {
        this.router.navigate(['/moSteps']);
    }    
}

