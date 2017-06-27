import { OnInit } from '@angular/core';
import { SidenavService } from '../../../core/services/sidenav.service';
export declare class TopbarComponent implements OnInit {
    private sidenavService;
    constructor(sidenavService: SidenavService);
    ngOnInit(): void;
    toggleSidenav(): void;
}
