Ext.Loader.setConfig({
    enabled: true,
    disableCaching: false,
});
Ext.QuickTips.init();

// setup the state provider, all state information will be saved to a cookie
var stateProvider = Ext.create('Ext.state.CookieProvider');
Ext.state.Manager.setProvider(stateProvider);

//original implementation
var getPath = Ext.Loader.getPath;

//custom impl
var customGetPath = function (klassName) {
    var result = getPath(klassName); //calls the original 
    var newResult = result.replace('.js', '.cshtml');
    return newResult;
};

//Ext.Loader.getPath = customGetPath;

Ext.application({
    autoCreateViewport: true,
    name: 'Dev',
    appFolder: "app",
    refs: [
        {
            ref: 'tabpanel',
            selector: 'tabpanel'
        },
        {
            ref: 'viewport',
            selector: 'viewport'
        },
        {
            ref: 'menutree',
            selector: 'treepanel'
        },
        {
            ref: 'output',
            selector: 'panel#outputpanel'
        }
    ],

    controllers: [
      'Viewport'
    ],

    views: ['Editor', 'FormParametros', 'GridParametros'],

    stores: ['FilesStore', 'PagesStore', 'PagesParametersStore'],

    init: function () {
        console.log("init: ", this);

        var self = this;

        //pasta
        self._createFileAction = Ext.create('Ext.Action', {
            text: 'Novo arquivo',
            icon: '/Content/png/novo.png',
            disabled: true,
            handler: function (widget, event) {
                self.createNewFile();
            }
        });

        self._createFolderAction = Ext.create('Ext.Action', {
            text: 'Nova Pasta',
            icon: '/Content/png/52.png',
            disabled: true,
            handler: function (widget, event) {
                self.createNewFolder();
            }
        });

        self._renameAction = Ext.create('Ext.Action', {
            text: 'Renomear',
            disabled: true,
            handler: function (widget, event) {
                self.renameFile();
            }
        });

        self._deleteAction = Ext.create('Ext.Action', {
            text: 'Excluir',
            icon: '/Content/png/excluir.png',
            disabled: true,
            handler: function (widget, event) {
                self.deleteFileOrFolder();
            }
        });

        self._moveAction = Ext.create('Ext.Action', {
            text: 'Mover',
            disabled: true,
            handler: function (widget, event) {
                Ext.Msg.alert('Developer', 'Função não implementada.');
            }
        });

        self._openFileAction = Ext.create('Ext.Action', {
            text: 'Editar',
            icon: '/Content/png/editar.png',
            disabled: true,
            handler: function (widget, event) {
                self.openFile();
            }
        });

        var contextMenu = Ext.create('Ext.menu.Menu', {
            items: [
                self._openFileAction,
                self._createFileAction,
                self._createFolderAction,
                self._renameAction,
                self._deleteAction,
                self._moveAction
            ]
        });

        function selectTheme(btn) {
            var theme = btn.text;
            var panel = self.getTabpanel();
            var active = panel.getActiveTab();
            var editor = active._editor;
            if (editor) {
                editor.setOption("theme", theme);
                self.getStateProvider().set("editor-theme", theme);
            }
        }


        self.tabHeaderContextMenu = Ext.create('Ext.menu.Menu', {
            ignoreParentClicks: true,
            items: [
            {
                text: 'Close All',
                handler: function () {
                    var panel = self.getTabpanel();
                    panel.items.each(function (it) {
                        it.close();
                    });
                }
            },
            {
                text: 'Close Others',
                handler: function () {
                    var panel = self.getTabpanel();
                    var active = panel.getActiveTab();

                    panel.items.each(function (it) {
                        if (it === active)
                            return;
                        it.close();
                    });
                }
            },
            {
                text: 'Tema',
                menu: {
                    items: [
                         {
                             text: 'default',
                             handler: selectTheme
                         },
                        {
                            text: 'eclipse',
                            handler: selectTheme
                        },
                        {
                            text: 'vibrant-ink',
                            handler: selectTheme
                        },
                       {
                           text: 'mbo',
                           handler: selectTheme
                       },
                       {
                           text: 'monokai',
                           handler: selectTheme
                       },
                         {
                             text: 'pastel-on-dark',
                             handler: selectTheme
                         },
                         {
                             text: 'xq-light',
                             handler: selectTheme
                         }
                    ],
                }
            },

            ]
        });

        this.control({
            'treepanel': {
                checkchange: function (node, checked, eOpts) {
                    if (node.get("expandable") === true) {
                        node.cascadeBy(function (n) {
                            n.set("checked", checked);
                        });
                    }
                },
                itemdblclick: function (treeview, treeItem) {
                    self.openFile(treeItem);
                },
                itemcontextmenu: function (view, rec, node, index, e) {
                    e.stopEvent();
                    contextMenu.showAt(e.getXY());
                    return false;
                }
            },

            'viewport': {
                actionShell: self.openShell,
                actionLogs: self.openLog
            },

            'tabpanel': {
                tabchange: function (tabpanel, newtab, oldtab) {
                    //selecionar o node de acordo com a tab selecionada
                    var item = self.getActiveTab();
                    var treePanel = self.getMenutree();

                    var store = treePanel.getStore();
                    var node = store.getNodeById(item._widgetName);
                    if (node) {
                        node.bubble(function (n) {
                            n.expand();
                        });
                        treePanel.getSelectionModel().select(node);
                    }
                }
            },

            'tabpanel > panel': {
                actionSaveFile: self.saveFile,
                actionCancelFile: function (tab) {
                    Ext.Msg.alert('Não implementado');
                    self.reopenFile(tab);
                },
                actionPreview: function (url) {
                    var tab = Ext.widget('panel', {
                        title: url,
                        frame: false,
                        padding: 0,
                        bodyPadding: 0,
                        plain: true,
                        loader: {
                            url: url,
                            loadMask: true,
                            autoLoad: true
                        }
                    });
                    self.createTab(tab, { border: false });
                }
            }
        });
    },

    launch: function () {
        var self = this;
        console.log("launch: ", self);

        var pageLoader = Ext.get('page-loader');
        if (pageLoader) {
            pageLoader.fadeOut({ remove: true, duration: 3000 });
        }

        var inputEls = Ext.select('*[name=__RequestVerificationToken]');
        if (inputEls) {
            var input = inputEls.elements[0];
            if (input && input.value) {
                var token = input.value;
                Ext.Ajax.on('beforerequest', function (klass, request) {
                    if (request.method == "POST") {
                        if (request.params) {
                            request.params["__RequestVerificationToken"] = token;
                        } else if (request.form) {
                            //todo: adicionar campo ao form
                        }
                    }
                }, this);
            }
        }
        var task = new Ext.util.DelayedTask(function () {
            Ext.Msg.alert('Sessão', 'Sessão expirada !', function () {
                app.suspendEvents();
                Ext.destroy(self);
                window.location.reload();
            });
        });

        Ext.Ajax.disableCaching = false;
        Ext.Ajax.timeout = 30 * 1000; //30sec


        //Ext.Ajax.on('beforerequest', function (klass, request) {
        //    self.getViewport().el.mask();
        //});

        //Ext.Ajax.on('requestcomplete', function (klass, request) {
        //    self.getViewport().el.unmask();
        //});

        Ext.Ajax.on('requestexception', function (conn, response, options) {
            console.log(arguments);
            self.getViewport().el.unmask();

            if (response.status === 403) {
                Ext.Msg.alert('Permissões', 'Sem permissão para executar esta função !');
            } else {
                Ext.Msg.alert(response.status.toString(), response.statusText.toString());
            }
        });



        var lastNode = -1;
        var treePanel = self.getMenutree();
        var store = treePanel.getStore();

        treePanel.on('beforeload', function () {
            var node;
            if (treePanel.getSelectionModel().hasSelection()) {
                node = treePanel.getSelectionModel().getSelection()[0];
                if (node)
                    lastNode = node.data.id;
            } else {
                lastNode = -1;
            }
        });

        treePanel.on("load", function () {
            if (lastNode >= 0) {
                var node = store.getNodeById(lastNode);
                if (node) {
                    node.bubble(function (n) {
                        n.expand();
                    });
                    treePanel.getSelectionModel().select(node);
                }
            }
        });

        treePanel.getSelectionModel().on({
            selectionchange: function (sm, selections) {
                if (selections.length) {
                    var treeItem = selections[0];

                    self._renameAction.enable();
                    self._deleteAction.enable();
                    self._moveAction.enable();

                    if (treeItem.isLeaf()) {
                        //arquivo
                        self._openFileAction.enable();
                        self._createFileAction.disable();
                        self._createFolderAction.disable();
                    } else {
                        //pasta
                        self._openFileAction.disable();
                        self._createFileAction.enable();
                        self._createFolderAction.enable();
                    }
                } else {
                    self._openFileAction.disable();
                    self._createFileAction.disable();
                    self._createFolderAction.disable();
                    self._renameAction.disable();
                    self._deleteAction.disable();
                    self._moveAction.disable();
                }
            }
        });
    },

    getStateProvider: function () {
        return stateProvider;
    },


    writeOutput: function (title, value) {
        //JSON.stringify
        var self = this;
        var output = self.getOutput();
        output.add(
            {
                xtype: 'text',
                text: Ext.String.format("{0}: {1}", title, JSON.stringify(value))
            });

        output.expand(false);
    },

    openLog: function () {
        var self = this;
        var widgetName = 'log';
        var title = 'Server Logs';
        var src, editor;
        var widget = this.getTabByWidgetName(widgetName) || Ext.widget('panel', {
            _widgetName: widgetName,
            title: title,
            layout: {
                align: 'stretch',
                pack: 'center',
                type: 'fit'
            },
            plain: true,
            border: 0,
            bodyPadding: 0,
            padding: 0,
            items: [
             {
                 xtype: 'textareafield',
                 bodyPadding: 0,
                 margin: 0,
                 region: 'center',
                 anchor: '100%',
                 forcefit: true
             }
            ],
            listeners: {
                afterrender: function () {
                    var txt = widget.query('textareafield')[0];
                    txt.setValue('');
                    var txtArea = txt.inputEl.dom;

                    editor = CodeMirror.fromTextArea(txtArea, {
                        mode: 'text/plain',
                        autofocus: true,
                        indentWithTabs: true,
                        indentUnit: 4,
                        lineNumbers: true,
                        styleActiveLine: true,
                        readOnly: true
                    });

                    widget._editor = editor;

                    editor.setSize('100%', '100%');

                    widget.fireEvent('activate'); //activate ñão foi chamado na primeira vez =/
                },

                activate: function () {
                    src = new EventSource('/Dump.axd');
                    src.onmessage = function (event) {
                        editor.replaceRange(event.data + "\n", CodeMirror.Pos(editor.lastLine()));
                    };

                    self.writeOutput("Log ativo.", src);

                },
                deactivate: function () {
                    if (src) {
                        src.close();
                    }

                    self.writeOutput("Log inativo.", {});
                },

                close: function () {
                    if (src) {
                        src.close();
                    }
                    self.writeOutput("Log inativo.", {});
                }
            }
        });

        if (widget && (widget instanceof Ext.panel.Panel)) {
            if (this.tabExists(widgetName)) {
                var panel = this.getTabpanel();
                panel.setActiveTab(widget);
            } else {
                this.createTab(widget);
            }
        }
    },

    openShell: function () {
        var self = this;
        var widgetName = 'shell';
        var title = 'Shell';
        var panel = self.getTabpanel();
        var widget = this.getTabByWidgetName(widgetName) || Ext.widget('panel', {
            _widgetName: widgetName,
            _isShell: true,
            title: title,
            layout: {
                align: 'stretch',
                pack: 'center',
                type: 'fit'
            },
            plain: true,
            border: 0,
            bodyPadding: 0,
            padding: 0,
            items: [
             {
                 xtype: 'textareafield',
                 bodyPadding: 0,
                 margin: 0,
                 region: 'center',
                 anchor: '100%'
             }
            ],
            tbar: {
                items: [
                    {
                        xtype: 'button',
                        text: 'Executar',
                        width: 100,
                        icon: '/Content/png/31.png',
                        handler: function () {
                            //execute
                            var str = widget._editor.getValue();
                            widget.el.mask("Aguarde...");
                            Ext.Ajax.request({
                                url: 'Services/shell',
                                params: {
                                    _csharpcode: Ext.JSON.encodeString(str)
                                },
                                success: function (response, opts) {
                                    var obj = Ext.decode(response.responseText);
                                    self.writeOutput("Resultado da Execução: ", obj.msg);
                                },
                                failure: function (response, opts) {
                                    var obj = Ext.decode(response.responseText);
                                    Ext.Msg.alert("Erro", obj);
                                },
                                callback: function () {
                                    widget.el.unmask();
                                }
                            });

                        },
                    }
                ]
            },
            listeners: {
                afterrender: function () {
                    var txt = widget.query('textareafield')[0];
                    txt.setValue('\
using System; \n\
\n\
public class Shell \n\
{ \n\
    public static void Main() \n\
    {  \n\
        Console.WriteLine("Hello, World");  \n\
    } \n\
}');
                    var txtArea = txt.inputEl.dom;

                    var editor = CodeMirror.fromTextArea(txtArea, {
                        mode: 'clike',
                        autofocus: true,
                        indentWithTabs: true,
                        indentUnit: 4,
                        lineNumbers: true,
                        matchBrackets: true,
                        autoCloseBrackets: true,
                        matchTags: true,
                        styleActiveLine: true,
                        theme: 'eclipse'
                    });

                    widget._editor = editor;

                    editor.setSize('100%', '100%');
                }
            }
        });

        if (widget && (widget instanceof Ext.panel.Panel)) {
            if (this.tabExists(widgetName)) {
                panel.setActiveTab(widget);
            } else {
                this.createTab(widget);
            }
        }
    },

    //opens the selectedfile
    openFile: function (treeItem) {
        var self = this;
        treeItem = treeItem || self.getMenutree().getSelectionModel().getSelection()[0];

        if (!treeItem.isLeaf())
            return;

        var widgetName = treeItem.get("id");
        var title = treeItem.get("fileName").replace(/^.*[\\\/]/, '');
        var panel = self.getTabpanel();
        panel.el.mask('Loading...');

        var fullFileName = treeItem.get("fileName");

        var widget = this.getTabByWidgetName(widgetName) || Ext.widget('Editor', {
            _isHidden: treeItem.get("IsHidden"),
            _widgetName: widgetName,
            tooltip: fullFileName,
            _widgetFullFileName: fullFileName,
            loader: {
                url: 'Services/getfile.cshtml',
                loadMask: true,
                autoLoad: true,
                params: {
                    id: widgetName
                },
                listeners: {
                    beforeload: function () {

                    },
                    load: function () {

                    },
                    exception: function () {

                    }
                },
                renderer: function (loader, response, active) {
                    var text = response.responseText;
                    var txt = widget.down('textareafield');

                    txt.setValue(text);

                    var extension = title.substring(title.lastIndexOf(".") + 1);
                    var m;
                    switch (extension) {
                        case "js":
                            m = "javascript";
                            break;
                        case "config":
                        case "xml":
                            m = "xml";
                            break;
                        case "css":
                            m = "css";
                            break;
                        case "cs":
                        case "cshtml":
                            m = "razor";
                            break;
                        case "htm":
                        case "html":
                            m = 'htmlembedded';
                            break;
                        default:
                            m = "text/plain";
                    }

                    var txtArea = txt.inputEl.dom;

                    var editor = CodeMirror.fromTextArea(txtArea, {
                        mode: m,
                        autofocus: true,
                        foldGutter: true,
                        indentWithTabs: true,
                        indentUnit: 4,
                        lineNumbers: true,
                        matchBrackets: true,
                        autoCloseBrackets: true,
                        matchTags: true,
                        styleActiveLine: true,
                        theme: self.getStateProvider().get("editor-theme", "vibrant-ink"),
                        "Ctrl-S": false,
                    });

                    widget._editor = editor;

                    editor.setSize('100%', '100%');

                    editor.on('change', function () {
                        if (widget._changed)
                            return;
                        widget._isHidden = undefined;
                        widget._changed = true;
                        widget.setTitle(widget.title + " *");
                    });

                    widget.updateLayout();
                    return true;
                }
            },
            title: title,
            listeners: {
                afterrender: function (sTab) {
                    var btn = sTab.tab.el;
                    btn.on('contextmenu', function (e) {
                        e.stopEvent();
                        self.tabHeaderContextMenu.showAt(e.getXY());
                    });
                },

                beforeclose: function () {
                    if (widget._changed) {
                        Ext.Msg.confirm('Gravar?', 'Arquivo alterado. Deseja gravar?', function (btn) {
                            if (btn == 'yes') {
                                self.saveFile(widget, function (success) {
                                    if (success)
                                        widget.close();
                                });

                            } else {
                                widget._changed = false;
                                widget.close();
                            }
                        });

                        return false;
                    }
                    return true;
                }
            }
        });

        panel.el.unmask();
        if (widget && (widget instanceof Ext.panel.Panel)) {
            if (this.tabExists(widgetName)) {
                panel.setActiveTab(widget);
            } else {
                this.createTab(widget);
            }
        }
    },

    reopenFile: function (tab) {
        var self = this;
        var fileId = tab._widgetName;
        var fileName = tab._widgetFullFileName;

        tab.close();
        //todo: criar um overload de openFile que aceita um fileName
        var treePanel = self.getMenutree();
        var store = treePanel.getStore();
        var node = store.getNodeById(fileId);
        if (node) {
            node.bubble(function (n) {
                n.expand();
            });
            treePanel.getSelectionModel().select(node);

            self.openFile();
        }
    },

    saveFile: function (tab, callback) {
        var self = this;

        function utf8_to_b64(str) {
            return window.btoa(unescape(encodeURIComponent(str)));
        }

        function b64_to_utf8(str) {
            return decodeURIComponent(escape(window.atob(str)));
        }

        if (!tab) tab = self.getActiveTab();

        if (tab && tab._changed) {
            var txt = tab._editor.getValue();
            //alert(utf8_to_b64(txt));
            tab.el.mask("Salvando...");
            Ext.Ajax.request({
                url: 'Services/savefile',
                params: {
                    id: tab._widgetName,
                    isHidden: tab._isHidden,
                    content: utf8_to_b64(txt)
                },
                success: function () {
                    var store = Ext.getStore('FilesStore');
                    var record = store.getNodeById(tab._widgetName);
                    record.remove();
                    store.load();

                    self.writeOutput("Info", "Arquivo salvo com sucesso");
                    tab._changed = false;
                    tab.setTitle(tab.title.substring(0, tab.title.length - 2));
                },
                callback: function (opt, success) {
                    tab.el.unmask();
                    if (callback)
                        callback(success);
                }
            });
        } else {
            Ext.Msg.alert('Info', 'nada a salvar.');
        }
    },

    createNewFolder: function () {
        var self = this;
        var treePanel = self.getMenutree();
        var treeItem = treePanel.getSelectionModel().getSelection()[0];
        if (treeItem) {
            var myMsgBox = new Ext.window.MessageBox();
            myMsgBox.prompt('Novo Arquivo', 'Digite o nome para o diretório', function (btn, result) {
                if (btn != 'ok')
                    return;
                //ajax
                treePanel.el.mask("Aguarde...");
                Ext.Ajax.request({
                    url: 'Services/createfolder',
                    params: {
                        id: treeItem.get("id"),
                        dirName: result
                    },
                    success: function () {
                        var store = Ext.getStore('FilesStore');
                        store.load();
                        self.writeOutput("Info", "Diretório criado com sucesso");
                    },
                    callback: function () {
                        treePanel.el.unmask();
                    }
                });
            });
        } else {
            Ext.Msg.alert('Erro', 'Selecione uma pasta raiz');
        }
    },

    createNewFile: function () {
        var self = this;
        var treePanel = self.getMenutree();
        var treeItem = treePanel.getSelectionModel().getSelection()[0];
        if (treeItem) {
            var myMsgBox = new Ext.window.MessageBox();
            myMsgBox.prompt('Novo Arquivo', 'Digite o nome para o arquivo com a extensão', function (btn, result) {
                if (btn != 'ok')
                    return;
                //ajax

                var id = treeItem.get("id");
                treePanel.el.mask("Aguarde...");
                Ext.Ajax.request({
                    url: 'Services/createfile',
                    params: {
                        id: id,
                        fileName: result
                    },
                    success: function () {
                        self.writeOutput("Info", "Arquivo criado com sucesso");
                        var store = treePanel.getStore();
                        var fn = function () {
                            store.un('load', fn);

                            var rNode = store.getNodeById(id);
                            if (rNode) {
                                var fullFilename = rNode.data['fileName'] + result;
                                rNode.eachChild(function (n) {
                                    if (n.data['leaf'] && n.data['fileName'] === fullFilename) {
                                        treePanel.getSelectionModel().select(n);
                                        self.openFile();
                                        return false; //break
                                    } else {
                                        return true; //continue
                                    }
                                });
                            }
                        };

                        store.on('load', fn);

                        store.load();
                    },
                    callback: function () {
                        treePanel.el.unmask();
                    }
                });
            });
        } else {
            Ext.Msg.alert('Erro', 'Selecione uma pasta raiz');
        }
    },

    renameFile: function () {
        var self = this;
        var treePanel = self.getMenutree();
        var treeItem = treePanel.getSelectionModel().getSelection()[0];
        if (!treeItem) {
            return false;
        }

        if (treeItem.get("isLeaf") == true) {
            if (treeItem.hasChildNodes()) {
                Ext.Msg.alert('Erro', "Pasta não está vazia!");
                return false;
            }
        }

        var myMsgBox = new Ext.window.MessageBox();
        myMsgBox.prompt('Novo Nome', 'Digite o novo nome', function (btn, result) {
            if (btn != 'ok')
                return;
            //ajax
            treePanel.el.mask("Aguarde...");
            Ext.Ajax.request({
                url: 'Services/renamefile',
                params: {
                    id: treeItem.get("id"),
                    newFileName: result
                },
                success: function () {
                    var store = Ext.getStore('FilesStore');
                    store.load();
                    self.writeOutput("Info", "Arquivo renomeado com sucesso");
                },
                callback: function () {
                    treePanel.el.unmask();
                }
            });
        });

        return true;
    },

    deleteFileOrFolder: function () {
        var self = this;
        var treePanel = self.getMenutree();
        var treeItem = treePanel.getSelectionModel().getSelection()[0];
        if (!treeItem) {
            return false;
        }

        if (treeItem.get("isLeaf")) {
            if (treeItem.hasChildNodes()) {
                Ext.Msg.alert('Erro', "Pasta não está vazia!");
                return false;
            }
        }

        Ext.Msg.confirm('Exclusão', 'Confirma exclusão?', function (button) {
            if (button === 'yes') {

                treePanel.el.mask("Aguarde...");
                Ext.Ajax.request({
                    url: 'Services/deletefile',
                    params: {
                        id: treeItem.get("id")
                    },
                    success: function () {
                        var tab = self.getTabByWidgetName(treeItem.get("id"));
                        if (tab) {
                            tab.close();
                        }
                        var store = Ext.getStore('FilesStore');
                        store.load();
                        self.writeOutput("Info", "Arquivo excluído com sucesso");
                    },
                    callback: function () {
                        treePanel.el.unmask();
                    }
                });
            }
        });

        return true;
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
        return active._widgetName == widget._widgetName;
    },

    getTabByWidgetName: function (widgetName) {
        var panel = this.getTabpanel();
        var item = panel.items.findBy(function (it) {
            return it._widgetName === widgetName;
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
        console.log('adding to tabpanel:', item);

        panel.setActiveTab(item);

        return item;
    },

    showDropBoxError: function (error) {
        switch (error.status) {
            case Dropbox.ApiError.INVALID_TOKEN:
                // If you're using dropbox.js, the only cause behind this error is that
                // the user token expired.
                // Get the user through the authentication flow again.
                Ext.Msg.alert("Erro DropBox", "INVALID_TOKEN");
                break;

            case Dropbox.ApiError.NOT_FOUND:
                // The file or folder you tried to access is not in the user's Dropbox.
                // Handling this error is specific to your application.
                Ext.Msg.alert("Erro DropBox", "NOT_FOUND");
                break;

            case Dropbox.ApiError.OVER_QUOTA:
                // The user is over their Dropbox quota.
                // Tell them their Dropbox is full. Refreshing the page won't help.
                Ext.Msg.alert("Erro DropBox", "OVER_QUOTA");
                break;

            case Dropbox.ApiError.RATE_LIMITED:
                // Too many API requests. Tell the user to try again later.
                // Long-term, optimize your code to use fewer API calls.
                Ext.Msg.alert("Erro DropBox", "RATE_LIMITED");
                break;

            case Dropbox.ApiError.NETWORK_ERROR:
                // An error occurred at the XMLHttpRequest layer.
                // Most likely, the user's network connection is down.
                // API calls will not succeed until the user gets back online.
                Ext.Msg.alert("Erro DropBox", "NETWORK_ERROR");
                break;

            case Dropbox.ApiError.INVALID_PARAM:
            case Dropbox.ApiError.OAUTH_ERROR:
            case Dropbox.ApiError.INVALID_METHOD:
            default:
                // Caused by a bug in dropbox.js, in your application, or in Dropbox.
                // Tell the user an error occurred, ask them to refresh the page.
                Ext.Msg.alert("Erro DropBox", error.status);
        }
    },
});