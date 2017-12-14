var imageService = function () {

    //---------------------------------------------------------------------------------------------
    this.getStaticImage = function (item) {

        if (item == undefined)
            return undefined;
        if (Object.prototype.toString.call(item) === '[object String]') {
            return 'staticimage/' + item;
        }

        var imageFile = item['image_file'];
        return imageFile == null ? '' : 'staticimage/' + imageFile;
    }

    //---------------------------------------------------------------------------------------------
    this.getObjectIcon = function (object) {
        if (object.sub_type != undefined) {
            if (object.application == undefined)
                return object.sub_type;

            return object.sub_type + object.application;
        }
        return object.objectType;
    }

    //---------------------------------------------------------------------------------------------
    this.isCustomImage = function (object) {
        if (object.customImage == undefined || object.customImage == '')
            return false;
        
        return true;
    }


    //---------------------------------------------------------------------------------------------
    this.getStaticThumbnail = function (document) {

        var urlToRun = 'staticThumbnail/?ns=' + document.target;
        return urlToRun;
    }

    //---------------------------------------------------------------------------------------------
    this.getWorkerImage = function (item) {

        if (item == undefined)
            return;

        var imageFile = item['image_file'];
        if (imageFile == undefined || imageFile == '')
            return undefined;

        return 'staticimage/' + imageFile;
    }


};