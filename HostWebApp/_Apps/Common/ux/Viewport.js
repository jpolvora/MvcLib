Ext.define('Dev.ux.Viewport', {
    extend: 'Ext.container.Viewport',
    renderTo: Ext.getBody(),

    layout: {
        align: 'stretch',
        pack: 'center',
        type: 'border'
    },

    config: {
        title: 'Default Viewport'
    },

    initComponent: function () {
        var me = this;
        console.log('viewport', me);

        Ext.apply(me, {
            items: [
                /*NORTH*/
                {
                    xtype: 'panel',
                    region: 'north',
                    layout: {
                        type: 'fit',
                        border: false
                    },
                    titleCollapse: true,
                    animate: false,
                    header: {
                        title: me.getTitle(),
                        padding: "7 0 7 0",
                    }
                },
                /*EAST/WEST*/
                {
                    xtype: 'panel',
                    itemId: 'menu-panel',
                    region: Dev.app.getStateProvider().get("menu-panel", "west"),
                    width: 200,
                    title: 'Menu',
                    collapsible: true,
                    split: true,
                    hideCollapseTool: false,
                    defaultType: 'treepanel',
                    layout: {
                        type: 'accordion',
                        //fill: true,
                    },
                    defaults: {
                        animate: false,
                        animCollapse: false,
                        collapsible: false,
                        collapsed: false,
                        activeOnTop: true,
                        rootVisible: false,
                        expanded: true,
                        leaf: true,
                    },

                    tools: [
                        {
                            type: 'toggle',
                            handler: function () {
                                var pnl = me.query('#menu-panel')[0];
                                if (pnl.region === 'west') {
                                    pnl.setBorderRegion('east');
                                    Dev.app.getStateProvider().set("menu-panel", "east");
                                } else {
                                    pnl.setBorderRegion('west');
                                    Dev.app.getStateProvider().set("menu-panel", "west");
                                }
                            }
                        }
                    ],
                    items: me.getMenuItens.call(me)
                },
                /*CENTER*/
                {
                    xtype: 'tabpanel',
                    region: 'center',
                    itemId: 'screensPanel',
                    border: false,
                    layout: {
                        type: 'fit',
                        align: 'stretch'
                    },
                    defaults: { // defaults are applied to items, not the container
                        //autoScroll: true,
                        closable: true
                    },
                    items: me.getTabs.call(me)
                },
                /*SOUTH*/
                //{
                //    region: 'south',
                //    title: 'Output',
                //    itemId: 'outputpanel',
                //    collapsible: true,
                //    collapsed: true,
                //    layout: 'vbox',
                //    autoScroll: true,
                //    split: true,
                //    animCollapse: false,
                //    height: 100,
                //    minHeight: 100,
                //    tools: [
                //    {
                //        type: 'refresh',
                //        tooltip: 'Limpa todas as entradas',
                //        handler: function () {
                //            var pnl = me.query('#outputpanel')[0];
                //            pnl.removeAll(true);
                //        }
                //    }
                //    ],
                //    bbar: [
                //        {
                //            xtype: 'text',
                //            itemId: 'toolbarMsg'
                //        },
                //        '->',
                //        {
                //            xtype: 'text',
                //            itemId: 'usuarioText',
                //            text: '0 - Anônimo',
                //        },
                //        '-',
                //        {
                //            xtype: 'text',
                //            text: '0 - Empresa',
                //            itemId: 'empresaText'
                //        }
                //    ]
                //}
            ]
        });

        me.callParent(arguments);
    },

    getTabpanel: function () {
        return this.down('tabpanel');
    },

    getActiveTab: function () {
        var panel = this.getTabpanel();
        return panel.getActiveTab();
    },

    isActiveTab: function (widget) {
        var panel = this.getTabpanel();
        var active = panel.getActiveTab();
        if (!active)
            return false;
        return active._widgetName && active._widgetName == widget._widgetName;
    },

    getTabByWidgetName: function (widgetName) {
        var panel = this.getTabpanel();
        var item = panel.items.findBy(function (it) {
            return it.title === widgetName;
        });

        return item;
    },

    tabExists: function (widgetName) {
        var tabItem = this.getTabByWidgetName(widgetName);
        return !Ext.isEmpty(tabItem);
    },

    createTab: function (widget, cfg) {
        if (this.isActiveTab(widget))
            return {};

        var panel = this.getTabpanel();

        Ext.apply(widget, {
            closable: true
        });

        if (cfg)
            Ext.apply(widget, cfg);

        var item = panel.add(widget);

        return item;
    },

    openScreen: function (treeView, treeItem) {
        //debugger;
        console.log(treeItem);

        Ext.WindowManager.hideAll();

        if (!treeItem.get("leaf")) {
            return false;
        }

        var self = this.up('viewport'),
            screen = treeItem.get("text"),
            xtype = treeItem.raw.widget,
            mode = treeItem.raw.mode || "tab",
            singleton = !treeItem.raw.singleton || treeItem.raw.singleton === true,
            title = treeItem.raw.title || treeItem.get("text"),
            cfg = treeItem.raw.cfg || {},
            widget, win;

        if (mode === "tab") {
            widget = singleton && self.getTabByWidgetName.call(self, screen) || self.createTab.call(self, Ext.widget(xtype, {
                title: title
            }));

            var panel = self.getTabpanel();
            panel.setActiveTab(widget);
        } else {

            if (singleton) {
                Ext.WindowManager.each(function (cmp) {
                    if (cmp.title && cmp.title === title) {
                        win = cmp;
                        return false;
                    };
                    return true;
                });

                if (win && win.isHidden()) {
                    win.show();
                    win.restore();
                    return true;
                }
            }

            widget = Ext.widget(xtype, {
                //config
                isWindow: true,
            });


            var wincfg = Ext.applyIf(cfg, {
                title: title,
                width: treeItem.raw.width || 640,
                maximizable: true,
                modal: false,
                closable: true,
                closeAction: singleton ? "hide" : "destroy",
                layout: 'fit',
                autoScroll: true,
                constrain: true,
                //constrainTo: self.down('tab').el,
                bodyPadding: 5,
                items: widget,
                listeners: {
                    afterrender: function () {
                        var tb = win.down('pagingtoolbar');
                        if (!tb)
                            return;
                        tb.add([
                            '->',
                            {
                                text: 'Fechar',
                                icon: '/Content/png/cancelar.png',
                                handler: function (btn) {
                                    var w = btn.up('window');
                                    if (w)
                                        w.close();
                                }
                            }
                        ]);
                    },
                    show: function () {
                        var grid = win.down('fullgrid');
                        if (grid && grid.loadRecords)
                            grid.loadRecords();
                    }
                }
            });

            win = Ext.create('Ext.window.Window', wincfg);

            win.show();
        }
    },

    getMenuItens: function () {
        var me = this;
        return [
            {
                title: "Consultas",
                listeners: {
                    itemclick: me.openScreen
                },
                root: {
                    children: [
                        { text: 'Window', leaf: true, widget: 'fullgrid', singleton: true, mode: 'win', cfg: { width: 400, height: 300 } },
                        { text: 'Tab', leaf: true, widget: 'fullgrid', singleton: true, mode: 'tab' }
                    ]
                }
            }
        ];
    },

    getTabs: function () {
        var me = this;
        return [
            {
                title: 'Home',
                closable: false,
                frame: true,
                html: '<h2>Logo</h2><h2>Licenciado para XXX</h2>',

                bodyPadding: 5
            }
        ];
    }
});
