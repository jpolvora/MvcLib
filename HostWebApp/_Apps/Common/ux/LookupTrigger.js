Ext.define('Dev.ux.LookupTrigger', {
    extend: 'Ext.form.field.Trigger',
    alias: 'widget.lookuptrigger',

    config: {
        fieldToSet: '', //campo a ser setado o valor (registro atual)
        fieldToGet: 'Id', //campo a ser recuperado o valor (registro selecionado no lookup)
        widget: 'formwindow',
        title: 'Pesquisa',
        lookupWidth: 700,
        lookupHeight: 500,
        callback: function (obj, selectedRecord) {
            //retorne falso aqui para ignorar o resultado
            //ou faça validações/acrescente valores { Ext.apply(obj, {...}) }
            return obj;
        },
        filterStore: true, //filtra a store pelo registro atual
        emptyValue: 0 //valor vazio
    },

    constructor: function (config) {
        this.initConfig(config);
        this.callParent([config]);
    },

    tooltip: 'Duplo clique para pesquisar',
    editable: false,
    labelWidth: 100, //reduzir 4 pixels em relação aos outros labels! (120-4 = 116). Default = 100
    flex: 1,
    emptyText: '(Selecione)',
    submitValue: false,
    activeErrorsTpl: new Ext.XTemplate('<p>Campo obrigatório</p>'),
    allowBlank: true,
    blankText: 'texto em branco...',
    invalidText: 'texto inválido...',
    //validateOnChange: false, //!IMPORTANT!!! no caso de reset do form, irá dar erro.


    initComponent: function () {
        var me = this;
        Ext.apply(me, {
            listeners: {
                render: function (cmp) {
                    var el = cmp.getEl();
                    el.down('input').set({
                        'data-qtip': cmp.tooltip + " " + me.fieldLabel
                    });

                    el.on('dblclick', function (event, el) {
                        me.openLookup(me);
                    });
                }
            }
        });
    },

    // override onTriggerClick
    onTriggerClick: function () {
        var me = this;
        //todo: pegar o valor do lookup já existente e selecionar/filtrar no grid
        //opção para limpar a seleção
        me.openLookup(me);
    },

    openLookup: function (me) {
        me = me || this;
        var win, selectedRecord;

        var wrapped = Ext.widget(me.getWidget(), {
            isLookup: true
        });
        var grid = wrapped.down('gridpanel#mainGridPanel');
        grid.on('selectionchange', function (sm, selected) {
            if (selected.length > 0) {
                selectedRecord = selected[0];
                win.down('#btnSelect').enable();
            } else {
                selectedRecord = null;
                win.down('#btnSelect').disable();
            }
        });

        grid.on('itemdblclick', function (pnl, record, item, index, e, eOpts) {
            var fullgrid = pnl.up('fullgrid');
            var isLookup = fullgrid.getIsLookup();
            if (!isLookup)
                return;

            me.selectRecord(me, selectedRecord, win);
        });

        win = Ext.create('Ext.window.Window', {
            title: me.getTitle(),
            modal: true,
            icon: '/Content/png/pesquisar.png',
            maximizable: true,
            height: me.getLookupHeight(),
            width: me.getLookupWidth(),
            layout: 'fit',
            items: wrapped,

            listeners: {
                show: function () {
                    var store = grid.getStore();
                    if (store && !store.autoLoad) {
                        var selectFirst = false;
                        if (me.getFilterStore()) {
                            var form = me.up('form').getForm();
                            var rec = form.getRecord();
                            if (!rec.phantom) {
                                var pk = me.getFieldToGet();
                                var fk = me.getFieldToSet();
                                var currValue = form.findField(fk).getValue();

                                var empty = me.getEmptyValue();
                                if (empty == currValue || Ext.isEmpty(currValue)) {
                                    store.clearFilter();
                                } else {
                                    store.filter(pk, currValue);
                                    selectFirst = true;
                                }
                            }
                        }
                        store.load(function () {
                            if (selectFirst) {
                                grid.getSelectionModel().select(0);
                            }
                        });
                    }
                    var tb = win.down('pagingtoolbar');
                    if (!tb)
                        return;
                    tb.add([
                        '->',
                         {
                             text: 'Selecionar',
                             icon: '/Content/png/38.png',
                             itemId: 'btnSelect',
                             disabled: true,
                             handler: function () {
                                 me.selectRecord(me, selectedRecord, win);
                             }
                         },
                        {
                            text: 'Cancelar',
                            icon: '/Content/png/cancelar.png',
                            handler: function () {
                                win.close();
                            }
                        }
                    ]);
                }
            }
        });
        win.show();
    },

    selectRecord: function (me, selectedRecord, win) {
        me = me || this;

        if (selectedRecord) {
            var form = me.up('form');
            var formbasic = form.getForm();
            var obj = {};
            obj[me.getFieldToSet()] = selectedRecord.get(me.getFieldToGet());
            var callback = me.getCallback();
            var result = callback(obj, selectedRecord);
            if (result) {
                formbasic.setValues(result);
            }
        }
        win.close();
    }
});
