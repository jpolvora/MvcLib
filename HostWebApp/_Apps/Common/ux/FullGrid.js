Ext.define('Dev.ux.FullGrid', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.fullgrid',
    layout: {
        align: 'stretch',
        pack: 'center',
        type: 'border'
    },

    requires: [
        //'Dev.ux.FieldMoney',
        //'Dev.ux.Formatter'
         'Ext.ux.grid.FiltersFeature'
    ],

    _storeLoaded: false,

    config: {
        isWindow: false,
        isLookup: false, //indica se este grid faz parte de um lookup
        isExtraTab: false, //indica se este grid está compondo um outro grid (filtrado)
        extraTabParam: '',
        mainTab: null, //instância relacionada


        store: Ext.create('Ext.data.Store', {
            //storeId: 'simpsonsStore',
            pageSize: 10,
            autoLoad: false, //será carregado (loaded) qdo a tab for ativada
            fields: ['name', 'email', 'phone'],
            data: {
                'items': [
                    { 'name': 'Lisa', "email": "lisa@simpsons.com", "phone": "555-111-1224" },
                    { 'name': 'Bart', "email": "bart@simpsons.com", "phone": "555-222-1234" },
                    { 'name': 'Homer', "email": "home@simpsons.com", "phone": "555-222-1244" },
                    { 'name': 'Marge', "email": "marge@simpsons.com", "phone": "555-222-1254" }
                ]
            },
            proxy: {
                type: 'memory',
                reader: {
                    type: 'json',
                    root: 'items'
                }
            }
        }),

        columns: [
            { text: 'Name', dataIndex: 'name' },
            { text: 'Email', dataIndex: 'email', flex: 1 },
            { text: 'Phone', dataIndex: 'phone' }
        ],

        customFilters: [
            {
                type: 'numeric',
                dataIndex: 'Id'
            }
        ],

        printHandler: function () {
            console.log('print...');

            Ext.ux.grid.Printer.printAutomatically = false;
            var grid = this.up('gridpanel#mainGridPanel');

            Ext.ux.grid.Printer.print(grid);
        },

        autoSize: true,

        extraCmps: [ //extra componentes (region)
        ],

        extraTabs: [], //extra tabs

        formWindow: {
            xtype: 'formwindow',
            title: 'Hello World',
            formConfig: [
                {
                    xtype: 'form',
                    itemId: 'mainForm',
                    items: [
                        {
                            xtype: 'text',
                            text: 'Empty form.'
                        }
                    ]
                }
            ]
        }
    },

    constructor: function (config) {
        this.initConfig(config);
        this.callParent([config]);
    },

    initComponent: function () {
        var me = this;
        var filtersCfg = {
            ftype: 'filters',
            menuFilterText: 'Filtros',
            //autoReload: false, //don't reload automatically
            encode: true,
            local: false, //only filter locally
            // filters may be configured through the plugin,
            // or in the column definition within the headers configuration
            filters: me.getCustomFilters()
        };

        //configura a store
        var store = me.getStore();
        store.on('filterchange', function () {
            console.log('filterchange', store);
            me.toggleFilters(me, store);
        });

        var gridPanel = [
              {
                  xtype: 'gridpanel',
                  itemId: 'mainGridPanel',
                  title: 'Lista',
                  store: store,
                  columns: me.getColumns(),
                  features: [filtersCfg],
                  forceFit: true,
                  region: 'center',

                  selModel: {
                      mode: 'SINGLE',
                      allowDeselect: false,
                      listeners: {
                          selectionchange: function (sm, selected, eOpts) {
                              if (selected.length > 0) {
                                  me.down("#btnEdit").enable();
                                  me.down("#btnDelete").enable();
                              } else {
                                  me.down("#btnEdit").disable();
                                  me.down("#btnDelete").disable();
                              }
                              var record = selected[0];
                              me.recordChanged.call(me, record);
                          }
                      }
                  },

                  listeners: {
                      itemclick: function (pnl, record, item, index, e, eOpts) {
                          var isLookup = me.getIsLookup();
                          if (isLookup)
                              return;
                          var win = me.up('window');
                          if (!win)
                              Ext.WindowManager.hideAll();
                      },

                      itemdblclick: function (pnl, record, item, index, e, eOpts) {
                          var isLookup = me.getIsLookup();
                          if (isLookup)
                              return;

                          me.editRecord(me, record);
                      },

                      filterupdate: function () {
                          console.log('filterupdate');
                          me.toggleFilters(me, store);
                      }
                  },

                  tbar: [
                      {
                          text: 'Inserir',
                          tooltip: 'Inserir Novo registro',
                          icon: '/Content/png/novo.png',
                          handler: function (btn, evt) {
                              me.insertRecord(me, me.getValuesForNewRecord());
                          }
                      },
                      '-',
                      {
                          text: 'Editar',
                          itemId: 'btnEdit',
                          tooltip: 'Editar registro selecionado',
                          icon: '/Content/png/editar.png',
                          disabled: true,
                          handler: function (btn, evt) {
                              me.editRecord();
                          }
                      },
                      '-',
                      {
                          text: 'Excluir',
                          itemId: 'btnDelete',
                          tooltip: 'Excluir registro selecionado',
                          icon: '/Content/png/excluir.png',
                          disabled: true,
                          handler: function () {
                              me.deleteRecord();
                          }
                      },
                      '->',
                       {
                           text: 'Imprimir',
                           itemId: 'btnPrint',
                           tooltip: 'Imprime',
                           icon: '/Content/png/16.png',
                           handler: me.getPrintHandler()
                       },
                      {
                          text: 'Limpar Filtros',
                          itemId: 'btnClearFilters',
                          tooltip: 'Limpa todos os filtros',
                          icon: '/Content/png/61.png',
                          enableToggle: true,
                          handler: function () {
                              store.clearFilter();

                              var grid = me.down('gridpanel#mainGridPanel');
                              var filters = grid.filters.filters;
                              filters.each(function (item) {
                                  item.setActive(false);
                              });

                              store.load();
                          }
                      }
                  ],

                  bbar: {
                      xtype: 'pagingtoolbar',
                      store: store,
                      displayInfo: true,
                      plugins: [Ext.create('Ext.ux.PagingToolbarResizer')]
                  },
              }
        ];

        if (me.getIsLookup() || me.getIsExtraTab() || me.getExtraTabs().length == 0) {
            delete gridPanel[0].title;
            me.items = gridPanel;
        } else {
            me.items = [
              {
                  xtype: 'tabpanel',
                  region: 'center',
                  tabPosition: 'bottom',

                  items: Ext.Array.merge(gridPanel, me.getExtraTabs()),

                  listeners: {
                      beforetabchange: function (tp) {
                          var grid = me.down('gridpanel#mainGridPanel');
                          var active = tp.getActiveTab();
                          if (active === grid) {
                              //deixa mudar somente se houver um registro selecionado.
                              var record = grid.getSelectionModel().getSelection()[0];
                              return !!record;
                          } else {
                              return true;
                          }
                      }
                  }
              }
            ];
        }

        me.on('activate', function () {
            //debugger; //para funcionar no primeiro carregamento, deve haver uma tab já criada. { ViewPort::Home }
            me.loadRecords.call(me);
        });


        var cmps = me.getExtraCmps();
        for (var i = 0; i < cmps.length; i++) {
            me.items.push(cmps[i]);
        }
        me.callParent();
    },

    toggleFilters: function (me, store) {
        var count = store.filters.getCount();
        console.log('store possui filtros aplicados', count);

        var grid = me.down('gridpanel#mainGridPanel');
        var count2 = 0;
        grid.filters.filters.each(function (f) {
            if (f.active) {
                count2++;
            }
        });
        console.log('grid possui filtros aplicados', count2);

        var btn = grid.down('#btnClearFilters');

        if (me.getIsExtraTab()) {
            btn.setVisible(false);
            return;
        }

        if (btn) {
            if (count > 0 || count2 > 0) {
                btn.setTooltip(Ext.String.format("Existe(m) {0} filtro(s) aplicado(s)", count + count2));
                btn.toggle(false, true); //está filtrado, habilita o botão de filtros.
                //btn.enable();
            } else {
                btn.setTooltip("Nenhum filtro aplicado");
                btn.toggle(true, true); //não possui filtros, desabilita o botão de filtros.

                //btn.disable();
            }
        }
    },

    getValuesForNewRecord: function () {
        return {};
    },

    editRecord: function (me, record) {
        me = me || this;
        var grid = me.down('gridpanel#mainGridPanel');
        record = record || grid.getSelectionModel().getSelection()[0];
        if (!record)
            return;

        me.showFormWindow(me, record);
    },

    insertRecord: function (me, data) {
        me = me || this;
        data = data || {};
        var store = me.getStore();
        var record = store.add(data)[0]; //cria um record vazio
        me.showFormWindow(me, record);
    },

    showFormWindow: function (me, record) {
        var cfg = me.getFormWindow();
        Ext.apply(cfg, {
            record: record,
            store: me.getStore(),
            //renderTo: me.up('tabpanel#screensPanel').el
        });
        var win = Ext.widget(cfg);

        win.on('close', function (pnl) {
            if (pnl._reloadAfterClose)
                me.getStore().reload();
        });
        win.show();
    },

    deleteRecord: function (me, record) {
        me = me || this;
        var grid = me.down('gridpanel#mainGridPanel');
        record = record || grid.getSelectionModel().getSelection()[0];

        if (!record) {
            Ext.Msg.show({
                title: 'Alerta',
                msg: 'Nenhum registro selecionado',
                icon: Ext.Msg.OK,
                width: 300,
                buttons: Ext.Msg.OK
            });
            return;
        }

        Ext.Msg.show({
            title: 'Confirmação',
            msg: 'Tem certeza que deseja remover o registro selecionado?',
            icon: Ext.Msg.QUESTION,
            width: 300,
            buttons: Ext.Msg.OKCANCEL,
            fn: function (option) {
                if (option !== "ok")
                    return;

                var store = me.getStore();

                store.remove(record);
                grid.el.mask("Excluindo...");
                store.sync({
                    callback: function () {
                        if (grid && grid.el)
                            grid.el.unmask();

                        store.reload();
                    },
                    success: function () {
                        Ext.Msg.show({
                            title: 'Info',
                            msg: 'Registro excluído com sucesso.',
                            icon: Ext.Msg.INFO,
                            width: 200,
                            buttons: Ext.Msg.OK
                        });

                    }
                });
            }
        });
    },

    loadRecords: function () {
        var me = this;
        var store = me.getStore();
        if (store.autoLoad)
            return;

        store.load(function () {
            console.log('store loaded', store);
            me.toggleFilters(me, store);
        });
    },

    recordChanged: function (record) {
        var me = this;
        me.fireEvent('recordChanged', me, record);
    }
});
