import { Injectable, Type, ComponentFactoryResolver, EventEmitter } from '@angular/core';
import { Router } from '@angular/router';

import { ComponentInfo } from './../../shared/models/component-info.model';

import { Logger } from './logger.service';
import { HttpService } from './http.service';
import { UtilsService } from './utils.service';
import { WebSocketService } from './websocket.service';

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
    public router: Router,
    public webSocketService: WebSocketService,
    public httpService: HttpService,
    public logger: Logger,
    public utils: UtilsService) {

    this.subscriptions.push(this.webSocketService.windowOpen.subscribe(data => {
      data.component.tbLoaderDoc = true; //tengo traccia dei componenti che corrispondono a documenti MFC
      this.componentsToCreate.push(data.component);
      this.createNextComponent();

    }));

    this.subscriptions.push(this.webSocketService.windowClose.subscribe(data => {
      if (data && data.id) {
        this.removeComponentById(data.id);
      }
    }));

    this.subscriptions.push(this.webSocketService.close.subscribe(data => {
      for (let i = this.components.length - 1; i >= 0; i--) {
        let cmp = this.components[i];
        if (cmp.tbLoaderDoc) {
          this.removeComponentById(cmp.id);
        }
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
      this.logger.debug('ComponentService: cannot remove component with id ' + component.id + ' because it does not exist');
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
      this.logger.debug('ComponentService: cannot remove component with id ' + componentId + ' because it does not exist');
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
        this.router.navigate([{ outlets: { dynamic: null }, skipLocationChange: false, replaceUrl: false }]).then(() => {
          this.currentComponent = undefined;
          this.createNextComponent();
        });

      }
      )
      .catch(reason => {
        this.logger.warn(reason);
        this.componentCreationError.emit(reason);
        //cannot create client component: close server one!
        this.webSocketService.closeServerComponent(this.currentComponent.id);
        this.currentComponent = undefined;
        this.createNextComponent();
      });
  }
  createComponent<T>(component: Type<T>, resolver: ComponentFactoryResolver, args: any = {}) {
    if (!this.currentComponent) {
      this.currentComponent = new ComponentInfo();
      this.currentComponent.id = this.utils.generateGUID();
    }
    this.currentComponent.factory = resolver.resolveComponentFactory(component);
    this.currentComponent.args = args;

    // check se tab già esistente, configurabile trmite l'attributo name nella createComponent
    this.currentComponent.name = args.name ? args.name : '';
    for (let c of this.components) {
      if (c.name && c.name === this.currentComponent.name) {
        return false;
      }
    };

    if (this.currentComponent.parentId) {
      this.components.some(cmp => {
        if (cmp.id == this.currentComponent.parentId) {
          this.currentComponent.document = cmp.document;
          this.activateComponent = false;
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