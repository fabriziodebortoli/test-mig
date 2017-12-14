///////////////////////////////////////
/// Combobox editor for boolean values.
///////////////////////////////////////
Ext.define('MA.BoolCB', {
    extend: 'Ext.form.ComboBox',
    allowBlank: false,
    store: [true, false],
    mode: 'local',
    triggerAction: 'all',
    selectOnFocus: true
    //,

});

///////////////////////////////////////
/// Combobox editor for boolean values.
///////////////////////////////////////
Ext.define('MA.GenericIconCB', {
    extend: 'MA.BoolCB',
    listeners: {
        render: function (c) {
            new Ext.ToolTip({
                target: c.getEl(),
                html: propertyNotRendered
            });
        }
    }
});

Ext.define('MA.PictureIconPicker', {
    extend: 'Ext.panel.Panel',    
    collapsible: false,    
    header: false,
    frame: false,
    items: [{
        xtype: 'form',
        frame: true,
        timeout: 30,
        items: [{
            xtype: 'filefield',
            name: 'photo',            
            msgTarget: 'side',
            allowBlank: false,
            anchor: '100%',
            buttonText: 'Select image...',
            buttonOnly: true,
            listeners: {

                change: function (sender, value, eOpts) {
                    var form = this.up('form').getForm();                    
                    var sValue = sender.getValue();                    
                    var form = this.up('form').getForm();
                    if (form.isValid()) {
                        form.submit({
                            url: 'upload-file/',
                            waitMsg: 'Uploading your image...',
                            success: function (fp, o) {
                                safeAlert('Success', 'Your image "' + o.result.file + '" has been uploaded.');
                                // Ext.Msg.alert('Success', 'Your photo "' + o.result.file + '" has been uploaded.');
                            }
                        });
                    }
                }
            }
        }]
    }
    ],

    setValue: function (oVal) {
        this.items.getAt(0).items.getAt(0).setValue(oVal);
    },

    getValue: function () {
        var oValue = this.items.getAt(0).items.getAt(0).getValue();
      
        return oValue;
    },

    resetOriginalValue: function(){
        this.items.getAt(0).items.getAt(0).resetOriginalValue();
    },

    isValid: function () {
        var oRes = this.items.getAt(0).items.getAt(0).isValid();
       
        return oRes;
    }

});

Ext.define('MA.form.ComboBox', {
    extend: 'Ext.form.ComboBox',
    allowBlank: false,
    displayField: 'name',
    valueField: 'value',
    typeAhead: true,
    mode: 'local',
    triggerAction: 'all',
    selectOnFocus: true

});

Ext.define('MA.TextAlignCB', {
    extend: 'MA.form.ComboBox',
    store: {
        fields: ['value', 'name'],
        data: [
            { "value": Alignment_Left.value, "name": Alignment_Left.text },
            { "value": Alignment_Center.value, "name": Alignment_Center.text },
            { "value": Alignment_Right.value, "name": Alignment_Right.text }
        ]
    },
    emptyText: '-- Select alignment --'

});

Ext.define('MA.VertAlignCB', {
    extend: 'MA.form.ComboBox',
    store: {
        fields: ['value', 'name'],
        data: [
            { "value": Vertical_Alignment_Top.value, "name": Vertical_Alignment_Top.text },
            { "value": Vertical_Alignment_Center.value, "name": Vertical_Alignment_Center.text },
            { "value": Vertical_Alignment_Bottom.value, "name": Vertical_Alignment_Bottom.text }
        ]
    },
    emptyText: '-- Select alignment --'
});

Ext.define('MA.ComboTypeCB', {
    extend: 'MA.form.ComboBox',
    store: {
        fields: ['value', 'name'],
        data: [
            { "value": comboType_Simple.value, "name": comboType_Simple.text },
            { "value": comboType_Dropdown.value, "name": comboType_Dropdown.text },
            { "value": comboType_DropdownList.value, "name": comboType_DropdownList.text }
        ]
    },
    emptyText: '-- Select combo type --'
});

Ext.define('MA.OwnerDrawCB', {
    extend: 'MA.form.ComboBox',
    store: {
        fields: ['value', 'name'],
        data: [
            { "value": ownerDraw_No.value, "name": ownerDraw_No.text },
            { "value": ownerDraw_Fixed.value, "name": ownerDraw_Fixed.text },
            { "value": ownerDraw_Variable.value, "name": ownerDraw_Variable.text }
        ]
    },
    emptyText: '-- Select ownerDraw --'
});

Ext.define('MA.SelectionCB', {
    extend: 'MA.form.ComboBox',
    store: {
        fields: ['value', 'name'],
        data: [
            { "value": selection_Single.value, "name": selection_Single.text },
            { "value": selection_Multiple.value, "name": selection_Multiple.text },
            { "value": selection_Extended.value, "name": selection_Extended.text },
            { "value": selection_None.value, "name": selection_None.text }
        ]
    },
   
    emptyText: '-- Select selection --'
});

Ext.define('MA.ColorCB', {
    extend: 'MA.form.ComboBox',
    store: {
        fields: ['value'],
        data: [
            { "value": "Black" },
            { "value": "Gray" },
            { "value": "White" },
            { "value": "Etched" }            
        ]
    },

    emptyText: '-- Select color --'
});
