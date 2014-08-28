Ext.define('Dev.view.LogViewer', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.logviewer',
    title: 'Log Viewer',
    layout: {
        fill: true,
        border: false,
    },

    autoScroll: true,

    initComponent: function () {
        var self = this;

        console.log('loaded', self);

        self.callParent(arguments);
    }
});