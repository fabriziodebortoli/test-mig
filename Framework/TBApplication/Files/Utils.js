
function S4() {
    return (((1 + Math.random()) * 0x10000) | 0).toString(16).substring(1);
}

// then to call it, plus stitch in '4' in the third group
function guid() { return (S4() + S4() + "-" + S4() + "-4" + S4().substr(0, 3) + "-" + S4() + "-" + S4() + S4() + S4()).toLowerCase(); }


////////////////////////////////////////////////////////////////////////
/// Escapes special chars (i.e.: {+,*,.,^,?}) in order to get a regex 
/// which looks for exact matches of them.
////////////////////////////////////////////////////////////////////////
addEscapeForRegex = function (sValue) {
    var sResult = sValue;

    if (sResult) {
        sResult = sResult.replace(/[\.]/g, "\\.");
        sResult = sResult.replace(/[\*]/g, "\\*");
        sResult = sResult.replace(/[\+]/g, "\\+");
        sResult = sResult.replace(/[\^]/g, "\\^");
        sResult = sResult.replace(/[\?]/g, "\\?");       
    }
    return sResult;
};

// manipulate prop name so that it shows the property is 
// not rendered by the editor.
getNotRenderedPropName = function (sName) {
    return "(" + sName + ")";
};

Ext.define('SpatialPropertyHelper', {

    scalingFactorX: 1.75,
    scalingFactorY: 2.1,

    singleton: true,

    setScalingFactor: function (fX, fY) {
       scalingFactorX = fX;
       scalingFactorY = fY;
    },

    transformUnitToPx: function (jsonItem) {
        if (Ext.isDefined(jsonItem)) {
            if (Ext.isDefined(jsonItem.width)) {
                jsonItem.width = Math.round(jsonItem.width * this.scalingFactorX);
            }
            if (Ext.isDefined(jsonItem.height)) {
                jsonItem.height = Math.round(jsonItem.height * this.scalingFactorY);
            }
            if (Ext.isDefined(jsonItem.x)) {
                jsonItem.x = Math.round(jsonItem.x * this.scalingFactorX);
            }
            if (Ext.isDefined(jsonItem.y)) {
                jsonItem.y = Math.round(jsonItem.y * this.scalingFactorY);
            }
            if (jsonItem.items) {
                for (var i = 0; i < jsonItem.items.length; i++) {
                    this.transformUnitToPx(jsonItem.items[i]);
                }
            }
        }
    },

    transformPxToUnit: function (jsonItem) {
        if (Ext.isDefined(jsonItem)) {
            if (Ext.isDefined(jsonItem.width)) {
                jsonItem.width = Math.round(jsonItem.width / this.scalingFactorX);
            }
            if (Ext.isDefined(jsonItem.height)) {
                jsonItem.height = Math.round(jsonItem.height / this.scalingFactorY);
            }
            if (Ext.isDefined(jsonItem.x)) {
                jsonItem.x = Math.round(jsonItem.x / this.scalingFactorX);
            }
            if (Ext.isDefined(jsonItem.y)) {
                jsonItem.y = Math.round(jsonItem.y / this.scalingFactorY);
            }
            if (jsonItem.items) {
                for (var i = 0; i < jsonItem.items.length; i++) {
                    this.transformPxToUnit(jsonItem.items[i]);
                }
            }
        }
    }
});

Ext.define('DefaultConfigHelper', {

    singleton: true,
    // populated in the constructor.
    renderEngineDefaultMap: {},
    rcDefaultMap: {},

    editableProps: ['collapsible', 'control', 'collapseDirection', 'title', 'labelWidth',
        'header', 'height', 'label', 'layout', 'datasource', 'enabled', 'hfill',
         'readOnly', 'resizable', 'text', 'textAlign', 'x', 'y', 'visible', 'width', 'labelOnLeft'],
    rcEditableProps: ['acceptFiles', 'alwaysShowSelection', 'auto', 'autoHScroll', 'autoVScroll', 'bitmap', 'border', 'centerImage', 'checkBoxes', 'clientEdge',
                    'color', 'disableDragDrop',
                    'disableNoScroll', 'defaultButton', 'editLabels', 'endEllipsis', 'flat', 'group', 'hasButtons', 'hasLines', 'hasStrings',
                    'helpID', 'horizontalScroll', 'icon', 'infoTip', 'labelOnLeft', 'leftScrollbar', 'linesAtRoot', 'lowerCase', 'modalFrame', 'multiline',
                    'name', 'noHideSelection', 'noIntegralHeight',
                    'noPrefix', 'noRedraw', 'notify', 'noWrap', 'oemConvert', 'ownerDraw',
                    'password', 'pathEllipsis', 'pushLike', 'realSizeControl', 'rigthAlignText', 'rigthToLeftReading', 'selection', 'simple', 'staticEdge', 'sort',
                    'sunken', 'tabIndex', 'tabstop', 'threeState', 'tooltips',
                    'transparent', 'upperCase', 'useTabStop', 'vertAlign', 'verticalAlignment', 'verticalScroll',  'wantReturn', 'wordEllipsis', 'wantKeyInput', 'comboType'
    ],

    readonlyProps: ['type'],

    ////////////////////////////////////////////////////////////////////////////////////
    /// map stating when a property is editable, e.g. multiline: {ownerDraw: true} means
    /// multiline is editable when ownerDraw is true
    ////////////////////////////////////////////////////////////////////////////////////
    dependencyMap: {},

    //////////////////////////////////////////
    /// True if the property is shownn to the user in the property grid. 
    /// In this case the property can be saved on the json file.
    //////////////////////////////////////////
    isUserProperty: function (sProp) {

        return (this.editableProps.contains(sProp) || this.readonlyProps.contains(sProp) || this.rcEditableProps.contains(sProp));
    
    },

    filterPropGrid: function(record){
        var fieldName = record.data.name;
        if (fieldName == 'type') {
            return false;
        }
        
        return true;
    },

    isDefaultValueProp: function (oObj, sProp) {
             
        // text align non trattato come default X demo
        if (sProp === 'textAlign') {
            return false;
        }

    	if (oObj && sProp) {
            //var oRenderDefault = this.getRenderEngineDefault(oObj.type);
            // we have got a render engine default
           /* if (oRenderDefault && Ext.isDefined(oRenderDefault[sProp])) {
                // render engine default contains the prop
                if (oRenderDefault[sProp] == oObj[sProp]) {
                    // and the given value is a default.
                    return true;
                } 
            } else */{
                var oRcDefault = this.getRCDefault(oObj.type);
                if (oRcDefault && Ext.isDefined(oRcDefault[sProp])) {
                    // render engine default contains the prop
                    if (oRcDefault[sProp] == oObj[sProp]) {
                        // and the given value is a default.
                        return true;
                    }
                }
            }
        }
        return false;
    },
    /////////////////////////////////
    /// Returns true if the given property "propId" is among the ones 
    /// handled by renderEngine.
    /////////////////////////////////
    isRenderable: function (propId) {
        return this.editableProps.contains(propId);
        //return true;
    },

    /////////////////////////////////
    /// Returns true if the given property "propId" is among the 
    /// readonly ones or it is not editable because of any 
    // failing dependency.
    /////////////////////////////////
    isReadOnly: function (oObj, propId) {
        if (oObj) {
            // TODO: this may be optimized.
            if (this.readonlyProps.contains(propId)) {
                // property is a readonly one,
                // regardless to any other prop value.
                return true;
            }
            // prop is not a readonly one, check for any dependency        
            //dependencyMap
            var oDependency = (this.dependencyMap[oObj.type] || {})[propId];
            if (oDependency ) {

                for (var prop in oDependency) {
                    // array of possible values for prop
                    var aoPropValues = oDependency[prop];
                   
                    for (var iCount = 0; iCount < aoPropValues.length; iCount++) {                        
                        if (aoPropValues[iCount] == oObj[prop]) {
                            return false;
                        }
                    }
                    //// works with propId depending on just one other property value.
                    //// if the assumption is found wrong update this code.
                    //return iMatched;
                }
                // dependency not satisfied, it is readonly.
                return true;
            }
            
        }
        // nothing said the property is not readonly, then it is.
        return false;
    },

    ////////////////////////////////////
    /// Returns a default object for the given type.
    ////////////////////////////////////
    getRenderEngineDefault: function (oType) {
        return Ext.clone(this.renderEngineDefaultMap[oType] || {});
    },

    getRCDefault: function (oType) {
        return Ext.clone(this.rcDefaultMap[oType] || {});
    },

    ////////////////////////////////////
    /// Returns a default object with the type for the given type.
    ////////////////////////////////////
    getTypedDefault: function (oType) {
        var oConfig = this.getRenderEngineDefault(oType);
        oConfig.type = oType;
        return oConfig;
    },

    constructor: function () {
        this.initRenderEngineDefaultMap();
        this.initRcDefaultMap();
        this.initPropDependencyMap();
    },

    initPropDependencyMap: function () {

        this.dependencyMap[WndObjType_Button] = {
            multiline: { ownerDraw: [false] },
            bitmap: { ownerDraw: [false] },
            flat: { ownerDraw: [false] },
            icon: { ownerDraw: [false] },
            notify: { ownerDraw: [false] },
            defaultButton: { ownerDraw: [false] }
        };

        this.dependencyMap[WndObjType_Edit] = {

            autoVScroll: { multiline: [true] },
            horizontalScroll: { multiline: [true] },
            verticalScroll: { multiline: [true] },
            password: { multiline: [false] }
        };

        this.dependencyMap[WndObjType_Combo] = {

            hasStrings: { ownerDraw: [ownerDraw_Fixed.value, ownerDraw_Variable.value] }
        };
    },

    initRcDefaultMap: function () {
        // Label defaults
        this.rcDefaultMap[WndObjType_Label] = {
            // coming from formEditor
            textAlign: Alignment_Right.value,
            acceptFiles: false,
            border: false,
            text: "",
            centerImage: false,
            clientEdge: false,
            enabled: true,
            endEllipsis: false,
            group: true,
            helpID: false,
            modalFrame: false,
            noPrefix: false,
            noWrap: false,
            notify: false,
            pathEllipsis: false,
            realSizeControl: false,
            rigthToLeftReading: false,
            simple: false,
            staticEdge: false,
            sunken: false,
            tabstop: false,
            tabIndex: 0,
            transparent: false,
            visible: true,
            wordEllipsis:false
        };

        this.rcDefaultMap[WndObjType_Edit] = {
            acceptFiles: false,
        	x: 0,
        	y: 0,
        	visible: true,
        	enabled: true,
        	datasource: "",
        	control: "",
        	textAlign: Alignment_Left.value,
        	autoHScroll: true,
        	autoVScroll: false,
        	horizontalScroll: false,
            verticalScroll: false,
        	border: false,
        	clientEdge: false,
        	leftScrollbar: false,
        	lowerCase: false,
        	modalFrame: false,
        	number: false,
        	rightAlignText: false,
        	rigthToLeftReading: false,
        	staticEdge: false,
        	tabIndex: 0,
        	transparent: false,
        	upperCase: false,
        	helpID: false,
        	multiline: false,
        	noHideSelection: false,
        	oemConvert: false,
        	password: false,
        	tabstop: true,
        	readOnly: false,
        	wantReturn: false,
        	group: false
        };

        // Button defaults
        this.rcDefaultMap[WndObjType_Button] = {
            acceptFiles: false,
            bitmap: false,
            text: "",
            defaultButton: false,
            enabled: true,
            flat: false,
            group: false,
            helpID: false,
            textAlign: Alignment_Right.value,
            icon: false,
            modalFrame: false,
            multiline: false,
            notify: false,
            ownerDraw: false,
            rigthToLeftReading: false,
            staticEdge: false,
            tabstop: false,
            tabIndex: 0,
            transparent: false,
            vertAlign: Vertical_Alignment_Top.value,
            visible: true
        };

        // CheckBox defaults
        this.rcDefaultMap[WndObjType_Check] = {
            acceptFiles: false,
            auto: false,
            bitmap: false,
            text: "",
            enabled: true,
            flat: false,
            group: false,
            helpID: false,
            textAlign: Alignment_Right.value,
            icon: false,
            labelOnLeft: false,
            modalFrame: false,
            multiline: false,
            notify: false,
            pushLike: false,
            rigthToLeftReading: false,
            staticEdge: false,
            tabstop: false,
            tabIndex: 0,
            transparent: false,
            threeState: false,
            vertAlign: Vertical_Alignment_Top.value,
            visible: true
        };

        this.rcDefaultMap[WndObjType_Group] = {
            x: 0,
            y: 0,
            visible: true,
            // groupBox caption alignment
            // TODO: support rendering it.
            horizontalAlignment: "default",
            // RC caption field
            text: "",
            bitmap: false,
            icon: false,
            hfill: false,
            clientEdge: false,
            flat: false,
            modalFrame: false,
            notify: false,
            rightAlignText: false,
            rightToLeftReadingOrder: false,
            staticEdge: false,
            tabIndex: 0,
            transparent: false,
            acceptFiles: false,
            enabled: true,
            helpID: false,
            group: false,
            tabstop: false
        };

        // ComboBox defaults
        this.rcDefaultMap[WndObjType_Combo] = {
            acceptFiles: false,
            auto: false,
            comboType: comboType_Dropdown.value,
            datasource: "",
            disableNoScroll: false,
            enabled: true,
            group: false,
            hasStrings: false,
            helpID: false,
            horizontalScroll: false,
            leftScrollbar: false,
            lowerCase: false,
            modalFrame: false,
            noIntegralHeight: false,
            oemConvert: false,
            ownerDraw: ownerDraw_No.value,
            rightAlignText: false,
            rigthToLeftReading: false,
            sort: false,
            staticEdge: false,
            tabIndex: 0,
            tabstop: false,
            transparent: false,
            upperCase: false,
            verticalScroll: true,
            visible: true
        };

        this.rcDefaultMap[WndObjType_List] = {           
            acceptFiles: false,
            border: true,                        
            clientEdge: false,
            disableNoScroll: false,
            enabled: false,            
            group: false,
            hasStrings: false,
            helpID: false,
            leftScrollbar: false,
            modalFrame: false,
            multiColumn: false,
            noData: false,
            noIntegralHeight: true,
            noRedraw: false,
            notify: false,
            ownerDraw: ownerDraw_No.value,
            rightAlignText: false,
            rigthToLeftReading: false,
            selection: selection_Single.value,
            sort: false,            
            staticEdge: false,            
            tabIndex: 0,
            tabstop: false,
            transparent: false,
            useTabStop:false,
            verticalScroll: true,
            visible: true,
            wantKeyInput: true
        };

        this.rcDefaultMap[WndObjType_Radio] = {
            auto: true,
            bitmap: false,
            clientEdge: false,
            enabled: true,
            flat: false,
            group: false,
            helpID: false,
            horizontalAlignment: "default",
            icon: false,
            labelOnLeft: false,
            modalFrame: false,
            multiline: false,
            notify: false,
            pushLike: false,
            rightAlignText: false,
            rigthToLeftReading: false,
            staticEdge: false,
            tabIndex: 0,
            tabstop: false,
            transparent: false,
            verticalAlignment: 'default',
            visible: true
        };

        this.rcDefaultMap[WndObjType_Tree] = {
            checkBoxes: false,
            disableDragDrop: false,
            editLabels: false,
            hasButtons: false,
            hasLines: false,
            infoTip: false,
            linesAtRoot: false,
            tooltips: true,
            alwaysShowSelection: false
        };

        this.rcDefaultMap[WndObjType_Image] = {
            border: false,
            centerImage: false,
            clientEdge: false,
            color: "black",
            modalFrame: false,
            notify: false,
            realSizeImage: false,
            rightJustify: false,
            staticEdge: false,
            sunken: false,
            tabstop: false,
            transparent: false,
            acceptFiles: false,
            disabled: false,
            helpId: false,
            visible: true,
            group: false,


        };
    },


    initRenderEngineDefaultMap: function (){
        // view object default config.
        this.renderEngineDefaultMap[WndObjType_View] = {
            x: 0,
            y: 0,
            width: 150,
            height: 150,
            hfill: false,
            visible: true,
            enabled: true,
            region: 'center',
            scrollable: true,
            collapsible: false,
            layout: 'absolute',
            cls: 'object-drop-target',
            objectdroptarget: true
        };
        // view object default config.
        this.renderEngineDefaultMap[WndObjType_Thread] = {
            x: 0,
            y: 0,
            width: 150,
            height: 150,
            hfill: false,
            visible: true,
            enabled: true,
            region: 'center',
            scrollable: true,
            collapsible: false,
            layout: 'absolute',
            cls: 'object-drop-target',
            objectdroptarget: true
        };

        this.renderEngineDefaultMap[WndObjType_RadarFrame] =
            this.renderEngineDefaultMap[WndObjType_Dialog] =
            this.renderEngineDefaultMap[WndObjType_PropertyDialog] = {
                x: 0,
                y: 0,
                width: 150,
                height: 150,
                hfill: false,
                visible: true,
                enabled: true,
                scrollable: false,
                layout: 'absolute'
            };

        this.renderEngineDefaultMap[WndObjType_Panel] = {
            x: 0,
            y: 0,
            hfill: false,
            visible: true,
            enabled: true,
            layout: 'absolute',
            cls: 'object-drop-target',
            region: 'center',           
            objectdroptarget: true,
            title: "",
            header: false,
            collapsible: false,
            collapseDirection: 'top',
            resizable: false,
            scrollable: true,
            // coming from formEditor            
            width: 300,
            height: 300
        };

        this.renderEngineDefaultMap[WndObjType_Tabber] = {
            x: 0,
            y: 0,
            width: 150,
            height: 150,
            hfill: false,
            visible: true,
            enabled: true,
            padding: 0
        };

        this.renderEngineDefaultMap[WndObjType_Tab] = {
            x: 0,
            y: 0,
            width: "100%",
            height: "100%",            
            hfill: false,
            visible: true,
            enabled: true,
            padding: 0,
            scrollable: true,
            iconAlign: 'top',
            layout: 'absolute',
            cls: 'object-drop-target',
            objectdroptarget: true
        };

        this.renderEngineDefaultMap[WndObjType_Toolbar] = {
            x: 0,
            y: 0,
            width: 150,
            height: 150,
            hfill: false,
            visible: true,
            enabled: true,
            layout: 'absolute',
            padding: '0 0 0 0',          
            dock: 'top'
        };

        // these defaults are not for a separator (which maps to a WndObjType_ToolbarButton too)
        // but these values should be safe as well since ExtJS is supposed to ignore them.
        // Maybe some problems may arise with the style prop.
        this.renderEngineDefaultMap[WndObjType_ToolbarButton] = {
            x: 0,
            y: 0,
            width: 150,
            height: 150,
            hfill: false,
            visible: true,
            enabled: true,
            iconAlign: 'top',
            scale: 'medium',
            style: { 'overflow': 'visible' }
        };

        this.renderEngineDefaultMap[WndObjType_TabbedToolbar] = {
            x: 0,
            y: 0,
            width: 150,
            height: 150,
            hfill: false,
            visible: true,
            enabled: true,
            padding: '0 0 0 0',
            layout: 'absolute',
            region: "north",
            collapsible: false
        };

        this.renderEngineDefaultMap[WndObjType_Edit] = {
            x: 0,
            y: 0,
            hfill: false,
            visible: true,
            enabled: true,
            textAlign: Alignment_Left.value,
            label: "",
            labelWidth: 0,
            // TODO: needed in the rc defult as well.
            datasource: "",
            // TODO: needed in the rc defult as well.
            control : "",
            enableKeyEvents: true,
            minHeighyt: 10,
            // coming from formEditor     
            // text: 'Edit',            
            width: 150,
            height: 20
        };

        this.renderEngineDefaultMap[WndObjType_WebLinkEdit] =
            this.renderEngineDefaultMap[WndObjType_AddressEdit] =
            this.renderEngineDefaultMap[WndObjType_NamespaceEdit] = {
            x: 0,
            y: 0,
            width: 150,
            height: 150,
            hfill: false,
            visible: true,
            enabled: true,
            vertAlign:Vertical_Alignment_Top.value,
            labelAlign: 'left',
            enableKeyEvents: true
        };
       
        // according to the renderEngine method processBodyEditTable, 
        // processRadio, processMenuItem, processRadar, processBodyEdit
        // it has no defaults.
        this.renderEngineDefaultMap[WndObjType_MenuItem] =
            this.renderEngineDefaultMap[WndObjType_Radar] =  {
                //x: 0,
                //y: 0,
                //width: 150,
                //height: 150,
                //hfill: false,
                //visible: true,
                //enabled: true
            };

        this.renderEngineDefaultMap[WndObjType_BodyEdit] = {
           
        };

        this.renderEngineDefaultMap[WndObjType_Radio] = {
            x: 0,
            y: 0,
            width: 70,
            height: 25,
            label: 'radiobutton',
            textAlign: Alignment_Right.value
        };

        // Label defaults
        this.renderEngineDefaultMap[WndObjType_Label] = {
            x: 0,
            y: 0,
            // coming from formEditor
            text: 'Label',
            width: 150,
            height: 20,
            visible: true,
            enabled: true,
            textAlign: Alignment_Right.value
        };

        // button defaults.
        this.renderEngineDefaultMap[WndObjType_Button] = {
            x: 0,
            y: 0,
            // coming from formEditor
            text: 'Button',
            width: 150,
            height: 20,
            hfill: false,
            visible: true,
            enabled: true
        };


        this.renderEngineDefaultMap[WndObjType_Combo] = {
            x: 0,
            y: 0,
            // coming from formEditor
            label: "",
            width: 200,
            hfill: false,
            visible: true,
            enabled: true,            
            datasource: "",            
            control: "",
            labelWidth: 0,
            height: 20
        };

        this.renderEngineDefaultMap[WndObjType_Check] = {
            x: 0,
            y: 0,
            hfill: false,
            visible: true,
            enabled: true,
            // coming from formEditor
            text: 'Check box',
            // TODO: needed in the rc defult as well.
            datasource: "",
            textAlign: Alignment_Right.value,
            labelOnLeft: false,
            width: 150,
            height: 20
        };

        this.renderEngineDefaultMap[WndObjType_Table] = {
            x: 0,
            y: 0,
            hfill: false,
            visible: true,
            enabled: true,
            // coming from formEditor
            items: [{
                text: 'Column 1', id: guid()
            }, {
                text: 'Column 2', id: guid()
            }],
            width: 250,
            height: 160
        };

        this.renderEngineDefaultMap[WndObjType_Group] = {
            x: 0,
            y: 0,
            hfill: false,
            visible: true,
            enabled: true,
            layout: 'absolute',
            cls: 'object-drop-target',
            objectdroptarget: true,
            bodyStyle: { 'background-color': 'transparent', 'z-index': -1 },
            text : "",
            // coming from formEditor
            width: 150,
            height: 150
        };

        this.renderEngineDefaultMap[WndObjType_RadioGroup] = {
            x: 0,
            y: 0,
            hfill: false,
            visible: true,
            enabled: true,
            width: 150,
            height: 150,
            layout: 'absolute',
            columns: 1,            
            vertical: true
        };
        
        this.renderEngineDefaultMap[WndObjType_Menu] = {
            x: 0,
            y: 0,
            hfill: false,
            visible: true,
            enabled: true,
            margin: '0 0 10 0',
            floating: true
        };
      
        this.renderEngineDefaultMap[WndObjType_MailAddressEdit] = {
            x: 0,
            y: 0,
            width: 150,
            height: 150,
            hfill: false,
            visible: true,
            enabled: true,
            labelAlign: 'left',
            enableKeyEvents: true,
            tabIndex: 0
        };

        this.renderEngineDefaultMap[WndObjType_Image] = {
            x: 0,
            y: 0,
            width: 50,
            height: 50,
            hfill: false,
            visible: true,
            enabled: true,
            shrinkWrap: true,
            src: "editorImages/camera31.png",
            icon: "editorImages/camera31.png",
            tabIndex: 0
        };        
       
        this.renderEngineDefaultMap[WndObjType_CheckList] = {
            x: 0,
            y: 0,
            width: 150,
            height: 150,
            hfill: false,
            visible: true,
            enabled: true,
            hideHeaders: true,
            selModel: {
                selType: 'rowmodel'
            },
            resizable: false,
            tabIndex: 0
        };
        // still empty as processTileGroup is not implemented yet.
        this.renderEngineDefaultMap[WndObjType_TileGroup] = {
            x: 0,
            y: 0,
            width: 150,
            height: 150,
            hfill: false,
            visible: true,
            enabled: true
        };

        this.renderEngineDefaultMap[WndObjType_Tile] = {
            x: 0,
            y: 0,
            width: 150,
            height: 150,
            hfill: false,
            visible: true,
            enabled: true,
            layout: 'vbox',
            cls: 'object-drop-target',
            objectdroptarget: true
        };

        this.renderEngineDefaultMap[WndObjType_TilePart] = {
            x: 0,
            y: 0,
            width: 150,
            height: 150,
            hfill: false,
            visible: true,
            enabled: true,
            collapsible: false,
            border: 0,           
            layout: 'hbox'
        };

        this.renderEngineDefaultMap[WndObjType_TilePartStatic] = {
            x: 0,
            y: 0,
            width: 150,
            height: 150,
            hfill: false,
            visible: true,
            enabled: true,
            bodyStyle: { 'overflow': 'visible', 'z-index': 10000 },
            style: { 'overflow': 'visible' },            
            border: '2px solid green',           
            layout: 'absolute'
        };

        this.renderEngineDefaultMap[WndObjType_TilePartContent] = {
            x: 0,
            y: 0,
            collapsible: false,
            width: 400,
            height: 400,
            hfill: false,
            visible: true,
            enabled: true,
            border: 0,
            layout: 'absolute'
        };

        this.renderEngineDefaultMap[WndObjType_List] = {
            x: 0,
            y: 0,
            hfill: false,
            visible: true,
            enabled: true,
            layout: 'absolute',            
            region: 'center',
            objectdroptarget: true,
            title: "Listbox",
            header: true,
            collapsible: false,
            collapseDirection: 'top',
            resizable: false,
            // coming from formEditor            
            width: 300,
            height: 300,
            tabIndex: 0
        };

        this.renderEngineDefaultMap[WndObjType_Tree] = {
            x: 0,
            y: 0,
            visible: true,
            enabled: true,
            width: 150,
            height: 150,
            tabIndex: 0
        };
}
});

Ext.define('PropertyDescriptionHelper', {

    singleton: true,
    
    descriptionMap: {
        acceptFiles: "Specifies that the control will accept drag and drop.",
        auto: "Specifies that the control automatically toggles its state when selected.",
        autoVScroll: "Automatically scrolls text up when the user presses ENTER on the last line.",
        bitmap: "Specifies that the control displays a bitmap instead of text.",
        border: "Specifies that the control will have a thin border",
        centerImage: "Centers the text vertically within the client area of the control",
        color: "One of: Black, Gray, White, or Etched",
        disableNoScroll: "Specifies that the control will show a scrollbar in the list box at all times.",
        flat: "Specifies that the visual appearance of the control is two-dimensional",
        group: "Specifies the first control in a group of controls based on tab order.",
        hasStrings: "Specifies that an owner-draw control contains items consisting of strings.",
        horizontalScroll: "Specifies that the control will have a horizontal scroll bar.",
        icon: "Specify that the control displays an icon instead of text.",
        label: "Specifies the text displayed by the control.",
        leftScrollbar: "Specifies that the vertical scrollbar (if present) will be on the left side of the control.",
        modalFrame: "Specifies that the control will have a double border.",
        multiColumn: "Specifies a multicolumn list box that is scrolled horizontally.",
        noData: "Specifies that an owner-draw control does not contain any data.",
        noIntegralHeight: "Specifies that the size if the control will be exactly the size specified by the application.",
        noRedraw: "Specifies that the control appearance is not updated when changes are made.",
        notify: "Specifies that the control will notify its parent when it is clicked or double clicked.",
        ownerDraw: "One of: No, Fixed or Variable.",
        pushLike: "Specifies that the control will look and act like a push button.",
        realSizeImage:"Prevents an icon or bitmap from being resized as it is being drawn.",
        rightAlignText: "Specifies that the control's text is right-aligned.",
        rightToLeftReadingOrder: "For languages that support reading order alignment, specifies right-to-left reading order.",
        rightJustify:"When type is Bitmap or Icon, specifies that the lower right corner of the control is fixed.",
        selection: "One of: Single, Multiple, Extended, or None.",
        sort: "Automatically sort strings added to the list box.",
        staticEdge: "Specifies that the control will have a three dimensinal border.",
        tabstop: "Specifies that the user can move to this control with the TAB key.",
        transparent: "Specifies that the control will have a transparent background.",
        useTabStop: "Specifies that the control will recognize and expand tab characters.",
        verticalAlignment: "One of: default, bottom, top or center.",
        verticalScroll: "Specifies that the control will have a vertical scrollbar.",
        visible: "Specifies that the control is initially visible.",
        wantKeyInput: "Specifies that the owner of the control receives WM_VKEYTOITEM messages when a key is pressed while the control has focus.",
        tabIndex: "The tab order index for the control."
    },

    getDescription : function(sProperty){
        return this.descriptionMap[sProperty] || sProperty;
    }

});