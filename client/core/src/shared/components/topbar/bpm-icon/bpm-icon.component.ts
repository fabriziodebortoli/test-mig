import { ComponentService } from './../../../../core/services/component.service';
import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';
import { URLSearchParams, Http, Response } from '@angular/http';

@Component({
    selector: 'tb-bpm-icon',
    templateUrl: './bpm-icon.component.html',
    styleUrls: ['./bpm-icon.component.scss']
})
export class BPMIconComponent {

    constructor(public componentService: ComponentService) { }

    openBPMPage() {
        this.componentService.createComponentFromUrl('bpm/page', true);
    }

}
