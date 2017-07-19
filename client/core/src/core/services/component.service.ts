

import { Injectable, Type, ComponentFactoryResolver, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';

import { Logger } from './logger.service';
import { HttpService } from './http.service';
import { UtilsService } from './utils.service';
import { WebSocketService } from './websocket.service';

import { ComponentInfo } from '../../shared/models';

@Injectable()
export class ComponentService {
  components: Array<ComponentInfo> = [];
  componentsToCreate = new Array<ComponentInfo>();
  currentComponent: ComponentInfo; //componente in fase di creazione
  subscriptions = [];

  componentInfoCreated = new EventEmitter<ComponentCreatedArgs>();
  componentInfoAdded = new EventEmitter<ComponentInfo>();
  componentInfoRemoved = new EventEmitter<ComponentInfo>();
  componentCreationError: EventEmitter<string> = new EventEmitter();

  activateComponent = false;

  constructor(
    private router: Router,
    private webSocketService: WebSocketService,
    private httpService: HttpService,
    private logger: Logger,
    private utils: UtilsService) {
    this.subscriptions.push(this.webSocketService.windowOpen.subscribe(data => {
      this.componentsToCreate.push(data.component);
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
  createReportComponent(ns: string, activate: boolean, args: any = undefined) {
    let url = 'rs/reportingstudio/' + ns + '/';
    args = this.argsToString(args);
    if (args !== undefined) {
      url += args;
    }
    this.createComponentFromUrl(url, activate);
  }
  dispose() {
    this.subscriptions.forEach(subs => subs.unsubscribe());
  }
  /*per ogni componente inviato dal server, istanzia il componente angular associato usando il routing
  nel caso di più componenti, le creazioni vanno effettuate in cascata col meccanismo della promise
  per sfruttare lo stesso router outlet
  */
  createNextComponent() {
    if (this.currentComponent) {
      return;
    }
    if (this.componentsToCreate.length === 0) {
      return;
    }
    this.currentComponent = this.componentsToCreate.pop();
    let url = this.currentComponent.app.toLowerCase() + '/' + this.currentComponent.mod.toLowerCase() + '/' + this.currentComponent.name;
    const args = this.argsToString(this.currentComponent.args);
    if (args !== undefined) {
      url += '/' + args;
    }
    this.createComponentFromUrl(url, true);
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

  createComponentFromUrl(url: string, activate: boolean): void {
    this.activateComponent = activate;
    this.router.navigate([{ outlets: { dynamic: 'proxy/' + url }, skipLocationChange: false, replaceUrl: false }])
      .then(
      success => {
        this.router.navigate([{ outlets: { dynamic: null }, skipLocationChange: false, replaceUrl: false }]).then(success1 => {
          this.currentComponent = undefined;
          this.createNextComponent();
        });

      }
      )
      .catch(reason => {
        console.log(reason);
        this.componentCreationError.emit(reason);
        //cannot create client component: close server one!
        this.webSocketService.closeServerComponent(this.currentComponent.id);
        this.currentComponent = undefined;
        this.createNextComponent();
      });
  }
  createComponent<T>(component: Type<T>, resolver: ComponentFactoryResolver, args: any = {}) {
    if (!this.currentComponent.id) {
      this.currentComponent.id = this.utils.generateGUID();
    }
    this.currentComponent.factory = resolver.resolveComponentFactory(component);
    this.currentComponent.args = args;
    if (this.currentComponent.modal) {
    this.components.some(cmp =>{
      if (cmp.id == this.currentComponent.parentId){
        cmp.document.eventData.openDynamicDialog.emit(this.currentComponent);
        return true;
      }
      return false;
    });
    }
    else {
      this.addComponent(this.currentComponent);
    }

  }

  onComponentCreated(info: ComponentInfo) {
    this.componentInfoCreated.emit(new ComponentCreatedArgs(this.components.indexOf(info), this.activateComponent));
  }
}
export class ComponentCreatedArgs {
  constructor(public index: Number, public activate: boolean) {

  }
}