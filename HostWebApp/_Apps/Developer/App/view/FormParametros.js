Ext.define('Dev.view.FormParametros', {
    extend: 'Ext.window.Window',
    alias: 'widget.formparametros',
    title: 'Parâmetros',
    width: 800,
    layout: 'fit',
    bordered: false,
    autoShow: true,
    modal: true,
    constrain: true,

    initComponent: function () {
        var me = this;

        Ext.apply(me, {
            items: [
            {
                xtype: 'form',
                plain: true,
                bodyPadding: 5,
                bordered: false,
                defaultType: 'textfield',
                fieldDefaults: {
                    anchor: '100%',
                    msgTarget: 'side',
                    allowOnlyWhitespace: false,
                    labelAlign: 'right',
                    labelWidth: 120,
                    labelStyle: 'font-weight:bold'
                },
                items: [
                     {
                         xtype: 'tabpanel',
                         headerPosition: 'top',
                         defaults: {
                             // defaults are applied to items, not the container
                             autoScroll: true,
                             closable: false,
                             bodyPadding: 5
                         },

                         listeners: {
                             beforetabchange: function () {
                                 var store = Ext.getStore("PagesStore");
                                 var page = store.getById(me._id);
                                 if (!page || page.phantom) {
                                     Ext.Msg.alert("Info", "Necessário gravar antes de configurar parâmetros customizados.");
                                     return false;
                                 }

                                 return true;
                             }
                         },
                         items: [
                             {
                                 title: 'Page Parameters',
                                 layout: {
                                     type: 'vbox',
                                     align: 'stretch'  // Child items are stretched to full width
                                 },
                                 items: [
                                     {
                                         xtype: 'textfield',
                                         name: 'Id',
                                         fieldLabel: 'Id',
                                         readOnly: true
                                     },
                                     {
                                         xtype: 'textfield',
                                         name: 'VirtualPath',
                                         fieldLabel: 'VirtualPath',
                                         readOnly: true,

                                     },
                                     {
                                         xtype: 'textfield',
                                         name: 'Description',
                                         fieldLabel: 'Description',
                                         allowBlank: false,
                                         enforceMaxLength: true,
                                         maxLength: 100,
                                         anchor: '100%'
                                     },
                                     {
                                         xtype: 'textfield',
                                         name: 'LinkText',
                                         fieldLabel: 'LinkText',
                                         enforceMaxLength: true,
                                         maxLength: 60
                                     },
                                     {
                                         xtype: 'textfield',
                                         name: 'Permalink',
                                         fieldLabel: 'Permalink',
                                         enforceMaxLength: true,
                                         maxLength: 100
                                     },
                                     {
                                         xtype: 'checkboxfield',
                                         name: 'IsMenuItem',
                                         fieldLabel: 'IsMenuItem',
                                         inputValue: 'true',
                                         uncheckedValue: 'false'
                                     },
                                     {
                                         xtype: 'textfield',
                                         name: 'MenuItemGroup',
                                         fieldLabel: 'MenuItemGroup',
                                         enforceMaxLength: true,
                                         maxLength: 50
                                     },
                                     {
                                         xtype: 'textfield',
                                         name: 'Layout',
                                         fieldLabel: 'Layout',
                                         enforceMaxLength: true,
                                         maxLength: 50
                                     },
                                     {
                                         xtype: 'textfield',
                                         name: 'Title',
                                         fieldLabel: 'Title',
                                         enforceMaxLength: true,
                                         maxLength: 60
                                     },
                                     {
                                         xtype: 'textfield',
                                         name: 'MetaKeywords',
                                         fieldLabel: 'MetaKeywords',
                                         enforceMaxLength: true,
                                         maxLength: 100
                                     },
                                     {
                                         xtype: 'textareafield',
                                         name: 'MetaDescription',
                                         fieldLabel: 'MetaDescription',
                                         enforceMaxLength: true,
                                         maxLength: 256
                                     }
                                 ],

                                 dockedItems: [{
                                     xtype: 'toolbar',
                                     dock: 'bottom',
                                     items: [
                                    {
                                        action: 'save',
                                        tooltip: 'Grava alterações',
                                        formBind: true, //only enabled once the form is valid
                                        disabled: true,
                                        icon: '/Content/png/gravar.png',
                                        text: 'Gravar',
                                        handler: function () {
                                            var form = me.down('form'),
                                                record = form.getRecord(),
                                                values = form.getValues(),
                                                store = Ext.getStore("PagesStore");

                                            if (record.phantom) {
                                                store.add(record);
                                            }

                                            record.set(values);

                                            me.el.mask("Aguarde...");
                                            store.sync({
                                                success: function () {
                                                    Ext.Msg.alert("Info", "Dados gravados com sucesso!");
                                                },
                                                callback: function () {
                                                    me.el.unmask();
                                                }
                                            });
                                        }
                                    },
                                     {
                                         text: 'Cancelar',
                                         tooltip: 'Cancela edição e fecha a janela',
                                         icon: '/Content/png/cancelar.png',
                                         handler: function () {
                                             var form = me.down('form'),
                                                 record = form.getRecord(),
                                                 store = record.store;

                                             if (store) {
                                                 record.store.rejectChanges();
                                             }
                                             me.close();
                                         }
                                     },
                                     {
                                         text: 'Excluir',
                                         tooltip: 'Excluir o registro e fecha a janela',
                                         icon: '/Content/png/excluir.png',
                                         handler: function () {
                                             var form = me.down('form'),
                                                 record = form.getRecord(),
                                                 store = record.store,
                                                 win = me;

                                             Ext.Msg.confirm('Atenção?', 'Confirma exclusão ?', function (btn) {
                                                 if (btn == 'yes') {
                                                     form.getForm().reset();

                                                     if (record.phantom) {
                                                         record.reject();
                                                         win.close();

                                                     } else {
                                                         if (store) {
                                                             record.store.rejectChanges();
                                                         }
                                                         store.remove([record]);

                                                         win.el.mask("Excluindo...");
                                                         store.sync({
                                                             callback: function () {
                                                                 if (win)
                                                                     win.el.unmask();
                                                             },
                                                             success: function () {
                                                                 Ext.Msg.alert("Info", "Registro excluído com sucesso!");
                                                                 win.close();
                                                             }
                                                         });
                                                     }
                                                 }
                                             });
                                         }

                                     }]
                                 }],

                             },

                              {
                                  title: 'Custom Parameters',
                                  listeners: {
                                      activate: function (tab) {
                                          console.log('grid activated', tab);
                                          var grid = tab.down('gridpanel');
                                          var store = grid.store;
                                          store.clearFilter();
                                          store.filter("DbPageId", me._id);
                                          store.load();
                                      },
                                      deactivate: function (tab) {
                                          console.log('grid deactivated', tab);
                                      }
                                  },
                                  items: [
                                        {
                                            xtype: 'gridparametros',
                                            _pageId: me._id
                                        }
                                  ]
                              }
                         ],
                     }
                ]
            }
            ],


            listeners: {

                show: function () {
                    me.setTitle("Parâmetros para [" + me._virtualPath + "]");
                },

                afterrender: function () {
                    me.el.mask("Aguarde...");

                    var store = Ext.getStore('PagesStore');
                    store.clearFilter();
                    store.filter("Id", me._id);
                    //load store
                    store.load(function (records, operation, success) {
                        me.el.unmask();

                        if (success) {
                            var record = store.getById(me._id) || Ext.create(store.model);
                            me._editing = (record.phantom == false);
                            me.setTitle(me.title + (me._editing ? " (Editando)" : " (Inserindo)"));
                            if (record.phantom) {
                                record.set("Id", me._id);
                                record.set("VirtualPath", me._virtualPath);
                            }
                            var form = me.down('form');
                            form.loadRecord(record);

                        }
                    });
                },

                close: function () {
                    var form = me.down('form'),
                           record = form.getRecord(),
                           store = record.store;

                    form.getForm().reset();

                    record.reject();

                    if (store) {
                        record.store.rejectChanges();
                    }
                }
            }
        });

        me.callParent(arguments);
    }
});