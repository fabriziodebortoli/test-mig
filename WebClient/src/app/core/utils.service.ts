import { Injectable } from '@angular/core';
import {Logger} from 'libclient';

@Injectable()
export class UtilsService {

  constructor(private logger: Logger) {
    this.logger.debug('UtilsService instantiated - ' + Math.round(new Date().getTime() / 1000));
  }

  serializeData(data) {
    let buffer = [];

    // Serialize each key in the object.
    for (let name in data) {
      if (!data.hasOwnProperty(name)) {
        continue;
      }

      let value = data[name];

      buffer.push(
        encodeURIComponent(name) + '=' + encodeURIComponent((value == null) ? '' : value)
      );
    }

    // Serialize the buffer and clean it up for transportation.
    let source = buffer.join('&').replace(/%20/g, '+');
    return (source);
  };

  generateGUID() {
    function s4() {
      return Math.floor((1 + Math.random()) * 0x10000)
        .toString(16)
        .substring(1);
    }

    return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
      s4() + '-' + s4() + s4() + s4();
  };

  toArray(items) {
    let filtered = [];

    if (items === undefined) {
      return filtered;
    }

    if (Object.prototype.toString.call(items) === '[object Array]') {
      return items;
    }
    else {
      filtered.push(items);
      return filtered;
    }
  };
}
