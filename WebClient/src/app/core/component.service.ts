import { HttpService } from './http.service';
import { WebSocketService } from './websocket.service';
import { ComponentInfo } from 'tb-shared';
import { Router } from '@angular/router';
import { Injectable, Type, ComponentFactoryResolver } from '@angular/core';

@Injectable()
export class ComponentService {
  components: Array<ComponentInfo> = [];
  componentsToCreate = new Array<any>();
  currentComponentId: string; //id del componente in fase di creazione

  constructor(
    private router: Router,
    private webSocketService: WebSocketService,
    private httpService: HttpService) {
    this.webSocketService.windowOpen.subscribe(data => {
      this.componentsToCreate.push(...data.components);
      this.createNextComponent();

    });
    this.webSocketService.windowClose.subscribe(data => {
      if (data && data.id) {
        for (let i = 0; i < this.components.length; i++) {
          let info: ComponentInfo = this.components[i];
          if (info.id == data.id) {
            this.components.splice(i, 1);
            break;
          }
        }
      }
    });

  }
  /*per ogni componente inviato dal server, istanzia il componente angular associato usando il routing
  nel caso di più componenti, le creazioni vanno effettuate in cascata col meccanismo della promise
  per sfruttare lo stesso router outlet
  */
  createNextComponent() {
    if (this.componentsToCreate.length === 0) {
      this.currentComponentId = undefined;
      return;
    }
    let cmp = this.componentsToCreate.pop();
    //"D.ERP.Languages.Languages\IDD_LANGUAGES"
    let url: string = cmp.url;
    this.currentComponentId = cmp.id;

    //TODO gestire meglio gli url, controllo errori
    url = url.substring(2, url.indexOf('\\')).replace(/\./g, '/');
    this.createComponentFromUrl(url).then(() => {
      
    });
  }

  addComponent<T>(component: ComponentInfo) {
    this.components.push(component);
  }
  /**invia un messaggo al server di distruggere il componente/ */
  tryDestroyComponent(component: ComponentInfo) {
    this.webSocketService.doCommand(component.id, 'ID_FILE_CLOSE');
  }
  removeComponent(component: ComponentInfo) {
    let idx = this.components.indexOf(component);
    if (idx === -1) {
      console.debug('ComponentService: cannot remove conponent with id ' + component.id + ' because it does not exist');
      return;
    }
    this.components.splice(idx, 1);
  }
  createComponentFromUrl(url: string): Promise<void> {
    return new Promise<void>(resolve => {
      this.router.navigate([{ outlets: { dynamic: url }, skipLocationChange: false, replaceUrl: false }])
        .then(
        success => {
          this.router.navigate([{ outlets: { dynamic: null }, skipLocationChange: false, replaceUrl: false }]);
          resolve();
        }
        );
    });
  }
  createComponent<T>(component: Type<T>, resolver: ComponentFactoryResolver) {
    let info = new ComponentInfo();
    info.id = this.currentComponentId;
    info.factory = resolver.resolveComponentFactory(component);
    this.addComponent(info);

    this.createNextComponent();
  }
}
