import { OnInit } from '@angular/core';
import { StateButton } from '../../../shared/models/state-button.model';
export declare class StateButtonComponent implements OnInit {
    button: StateButton;
    constructor();
    ngOnInit(): void;
    onClick(): void;
}
