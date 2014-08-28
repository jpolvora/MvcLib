Ext.define('Dev.ux.UpperTextField', {
    extend: 'Ext.form.field.Text',
    alias: 'widget.uppertextfield',

    //configuration
    config: {
        uppercaseValue: true //defaults to true
    },

    constructor: function (config) {
        this.initConfig(config);
        this.callParent([config]);
    },

    initComponent: function () {
        var me = this;
        Ext.apply(me, {
            fieldStyle: 'text-transform:uppercase',
            labelStyle: me.allowBlank ? '' : 'font-weight:bold',
            labelSeparator: me.allowBlank ? '' : '* :',
        });

        me.callParent();
    },

    //overriden function
    getValue: function () {
        var val = this.callParent();
        return this.getUppercaseValue() ? val.toUpperCase() : val;
    }
});