import { Injectable } from '@angular/core';

import { Logger } from './logger.service';

@Injectable()
export class UtilsService {

  constructor(public logger: Logger) { }

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

    // serializeData(data): URLSearchParams {
      
    //       let urlSearchParams = new URLSearchParams();
      
    //       // let buffer = [];
      
    //       // Serialize each key in the object.
    //       for (let name in data) {
    //         if (!data.hasOwnProperty(name)) {
    //           continue;
    //         }
      
    //         let value = data[name];
      
    //         // buffer.push(
    //         //   encodeURIComponent(name) + '=' + encodeURIComponent((value == null) ? '' : value)
    //         // );
      
    //         urlSearchParams.append(name, (value == null) ? '' : value);
    //       }
      
    //       // Serialize the buffer and clean it up for transportation.
    //       // let source = buffer.join('&').replace(/%20/g, '+');
    //       // return (source);
    //       return urlSearchParams;
    //     };

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


  //---------------------------------------------------------------------------------------------
  getCurrentDate() {
    const d = new Date();
    const p = parseInt(
      d.getFullYear() +
      ('00' + (d.getMonth() + 1)).slice(-2) +
      ('00' + d.getDate()).slice(-2) +
      ('00' + d.getHours()).slice(-2) +
      ('00' + d.getMinutes()).slice(-2) +
      ('00' + d.getSeconds()).slice(-2), 10);

    return p;
  }


  //---------------------------------------------------------------------------------------------
  parseBool(str) {

    if (typeof str === 'boolean')
      return str;

    if (typeof str === 'string' && str.toLowerCase() == 'true')
      return true;

    return (parseInt(str) > 0);
  }

  //---------------------------------------------------------------------------------------------
  getApplicationFromQueryString() {
    var application = '';
    var pageUrl = window.location.search;
    var index = pageUrl.indexOf("?app=");
    if (index < 0)
      return application;
    application = pageUrl.substring(index + "?app=".length);
    index = application.indexOf("&");
    if (index < 0)
      return application;

    return application.substring(0, index);
  }

  // ---------------------------------------------------------------------------------------------
  hexToRgba(hex): any {
    let result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result ? {
      r: parseInt(result[1], 16),
      g: parseInt(result[2], 16),
      b: parseInt(result[3], 16),
      a: 1
    } : null;
  }


}
