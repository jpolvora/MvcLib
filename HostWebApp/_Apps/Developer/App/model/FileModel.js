Ext.define('Dev.model.FileModel', {
    extend: 'Ext.data.Model',

    fields: [
        { name: 'id', type: 'int', defaultValue: 0 },
        { name: 'name', type: 'string' },
        { name: 'fileName', type: 'string' },
        { name: 'fileTime', type: 'date', dateFormat: 'c', persist: false },
        { name: 'IsHidden', type: 'boolean' },
        { name: "checked", type: 'boolean', defaultValue: null, useNull: true, persist: false }
    ],

    proxy: {
        type: 'ajax',
        reader: {
            type: 'json',
            listeners: {
                exception: function (proxy, exception, operation) {
                    console.warn(arguments);
                }
            }
        },
        api: {
            create: null,
            read: 'Services/read',
            update: null,
            destroy: null
        }
    }
});