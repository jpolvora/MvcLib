Ext.define('Dev.model.PageParameterModel', {
    extend: 'Ext.data.Model',

    fields: [
        { name: 'Id', type: 'int', defaultValue: 0 },
        { name: 'DbPageId', type: 'int' },
        { name: 'Key', type: 'string' },
        { name: 'Value', type: 'string' },
        { name: 'Created', type: 'date', persist: false },
        { name: 'CreatedBy', type: 'string', persist: false },
        { name: 'Modified', type: 'date', persist: false },
        { name: 'ModifiedBy', type: 'string', persist: false }
    ],

    //belongsTo: {
    //    model: 'PageModel',
    //    name: 'Page',
    //    foreignKey: 'DbPageId'
    //},

    //hasOne: {
    //    model: 'PageModel',
    //    name: 'Pedido',
    //    foreignKey: 'DbPageId',
    //    getterName: 'getPage'
    //},


    /* Se alterar IdProperty, deve alterar todas as outras properties (msg, total, root...) */
    proxy: {
        type: 'ajax',
        reader: {
            type: 'json',
            root: 'data',
            messageProperty: 'msg',
            totalProperty: 'total',
            successProperty: 'success',
            idProperty: "Id",
            listeners: {
                exception: function (proxy, exception, operation) {
                    console.warn(arguments);
                }
            }
        },
        api: {
            create: 'Services/createPageParameter',
            read: 'Services/getPageParameters',
            update: 'Services/updatePageParameter',
            destroy: 'Services/deletePageParameter'
        }
    }
});