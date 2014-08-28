Ext.define('Dev.model.PageModel', {
    extend: 'Ext.data.Model',

    fields: [
        { name: 'Id', type: 'int', defaultValue: 0 },
        { name: 'VirtualPath', type: 'string' },
        { name: 'Description', type: 'string' },
        { name: 'LinkText', type: 'string' },
        { name: 'Permalink', type: 'string' },
        { name: 'IsMenuItem', type: 'boolean' },
        { name: 'MenuItemGroup', type: 'string' },
        { name: 'Layout', type: 'string' },
        { name: 'Title', type: 'string' },
        { name: 'MetaKeywords', type: 'string' },
        { name: 'MetaDescription', type: 'string' },
        { name: 'Created', type: 'date', persist: false },
        { name: 'CreatedBy', type: 'string', persist: false },
        { name: 'Modified', type: 'date', persist: false },
        { name: 'ModifiedBy', type: 'string', persist: false }
    ],

    //hasMany: {
    //    model: 'PageParameterModel',
    //    name: 'Parameters'
    //},

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
            create: 'Services/createPage',
            read: 'Services/getPages',
            update: 'Services/updatePage',
            destroy: 'Services/deletePage'
        }
    }
});