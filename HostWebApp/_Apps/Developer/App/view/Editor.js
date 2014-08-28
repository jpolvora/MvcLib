Ext.define('Dev.view.Editor', {
    extend: 'Ext.panel.Panel',
    alias: 'widget.Editor',
    title: 'Editor',
    _widgetName: '',
    _isHidden: false,

    layout: {
        align: 'stretch',
        pack: 'center',
        type: 'border'
    },
    plain: true,
    border: 0,
    bodyPadding: 0,
    padding: 0,
    autoScroll: true,   

    initComponent: function () {
        var self = this;

        var isHiddenOriginal = null;

        //self.addEvents({
        //    actionSaveFile: true,
        //    actionCancelFile: true,
        //    actionPreview: true
        //});

        Ext.apply(self, {
            tbar: {
                padding: "3 0 3 0",
                items: [
                {
                    xtype: 'splitbutton',
                    text: 'Gravar',
                    width: 100,
                    icon: '/Content/png/gravar.png',
                    handler: function () {
                        self.fireEvent('actionSaveFile', self, function () {
                            console.log('save-file callback');
                            isHiddenOriginal = self._isHidden;
                        });
                    },
                    menu: {
                        xtype: 'menu',
                        items: [
                            {
                                //xtype: 'button',
                                text: 'Download From DropBox',
                                icon: '/Content/png/54.png',
                                handler: function () {
                                    var client = window._DropBoxClient;
                                    if (client.isAuthenticated()) {
                                        self.el.mask("Baixando arquivo do DropBox");
                                        client.readFile(self._widgetFullFileName, function (error, data) {
                                            self.el.unmask();
                                            if (error) {
                                                return Dev.app.showDropBoxError(error); // Something went wrong.
                                            }

                                            self._editor.setValue(data);

                                            //alert(data); // data has the file's contents
                                        });
                                    } else {
                                        Ext.Msg.alert("DropBox", "Faça autenticação em Arquivo => DropBox");
                                    }
                                }
                            },
                            {
                                //xtype: 'button',
                                text: 'Upload to DropBox',
                                icon: '/Content/png/55.png',
                                handler: function () {
                                    var client = window._DropBoxClient;
                                    if (client.isAuthenticated()) {
                                        self.el.mask("Enviando arquivo para Dropbox");
                                        var txt = self._editor.getValue();
                                        client.writeFile(self._widgetFullFileName, txt, function (error, stat) {
                                            self.el.unmask();
                                            if (error) {
                                                return Dev.app.showDropBoxError(error); // Something went wrong.
                                            }
                                            Ext.Msg.alert("DropBox", "Arquivo salvo com sucesso. Revisão: " + stat.versionTag);
                                        });
                                    } else {
                                        Ext.Msg.alert("DropBox", "Faça autenticação em Arquivo => DropBox");
                                    }
                                }
                            }
                        ]
                    }
                },
                {
                    xtype: 'button',
                    text: 'Cancelar',
                    width: 100,
                    icon: '/Content/png/cancelar.png',
                    handler: function () {
                        if (self._changed) {
                            self.fireEvent('actionCancelFile', self);
                        }
                    }
                },
                {
                    xtype: 'button',
                    text: 'Preview',
                    width: 100,
                    icon: '/Content/png/77.png',
                    handler: function () {
                        //var url = self._widgetFullFileName;
                        //window.open(url, "_blank");

                        self.fireEvent('actionPreview', self._widgetFullFileName);
                    }
                },
                {
                    xtype: 'button',
                    text: 'Parâmetros',
                    width: 100,
                    icon: '/Content/png/73.png',
                    handler: function () {
                        Ext.widget("formparametros", {
                            _id: self._widgetName,
                            _virtualPath: self._widgetFullFileName
                        });
                    }
                },
                '-',
                '->',
                  {
                      xtype: 'checkbox',
                      width: 100,
                      boxLabel: 'Oculto',
                      inputValue: 'true',
                      uncheckedValue: 'false',

                      listeners: {
                          afterrender: function (chk) {
                              chk.setValue(self._isHidden);
                          }
                      },
                      handler: function (chk, val) {
                          if (isHiddenOriginal == null)
                              isHiddenOriginal = self._isHidden;

                          if (val == isHiddenOriginal) {
                              self._changed = false;
                              if (self.title.indexOf("*") > 0) {
                                  self.setTitle(self.title.substring(0, self.title.length - 2));
                              }
                          } else {
                              if (!self._changed) {
                                  self._changed = true;
                                  self.setTitle(self.title + " *");
                              }
                          }

                          self._isHidden = val;
                      }
                  }
                ]
            },
            items: [
                 {
                     xtype: 'textarea',
                     bodyPadding: 0,
                     margin: 0,
                     region: 'center',
                     anchor: '100%'
                 }
            ]
        });



        self.callParent(arguments);
    }
});