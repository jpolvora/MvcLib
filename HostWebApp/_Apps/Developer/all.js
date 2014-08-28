Ext.require(['*']);

Ext.onReady(function () {

    var pageLoader = Ext.get('page-loader');
    if (pageLoader) {
        pageLoader.fadeOut({ remove: true, duration: 3000 });
    }

    Ext.QuickTips.init();

    Ext.define('FileModel', {
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

    Ext.create('Ext.data.TreeStore', {
        model: 'FileModel',
        storeId: 'FilesStore',
        autoLoad: true
    });

    var viewPort = Ext.create('Ext.container.Viewport', {
        layout: 'border',
        items: [
            {
                region: 'north',
                //html: '<h1 class="x-panel-header">Page Title</h1>',
                title: 'Developer',
                border: false,
                margins: '0 0 5 0',
                dockedItems: [
                        {
                            xtype: 'toolbar',
                            dock: 'top',
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
                                                    client.authenticate(function (error, client) {
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

                                                            client.getAccountInfo(function (error, accountInfo) {
                                                                me.el.unmask();
                                                                if (error) {
                                                                    return Dev.app.showDropBoxError(error);  // Something went wrong.
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
                                    menu: {
                                        xtype: 'menu',
                                        items: [
                                            {
                                                icon: '/Content/png/62.png',
                                                handler: function () {
                                                    me.fireEvent('actionLogs');
                                                },
                                                text: 'Logs'
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
                                                            Ext.Msg.alert(obj);
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
                region: 'west',
                title: 'Menu',
                split: true,
                width: 200,
                minWidth: 175,
                maxWidth: 400,
                collapsible: true,
                collapsed: true,
                animCollapse: false,
                defaults: {
                    animate: false,
                },
                margins: '0 0 0 5',
                layout: 'accordion',
                items: [
                    {
                        title: 'Navigation',
                        iconCls: 'nav' // see the HEAD section for style used
                    },
                    {
                        title: 'Settings',
                        html: '<p>Some settings in here.</p>',
                        iconCls: 'settings'
                    },
                    {
                        title: 'Information',
                        html: '<p>Some info in here.</p>',
                        iconCls: 'info'
                    }
                ]
            },
            {
                region: 'south',
                title: 'Output',
                collapsible: true,
                collapsed: true,
                html: 'Information goes here',
                split: true,
                height: 100,
                minHeight: 100
            },
          {
              xtype: 'tabpanel',
              region: 'east',
              title: 'East Side',
              dockedItems: [
                  {
                      dock: 'top',
                      xtype: 'toolbar',
                      items: [
                          '->',
                          {
                              xtype: 'button',
                              text: 'test',
                              tooltip: 'Test Button'
                          }
                      ]
                  }
              ],
              animCollapse: false,
              collapsible: true,
              split: true,
              width: 225, // give east and west regions a width
              minSize: 175,
              maxSize: 400,
              margins: '0 5 0 0',
              activeTab: 0,
              tabPosition: 'bottom',
              items: [
                 {
                     xtype: 'treepanel',
                     itemId: 'filestreepanel',
                     title: 'DB Explorer',
                     store: 'FilesStore',
                     autoScroll: true,
                     useArrows: true,
                     folderSort: true,
                     rootVisible: true,
                     columns: [
                         { xtype: 'treecolumn', header: 'Arquivo/Pasta', dataIndex: 'name', width: 200, flex: 0 },
                         { xtype: 'datecolumn', header: 'Data Modificação', dataIndex: 'fileTime', format: 'd/m/Y H:i:s', flex: 1, hidden: true },
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
                                    var tree = Ext.getCmp('filestreepanel');
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

              Ext.create('Ext.grid.PropertyGrid', {
                  title: 'Property Grid',
                  closable: true,
                  source: {
                      "(name)": "Properties Grid",
                      "grouping": false,
                      "autoFitColumns": true,
                      "productionQuality": false,
                      "created": Ext.Date.parse('10/15/2006', 'm/d/Y'),
                      "tested": false,
                      "version": 0.01,
                      "borderWidth": 1
                  }
              })]
          },
            {
                region: 'center',
                xtype: 'tabpanel', // TabPanel itself has no title
                activeTab: 0,      // First tab active by default
                items: {
                    title: 'Default Tab',
                    html: 'The first tab\'s content. Others may be added dynamically'
                }
            }
        ]
    });
});