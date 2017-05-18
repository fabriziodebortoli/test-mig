import { UtilsService } from './utils.service';
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
  componentInfoAdded = new EventEmitter<ComponentInfo>();
  componentInfoRemoved = new EventEmitter<ComponentInfo>();
  componentCreationError: EventEmitter<string> = new EventEmitter();
  constructor(
    private router: Router,
    private webSocketService: WebSocketService,
    private httpService: HttpService,
    private logger: Logger,
    private utils: UtilsService) {
    this.subscriptions.push(this.webSocketService.windowOpen.subscribe(data => {
      this.componentsToCreate.push(...data.components);
      this.createNextComponent();

    }));
    this.subscriptions.push(this.webSocketService.windowClose.subscribe(data => {
      if (data && data.id) {
        this.removeComponentById(data.id);
      }
    }));
  }
  argsToString(args) {
    if (typeof (args) === 'object') {
      if (Object.keys(args).length) {
        return JSON.stringify(args);
      } else {
        return undefined;
      }
    } else {
      return undefined;
    }

  }
  createReportComponent(ns: string, args: any = undefined) {
    let url = 'rs/reportingstudio/' + ns + '/';
    args = this.argsToString(args);
    if (args !== undefined) {
      url += args;
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
    const cmp = this.componentsToCreate.pop();
    this.currentComponentId = cmp.id;
    let url = cmp.app.toLowerCase() + '/' + cmp.mod.toLowerCase() + '/' + cmp.name;
    const args = this.argsToString(cmp.args);
    if (args !== undefined) {
      url += '/' + args;
    }
    this.createComponentFromUrl(url);
  }

  addComponent<T>(component: ComponentInfo) {
    this.components.push(component);
    this.componentInfoAdded.emit(component);
  }

  removeComponent(component: ComponentInfo) {
    let idx = this.components.indexOf(component);
    if (idx === -1) {
      console.debug('ComponentService: cannot remove conponent with id ' + component.id + ' because it does not exist');
      return;
    }
    this.components.splice(idx, 1);
    this.componentInfoRemoved.emit(component);
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
    this.componentInfoRemoved.emit(removed);
  }

  createComponentFromUrl(url: string): void {
    this.router.navigate([{ outlets: { dynamic: 'proxy/' + url }, skipLocationChange: false, replaceUrl: false }])
      .then(
      success => {
        this.router.navigate([{ outlets: { dynamic: null }, skipLocationChange: false, replaceUrl: false }]).then(success1 => {
          this.creatingComponent = false;
          this.createNextComponent();
        });

      }
      )
      .catch(reason => {
        console.log(reason);
        this.componentCreationError.emit(reason);

      });
  }
  createComponent<T>(component: Type<T>, resolver: ComponentFactoryResolver, args: any = {}) {
    const info = new ComponentInfo();
    info.id = this.currentComponentId ? this.currentComponentId : this.utils.generateGUID();
    info.factory = resolver.resolveComponentFactory(component);
    info.args = args;
    this.addComponent(info);
  }

  onComponentCreated(info: ComponentInfo) {
    this.componentCreatedSource.next(this.components.indexOf(info) + 2);
  }
}
