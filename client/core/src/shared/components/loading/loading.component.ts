import { LoadingService } from './../../../core/services/loading.service';
import { Component, OnInit, Input } from '@angular/core';
import { animate, transition, trigger, state, style, keyframes, group } from "@angular/animations";

@Component({
    selector: 'tb-loading',
    templateUrl: './loading.component.html',
    styleUrls: ['./loading.component.scss'],
    animations: [
      trigger(
        'fadeInOut', [
          transition(':enter', [style({ opacity: 0 }), animate('100ms', style({ 'opacity': 1 }))]),
          transition(':leave', [style({ 'opacity': 1 }), animate('500ms', style({ 'opacity': 0 }))])
        ]
      )
    ]
})
export class LoadingComponent {

    constructor(public loadingService: LoadingService) {

    }


}
