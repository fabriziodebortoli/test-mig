import { ComponentInfo } from './../models/component.info';
import { Router } from '@angular/router';
import { Injectable, Type, ComponentFactoryResolver } from '@angular/core';

@Injectable()
export class ComponentService {
  components: Array<ComponentInfo> = [];

  constructor(private router: Router) {

  }


  addComponent<T>(component: ComponentInfo) {
    this.components.push(component);
  }
  removeComponent(component: ComponentInfo) {
    this.components.splice(this.components.indexOf(component), 1);
  }
  createComponentFromUrl(url: string) {
    this.router.navigate([{ outlets: { dynamic: url }, skipLocationChange: false, replaceUrl: false }]).then(
      success => this.router.navigate([{ outlets: { dynamic: null }, skipLocationChange: false, replaceUrl: false }])
    );
  }
  createComponent<T>(component: Type<T>, resolver: ComponentFactoryResolver) {
    let info = new ComponentInfo();
    info.factory = resolver.resolveComponentFactory(component);
    this.addComponent(info);
  }
}
