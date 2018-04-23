import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { Component, Input, OnInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';

@Component({
    selector: 'gaugePercentageCard',
    templateUrl: './gaugePercentageCard-component.html',
    styleUrls: ['./gaugePercentageCard-component.scss']
})

export class gaugePercentageCardComponent implements OnInit {
    @Input() backgroundColor: string = '#929fba';
    @Input() textColor: string = "#ffffff";
    @Input() header: string = 'Progress';
    @Input() value: number;
    @Input() max: number;

    valuePerc: number;
    maxPerc: string;

    ngOnInit() {
        if (this.max === 0) {
            this.valuePerc = 0;
            this.maxPerc = '{ max: 100 }';
        }
        else {
            this.valuePerc = Math.round(this.value * 100 / this.max);
            if (this.value > this.max)
            {
                this.maxPerc = '{ max: ' + this.valuePerc + ' }';
            }
            else
            {
                this.maxPerc = '{ max: 100 }';
            }
        }
    }

    public colors: any[] = [{
        to: 100,
        color: '#929fba'
    }, {
        from: 100,
        to: this.maxPerc,
        color: '#2e3951'
    }];
}

