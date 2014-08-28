Ext.define('Dev.store.PagesStore', {
    extend: 'Ext.data.Store',
    model: 'Dev.model.PageModel',

    constructor: function () {
        var me = this;

        console.log('ctor_store:' + me.self.getName());


        Ext.applyIf(me, {
            pageSize: 20,
            autoLoad: false
        });
        me.callParent(arguments);
    }
});