Ext.define('Dev.view.GridParametros', {
    extend: 'Ext.grid.Panel',
    alias: 'widget.gridparametros',
    store: "PagesParametersStore",
    plugins: [
       Ext.create('Ext.grid.plugin.RowEditing', {
           clicksToEdit: 2
       })
    ],
    minHeight: 400,

    initComponent: function () {
        var self = this;

        Ext.applyIf(self, {
            selModel: {
                mode: 'SINGLE',
                allowDeselect: true
            },

            viewConfig: {
                stripeRows: true,
            }
        });

        this.tbar = [
            {
                text: 'Novo',
                tooltip: 'Inserir Novo registro',
                icon: '/Content/png/novo.png',
                handler: function () {
                    var record = Ext.create(self.store.model);
                    record.set("DbPageId", self._pageId);
                    self.store.add(record);
                }
            },
            '-',
            {
                text: 'Sincronizar',
                tooltip: 'Salvar todas as alterações/inserções',
                icon: '/Content/png/gravar.png',
                handler: function () {

                    var store = self.store;
                    var hasModified = store.getModifiedRecords();
                    var hasDeleted = store.getRemovedRecords();

                    if (hasModified.length > 0 || hasDeleted.length > 0) {
                        self.el.mask("Gravando...");
                        self.store.sync({
                            success: function () {
                                Ext.Msg.alert("Info", "Store sincronizada com sucesso!");
                            },
                            callback: function () {
                                self.el.unmask();
                            }
                        });
                    } else {
                        Ext.Msg.alert("Info", "Nada a sincronizar!");
                    }
                }
            },
            '->',
            {
                text: 'Excluir',
                tooltip: 'Excluir registro selecionado',
                icon: '/Content/png/excluir.png',
                handler: function () {
                    var record = self.getSelectionModel().getSelection()[0];
                    self.store.remove([record]);
                }
            }
        ];

        this.columns = {
            items: [
                //{ text: 'Id', dataIndex: 'Id', align: 'right' },
                //{ text: 'DbPageId', dataIndex: 'DbPageId', align: 'right' },
                {
                    text: 'Key', dataIndex: 'Key', editor: {
                        xtype: 'textfield',
                        allowBlank: false
                    }
                },
                {
                    text: 'Value', dataIndex: 'Value', editor: {
                        xtype: 'textareafield',
                        allowBlank: false
                    }
                }
            ],
            defaults: {
                autosize: true
            }
        };

        this.bbar = {
            xtype: 'pagingtoolbar',
            store: self.store,
            displayInfo: true
        };

        self.callParent(arguments);
    }
});