import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { Component, Input, OnInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';

@Component({
    selector: 'gaugeNumberCard',
    templateUrl: './gaugeNumberCard-component.html',
    styleUrls: ['./gaugeNumberCard-component.scss']
})

export class gaugeNumberCardComponent implements OnInit {
    @Input() backgroundColor: string = '#929fba';
    @Input() textColor: string = "text-white";
    @Input() header: string = 'Progress';
    @Input() value: number;
    @Input() max: number;

    color = '#ffffff';
    
    ngOnInit() {
    }

    public colors: any[] = [{
        to: this.value,
        color: '#00C851'
    }, {
        from: this.value,
        to: this.max,
        color: '#007E33'
    }];
}

