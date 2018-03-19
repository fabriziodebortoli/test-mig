import { Injectable } from '@angular/core';
import { HttpService } from './http.service';

@Injectable()
export class ActivationService {

    constructor(public httpService: HttpService) { }

    modules: any[] = [];

    getModules() {
        this.httpService.getModules().take(1).subscribe((json) => {
            this.modules = json.Modules.Module.map(s => s.name.toLowerCase());
        });
    }

    isActivated(application: string, functionality: string) : boolean {
        let key = (application + "." + functionality).toLowerCase();

        return this.modules.find(s => s.name == key) !== undefined;
    }
}