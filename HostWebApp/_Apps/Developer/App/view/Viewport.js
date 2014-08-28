Ext.define('Dev.view.Viewport', {
    extend: 'Ext.container.Viewport',
    renderTo: Ext.getBody(),

    layout: {
        align: 'stretch',
        pack: 'center',
        type: 'border'
    },

    initComponent: function () {
        var me = this;
        console.log('viewport', me);

        Ext.applyIf(me, {
            items: [
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
                        title: 'Developer - KorePages (alpha)',
                        padding: "7 0 7 0",
                    },

                    cls: 'northcls',
                    items: [
                        {
                            xtype: 'toolbar',
                            dock: 'top',
                            padding: "5 0 5 0",
                            layout: {
                                align: 'stretchmax',
                                type: 'hbox'
                            },
                            items: [
                                {
                                    xtype: 'splitbutton',
                                    text: 'Arquivo',
                                    width: 100,
                                    icon: '/Content/png/26.png',
                                    handler: function (btn) {
                                        btn.showMenu();
                                    },
                                    menu: {
                                        xtype: 'menu',
                                        items: [
                                            {
                                                //xtype: 'button',
                                                text: 'Download Zip',
                                                icon: '/Content/png/54.png',
                                                href: 'Services/zip', //GET
                                                hrefTarget: '_blank'
                                            },
                                            {
                                                //xtype: 'button',
                                                text: 'Upload Zip',
                                                icon: '/Content/png/55.png',
                                                handler: function () {
                                                    Ext.create('Ext.window.Window', {
                                                        title: 'Upload Zip',
                                                        autoShow: true,
                                                        width: 640,
                                                        bodyPadding: 10,
                                                        frame: true,
                                                        items: [{
                                                            xtype: 'form',
                                                            items: [
                                                                {
                                                                    xtype: 'filefield',
                                                                    name: 'zip',
                                                                    fieldLabel: 'Zip',
                                                                    labelWidth: 50,
                                                                    msgTarget: 'side',
                                                                    allowBlank: false,
                                                                    anchor: '100%',
                                                                    buttonText: 'Selecione o arquivo zip'
                                                                }
                                                            ],

                                                            buttons: [{
                                                                text: 'Upload',
                                                                handler: function () {
                                                                    var self = this;
                                                                    var form = this.up('form').getForm();
                                                                    if (form.isValid()) {
                                                                        form.submit({
                                                                            url: 'Services/zip', //POST
                                                                            waitMsg: 'Enviando arquivo...',
                                                                            success: function (fp, o) {
                                                                                Ext.Msg.alert('Success', 'Arquivo "' + o.result.file + '" foi processado.');
                                                                                Ext.getStore('FilesStore').reload();
                                                                                var win = self.up('window');
                                                                                win.close();
                                                                            }
                                                                        });
                                                                    }
                                                                }
                                                            }]
                                                        }]
                                                    });
                                                }
                                            },
                                            '-',
                                            {
                                                //xtype: 'button',
                                                text: 'Authorize DropBox',
                                                icon: '/Content/png/54.png',
                                                handler: function () {
                                                    me.el.mask("Conectando com o DropBox");
                                                    var client = window._DropBoxClient;
                                                    client.authenticate(function (error, cl) {
                                                        if (error) {
                                                            me.el.unmask();
                                                            // Replace with a call to your own error-handling code.
                                                            //
                                                            // Don't forget to return from the callback, so you don't execute the code
                                                            // that assumes everything went well.
                                                            return Dev.app.showDropBoxError(error);  // Something went wrong.
                                                        } else {

                                                            // Replace with a call to your own application code.
                                                            //
                                                            // The user authorized your app, and everything went well.
                                                            // client is a Dropbox.Client instance that you can use to make API calls.

                                                            client.getAccountInfo(function (err, accountInfo) {
                                                                me.el.unmask();
                                                                if (err) {
                                                                    return Dev.app.showDropBoxError(err);  // Something went wrong.
                                                                } else {
                                                                    Ext.Msg.alert("DropBox conectado com sucesso.", "Hello, " + accountInfo.name + "!");
                                                                }
                                                            });
                                                        }
                                                    });
                                                }
                                            },
                                            {
                                                text: 'Sync All DropBox',
                                                icon: '/Content/png/38.png',
                                                handler: function () { }
                                            }
                                        ]
                                    }
                                },
                                '-',
                                {
                                    xtype: 'splitbutton',
                                    text: 'Utils',
                                    icon: '/Content/png/50.png',
                                    width: 100,
                                    handler: function (btn) {
                                        btn.showMenu();
                                    },
                                    menu: {
                                        xtype: 'menu',
                                        items: [
                                            {
                                                icon: '/Content/png/62.png',
                                                handler: function () {
                                                    me.fireEvent('actionLogs');
                                                },
                                                text: 'Server Logs'
                                            },
                                            {
                                                //xtype: 'button',
                                                text: 'Restart',
                                                icon: '/Content/png/42.png',
                                                handler: function () {
                                                    Ext.Ajax.request({
                                                        url: 'Services/apprestart',
                                                        success: function (response, opts) {
                                                            Ext.Msg.alert("Restart", "Pressione OK para recarregar a página.");
                                                            location.reload();
                                                        },
                                                        failure: function (response, opts) {
                                                            Ext.Msg.alert("Erro", response.responseText);
                                                            location.reload();
                                                        }
                                                    });
                                                },
                                            }
                                        ]
                                    }
                                },
                                {
                                    xtype: 'splitbutton',
                                    text: 'Build',
                                    icon: '/Content/png/68.png',
                                    width: 100,
                                    handler: function(btn) {
                                        btn.showMenu();
                                    },
                                    menu: {
                                        xtype: 'menu',
                                        items: [
                                            {
                                                text: 'Shell/Exec',
                                                icon: '/Content/png/31.png',
                                                handler: function () {
                                                    me.fireEvent('actionShell');
                                                }
                                            },
                                            {
                                                //xtype: 'button',
                                                text: 'Build',
                                                icon: '/Content/png/61.png',
                                                handler: function () {
                                                    Ext.Ajax.request({
                                                        url: 'Services/build',
                                                        success: function (response, opts) {
                                                            var obj = Ext.decode(response.responseText);
                                                            Dev.app.writeOutput("Resultado da Compilação", obj.msg);
                                                        },
                                                        failure: function (response, opts) {
                                                            Ext.Msg.alert("Erro", response.responseText);
                                                        }
                                                    });
                                                },
                                            }
                                        ]
                                    }
                                },
                                '->',
                                {
                                    xtype: 'button',
                                    href: '/',
                                    hrefTarget: '',
                                    text: '~/',
                                    icon: '/Content/png/17.png',
                                    width: 100,
                                },

                                {
                                    xtype: 'button',
                                    href: '/_session/logout.cshtml',
                                    icon: '/Content/png/27.png',
                                    hrefTarget: '',
                                    text: 'Sair',
                                    width: 100,
                                }
                            ],
                        }
                    ]
                },
            {
                xtype: 'panel',
                itemId: 'devtools-panel',
                region: Dev.app.getStateProvider().get("devtools-region", "east"),
                width: 300,
                title: 'Developer Tools',
                collapsible: true,
                animCollapse: false,
                split: true,
                multi: false,
                hideCollapseTool: false,
                layout: {
                    type: 'accordion',
                },
                defaults: {
                    animate: false,
                    collapsible: true,
                    collapsed: true,
                    activeOnTop: false,
                    rootVisible: false,
                    expanded: false

                },
                tools: [
                    {
                        type: 'toggle',
                        handler: function () {
                            var pnl = me.query('#devtools-panel')[0];

                            if (pnl.region === 'east') {
                                pnl.setBorderRegion('west');
                                Dev.app.getStateProvider().set("devtools-region", "west");
                            } else {
                                pnl.setBorderRegion('east');
                                Dev.app.getStateProvider().set("devtools-region", "east");
                            }
                        }
                    }
                ],
                items: [
                    {
                        xtype: 'treepanel',
                        title: 'DB Explorer',
                        store: 'FilesStore',
                        autoScroll: true,
                        useArrows: true,
                        folderSort: true,
                        columns: [
                            { xtype: 'treecolumn', header: 'Arquivo/Pasta', dataIndex: 'name', width: 200, flex: 0 },
                            { xtype: 'datecolumn', header: 'Data Modificação', dataIndex: 'fileTime', format: 'd/m/Y H:i:s', flex: 1 },
                            {
                                xtype: 'checkcolumn', header: 'Hidden', dataIndex: 'IsHidden', flex: 1, processEvent: function () {
                                    return false;
                                }
                            }
                        ],
                        tools: [
                            {
                                itemId: 'refreshTool',
                                type: 'refresh',
                                tooltip: 'Atualizar arquivos',
                                handler: function (event, toolEl, panelHeader) {
                                    var el = me.down('treepanel');
                                    el.mask("Aguarde...");
                                    Ext.getStore('FilesStore').load({
                                        callback: function () {
                                            el.unmask();
                                        }
                                    });
                                }
                            },
                               {
                                   type: 'expand',
                                   collapsible: true,
                                   tooltip: 'Habilita/Desabilita Checkboxes',
                                   handler: function (event, toolEl, panelHeader) {
                                       var tree = me.down('treepanel');
                                       var node = tree.getRootNode();
                                       var hasCheck = node.get("checked");
                                       console.log(hasCheck);
                                       node.cascadeBy(function (n) {
                                           if (!Ext.isEmpty(hasCheck)) {
                                               n.set("checked", null);
                                           } else {
                                               n.set("checked", false);
                                           }
                                       });
                                   }
                               }
                        ],
                    },
                    {
                        xtype: 'panel',
                        title: 'Roles',
                        html: '<p>Roles</p>',
                        collapsible: true,
                        collapsed: true,
                    },
                    {
                        xtype: 'panel',
                        title: 'Users',
                        html: '<p>Users</p>',
                        collapsible: true,
                        collapsed: true,
                    }
                ]
            },
                {
                    xtype: 'tabpanel',
                    region: 'center',
                    headerPosition: 'top',
                    //collapsible: true,
                    defaults: { // defaults are applied to items, not the container
                        closable: true
                    }
                },
                {
                    region: 'south',
                    title: 'Output',
                    itemId: 'outputpanel',
                    collapsible: true,
                    collapsed: true,
                    layout: 'vbox',
                    autoScroll: true,
                    split: true,
                    animCollapse: false,
                    height: 100,
                    minHeight: 100,
                    tools: [
                    {
                        type: 'refresh',
                        tooltip: 'Limpa todas as entradas',
                        handler: function () {
                            var pnl = me.query('#outputpanel')[0];
                            pnl.removeAll(true);
                        }
                    }
                    ],
                    bbar: [
                        {
                            xtype: 'text',
                            itemId: 'toolbarMsg'
                        },
                        '->',
                        {
                            xtype: 'text',
                            itemId: 'usuarioText',
                            text: '0 - Anônimo',
                        },
                        '-',
                        {
                            xtype: 'text',
                            text: '0 - Empresa',
                            itemId: 'empresaText'
                        }
                    ]
                }
            ]
        });

        me.callParent(arguments);
    }
});