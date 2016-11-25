import { UtilsService } from 'tb-core';
import { Logger } from 'libclient';
import { Http } from '@angular/http';
import { Injectable } from '@angular/core';
import { ImageService } from './image.service';

@Injectable()
export class MenuService {

    private selectedApplication: any;
    private selectedGroup: any;
    private selectedMenu: any;

    public applicationMenu: any;
    public environmentMenu: any;

    constructor(private http: Http, private logger: Logger, private utils: UtilsService, private imageService: ImageService) {
        this.logger.debug('MenuService instantiated - ' + Math.round(new Date().getTime() / 1000));
    }

    setSelectedApplication(app) {
        this.selectedApplication = app;
        this.selectedGroup = undefined;
        this.selectedMenu = undefined;
    }

    getSelectedApplication(app) {
        return this.selectedApplication;
    }

    setSelectedGroup(group) {
        this.selectedGroup = group;
        this.selectedMenu = undefined;
    }

    getSelectedGroup() {
        return this.selectedGroup;
    }

    setSelectedMenu(menu) {
        this.selectedMenu = menu;
    } 

    getSelectedMenu() {
        return this.selectedMenu;
    }

    getApplicationIcon(application) {
        return this.imageService.getStaticImage(application);
    }
}