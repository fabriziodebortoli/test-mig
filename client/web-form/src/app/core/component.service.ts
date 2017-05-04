import { Subject } from 'rxjs/Subject';
import { Injectable, Type, ComponentFactoryResolver, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';

import { Logger } from './logger.service';
import { HttpService } from './http.service';
import { WebSocketService } from './websocket.service';

import { ComponentInfo } from './../shared/models/component.info';

@Injectable()
export class ComponentService {
  components: Array<ComponentInfo> = [];
  componentsToCreate = new Array<any>();
  currentComponentId: string; //id del componente in fase di creazione
  creatingComponent = false;//semaforo
  subscriptions = [];
  private componentCreatedSource = new Subject<number>();
  componentCreated$ = this.componentCreatedSource.asObservable();
  componentDestroyed = new EventEmitter<ComponentInfo>();

  constructor(
    private router: Router,
    private webSocketService: WebSocketService,
    private httpService: HttpService,
    private logger: Logger) {
    this.subscriptions.push(this.webSocketService.windowOpen.subscribe(data => {
      this.componentsToCreate.push(...data.components);
      this.createNextComponent();

    }));
    this.subscriptions.push(this.webSocketService.windowClose.subscribe(data => {
      if (data && data.id) {
        this.removeComponentById(data.id);
      }
    }));

    this.subscriptions.push(this.webSocketService.runReport.subscribe(data => {
      this.createReportComponent(data.ns, data.args);
    }));
  }
  createReportComponent(ns: string, args: any = undefined) {
    let url = 'rs/reportingstudio/' + ns + '/';
    if (args) {
      if (typeof (args) === 'string') {
        url += args;
      }
      else if (typeof (args) === 'object') {
        if (Object.keys(args).length) {
          url += JSON.stringify(args);
        }
      }

    }
    this.createComponentFromUrl(url);
  }
  dispose() {
    this.subscriptions.forEach(subs => subs.unsubscribe());
  }
  /*per ogni componente inviato dal server, istanzia il componente angular associato usando il routing
  nel caso di più componenti, le creazioni vanno effettuate in cascata col meccanismo della promise
  per sfruttare lo stesso router outlet
  */
  createNextComponent() {
    if (this.creatingComponent) {
      return;
    }
    if (this.componentsToCreate.length === 0) {
      this.currentComponentId = undefined;
      return;
    }
    this.creatingComponent = true;
    let cmp = this.componentsToCreate.pop();
    //"D.ERP.Languages.Languages\IDD_LANGUAGES"
    let url: string = cmp.url;
    this.currentComponentId = cmp.id;
    let tokens = url.split('.');
    if (tokens.length !== 4) {
      this.logger.error('Invalid component url: \'' + url + '\'');
      return;
    }
    let app = tokens[1];
    let mod = tokens[2];
    tokens = tokens[3].split('\\');
    if (tokens.length !== 2) {
      this.logger.error('Invalid component url: \'' + url + '\'');
      return;
    }
    let cmpName = tokens[1];
    url = app.toLowerCase() + '/' + mod.toLowerCase() + '/' + cmpName;
    this.createComponentFromUrl(url).then(() => {

    });
  }

  addComponent<T>(component: ComponentInfo) {
    this.components.push(component);
  }

  removeComponent(component: ComponentInfo) {
    let idx = this.components.indexOf(component);
    if (idx === -1) {
      console.debug('ComponentService: cannot remove conponent with id ' + component.id + ' because it does not exist');
      return;
    }
    this.components.splice(idx, 1);
    this.componentDestroyed.emit(component);
  }

  removeComponentById(componentId: string) {
    let removed: ComponentInfo;
    let idx = -1;
    for (let i = 0; i < this.components.length; i++) {
      const comp: ComponentInfo = this.components[i];
      if (comp.id === componentId) {
        idx = i;
        removed = comp;
        break;
      }
    }
    if (idx === -1) {
      console.debug('ComponentService: cannot remove conponent with id ' + componentId + ' because it does not exist');
      return;
    }
    this.components.splice(idx, 1);
    this.componentDestroyed.emit(removed);
  }

  createComponentFromUrl(url: string): Promise<void> {
    return new Promise<void>(resolve => {
      this.router.navigate([{ outlets: { dynamic: 'proxy/' + url }, skipLocationChange: false, replaceUrl: false }])
        .then(
        success => {
          this.router.navigate([{ outlets: { dynamic: null }, skipLocationChange: false, replaceUrl: false }]).then(success1 => {
            resolve();
            this.creatingComponent = false;

            this.createNextComponent();
          });

        }
        );
    });
  }
  createComponent<T>(component: Type<T>, resolver: ComponentFactoryResolver, args: any = {}, generatedID: boolean = false) {
    let info = new ComponentInfo();
    info.id = generatedID ? args.id : this.currentComponentId;
    info.factory = resolver.resolveComponentFactory(component);
    info.args = args;
    this.addComponent(info);
  }

  onComponentCreated(info: ComponentInfo) {
    this.componentCreatedSource.next(this.components.indexOf(info) + 1);
  }
}
