Ext.define('Dev.ux.FormWindow', {
    extend: 'Ext.window.Window',
    requires: [
       //'Dev.ux.FieldMoney',
       //'Dev.ux.Formatter',
       'Dev.ux.LookupTrigger',
       'Dev.ux.UpperTextField',
       'Dev.ux.NumericField'
    ],
    alias: 'widget.formwindow',
    title: 'Hello',
    //height: 300, //auto
    width: 400,
    modal: true,
    layout: 'fit',
    constrain: true,
    resizable: true,
    maximizable: true,
    minimizable: true,
    bodyPadding: 5,

    config: {
        record: {},
        store: {},
        formConfig: [
             {
                 xtype: 'form',
                 itemId: 'mainForm',
                 border: false,
                 selectOnFocus: true,
                 defaults: {
                     labelAlign: 'right',
                     labelStyle: 'font-weight:bold',
                     anchor: '100%',
                     fieldStyle: 'text-transform:uppercase',
                     msgTarget: 'side',
                     allowOnlyWhitespace: false,
                     selectOnFocus: true
                 },
             }
        ],

        afterSave: function (record) {
            console.log('after save', record);
            return true; //retornando falso irá sair do método que chamou o callback
        }
    },

    _reloadAfterClose: false,

    constructor: function (config) {
        this.initConfig(config);
        this.callParent([config]);
    },

    initComponent: function () {
        var me = this;
        me.items = me.getFormConfig();

        me.on('beforeclose', function () {
            return me.onbeforeclose(me);
        });

        me.on('show', function () {
            me.onshow(me);
        });

        Ext.apply(me, {
            bbar: [
                '->',
                {
                    tooltip: 'Grava alterações',
                    scale: 'medium',
                    formBind: true, //only enabled once the form is valid
                    icon: '/Content/png/gravar.png',
                    text: 'Gravar',
                    handler: function (btn) {
                        me.gravar(btn);
                    }
                },
                {
                    text: 'Cancelar',
                    scale: 'medium',
                    tooltip: 'Cancela edição e fecha a janela',
                    icon: '/Content/png/cancelar.png',
                    handler: function (btn) {
                        me.cancelar(me);
                    }
                },
                '->'
            ]
        });

        me.callParent();
    },

    gravar: function (btn) {
        var me = btn.up('window'),
                         form = me.down('form'),
                         record = form.getRecord(),
                         store = me.getStore(),
                         callback = me.getAfterSave();

        form.updateRecord();

        var formbasic = form.getForm();
        if (!formbasic.isValid()) { //trigger validation ???
            Ext.Msg.alert("Gravar", "Dados inválidos. Corrija-os");
            return false;
        }

        if (!record.phantom && !record.dirty) {
            Ext.Msg.alert("Gravar", "Nada foi alterado.");
            
            callback.call(me, record);
            return false;
        }

        var errors = record.validate();

        if (errors.length > 0) {
            Ext.Msg.alert("Erro", errors[0]);
            return false;
        }

        me.el.mask("Gravando...");

        store.sync({
            success: function () {
                var result = callback.call(me, record);
                if (!result)
                    return;

                Ext.Msg.show({
                    title: 'Info',
                    msg: 'Registro GRAVADO com sucesso. Fechar?',
                    icon: Ext.Msg.INFO,
                    width: 200,
                    buttons: Ext.Msg.YESNO,
                    fn: function (opt) {
                        me._reloadAfterClose = true;
                        if (opt == 'yes')
                            me.close();
                    }
                });
            },
            callback: function (batch) {
                console.log('synced:', batch);
                if (me.el)
                    me.el.unmask();
            }
        }, me);

        return true;
    },

    cancelar: function (me) {
        var form = me.down('form'),
            record = form.getRecord(),
            store = record.store;

        form.getForm().reset(true);
        record.reject();

        if (store) {
            store.rejectChanges();
        }

        me.close();
    },

    onbeforeclose: function (me) {
        me = me || this;
        var form = me.down('form#mainForm');
        var rec = form.getRecord();
        var canClose = !rec || !rec.phantom && !rec.dirty;
        if (canClose)
            return true;

        Ext.Msg.confirm("Confirmação", "Registro não foi gravado, confirma fechamento da janela ?", function (btn) {
            if (btn == "yes") {
                me._reloadAfterClose = true;
                me.cancelar(me);
            }
        });
        return false;
    },

    onshow: function (me) {
        me = me || this;
        var record = me.getRecord();
        var title = me.title;
        if (record.phantom) {
            me.setTitle(title + " [Inserindo]");
        } else {
            me.setTitle(title + " [Editando]");
        }
        var form = me.down('form#mainForm');
        form.loadRecord(record);
    }
});