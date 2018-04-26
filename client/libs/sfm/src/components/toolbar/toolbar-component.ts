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

    onCustomers() {
        this.router.navigate(['/customers']);
    }
    
    onJobs() {
        this.router.navigate(['/jobs']);
    }
    
    onSaleOrders() {
        this.router.navigate(['/saleOrders']);
    }

    onItems() {
        this.router.navigate(['/items']);
    }

    onOperations() {
        this.router.navigate(['/operations']);
    }

    onWorkCenters() {
        this.router.navigate(['/workCenters']);
    }
    
    onMOs() {
        this.router.navigate(['/mos']);
    }    
    
    onMORoutingSteps() {
        this.router.navigate(['/moSteps']);
    }    

    onLogout() {
        
    }
}

