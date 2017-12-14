
/// <reference path="const.js" />
/// <reference path="documentevents.js" />
function setDisabled(extItem, disabled) {
    if (!extItem) {
        return;
    }
    if (extItem.setDisabled)
        extItem.setDisabled(disabled);
    if (extItem.setReadOnly)
        extItem.setReadOnly(disabled);
    if (extItem.setEditable)
        extItem.setEditable(!disabled);
}
function isDisabled(extItem) {
    if (extItem.isDisabled)
        return extItem.isDisabled();
    if (extItem.isReadOnly)
        return extItem.isReadOnly();
    if (extItem.isEditable)
        return !extItem.isEditable();
}

// override of alignPicker adding width calculation to 
// the standard one.
Ext.require('Ext.form.field.Picker', function () {
    var Picker = Ext.form.field.Picker;
    Picker.prototype.alignPicker = Ext.Function.createSequence(
			   Picker.prototype.alignPicker, function (width, height) {
			       if (this.isExpanded && !this.matchFieldWidth) {
			           var picker = this.getPicker();
			           var paging = picker.pagingToolbar;
			           var iItems = paging.items.getCount();
			           var iTotWidth = 0;
			           for (var i = 0; i < iItems; i++) {
			               var oItem = paging.items.getAt(i);
			               if (oItem.margin$) {
			                   iTotWidth += oItem.getBox().width + oItem.margin$.width;
			               } else {
			                   iTotWidth += oItem.getBox().width;
			               }
			           }

			           picker.setSize(iTotWidth, picker.store &&
                                 picker.store.getCount() ? null : 0);
			       }
			   })
});

//Extracts width and height of image at the given Url and make them available to the callback function
function getMeta(url, callback) {
    var img = new Image();
    img.src = url;
    img.onload = function () {
        callback(this.width, this.height);
    };
}


Ext.define('RenderEngine', function () {
    var me, mainPanel, events, mainFrame, treeDocumentStructure, staticIdentifier = 0;
    return {
        mixins: {
            observable: 'Ext.util.Observable'
        },
        onBeforeUnload: function (e) {
            //TODO  var MESSAGE = 'WARNING : You have unsaved changes !!!'; 
            return null;
        },
        getMainFrame: function () {
            return mainFrame;
        },
        getMainPanel: function () {
            return mainPanel;
        },
        constructor: function (config) {
            me = this;
            events = new DocumentEvents(this);
            this.mixins.observable.constructor.call(this, config);
            Ext.dom.Element(window, 'beforeunload', this.onBeforeUnload, this, {
                normalized: false //we need this for firefox 
            });
        },

        findExtJsObj: function (jsonItemId, jsonItem){
            if (!Ext.isDefined(jsonItem.items)){
                return null;
            }

            for (var i = 0; i < jsonItem.items.length; i++) {
                var childJsonItem = jsonItem.items[i];

                if (childJsonItem.id == jsonItemId){
                    return { extJsObj: childJsonItem.extJsObj, parentJsonItem: jsonItem };
                }

                var retObj = me.findExtJsObj(jsonItemId, childJsonItem);
                if (retObj !== null)
                    return retObj;
            }

            return null;
        },

        addViewJsonTree: function (dataFeed) {
            

            if (treeDocumentStructure === undefined) {
                Ext.define('TopWndDescription', {
                    extend: 'Ext.data.Model',
                    fields: ['id','type', 'text', 'items'],
                    hasMany: 'SubWndDescription'
                });

                Ext.define('SubWndDescription', {
                    extend: 'Ext.data.Model',
                    fields: ['id','type', 'text', 'items']
                });
            }
            else {
                mainPanel.remove(treeDocumentStructure);
            }

            var treeStore = Ext.create('Ext.data.TreeStore', {
                model: 'TopWndDescription',
                proxy: {
                    type: 'memory',
                    data: dataFeed,
                    reader: {
                        type: 'json',
                        rootProperty: 'items'
                    }
                },
                root: {
                    text: "Objects",
                    expanded: true,
                },
                listeners: {
                    nodebeforeappend: function (a, node, eOpts) {
                        var nodeText = getTypeDescription(node.data.type);
                        if (node.data.type == WndObjType_LayoutContainer) {
                            nodeText = nodeText + ' ' + getLayoutTypeDescription(node.data.layoutType);
                        }
                        if (node.data.text) {
                            nodeText = nodeText + ' --- Text: [ ' + node.data.text +' ]';
                        }
                        node.data.text = nodeText;
                    }
                }
            });


            treeDocumentStructure = Ext.create('Ext.tree.Panel', {
                height: 650,
                width: 700,
                store: treeStore,
                title: 'Document Structure',
                useArrows: true,
                rootVisible: false,
                listeners: {
                    beforeitemclick: function (a, record, item, index, e, eOpts) {
                        //se e' una foglia, non deve tentare di espanderla, non ho capito perche non lo fa automaticamente.. :(
                        if (record.childNodes.length == 0) {
                            return false;
                        }
                    },
                    beforeitemdblclick: function (a, record, item, index, e, eOpts) {
                        //se e' una foglia, non deve tentare di espanderla, non ho capito perche non lo fa automaticamente.. :(
                        if (record.childNodes.length == 0) {
                            return false;
                        }
                    }
                }

                    
                   
            });

            mainPanel.add(treeDocumentStructure);
        },

        applyDelta: function (deltaObjs) {

           /* Ext.ComponentManager.each(function (key, value, length) {
                var component = Ext.ComponentManager.get(key);
                if (Ext.isDefined(component)) {
                    component.removeCls('elementUpdated');
                }
            }, this);*/

            var obj;
            if (!deltaObjs)
                return;
            //to avoid loops or unexpected behaviours: in this case active tab or focus is set by a server state change, not by user,
            //so I shouldn't notify server
            events.suspendNotify();
            Ext.suspendLayouts();
            for (var i = 0; i < deltaObjs.length; i++) {
                var jsonItem = deltaObjs[i];
                obj = applyDeltaToItem(jsonItem);
            }

            if (Ext.isDefined(mainPanel)) {
                mainPanel.scrollTo(0,0);
            }

            me.addViewJsonTree(mainPanel.jsonItem);

            Ext.resumeLayouts(true);
            events.resumeNotify();

            return obj;
        },

        getEvents: function () { return events; },

        updateItem: function (jsonItem) {
            var sID = jsonItem.id;
            var bHasButton = false;
            if (jsonItem.hasHKL) {
                sID = "txthkl_" + sID;
            } else if (jsonItem.type == WndObjType_WebLinkEdit ||
                jsonItem.type == WndObjType_AddressEdit || (jsonItem.type == WndObjType_Edit && (jsonItem.button || jsonItem.hasButton))) {
                sID = "txtbtn_" + sID;
                bHasButton = true;
            } else if (jsonItem.type == WndObjType_Combo && jsonItem.isHyperLink) {
                sID = "cbLink_" + sID;
            } else if (jsonItem.type == WndObjType_MailAddressEdit) {
                sID = "emailtxt_" + sID;
            }
            var extItem = Ext.ComponentManager.get(sID);
            if (!Ext.isDefined(extItem)) {
                safeAlert("ExtJs component with id " + sID + " and type: " + getTypeDescription(jsonItem.type) + " not found");
                return;
            }

            if (jsonItem.type == WndObjType_Tab) {
                updateItemProp(extItem, jsonItem, 'active', function (ei, val) {
                    safeAlert("updateItemProp: value is " + val);
                    try {
                        //get parent tabber
                        var parentExtItem = Ext.ComponentManager.get(extItem.jsonItem.parentId);
                        if (jsonItem.active === true && parentExtItem) {
                           parentExtItem.setActiveTab(extItem);
                        }
                    }
                    catch (oEx) {
                        safeAlert("updateItemProp 'active' failed. Details: " + oEx.toString());
                    }
                });
            }
               
            if (jsonItem.type == WndObjType_Radar) {
                // update grid peculiar fields
                var aoFields = jsonItem.fields || null;
                var aoStoreFields = [];
                if (aoFields) {
                    for (var iField = 0; iField < aoFields.length; iField++) {
                        aoStoreFields.push(aoFields[iField].dataIndex);
                    }
                }
                var aoRows = jsonItem.table || null;
                var oStore = null;

                var aoRows = [];
                if (jsonItem.table) {
                    for (var iRow = 0; iRow < jsonItem.table.length; iRow++) {
                        if (jsonItem.table[iRow]) {
                            aoRows.push(jsonItem.table[iRow].columns);
                        }
                    }
                }
                // build the new store
                if (aoRows) {
                    oStore = Ext.create('Ext.data.Store', {
                        fields: aoStoreFields,
                        data: {
                            'items': aoRows
                        },
                        proxy: {
                            type: 'memory',
                            reader: {
                                type: 'json',
                                rootProperty: 'items'
                            }
                        }
                    });
                    oStore.parentGrid = extItem;
                }
                extItem.reconfigure(oStore, aoFields);
                if (jsonItem.activeRow >= 0) {
                    try {
                        extItem.getSelectionModel().select(jsonItem.activeRow, false, true);
                    } catch (oEx) {
                        safeAlert("updateItem::Error setting the selected row of a radar grid.");
                    }
                }
            }
            if (extItem) {
                updateItemProp(extItem, jsonItem, 'text', function (ei, val) {
                    safeAlert("updateItemProp: value is " + val);
                    try {
                        if (ei.setText)
                            ei.setText(val);
                        if (ei.setValue)
                            ei.setValue(val);
                        if (ei.setTitle)
                            ei.setTitle(val);
                        if (ei.setBoxLabel)
                            ei.setBoxLabel(val);

                        if (jsonItem.type == WndObjType_Check || jsonItem.type == WndObjType_Radio) {
                            if (ei.setBoxLabel) {
                                ei.setBoxLabel(val);
                            }
                        }
                    }
                    catch (oEx) {
                        safeAlert("updateItemProp 'text' failed. Details: " + oEx.toString());
                    }
                });

                updateItemProp(extItem, jsonItem, 'enabled', function (ei, val) {
                    try {
                        if (ei.customSetDisabled) {
                            ei.customSetDisabled(val !== true);
                        } else {
                            ei.setDisabled(val !== true);
                        }
                    }
                    catch (oEx) {
                        safeAlert("updateItemProp 'enabled' failed. Details: " + oEx.toString());
                    }
                });
                updateItemProp(extItem, jsonItem, 'readOnly', function (ei, val) {
                    ei.isReadOnly = val;
                    if (ei.setReadOnly) {
                        ei.setReadOnly(val);
                    }
                });


                updateItemProp(extItem, jsonItem, 'latestUpdate', function (ei, val) {
                    if (ei.store) {
                        ei.store.loadPage(ei.store.currentPage || 1);
                    }
                });
                updateItemProp(extItem, jsonItem, 'label', function (ei, val) {
                    switch (jsonItem.type) {
                        case WndObjType_Check:
                        case WndObjType_Radio:
                            if (ei.setBoxLabel) {
                                ei.setBoxLabel(val);
                            }
                            break;
                        default:
                            if (ei.setFieldLabel) {
                                ei.setFieldLabel(val);
                            }
                            break;
                    }
                });

                updateItemProp(extItem, jsonItem, 'labelWidth', function (ei, val) {
                    switch (jsonItem.type) {
                        case WndObjType_Check:
                        case WndObjType_Radio:
                            if (ei.setBoxLabel) {
                                ei.setBoxLabel(val);
                            }
                            break;
                        default:
                            if (ei.setLabelWidth) {
                                ei.setLabelWidth(val);
                            }
                            break;
                    }
                });

                updateItemProp(extItem, jsonItem, 'visible', function (ei, val) {
                    if (ei.setVisible)
                        ei.setVisible(val);
                });
                updateItemProp(extItem, jsonItem, 'isVisible', function (ei, val) {
                    if (ei.setVisible) {
                        ei.setVisible(val);
                    } else if (ei.setHidden) {
                        ei.setHidden(val);
                    }
                });
                updateItemProp(extItem, jsonItem, 'width', function (ei, val) {
                    if (ei.setWidth && !jsonItem.hfill) {
                        ei.setWidth(val);
                    }
                });
                updateItemProp(extItem, jsonItem, 'hfill', function (ei, val) {
                    if (ei.setWidth)
                        ei.setWidth(val ? "100%" : jsonItem.width);
                });
                updateItemProp(extItem, jsonItem, 'height', function (ei, val) {
                    if (ei.setHeight)
                        ei.setHeight(val);
                });
                if ( !Ext.isDefined(extItem.jsonItem)) {
                    safeAlert("Undefined extItem.jsonItem. " + "JsonItem candidate to update has id " + sID + " and type: " + getTypeDescription(jsonItem.type));
                    return
                }

                if (extItem.jsonItem.type != WndObjType_Dialog && extItem.jsonItem.type != WndObjType_Toolbar && extItem.jsonItem.type != WndObjType_Tab) {
                    updateItemProp(extItem, jsonItem, 'x', function (ei, val) {
                        if (ei.setLocalX)
                            ei.setLocalX(val);
                    });
                    updateItemProp(extItem, jsonItem, 'y', function (ei, val) {
                        if (ei.setLocalY)
                            ei.setLocalY(val);
                    });
                }
                updateItemProp(extItem, jsonItem, 'checked', function (ei, val) {
                    if (ei.setValue && ei.getValue) {
                        if (ei.getValue() != val) {
                            ei.setValue(val);
                        }
                    }
                });
                updateItemProp(extItem, jsonItem, 'datasource');
                updateItemProp(extItem, jsonItem, 'control');
                if (jsonItem.hasFocus) {
                    extItem.focus();
                }

                updateItemProp(extItem, jsonItem, 'icon', function (ei, val) {
                    if (ei.setIcon) {
                        ei.setIcon(val);
                    }
                    else if (ei.setSrc) {
                        ei.setSrc(val);
                    }
                });
                updateItemProp(extItem, jsonItem, 'selected', function (ei, val) {
                    if (ei.getSelectionModel) {
                        var eMode = ei.getSelectionModel().getSelectionMode();
                        ei.getSelectionModel().deselectAll();
                        for (var iCount = 0; iCount < val.length; iCount++) {
                            // index to be selected.
                            var iCurrSelected = val[iCount].index;
                            var iCurrPage = ei.store.getPageFromRecordIndex(iCurrSelected);
                            if (iCurrPage != ei.store.currentPage) {
                                ei.store.loadPage(iCurrPage);
                            }
                            if (iCurrSelected >= ei.store.pageSize) {
                                iCurrSelected %= ei.store.pageSize;
                            }
                            if (!ei.getSelectionModel().isSelected(iCurrSelected)) {
                                if (iCurrSelected != ei.editingRow) {
                                    ei.getSelectionModel().select(iCurrSelected, true);
                                }
                            }
                        }
                    }
                });
                updateItemProp(extItem, jsonItem, 'items', function (ei, val) {
                    // is extItem a checklist?
                    if (jsonItem.type == WndObjType_CheckList) {
                        // if so, retrieve the checklist store
                        // and update it
                        var oListStore = extItem.getStore();
                        // clear the store
                        oListStore.removeAll();
                        for (var iCount = 0; iCount < jsonItem.items.length; iCount++) {
                            // add items to the store.
                            oListStore.add(jsonItem.items[iCount]);
                        }
                    }

                    if (ei.setIcon)
                        ei.setIcon(val);
                });
                updateItemProp(extItem, jsonItem, 'isMultisel', function (ei, val) {
                    if (ei.getSelectionModel) {
                        ei.getSelectionModel().setSelectionMode(val ? 'MULTI' : 'NONE');
                    }
                });
                updateItemProp(extItem, jsonItem, 'contentModified', function (ei, val) {
                    if (val) {
                        if (ei.getStore && ei.getStore()) {
                            ei.getStore().reload();
                        }
                    }
                });
                updateItemProp(extItem, jsonItem, 'foundRow', function (ei, val) {
                    // force a graphical refresh
                    ei.getStore().fireEvent('refresh');
                });
                updateItemProp(extItem, jsonItem, 'foundColumn', function (ei, val) {
                    // force a graphical refresh
                    ei.getStore().fireEvent('refresh');
                });
                if (bHasButton) {
                    // handle embedded button updates. They are processed 
                    // separately as they may conflict with guest control properties.
                    updateItemProp(extItem, jsonItem, 'buttonText', function (ei, val) {
                        if (ei.setButtonText)
                            ei.setButtonText(val);
                    });
                    updateItemProp(extItem, jsonItem, 'buttonTooltip', function (ei, val) {
                        if (ei.setButtonTooltip)
                            ei.setButtonTooltip(val);
                    });
                    updateItemProp(extItem, jsonItem, 'buttonEnabled', function (ei, val) {
                        if (ei.setButtonEnabled)
                            ei.setButtonEnabled(val);
                    });
                    updateItemProp(extItem, jsonItem, 'buttonIcon', function (ei, val) {
                        if (ei.setIcon)
                            ei.setIcon(val);
                    });

                }
                updateItemProp(extItem, jsonItem, 'textAlign', function (ei, val) {
                    if (ei.setStyle) {
                        var sStyle = extractStyle(ei.jsonItem);
                        ei.setStyle(sStyle);
                    }
                });

                //[Silvano - remove] only in debug to see element target of an update from server
              //  extItem.addCls('elementUpdated');

            }
            function updateItemProp(extItem, jsonItem, propName, fn) {
                try {
                    if (extItem && jsonItem) {
                        if (Ext.isDefined(jsonItem[propName]) && jsonItem[propName] !== extItem.jsonItem[propName]) {
                            extItem.jsonItem[propName] = jsonItem[propName];
                            if (fn) {
                                try {
                                    fn(extItem, jsonItem[propName]);
                                } catch (oEx) {
                                    safeAlert("updateItemProp::error updating property " + propName);
                                }
                            }
                        }
                    }
                }
                catch (oEx) {
                    safeAlert("updateItemProp::error updating property " + propName + " Details: " + oEx.toString());
                }
            }
        },

        createItem: function (jsonItem, parentExtItem) {
            var currentExtItem = null;
            var recurse = true; //containers have to recurse, leaf elements no, it depends on the type
            var oDefaultConfig = DefaultConfigHelper.getRenderEngineDefault(jsonItem.type);

            switch (jsonItem.type) {
                case WndObjType_Thread:
                    currentExtItem = processThread(jsonItem);
                    break;
                case WndObjType_Frame:
                    currentExtItem = processFrame(jsonItem, parentExtItem);
                    break;
                case WndObjType_RadarFrame:
                case WndObjType_Dialog:
                case WndObjType_PropertyDialog:
                    currentExtItem = processDialog(jsonItem, parentExtItem);
                    break;
                case WndObjType_View:
                    currentExtItem = processView(jsonItem, parentExtItem);
                    break;
                case WndObjType_List:
                case WndObjType_Panel:
                    break;
                case WndObjType_Table:
                    currentExtItem = processBodyEditTable(jsonItem, parentExtItem, oDefaultConfig);
                    recurse = false;
                    break;
                case WndObjType_Tabber:
                case WndObjType_TileManager:
                    currentExtItem = processTabber(jsonItem, parentExtItem);
                    break;
                case WndObjType_Tab:
                    currentExtItem = processTab(jsonItem, parentExtItem);
                    break;
                case WndObjType_Toolbar:
                    currentExtItem = processToolbar(jsonItem, parentExtItem);
                    break;
                case WndObjType_ToolbarButton:
                    currentExtItem = processToolbarButton(jsonItem, parentExtItem);
                    recurse = false;
                    break;
                case WndObjType_TabbedToolbar:
                    currentExtItem = processTabbedToolbar(jsonItem, parentExtItem);
                    recurse = false;
                    break;
                case WndObjType_Label:
                    currentExtItem = processLabel(jsonItem, parentExtItem, oDefaultConfig);
                    recurse = false;
                    break;
                case WndObjType_Edit:
                case WndObjType_WebLinkEdit:
                case WndObjType_AddressEdit:
                    currentExtItem = processEdit(jsonItem, parentExtItem, oDefaultConfig);
                    recurse = false;
                    break;
                case WndObjType_Combo:
                    currentExtItem = processCombo(jsonItem, parentExtItem, oDefaultConfig);
                    recurse = false;
                    break;
                case WndObjType_Button:
                    currentExtItem = processButton(jsonItem, parentExtItem, oDefaultConfig);
                    recurse = false;
                    break;
                case WndObjType_Check:
                    currentExtItem = processCheckBox(jsonItem, parentExtItem, oDefaultConfig);
                    recurse = false;
                    break;
                case WndObjType_Group:
                    currentExtItem = processGroupBox(jsonItem, parentExtItem, oDefaultConfig);
                    recurse = true;
                    break;
                case WndObjType_WebLinkEdit:
                    currentExtItem = processNamespaceEdit(jsonItem, parentExtItem);
                    recurse = false;
                    break;
                case WndObjType_RadioGroup:
                    currentExtItem = processRadioGroup(jsonItem, parentExtItem);
                    recurse = true;
                    break;
                case WndObjType_Radio:
                    currentExtItem = processRadio(jsonItem, parentExtItem, oDefaultConfig);
                    recurse = false;
                    break;
                case WndObjType_Menu:
                    currentExtItem = processMenu(jsonItem, parentExtItem);
                    recurse = true;
                    break;
                case WndObjType_MenuItem:
                    currentExtItem = processMenuItem(jsonItem, parentExtItem);
                    recurse = false;
                    break;
                case WndObjType_Radar:
                    currentExtItem = processRadar(jsonItem, parentExtItem);
                    break;
                case WndObjType_MailAddressEdit:
                    currentExtItem = processMailAddressEdit(jsonItem, parentExtItem);
                    recurse = false;
                    break;
                case WndObjType_Image:
                    currentExtItem = processImage(jsonItem, parentExtItem, oDefaultConfig);
                    recurse = false;
                    break;
                case WndObjType_LayoutContainer:
                    currentExtItem = processContainer(jsonItem, parentExtItem);
                    recurse = true;
                    break;
                case WndObjType_BeButton:
                    currentExtItem = processBeButton(jsonItem, parentExtItem);
                    recurse = false;
                    break;
                case WndObjType_BodyEdit:
                    currentExtItem = processBodyEdit(jsonItem, parentExtItem);
                    recurse = false;
                    break;
                case WndObjType_CheckList:
                    currentExtItem = processCheckList(jsonItem, parentExtItem);
                    recurse = false;
                    break;
                case WndObjType_TileGroup:
                    currentExtItem = processTileGroup(jsonItem, parentExtItem);
                    recurse = true;
                    break;
                case WndObjType_Tile:
                    currentExtItem = processTile(jsonItem, parentExtItem);
                    recurse = true;
                    break;
                case WndObjType_TilePart:
                    currentExtItem = processTilePart(jsonItem, parentExtItem);
                    recurse = true;
                    break;
                case WndObjType_TilePartStatic:
                    currentExtItem = processTileStatic(jsonItem, parentExtItem);
                    recurse = true;
                    break;
                case WndObjType_TilePartContent:
                    currentExtItem = processTileContent(jsonItem, parentExtItem);
                    recurse = true;
                    break;
                case WndObjType_Tree:
                    currentExtItem = processTree(jsonItem, parentExtItem, oDefaultConfig);
                    recurse = false;
                    break;

                case WndObjType_HeaderStrip:
                    currentExtItem = processHeaderStrip(jsonItem, parentExtItem, oDefaultConfig);
                    break;

                    //BEGIN report elements  
                case WndObjType_Report:
                    currentExtItem = processWoormReport(jsonItem, parentExtItem, oDefaultConfig);
                    recurse = true;
                    break;

                case WndObjType_FieldReport:
                    currentExtItem = processFieldReport(jsonItem, parentExtItem, oDefaultConfig);
                    recurse = false;
                    break;

                case WndObjType_TableReport: 
                    currentExtItem = processTableReport(jsonItem, parentExtItem, oDefaultConfig);
                    recurse = false;
                    break;

                    //END report elements

                default:
                    safeAlert("jsonItem.type not processed: " + jsonItem.type);
            }

            if (currentExtItem && !Ext.isDefined(currentExtItem.jsonItem)) {
                currentExtItem.jsonItem = jsonItem;
                jsonItem.extJsObj = currentExtItem;

                if (jsonItem.hasFocus) //I have to delay the set focus action until the control is rendered
                    currentExtItem.addListener("render", focusOnRender);
            }
            else
                currentExtItem = parentExtItem;

            if (recurse && jsonItem.items) {
                for (var i = 0; i < jsonItem.items.length; i++) {
                    me.createItem(jsonItem.items[i], currentExtItem);
                }
            }

            return currentExtItem;
        },
        editForm: function () { events.onEditForm(); }

    }
    function applyDeltaToItem(jsonItem) {
        var obj;

        switch (jsonItem.descState) {
            case DescState_REMOVED:
                {

                    if (!Ext.isDefined(jsonItem.descState))
                        jsonItem.descState = DescState_ADDED;//nel caso dell'editor di file statici non ha senso parlare di stato
                    if (jsonItem.type == WndObjType_BodyEdit) {
                        // as body edit is not directly mapped into JS controls, 
                        // instead a grid maps the body edit table, then removing
                        // a body edit amounts to remove its related grid.
                        var asId = jsonItem.id.split("id_");
                        jsonItem.id = "id_t_" + asId[1];
                    }
                    if (jsonItem.type == WndObjType_Combo && jsonItem.isHyperLink) {
                        jsonItem.id = "cbLink_" + jsonItem.id
                    }
                    if (jsonItem.type == WndObjType_MailAddressEdit) {
                        jsonItem.id = "emailtxt_" + jsonItem.id
                    }
                    if (jsonItem.type == WndObjType_WebLinkEdit) {
                        jsonItem.id = "txtbtn_" + jsonItem.id
                    }
                    if (jsonItem.type == WndObjType_AddressEdit) {
                        jsonItem.id = "txtbtn_" + jsonItem.id
                    }

                    if (jsonItem.hasHKL) {
                        jsonItem.id = "txthkl_" + jsonItem.id
                    }

                    // To find the ExtJs item, we navigate the Json tree, not the ExtJs hierarchy 
                    // (in some cases, the latter could differs from the json one). Each json item has a reference to related 
                    // ExtJs item.
                    // retItemObject has this format { extJsObj: <extJsItem>, parentJsonItem: <parentJsonItem> };
                    var retItem = me.findExtJsObj(jsonItem.id, mainPanel.jsonItem);
                    if (retItem == null) {
                        alert("Cannot find ext js component with jsonItem.id = " + jsonItem.id);
                    }
                    var extItem = retItem.extJsObj;

                    //begin debug
                    //to do Silvano, comment following lines, only to catch some hierarchy  misalignment in debug
                    var extItemFoundInExtJSHierarchy = Ext.ComponentManager.get(jsonItem.id);
                    if (extItem !== extItemFoundInExtJSHierarchy) {
                        alert('wrong ext item found during deletion');
                    }
                    //end debug

                    if (extItem) {
                       
                        var parentExtItem = extItem.ownerCt;
                        if (parentExtItem) {
                            parentExtItem.remove(extItem);
                        }
                        
                        //update parent item json structure
                        if (retItem.parentJsonItem.items) {
                            for (var i = 0; i < retItem.parentJsonItem.items.length; i++) {
                                if (retItem.parentJsonItem.items[i].id === jsonItem.id) {
                                    retItem.parentJsonItem.items.splice(i, 1);
                                    break;
                                }
                            }
                        }
                        
                       
                        if (extItem.destroy)
                            extItem.destroy();
                        if (extItem.close)
                            extItem.close();
                        else if (extItem.hide)
                            extItem.hide();

                        if (mainPanel.jsonItem.items.length == 0) {
                            window.close();
                        }
                    }
                    break;
                }
            case DescState_UNCHANGED:
                {
                    break;
                }
            case DescState_UPDATED:
                {
                    me.updateItem(jsonItem);

                    if (jsonItem.items) {
                        for (var i = 0; i < jsonItem.items.length; i++) {
                            applyDeltaToItem(jsonItem.items[i]);
                        }
                    }
                    break;

                }
            case DescState_ADDED:
                {

                    if (jsonItem.parentId) {
                        var parentItem = Ext.getCmp(jsonItem.parentId);
                        if (parentItem) {
                            var loading = parentItem.loadingImage;
                            if (loading)
                                parentItem.remove(loading);
                            obj = me.createItem(jsonItem, parentItem);

                            //update parent item json structure
                            if (!parentItem.jsonItem.items)
                                parentItem.jsonItem.items = [];
                            parentItem.jsonItem.items.push(jsonItem);

                        }
                    }
                    else { //se non ho parent, allora sono la root

                        obj = me.createItem(jsonItem);
                    }

                    break;
                }
        }
        return obj;
    }

    function focusOnRender(sender) {
        sender.focus();
        sender.removeListener('focus', focusOnRender);
        sender.fireEvent('focus', sender);
    }

    function addItemToParent(parentExtItem, jsonItem, currentExtItem) {

        if (parentExtItem && currentExtItem) {
            if (Ext.isDefined(jsonItem.ordinalPosition)) {
                parentExtItem.insert(jsonItem.ordinalPosition, currentExtItem);
            } else {
                parentExtItem.add(currentExtItem);
            }
        }
    }

    function processThread(jsonItem) {
        mainPanel = Ext.create('Ext.container.Viewport', {
            layout: 'fit',
            scrollable: true,
            id: jsonItem.id,
            items: []
        });

        return mainPanel;
    }

    function processFrame(jsonItem, parentExtItem) {
        document.title = jsonItem.text;
        mainFrame = Ext.create('Ext.panel.Panel', {
            title: jsonItem.text,
            id: jsonItem.id,
            scrollable: true,
            collapsible: true,
            layout: 'absolute',
            defaults: {
                bodyStyle: 'padding:0px'
            }
        });
        if (parentExtItem)
            parentExtItem.insert(0, mainFrame);

        return mainFrame;
    }

    function processLabel(jsonItem, parentExtItem, oDefaultConfig) {

        // get the default values, if any.
        var oConfig = oDefaultConfig || {};
        // set the current values, according to the given jsonItem.
        Ext.apply(oConfig, {
            id: jsonItem.id + (staticIdentifier++),
            text: jsonItem.text,
            x: jsonItem.x,
            y: jsonItem.y,
            width: jsonItem.width,
            //minWidth: jsonItem.width,
            height: jsonItem.height,
            //anchor: 'r',
            listeners: {
                focus: events.onFocus
            }
        });

        oConfig.style = extractStyle(jsonItem);

        var currentExtItem = Ext.create('Ext.form.Label', oConfig);

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }
    function processEdit(jsonItem, parentExtItem, oDefaultConfig) {
        var currentExtItem;
        // if (jsonItem.hasCalendar) {
        if (jsonItem.formatter && jsonItem.formatter.name == dateFormatter) {
            // it is a date picker.
            currentExtItem = Ext.create('Ext.form.field.Date', {
                width: jsonItem.hfill ? "100%" : jsonItem.width,
                height: jsonItem.height,
                //minWidth: jsonItem.width,
                //anchor: 'r',
                id: jsonItem.id,
                x: jsonItem.x,
                y: jsonItem.y,
                rawValue: jsonItem.text,
                disableKeyFilter: false,
                enableKeyEvents: true,
                disabled: jsonItem.enabled !== true,
                fieldLabel: jsonItem.label,
                // labelWidth: jsonItem.labelWidth,
                labelAlign: 'left',
                format: 'd/m/Y',
                listeners: {
                    blur: events.onBlur
                }
            });
        }
        else if (jsonItem.formatter && jsonItem.formatter.name == dateTimeFormatter) {
            // it is a date picker.            
            currentExtItem = Ext.create('MA.DateTime', {

            });
        }
        else if (jsonItem.hasHKL) {
            //  HKL to be processed before plain extra button,
            // as it is a sort of specialization.

            var oTxtCfg = {
                width: jsonItem.hfill ? "100%" : jsonItem.width,
                height: jsonItem.height,
                id: jsonItem.id,
                value: jsonItem.text,
                enabled: jsonItem.enabled,
                disabled: jsonItem.enabled !== true,
                fieldLabel: jsonItem.label,
                x: jsonItem.x,
                y: jsonItem.y,
                labelAlign: 'left',
                enableKeyEvents: true,
                listeners: {
                	blur: events.onBlur
                }
            };

            var oUpBtnCfg = {
                ownerId: jsonItem.id,
                height: jsonItem.height / 2,
                handler: events.onHKLUpClick,
                enabled: jsonItem.enabled,
                disabled: jsonItem.enabled !== true,
                listeners: {
                    focus: events.onFocus
                }
            };

            var oLowBtnCfg = {
                ownerId: jsonItem.id,
                height: jsonItem.height / 2,
                handler: events.onHKLLowClick,
                enabled: jsonItem.enabled,
                disabled: jsonItem.enabled !== true,
                listeners: {
                    focus: events.onFocus
                }
            };

            currentExtItem = Ext.create('MA.TxtHKL', {
                id: "txthkl_" + jsonItem.id,
                txtCfg: oTxtCfg,
                upBtnCfg: oUpBtnCfg,
                lowBtnCfg: oLowBtnCfg
                //,anchor: 'r'
            });
        }
            // TODO: it seems that WndObjType_WebLinkEdit should be directly processed by processNamespaceEdit. Is there any reason to 
            // pass through this?
        else if (jsonItem.button || jsonItem.buttonID || jsonItem.hasButton || jsonItem.type == WndObjType_WebLinkEdit || jsonItem.type == WndObjType_AddressEdit) {
            // textbox with a related state button.
            // hotlink and like stuff case
            currentExtItem = processNamespaceEdit(jsonItem, parentExtItem);
        }

        else {
            var sKind = 'Ext.form.field.Text';
            if (jsonItem.multiline) {
                sKind = 'Ext.form.field.TextArea';
            }

            // get the default values, if any.
            var oConfig = oDefaultConfig || {};
            // set the current values, according to the given jsonItem.
            Ext.apply(oConfig, {
                height: jsonItem.height,
                id: jsonItem.id,
                value: jsonItem.text,
                disabled: jsonItem.enabled !== true,
                x: jsonItem.x,
                y: jsonItem.y,
                grow: true,
                width: jsonItem.width,
                labelAlign: jsonItem.labelAlign,
                inputType: jsonItem.password ? 'password' : 'text',
                listeners: {
                	blur: events.onBlur
                }
            });
            if (jsonItem.readonly) {
                oConfig.readOnly = jsonItem.readOnly
            }

            if (Ext.isDefined(jsonItem.label)) {
                oConfig.fieldLabel = jsonItem.label;
                oConfig.labelWidth = jsonItem.labelWidth
            } else {
                oConfig.labelWidth = 0;
            }
            oConfig.fieldStyle = extractStyle(jsonItem);

            currentExtItem = Ext.create(sKind, oConfig);
        }
        if (jsonItem.formatter) {
            var adapter = attachFormatter(currentExtItem, jsonItem.formatter);
            if (adapter) {
                if (currentExtItem.getValue() != jsonItem.text) {
                    // value has not been set, try again with a sanitized value.  

                    if (adapter.oFormatter) {
                        if (adapter.oFormatter.serverToClient) {
                            currentExtItem.setValue(adapter.oFormatter.serverToClient(jsonItem.text));
                        }
                    }
                    else {
                        console.log("no formatter created for json " + jsonItem.formatter.name);
                    }
                }
            } else {
                console.log("no adapter created for formatter " + jsonItem.formatter.name);
            }
        }

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }

    function processNamespaceEdit(jsonItem, parentExtItem) {
        if (jsonItem.button) {
            jsonItem.button.ownerId = jsonItem.id;
            jsonItem.button.disabled = jsonItem.button.enabled !== true;
            jsonItem.button.handler = events.onClick;
            jsonItem.button.listeners = {
                focus: events.onFocus
            };
            if (jsonItem.button.width <= 0) {
                jsonItem.button.width = null;

            }
            if (jsonItem.button.height <= 0) {
                jsonItem.button.height = null;
            }
        } else if (jsonItem.buttonID && jsonItem.buttonID.length > 0) {
            // short description, button description is
            // embedded within the edit one.
            jsonItem.button = {
                handler: events.onClick,
                listeners: {
                    focus: events.onFocus
                },
                id: jsonItem.buttonID,
                ownerId: jsonItem.id,
                icon: jsonItem.buttonIcon,
                text: jsonItem.buttonText,
                tooltip: jsonItem.buttonTooltip,
                width: jsonItem.buttonWidth,
                // height: jsonItem.buttonHeight,
                disabled: jsonItem.buttonEnabled !== true
            };
        } else {
            jsonItem.button = null;
        }
        // hotlink and like stuff case
        var oTxtConfig = {
            width: jsonItem.hfill ? "100%" : jsonItem.width,
            height: jsonItem.height - jsonItem.labelHeight,
            id: jsonItem.id,
            value: jsonItem.text,
            enabled: jsonItem.enabled,
            disabled: jsonItem.enabled !== true,
            fieldStyle: 'text-align: top',
            width: jsonItem.width,
            multiline: jsonItem.multiline,
            x: jsonItem.x,
            y: jsonItem.y,
            labelAlign: jsonItem.labelAlign || 'left',
            enableKeyEvents: true,
            listeners: {
                focus: events.onFocus
            }
        };


        oTxtConfig.fieldStyle = extractStyle(jsonItem);


        // hotlink and like stuff case
        currentExtItem = Ext.create('MA.TxtBtn', {
            txtCfg: oTxtConfig,
            btnCfg: jsonItem.button,
            id: "txtbtn_" + jsonItem.id,
            icon: (jsonItem.button ? jsonItem.button.icon : ""),
            label: jsonItem.label,
            labelAlign: jsonItem.labelAlign || 'left',

            height: jsonItem.height
        });

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }

    function processImage(jsonItem, parentExtItem, oDefaultConfig) {

        // get the default values, if any.
        var oConfig = oDefaultConfig || {};
        // set the current values, according to the given jsonItem.
        Ext.apply(oConfig, {
            id: jsonItem.id,
            // src is used for rendering a default img
            // in editor mode. in that context we have to 
            // keep shown img and its icon (which is 
            // meaningful to EasyLok/WebLook)
            // src: jsonItem.src,
            width: jsonItem.width,
            height: jsonItem.height,
            x: jsonItem.x,
            y: jsonItem.y,
            shrinkWrap: true
        });
       
        oConfig.src = jsonItem.icon;
        
        var currentExtItem = Ext.create('Ext.Img', oConfig);


        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }

    function processMailAddressEdit(jsonItem, parentExtItem) {


        var oConfig = {
            editableTextCfg: {
                //minWidth: jsonItem.width,	            
                id: jsonItem.id,
                value: jsonItem.text,
                //anchor: 'r',
                enabled: jsonItem.enabled,
                disabled: jsonItem.enabled !== true,
                labelAlign: 'left',
                enableKeyEvents: true,
                listeners: {
                    focus: events.onFocus
                }
            },
            readOnlyTextCfg: {
                enabled: !jsonItem.enabled,
                //anchor: 'r',
                disabled: jsonItem.enabled == true
            },
            id: "emailtxt_" + jsonItem.id,
            label: jsonItem.label,
            labelAlign: jsonItem.labelAlign,
            height: jsonItem.height,
            width: jsonItem.hfill ? "100%" : jsonItem.width,
            x: jsonItem.x,
            y: jsonItem.y
        };


        oConfig.style = extractStyle(jsonItem);

        currentExtItem = Ext.create('MA.EmailEdit', oConfig);

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }

    function processCombo(jsonItem, parentExtItem, oDefaultConfig) {
        var currentExtItem;

        // get the default values, if any.
        var cbConfig = oDefaultConfig || {};

        cbConfig.style = extractStyle(cbConfig);
        // set the current values, according to the given jsonItem.
        Ext.apply(cbConfig, {
            id: jsonItem.id,
            value: jsonItem.text,
            disabled: jsonItem.enabled !== true,
            fieldLabel: jsonItem.label,
            labelWidth: jsonItem.labelWidth,
            labelAlign: jsonItem.labelAlign,
            width: jsonItem.width,
            x: jsonItem.x,
            y: jsonItem.y
        });

        
        cbConfig.store = new Ext.data.JsonStore({
            pageSize: 25,
            fields: [{ name: 'val', type: 'string' }],
            proxy: new Ext.data.HttpProxy({
                reader: {
                    type: 'json',
                    rootProperty: 'data',
                    totalProperty: 'num'
                },
                url: "fillCombo/",
                method: 'POST',
                extraParams: { "controlId": jsonItem.id, "session": window.session }
            })
        });
        cbConfig.listeners = {
            focus: events.onFocus,
            select: events.onSelectedItem
        };
        cbConfig.pageSize = 2;
        cbConfig.displayField = 'val';
        cbConfig.valueField = 'val';
        cbConfig.queryMode = 'remote';
        cbConfig.matchFieldWidth = false;
        cbConfig.listConfig = {
            loadingText: 'Loading...', width: 'auto', shrinkWrap: false, resizable: true
        };
        
        if (jsonItem.isHyperLink) {
            // TODO
            // MA.CBLink
            var sLabel = cbConfig.fieldLabel;
            cbConfig.fieldLabel = "";
            currentExtItem = Ext.create('MA.CBLink',
				{
				    editableComboCfg: cbConfig,
				    id: "cbLink_" + jsonItem.id,
				    disabled: cbConfig.disabled,
				    //anchor: 'r',
				    value: jsonItem.text,
				    labelAlign: jsonItem.labelAlign,
				    labelWidth: jsonItem.labelWidth,
				    label: sLabel
				});
        }
        else {
            currentExtItem = Ext.create('Ext.form.ComboBox', cbConfig);
        }
        // TODO: make it work even with the cbLink
        if (jsonItem.formatter)
            attachFormatter(currentExtItem, jsonItem.formatter);

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }

    function processGroupBox(jsonItem, parentExtItem, oDefaultConfig) {

        // get the default values, if any.
        var oConfig = oDefaultConfig || {};
        // set the current values, according to the given jsonItem.
        Ext.apply(oConfig, {
            id: jsonItem.id + (staticIdentifier++),
            title: jsonItem.text,
            width: jsonItem.width,
            height: jsonItem.height,
            height: jsonItem.height,
            x: jsonItem.x,
            y: jsonItem.y,
            disabled: jsonItem.enabled !== true,
            layout: jsonItem.layout || 'absolute',
            cls: 'object-drop-target',
            objectdroptarget: true,
            bodyStyle: { 'background-color': 'transparent', 'z-index': -1 }
        });

        var currentExtItem = Ext.create('Ext.form.FieldSet', oConfig);

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }

    function processRadioGroup(jsonItem, parentExtItem) {
        var currentExtItem = Ext.create('Ext.form.RadioGroup', {
            id: jsonItem.id,
            width: jsonItem.width,
            title: jsonItem.text,
            layout: jsonItem.layout || 'absolute',
            columns: 1,
            vertical: true,
            height: jsonItem.height,
            x: jsonItem.x,
            y: jsonItem.y
        });

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }

    function processRadio(jsonItem, parentExtItem, oDefaultConfig) {

        // get the default values, if any.
        var oConfig = oDefaultConfig || {};

        Ext.apply(oConfig, {
            id: jsonItem.id,
            width: jsonItem.width,
            title: jsonItem.text,
            height: jsonItem.height,
            name: jsonItem.groupName,
            checked: jsonItem.checked,
            boxLabel: jsonItem.label,
            disabled: jsonItem.enabled !== true,
            x: jsonItem.x,
            y: jsonItem.y,
            listeners: {
                focus: events.onFocus,
                change: events.onRadioChange
            }
        });

        // properly place the checkbox label.
        if (jsonItem.labelOnLeft) {
            oConfig.boxLabelAlign = 'before';
        }

        oConfig.style = extractStyle(oConfig);

        var currentExtItem = Ext.create('Ext.form.field.Radio', oConfig);


        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }

    function processButton(jsonItem, parentExtItem, oDefaultConfig) {
        // get the default values, if any.
        var oConfig = oDefaultConfig || {};
        // set the current values, according to the given jsonItem.
        Ext.apply(oConfig, {
            id: jsonItem.id,
            icon: jsonItem.icon,
            width: jsonItem.width,
            text: jsonItem.text,
            height: jsonItem.height,
            tooltip: jsonItem.tooltip,
            disabled: jsonItem.enabled !== true,
            x: jsonItem.x,
            y: jsonItem.y,
            handler: events.onClick,
            listeners: {
                focus: events.onFocus
            }
        });

        var currentExtItem = Ext.create('Ext.button.Button', oConfig);

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }

    function processBeButton(jsonItem, parentExtItem) {
        var currentExtItem = Ext.create('Ext.button.Button', {
            id: jsonItem.id,
            icon: jsonItem.iconNS,
            width: jsonItem.width,
            //minWidth: jsonItem.width,
            //anchor: 'r',
            text: jsonItem.text,
            height: jsonItem.height,
            tooltip: jsonItem.tooltip,
            disabled: jsonItem.enabled !== true,
            //hidden: jsonItem.visible !== true,
            handler: events.onClick,
            listeners: {
                focus: events.onFocus
            }
        });

        return currentExtItem;
    }
    function processToolbarButton(jsonItem, parentExtItem) {
        var currentExtItem;
        if (jsonItem.isSeparator) {
            currentExtItem = Ext.create('Ext.toolbar.Separator', {
                x: jsonItem.x,
                y: 0,
                hidden: jsonItem.visible !== true,
                width: jsonItem.width,
                height: jsonItem.height,
                id: jsonItem.id
            });
        } else if (jsonItem.hasMenu) {
            currentExtItem = Ext.create('Ext.button.Split', {
                id: jsonItem.id,
                icon: jsonItem.icon,
                iconAlign: 'top',
                scale: 'medium',
                //anchor: 'r',
                tooltip: jsonItem.tooltip,
                disabled: jsonItem.enabled !== true,
                hidden: jsonItem.visible !== true,
                text: jsonItem.text,
                x: jsonItem.x,
                y: 0,
                width: jsonItem.width,
                height: jsonItem.height,
                style: { 'overflow': 'visible' },
                handler: events.onClick,
                arrowHandler: events.arrowButtonClick,
                listeners: {
                    focus: events.onFocus
                }
            });
        } else {
            currentExtItem = Ext.create('Ext.button.Button', {
                id: jsonItem.id,
                icon: jsonItem.icon,
                iconAlign: 'top',
                scale: 'medium',
                tooltip: jsonItem.tooltip,
                //anchor: 'r',
                disabled: jsonItem.enabled !== true,
                hidden: jsonItem.visible !== true,
                x: jsonItem.x,
                y: 0,
                width: jsonItem.width,
                height: jsonItem.height,
                style: { 'overflow': 'visible' },
                text: jsonItem.text,
                handler: events.onClick,
                listeners: {
                    focus: events.onFocus
                }
            });
        }

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }

    function processToolbar(jsonItem, parentExtItem) {
        var currentExtItem = Ext.create('Ext.toolbar.Toolbar', {
            id: jsonItem.id,
            layout: jsonItem.layout || 'absolute',
            padding: '0 0 0 0',
            height: jsonItem.height,
            y: 0,
            dock: 'top'
        });
        if (parentExtItem)
            parentExtItem.addDocked(currentExtItem);
        return currentExtItem;
    }

    function processTabbedToolbar(jsonItem, parentExtItem) {

        var currentExtItem = Ext.create('Ext.tab.Panel', {
            id: jsonItem.id,
            width: "100%",
            x: jsonItem.x,
            y: 0,
            height: jsonItem.height + 20,
            padding: '0 0 0 0',
            layout: jsonItem.layout || 'absolute',
            region: "north",

            collapsible: false
        });

        if (parentExtItem)
            parentExtItem.add(currentExtItem);

        // TODO: process children, they have to be treated 
        // in a different manner as they must be embedded
        // into a tab
        if (jsonItem.items) {
            var iItemsSize = jsonItem.items.length
            for (var iCount = 0; iCount < iItemsSize; iCount++) {
                var oChild = processTabbedToolbarItem(jsonItem.items[iCount], currentExtItem);
                if (oChild) {
                    currentExtItem.add(oChild);
                }
            }
        }
        currentExtItem.setActiveTab(jsonItem.activeTabIndex);
        return currentExtItem;
    }
    function processTabbedToolbarItem(oItem, parentItem) {
        if (oItem) {
            var oCurrentExtItem = Ext.create('Ext.form.Panel', {
                id: oItem.id,
                // layout: 'hbox',
                layout: oItem.layout || 'absolute',
                title: oItem.text,
                height: oItem.height,
                listeners: {
                    // expand: onTabActivate,
                    activate: events.onTabActivate
                }
            });
            if (oItem.items) {
                var iItemsSize = oItem.items.length
                for (var iCount = 0; iCount < iItemsSize; iCount++) {
                    var oChild = me.createItem(oItem.items[iCount], oCurrentExtItem);
                    if (oChild) {
                        if (oCurrentExtItem != oChild)
                            oCurrentExtItem.add(oChild);
                    }
                }
            }

            if (oCurrentExtItem) {
                oCurrentExtItem.jsonItem = oItem;

                if (oItem.hasFocus) //I have to delay the set focus action until the control is rendered
                    oCurrentExtItem.addListener("render", focusOnRender);
            }
            return oCurrentExtItem;
        }
    }

    function processMenu(jsonItem, parentExtItem) {
        var oMenu = Ext.create('Ext.menu.Menu', {
            id: jsonItem.id,
            margin: '0 0 10 0',
            floating: true,
            items: [],
            owner: jsonItem.ownerID,
            listeners: {
                click: events.onMenuClick,
                hide: events.onMenuHide
            }
        });

        var ownerItem = Ext.ComponentManager.get(jsonItem.ownerID);

        // resume layout in order to display the menu 
        // in the proper position.
        Ext.resumeLayouts(true);
        events.resumeNotify();

        // show the menu by its owner.
        // TODO: verify what to do for a  
        // right button click menu.
        oMenu.showBy(ownerItem);

        // sospend the layout again as we come from an
        // apply delta which has already suspended it. So
        // we are reverting the rendering state as before 
        // this function call.
        events.suspendNotify();
        Ext.suspendLayouts();

        return oMenu;
    }

    function processMenuItem(jsonItem, parentExtItem) {
        var oMenuItem = Ext.create('Ext.menu.Item', {
            id: jsonItem.id,
            text: jsonItem.text,
            tooltip: jsonItem.tooltip,
            command: jsonItem.command,
            listeners: {
                click: events.onMenuItemClick
            }
        });
        if (parentExtItem)
            parentExtItem.add(oMenuItem);
        return oMenuItem;
    }

    function processDialog(jsonItem, parentExtItem) {

        var currentExtItem = Ext.create('Ext.window.Window', {
            id: jsonItem.id,
            scrollable: false,
            layout: jsonItem.layout || 'absolute',
            width: jsonItem.width,
            height: jsonItem.height,
            closable: false,
            title: jsonItem.text,
            floating: { shadow: false },
            listeners: {
                close: events.onCloseDialog,

                destroy: function (panel, eOpts) {
                    safeAlert("dialog destroy event.");
                },
                hide: function (panel, eOpts) {
                    safeAlert("dialog hide event.");
                }
            }
        });
        currentExtItem.show();
        //ownerCt is set automatically by the framework as soon as a component is added to a container.
        //calling show() manually indicating that your component is not part of a container hierarchy.
        //as we use ownerCt in remove operation, we rely on it
      //  currentExtItem.ownerCt = parentExtItem;
        return currentExtItem;
    }

    function processView(jsonItem, parentExtItem) {
        var mainView = Ext.create('Ext.form.Panel', {
            id: jsonItem.id,
            region: 'center',
            scrollable: true,
            collapsible: false,
            height: jsonItem.height,
            width: '100%',
            x: jsonItem.x,
            y: jsonItem.y,
            //anchor: 'r',
            layout: jsonItem.layout || 'absolute',
            cls: 'object-drop-target',
            objectdroptarget: true
        });
        if (parentExtItem)
            parentExtItem.add(mainView);
        return mainView;
    }

    function processContainer(jsonItem, parentExtItem) {
        var mainView = Ext.create('Ext.form.Panel', {
            region: 'center',
            scrollable: true,
            collapsible: false,
            //anchor: 'r',
            layout: getLayoutTypeDescription(jsonItem.layoutType)
        });
        if (parentExtItem)
            parentExtItem.add(mainView);
        return mainView;
    }

    function processBodyEdit(jsonItem, parentExtItem) {
        if (jsonItem.items) {
            var oGrid;
            var aoButtons = [];
            for (var iCount = 0; iCount < jsonItem.items.length; iCount++) {
                if (jsonItem.items[iCount].type == WndObjType_Table) {
                    // we found the table of the current body edit.
                    // copy into table description body edit position.
                    jsonItem.items[iCount].x = jsonItem.x;
                    jsonItem.items[iCount].y = jsonItem.y;
                    oGrid = me.createItem(jsonItem.items[iCount], parentExtItem);
                }
                if (jsonItem.items[iCount].type == WndObjType_BeButton) {
                    // a body edit button has been found
                    var oBtn = me.createItem(jsonItem.items[iCount], parentExtItem);
                    aoButtons.push(oBtn);
                }
            }
            if (oGrid) {
                var bBar = oGrid.getDockedItems('toolbar[dock="bottom"]');
                bBar[0].add(aoButtons);
            }
        }
        return oGrid;
    }

    function processBodyEditTable(jsonItem, parentExtItem, oDefaultConfig) {

        var oRenderer = function (value, metadata, record, rowIndex, colIndex, store) {
            //        if (value == colIndex) {
            //            metadata.css = 'red';
            //        }
            var oGrid = store.parentGrid;
            if (oGrid) {
                var searchRow = oGrid.jsonItem.foundRow;
                searchRow %= store.pageSize;
                var searchCol = oGrid.jsonItem.foundColumn;

                if (searchRow == rowIndex && searchCol == colIndex) {
                    // TODO: set the search color from the server.
                    metadata.style = 'background-color: red';
                }
            }
            return value;
        }
        var columns = [];
        var fields = [];
        for (var i = 0; i < jsonItem.items.length; i++) {
            var c = jsonItem.items[i];
            var sanitizedID = c.id.split("id_");         
            fields.push(sanitizedID[1]);
            fields.push(sanitizedID[1] + readOnlySuffix);
            var editor = c.items ? me.createItem(c.items[0]) : null;
            if (editor)
                setDisabled(editor, false);
            columns.push({ text: c.text, dataIndex: sanitizedID[1], editor: editor, renderer: oRenderer });
        }

        var iPageSize = 20;
        var iStart = 0;
        var iPage = 1;
        if (jsonItem.selected && jsonItem.selected.length > 0) {
            var iSelected = jsonItem.selected[0].index;
            if (iSelected > iPageSize) {
                iPage = Math.floor(iSelected / iPageSize) + 1;
                iStart = (iPage - 1) * iPageSize;
            }
        }
        var oStore = new Ext.data.JsonStore({
            pageSize: iPageSize,
            fields: fields,
            // autoLoad: true,
            autoLoad: { params: { start: iStart, page: iPage, limit: iPageSize } },
            proxy: new Ext.data.HttpProxy({
                reader: {
                    type: 'json',
                    rootProperty: 'data',
                    totalProperty: 'num'
                },
                url: "fillBodyEdit/",
                method: 'POST',
                extraParams: { "controlId": jsonItem.id, "session": window.session }
            }),
            listeners: {
                load: function (sender, records, successful, eOpts) {
                    events.suspendNotify();
                    events.onGridLoad(sender, records, successful, eOpts);
                    events.resumeNotify();
                }
            }
        });

        // get the default values, if any.
        var oConfig = oDefaultConfig || {};
        // set the current values, according to the given jsonItem.
        Ext.apply(oConfig, {
            id: jsonItem.id,
            width: jsonItem.width,
            height: jsonItem.height,
            //minWidth: jsonItem.width,
            //anchor: 'r',
            columns: columns,
            store: oStore,
            x: jsonItem.x,
            y: jsonItem.y,
            disabled: jsonItem.enabled !== true,
            plugins: [
            Ext.create('Ext.grid.plugin.CellEditing', {
                clicksToEdit: 1,
                listeners: {
                    beforeedit: events.onBeforeEditCell
                }
            })
            ],
            // paging bar on the bottom	        
            bbar: Ext.create('Ext.PagingToolbar', {
                store: oStore,
                displayInfo: true
            }
        )

        });

        var currentExtItem = Ext.create('Ext.grid.Panel', oConfig);

        currentExtItem.getSelectionModel().setSelectionMode(jsonItem.isMultisel ? 'MULTI' : 'SINGLE');
        currentExtItem.getSelectionModel().addListener('selectionchange', events.onGridSelectionChanged);
        oStore.parentGrid = currentExtItem;

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }

    function processCheckList(jsonItem, parentExtItem) {

        // build columns
        var aoCols = [
            {
                xtype: 'checkcolumn', flex: 0, text: 'checked', dataIndex: 'checked',
                listeners: {
                    checkchange: events.onListItemCheckChanged
                },
                parentId: jsonItem.id
            },
            { text: 'text', flex: 1, dataIndex: 'text' }
        ];
        // build the store for providing data to the grid.
        var iPageSize = 25;
        var oStore = new Ext.data.JsonStore({
            pageSize: iPageSize,
            fields: ['checked', 'text'],
            autoLoad: { params: { start: 0, page: 1, limit: iPageSize } },
            proxy: {
                type: 'memory',
                reader: {
                    type: 'json',
                    rootProperty: 'items'
                }
            }
        });

        var cellEditing = Ext.create('Ext.grid.plugin.CellEditing', {
            clicksToEdit: 1,
            listeners: {
                beforeedit: events.onBeforeEditCell
            }
        });

        // build the grid for checkedlistbox rendering.
        var oListBox = Ext.create('Ext.grid.Panel', {
            hideHeaders: true,
            store: oStore,
            selModel: {
                selType: 'rowmodel'
            },
            columns: aoCols,
            plugins: [cellEditing],
            height: jsonItem.height,
            width: jsonItem.width,
            id: jsonItem.id,
            x: jsonItem.x,
            y: jsonItem.y,
            resizable: false
        });

        addItemToParent(parentExtItem, jsonItem, oListBox);

        return oListBox;
    }

    function processRadar(jsonItem, parentExtItem) {
        var columns = [];
        var fields = [];
        if (jsonItem.fields) {
            for (var i = 0; i < jsonItem.fields.length; i++) {
                fields.push(jsonItem.fields[i]['field']);
            }
        }

        var store = new Ext.data.JsonStore({

        });

        var currentExtItem = Ext.create('Ext.grid.Panel', {
            id: jsonItem.id,
            width: jsonItem.width,
            height: jsonItem.height,
            //minWidth: jsonItem.width,
            //anchor: 'r',
            columns: [],
            pageSize: 2,
            store: store,
            disabled: jsonItem.enabled !== true,
            listeners: {
                itemdblclick: events.onRadarDblClickItem,
                afterrender: events.afterRadarRender
                ,
                select: events.radarSelect
            }
        });

        store.parentGrid = currentExtItem;

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }
    function processTabber(jsonItem, parentExtItem) {

        var oConfig = {
            id: jsonItem.id,
            width: '100%',
            height: jsonItem.height,
            //minWidth: jsonItem.width,
            //anchor: 'r',            
            padding: 0,
            layout: 'fit',
            x: jsonItem.x,
            y: jsonItem.y
        };
        oConfig.iconClass = "iconClass_" + jsonItem.id;
        if (jsonItem.iconWidth > 0 && jsonItem.iconHeight > 0) {
            Ext.util.CSS.createStyleSheet("." + oConfig.iconClass + " {height: " + jsonItem.iconHeight + "px; width:" + jsonItem.iconWidth + "px; margin-left: auto; margin-right : auto}", oConfig.iconClass);
        }
        else {
            Ext.util.CSS.createStyleSheet("." + oConfig.iconClass + " {height:25px; width:25px; margin-left: auto; margin-right : auto;  margin-top: auto; margin-bottom: auto;}", oConfig.iconClass);
        }
        if (jsonItem.isVertical) {
            oConfig.tabPosition = 'left';
            oConfig.tabRotation = 0;
        }
        var currentExtItem = Ext.create('Ext.tab.Panel', oConfig);

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }

    function processCheckBox(jsonItem, parentExtItem, oDefaultConfig) {

        // get the default values, if any.
        var oConfig = oDefaultConfig || {};

        // set the current values, according to the given jsonItem.
        Ext.apply(oConfig, {
            id: jsonItem.id,
            checked: jsonItem.checked,
            width: jsonItem.width,
            height: jsonItem.height,
            boxLabel: jsonItem.text,
            tooltip: jsonItem.tooltip,
            disabled: jsonItem.enabled !== true,
            handler: events.onClick,
            x: jsonItem.x,
            y: jsonItem.y,
            listeners: {
                focus: events.onFocus

            }
        });

        // properly place the checkbox label.
        if (jsonItem.labelOnLeft) {
            oConfig.boxLabelAlign = 'before';
        }
        // add style infos
        oConfig.style = extractStyle(oConfig);

        var currentExtItem = Ext.create('Ext.form.field.Checkbox', oConfig);

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }
    function processPanel(jsonItem, parentExtItem, oDefaultConfig) {

        // get the default values, if any.
        var oConfig = oDefaultConfig || {};
        // set the current values, according to the given jsonItem.
        Ext.apply(oConfig, {
            id: jsonItem.id,
            width: jsonItem.hfill ? "100%" : jsonItem.width,
            height: jsonItem.height,
            scrollable: false,
            x: jsonItem.x,
            y: jsonItem.y
        });

        if (jsonItem.layout) {
            oConfig.layout = jsonItem.layout
        }

        if (jsonItem.header) {
            oConfig.header = jsonItem.header;
        }
        if (Ext.isDefined(jsonItem.title)) {
            oConfig.title = jsonItem.title;
        }
        if (Ext.isDefined(jsonItem.collapsible)) {
            oConfig.collapsible = jsonItem.collapsible;
        }
        if (Ext.isDefined(jsonItem.collapseDirection)) {
            oConfig.collapseDirection = jsonItem.collapseDirection;
        }
        oConfig.resizable = jsonItem.editorResizable || jsonItem.resizable;

        oConfig.bodyStyle = extractStyle(jsonItem);

        var currentExtItem = Ext.create('Ext.panel.Panel', oConfig);

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }

    function processTileGroup(jsonItem, parentExtItem) {
        var currentExtItem = Ext.create('Ext.panel.Panel', {
            id: jsonItem.id,

            width: '100%',
            height: jsonItem.height,
            //scrollable: true,
            //minWidth: jsonItem.width,
            //anchor: 'r',
            x: jsonItem.x,
            y: jsonItem.y,
            layout: 'vbox',
            //layout: 'absolute',
            cls: 'object-drop-target',
            objectdroptarget: true
        });

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }

    function processTile(jsonItem, parentExtItem) {
        var oConfig = {
            id: jsonItem.id,
            collapsible: jsonItem.collapsible,
            width: '100%',
            height: jsonItem.height,
            //minWidth: jsonItem.width,
            //anchor: 'r',
            layout: 'absolute',
            cls: 'object-drop-target',
            objectdroptarget: true
        };

        if (jsonItem.hasTitle) {
            oConfig.title = jsonItem.text
        }
        var currentExtItem = Ext.create('Ext.panel.Panel', oConfig);

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }

    function processTilePart(jsonItem, parentExtItem) {

        var oConfig = {
            id: jsonItem.id,
            collapsible: false,
            width: jsonItem.width,
            height: jsonItem.height,
            border: 0,
            x: jsonItem.x,
            y: jsonItem.y - 25/*tile title height server-side */,
            layout: 'hbox'
        };

        var currentExtItem = Ext.create('Ext.panel.Panel', oConfig);

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }

    function processTileStatic(jsonItem, parentExtItem) {

        var oConfig = {
            id: jsonItem.id,
            collapsible: false,
            bodyStyle: { 'background-color': jsonItem.bkgColor, 'overflow': 'visible', 'z-index': 1},
            style: { 'overflow': 'visible' },
            width: jsonItem.hfill ? "100%" : jsonItem.width,
            height: jsonItem.height,
            border: '2px solid green',
            x: jsonItem.x,
            y: jsonItem.y,
            layout: 'absolute'
        };

        var currentExtItem = Ext.create('Ext.panel.Panel', oConfig);

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }

    function processTileContent(jsonItem, parentExtItem) {

        var oConfig = {
            id: jsonItem.id,
            collapsible: false,
            width: jsonItem.width,
            height: jsonItem.height,
            border: 0,
            x: jsonItem.x,
            y: jsonItem.y,
            layout: 'absolute'
        };

        var currentExtItem = Ext.create('Ext.panel.Panel', oConfig);

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }

    function processHeaderStrip(jsonItem, parentExtItem) {

        var oConfig = {
            id: jsonItem.id,
            collapsible: false,
            width: jsonItem.width,
            height: jsonItem.height,
            border: 0,
            x: jsonItem.x,
            y: jsonItem.y,
            layout: 'hbox'
        };

        var currentExtItem = Ext.create('Ext.panel.Panel', oConfig);

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }

    function processTree(jsonItem, parentExtItem, oDefaultConfig) {

        var store = Ext.create('Ext.data.TreeStore', {
            root: {
                text: 'Root',
                expanded: true,
                children: [{ text: 'Child 1', leaf: true }, { text: 'Child 2', leaf: true }]
            }
        });
        // get the default values, if any.
        var oConfig = oDefaultConfig || {};
        // set the current values, according to the given jsonItem.
        Ext.apply(oConfig, {
            id: jsonItem.id,
            width: jsonItem.width,
            height: jsonItem.height,
            disabled: jsonItem.enabled !== true,
            x: jsonItem.x,
            y: jsonItem.y,
            store: store
        });

        var currentExtItem = Ext.create('Ext.tree.TreePanel', oConfig);

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }

    function processTab(jsonItem, parentExtItem) {

        var config = {
            id: jsonItem.id,
            title: jsonItem.text.replace('&', ''),
            width: '100%',
            height: '100%',
            disabled: !jsonItem.enabled,
            //minHeight: 200,		
            //anchor: 'r',
            padding: 0,
            scrollable: true,
            icon: jsonItem.iconSource,
            iconCls: parentExtItem.iconClass,
            iconAlign: 'top',
            layout: 'fit',
            cls: 'object-drop-target',
            objectdroptarget: true,
            listeners: {
                // expand: onTabActivate,
                activate: events.onTabActivate
            }
        };

        var currentExtItem = Ext.create('Ext.form.Panel', config);
        if (parentExtItem)
            parentExtItem.add(currentExtItem);


        if (!jsonItem.items) {
            var loadingImage = Ext.create('Ext.Img',
			{
			    src: 'ajax-loader.gif',
                maxWidth: 40,
                maxHeight: 40,
                style:{'display': 'block', 'margin': 'auto' }
			});
           currentExtItem.add(loadingImage);
           currentExtItem.loadingImage = loadingImage;
        }
        if (jsonItem.active === true && parentExtItem) {
            parentExtItem.setActiveTab(currentExtItem);

        }

        return currentExtItem;
    }

    function processWoormReport(jsonItem, parentExtItem) {
        var config = {
            id: jsonItem.id,
            y: jsonItem.y,
            width: jsonItem.width,
            height: jsonItem.height,
            disabled: !jsonItem.enabled,
            padding: 0,
            layout: jsonItem.layout || 'absolute',
        };

        var currentExtItem = Ext.create('Ext.form.Panel', config);
        if (parentExtItem)
            parentExtItem.add(currentExtItem);
 
        return currentExtItem;
    }


    function processFieldReport(jsonItem, parentExtItem) {
       
        var oConfig = {};
        // set the current values, according to the given jsonItem.
        Ext.apply(oConfig, {
            id: jsonItem.id,
            text: jsonItem.text,
            x: jsonItem.x,
            y: jsonItem.y,
            width: jsonItem.width,
            height: jsonItem.height,
            border: 1,
            style: {
                'borderColor': 'black',
                'borderStyle': 'solid',
                'background-color': 'white'
            }
        });

        var currentExtItem = Ext.create('Ext.form.Label', oConfig);

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }
  
    function processTableReport(jsonItem, parentExtItem) {

        var oConfig = {};
        // set the current values, according to the given jsonItem.
        Ext.apply(oConfig, {
            id: jsonItem.id,
            text: jsonItem.text,
            x: jsonItem.x,
            y: jsonItem.y,
            width: jsonItem.width,
            height: jsonItem.height
        });

        oConfig.style = extractStyle(jsonItem);

        var currentExtItem = Ext.create('Ext.panel.Panel', oConfig);

        addItemToParent(parentExtItem, jsonItem, currentExtItem);

        return currentExtItem;
    }

    function extractStyle(jsonItem) {

        var oStyle = {};
        if (jsonItem.bold) {
            // bold style        
            oStyle['font-weight'] = 'bold';
        }
        if (jsonItem.italic) {
            // italic style        
            oStyle['font-style'] = "italic";
        }
        if (jsonItem.fontHeight) {
            // italic style        
            oStyle['font-size'] = jsonItem.fontHeight;
        }
        if (jsonItem.foreColor) {
            // font color        
            oStyle['color'] = jsonItem.foreColor;
        }
        if (jsonItem.bkgColor) {
            // bkg color        
            oStyle['background-color'] = jsonItem.bkgColor;
        }

        if (jsonItem.fontFaceName) {
            // font family        
            oStyle['font-family'] = jsonItem.fontFaceName;
        }
        if (Ext.isDefined(jsonItem.textAlign)) {
            // text alignment    
            switch (jsonItem.textAlign) {
                case Alignment_Center.value:
                    oStyle['text-align'] = 'center';
                    break;
                case Alignment_Right.value:
                    oStyle['text-align'] = 'right';
                    break;
                default:
                    oStyle['text-align'] = 'left';
                    break;
            }
        }
        if (Ext.isDefined(jsonItem.vertAlign)) {
            // text alignment    
            switch (jsonItem.vertAlign) {
                case Vertical_Alignment_Top.value:
                    oStyle['text-align'] = 'top';
                    break;
                case Vertical_Alignment_Center.value:
                    oStyle['text-align'] = 'center';
                    break;
                case Vertical_Alignment_Bottom.value:
                    oStyle['text-align'] = 'bottom';
                    break;
                default:
                    oStyle['text-align'] = 'top';
                    break;
            }
        }


        //// we have a font description
        //var oStyle = {
        //    'font-family': jsonItem.fontFaceName,
        //    'font-size': jsonItem.fontHeight,
        //    'font-style': sFontStyle,
        //    'font-weight': sFontWeight,
        //    'color': sForeColor,

        //}
        return oStyle;
    }
});
