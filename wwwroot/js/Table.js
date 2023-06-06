const tableSchema = {
    GridID: 'gridTable',
    PrimaryKey: 'tableID',
    ReadAfterRender: true,
    Column: [
        {
            ColumnName: '資料表編號',
            DataName: 'tableID',
            Hidden: true
        },
        {
            ColumnName: '資料表名稱',
            DataName: 'tableName',
            SortType: 'TableName'
        },
        {
            ColumnName: '描述',
            DataName: 'description',
        },
        {
            ColumnName: '欄位數量',
            DataName: 'columnCount',
            SortType: 'ColumnCount'
        },
        {
            ColumnName: '建立時間',
            DataName: 'createTime',
            SortType: 'CreateTime',
            Getter: function (item) {
                return CyTool.DateTimeString(item.createTime);
            }
        },
        {
            ColumnName: 'Schema',
            DataName: 'schema',
            Hidden: true
        }
    ],
    Height: 600,
    Page: {
        Enable: true,
        PageSize: 50,
        PageSizeSelect: [5, 10, 15, 20, 25, 30, 35, 40, 45, 50, 75, 100],
        PositionUp: true
    },
    Sort: {
        SortDefaultEnable: true,
        SortType: 'TableName',
        SortDesc: false
    },
    Event: {
        Read: {
            Url: pathBase + '/Home/QueryTable',
            QueryData: GetQueryName
        },
        RowSelect: function (item) {
            document.getElementById('tableName').value = item.tableName;
            document.getElementById('tableID').value = item.tableID;
            document.getElementById('tableSchema').value = item.schema;
            document.getElementById('btnSelect').removeAttribute('disabled');
            document.getElementById('btnEdit').removeAttribute('disabled');
        },
        RowDeselect: function (item) {
            document.getElementById('tableName').removeAttribute('value');
            document.getElementById('tableID').removeAttribute('value');
            document.getElementById('tableSchema').removeAttribute('value');
            document.getElementById('btnSelect').setAttribute('disabled', true);
            document.getElementById('btnEdit').setAttribute('disabled', true);
        },
        RowDoubleClick: function (item) {
            document.getElementById('tableName').value = item.tableName;
            document.getElementById('tableID').value = item.tableID;
            document.getElementById('tableSchema').value = item.schema;
            document.getElementById('formTable').submit();
        }
    },
    MultiSelect: false
}

const transferTable = {
    TransferID: 'listTable',
    Height: 480,
    Label: {
        From: '可選取資料表',
        To: '已選取資料夾'
    },
    Filter: true,
    Event: {
        AfterGo: CheckBeforeExport,
        AfterBack: CheckBeforeExport
    }
};
function GetQueryName() {
    return {
        QName: document.getElementById('queryTable').value,
    };
}

function btnEditClick() {
    let TableName = document.getElementById('tableName').value;
    let TableID = document.getElementById('tableID').value;
    if (!TableName || !TableID) return;
    fetch(pathBase + '/Home/GetTableDescription', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ TableName: TableName, TableID: TableID })
    }).then(res => res.json()).then(res => {
        if (res.success) {
            document.getElementById('labelTable').textContent = '[ ' + TableName + ' ] 資料表描述：';
            if (res.data) {
                document.getElementById('editDescription').value = res.data;
                document.getElementById('oldDescription').value = res.data;
                CyModal.SetTitle('editModal', '修改資料表描述');
                CyModal.Open('editModal');
            }
            else {
                document.getElementById('editDescription').value = '';
                document.getElementById('oldDescription').value = '';
                CyModal.SetTitle('editModal', '新增資料表描述');
                CyModal.Open('editModal');
            }
        }
        else {
            CyModal.Alert(res.message, 'M', '異常');
        }
    }).catch(res => function () {
        CyModal.Alert('查詢資料表描述發生異常：' + res, 'M', '異常');
    });
}

function BtnUpdateClick() {
    let newValue = document.getElementById('editDescription').value;
    let oldValue = document.getElementById('oldDescription').value;
    if (newValue == oldValue) {
        CyModal.Alert('無資料變更', 'S').then(function () {
            CyModal.Close('editModal');
        });
    }
    else {
        UpdateDescription(newValue, !oldValue);
    }
}

function UpdateDescription(newValue, insertNew) {
    let TableName = document.getElementById('tableName').value;
    let TableSchema = document.getElementById('tableSchema').value;
    if (!TableName || !TableSchema) return;

    fetch(pathBase + '/Home/UpdateTableDescription', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            TableName,
            Description: newValue,
            TableSchema,
            InsertNew: insertNew
        })
    }).then(res => res.json()).then(res => {
        if (res.success) {
            CyModal.Alert('修改成功', 'S').then(function () {
                CyGrid.Read('gridTable');
                CyModal.Close('editModal');
            });
        }
        else {
            CyModal.Alert(res.message);
        }
    }).catch(res => CyModal.Alert('修改發生異常：' + res));
}
function RenderEditModal() {
    let docFrag = document.createDocumentFragment();
    let wrap = document.createElement('div');
    docFrag.appendChild(wrap);
    wrap.setAttribute('style', 'display:flex;flex-direction:column;align-items:center;');
    let label = document.createElement('label');
    wrap.appendChild(label);
    label.setAttribute('for', 'editDescription');
    label.setAttribute('id', 'labelTable');
    label.appendChild(document.createTextNode('資料表描述'));
    let txtarea = document.createElement('textarea');
    wrap.appendChild(txtarea);
    txtarea.setAttribute('id', 'editDescription');
    txtarea.setAttribute('placeholder', '請輸入資料表描述');
    txtarea.setAttribute('style', 'margin-top:1rem;width:80%;height:150px;padding:0.8rem;margin-left:auto;margin-right:auto;border-color:gray;border-radius:0.5rem;resize:none;font-size:1rem;');
    let txtOld = document.createElement('input');
    docFrag.appendChild(txtOld);
    txtOld.setAttribute('type', 'hidden');
    txtOld.setAttribute('id', 'oldDescription');
    let divBtn = document.createElement('div');
    let btnCancel = document.createElement('button');
    wrap.appendChild(divBtn);
    divBtn.setAttribute('style', 'width:80%;margin-top:1.5rem;display:flex;justify-content:space-evenly;');
    divBtn.appendChild(btnCancel);
    btnCancel.className = 'cy-button btn-gray';
    let iconCancel = document.createElement('i');
    btnCancel.appendChild(iconCancel);
    btnCancel.appendChild(document.createTextNode('取消'));
    iconCancel.setAttribute('data-size', '1');
    iconCancel.appendChild(document.createTextNode('undo'));
    btnCancel.addEventListener('click', function () {
        CyModal.Close('editModal');
    });
    let btnUpdate = document.createElement('button');
    divBtn.appendChild(btnUpdate);
    btnUpdate.className = 'cy-button btn-yellow';
    let iconUpdate = document.createElement('i');
    btnUpdate.appendChild(iconUpdate);
    btnUpdate.appendChild(document.createTextNode('更新'));
    iconUpdate.setAttribute('data-size', '1');
    iconUpdate.appendChild(document.createTextNode('check'));
    btnUpdate.addEventListener('click', BtnUpdateClick);
    return docFrag;
}


function RenderExportModal() {
    let docFrag = document.createDocumentFragment();
    let wrap = document.createElement('div');
    docFrag.appendChild(wrap);
    let field = document.createElement('fieldset');
    wrap.appendChild(field);
    let legend = document.createElement('legend');
    field.appendChild(legend);
    legend.appendChild(document.createTextNode('資料表'));
    field.appendChild(GetRadioLabel('exportChoose', 'exportType', '選擇要匯出的資料表'));
    field.appendChild(GetRadioLabel('exportAll', 'exportType', '匯出所有資料表'));

    field = document.createElement('fieldset');
    wrap.appendChild(field);
    legend = document.createElement('legend');
    field.appendChild(legend);
    legend.appendChild(document.createTextNode('匯出檔案類型'));
    field.appendChild(GetRadioLabel('exportXlsx', 'fileType', '.xlsx'));
    field.appendChild(GetRadioLabel('exportPdf', 'fileType', '.pdf'));
    field.appendChild(GetRadioLabel('exportCsv', 'fileType', '.csv'));
    field.appendChild(GetRadioLabel('exportDocx', 'fileType', '.docx'));

    field = document.createElement('fieldset');
    wrap.appendChild(field);
    legend = document.createElement('legend');
    field.appendChild(legend);
    legend.appendChild(document.createTextNode('顏色格式'));
    let divTitle = document.createElement('div');
    field.appendChild(divTitle);
    divTitle.appendChild(GetCheckLabel('checkTitle', 'exportConfig', '標題'));
    divTitle.appendChild(GetColorPicker('titleFg', '#000000'));
    divTitle.appendChild(GetColorPicker('titleBg', '#a6e186'));
    let divSpecial = document.createElement('div');
    field.appendChild(divSpecial);
    divSpecial.appendChild(GetCheckLabel('checkSpecial', 'exportConfig', '特殊'));
    divSpecial.appendChild(GetColorPicker('specialFg', '#f88aa0'));
    divSpecial.appendChild(GetColorPicker('specialBg', '#ffffff'));

    let exportBtn = document.createElement('button');
    wrap.appendChild(exportBtn);
    exportBtn.className = 'cy-button btn-blue';
    let icon = document.createElement('i');
    icon.setAttribute('data-size', '1');
    icon.appendChild(document.createTextNode('file'));
    exportBtn.appendChild(icon);
    exportBtn.appendChild(document.createTextNode('匯出'));
    exportBtn.addEventListener('click', ExportBtnClick);

    let divTransfer = document.createElement('div');
    docFrag.appendChild(divTransfer);
    divTransfer.setAttribute('id', 'listTable');
    return docFrag;
}

function GetRadioLabel(id, name, text) {
    let div = document.createElement('div');
    let radio = document.createElement('input');
    div.appendChild(radio);
    radio.setAttribute('type', 'radio');
    radio.setAttribute('id', id);
    radio.setAttribute('name', name);
    if (id == 'exportChoose' || id == 'exportAll') {
        radio.addEventListener('change', CheckBeforeExport);
    }
    let label = document.createElement('label');
    div.appendChild(label);
    label.setAttribute('for', id);
    label.appendChild(document.createTextNode(text));
    return div;
}

function GetCheckLabel(id, name, text) {
    let div = document.createElement('div');
    let check = document.createElement('input');
    div.appendChild(check);
    check.setAttribute('type', 'checkbox');
    check.setAttribute('id', id);
    check.setAttribute('name', name);
    let label = document.createElement('label');
    div.appendChild(label);
    label.setAttribute('for', id);
    label.appendChild(document.createTextNode(text));
    return div;
}

function GetColorPicker(id, color) {
    let picker = document.createElement('input');
    picker.setAttribute('id', id);
    picker.setAttribute('type', 'color');
    picker.setAttribute('value', color);
    picker.addEventListener('change', function () {
        let parent = document.getElementById(id).parentNode;
        let target = parent.querySelector('label');
        let fg = parent.querySelectorAll('input[type="color"]')[0].value;
        let bg = parent.querySelectorAll('input[type="color"]')[1].value;
        target.setAttribute('style', 'color:' + fg + ';background-color:' + bg + ';');
    });
    return picker;
}

function BeforeOpenExportModal() {
    //document.getElementById('exportModal').querySelector('.cy-modal-main').querySelector('button').setAttribute('disabled', true);
    document.getElementById('exportModal').querySelector('.cy-modal-main').querySelector('button').removeAttribute('disabled');
    document.getElementById('exportAll').checked = true;
    document.getElementById('exportXlsx').checked = true;
    let field3 = document.getElementById('exportModal').querySelector('fieldset:nth-child(3)');
    field3.querySelectorAll('label')[0].setAttribute('style', 'color:#000000;background-color:#a6e186;');
    field3.querySelectorAll('label')[1].setAttribute('style', 'color:#ff0000;background-color:#ffffff;');
    document.getElementById('checkTitle').checked = true;
    document.getElementById('checkSpecial').checked = true;
    document.getElementById('titleFg').value = '#000000';
    document.getElementById('titleBg').value = '#a6e186';
    document.getElementById('specialFg').value = '#ff0000';
    document.getElementById('specialBg').value = '#ffffff';
}
function ExportBtnClick() {
    let fields = document.getElementById('exportModal').querySelectorAll('fieldset');
    let exportType = '';
    let radios = fields[0].querySelectorAll('input');
    for (let i = 0; i < radios.length; i++) {
        if (radios[i].checked) {
            exportType = radios[i].getAttribute('id');
        }
    }
    let fileType = '';
    radios = fields[1].querySelectorAll('input');
    for (let i = 0; i < radios.length; i++) {
        if (radios[i].checked) {
            fileType = radios[i].getAttribute('id');
        }
    }
    let tableid;
    if (exportType == 'exportChoose') {
        tableid = CyTransfer.GetToValue('listTable');
    }
    let titleColor = '', specialColor = '';
    if (document.getElementById('checkTitle').checked) {
        titleColor = document.getElementById('titleFg').value.substring(1) + document.getElementById('titleBg').value.substring(1);
    }
    if (document.getElementById('checkSpecial').checked) {
        specialColor = document.getElementById('specialFg').value.substring(1) + document.getElementById('specialBg').value.substring(1);
    }
    let progressID = CyTool.UUID();
    fetch(pathBase + '/Home/ExportFile', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            ExportType: exportType,
            FileType: fileType,
            TableID: tableid,
            TitleColor: titleColor,
            SpecialColor: specialColor,
            ProgressID: progressID
        })
    }).then(res => res.json())
        .then(res => {
            if (res.success) {
                document.getElementById('formFile').querySelector('input').value = res.token;
                document.getElementById('formFile').submit();
            }
            else CyModal.Alert(res.message, 'L');
        }).catch(err => CyModal.Alert('匯出發生異常'));
    window.setTimeout(function () {
        document.getElementById('exportProgress').classList.remove('hidden');
        document.getElementById('exportProgress').classList.add('show');
        GetProgress(progressID);
    }, 100);
}

function GetProgress(progressID) {
    fetch(pathBase + '/Home/GetProgress', {
        headers: { 'Content-Type': 'application/json' },
        method: 'POST',
        body: JSON.stringify(progressID)
    }).then(res => res.json()).then(res => {
        if (res.progress < 0) {
            document.getElementById('exportProgress').classList.add('hidden');
            document.getElementById('exportProgress').classList.remove('show');
            return;
        }
        if (res.progress < 1) {
            document.getElementById('exportFinish').style.width = (res.progress * 100) + '%';
            document.getElementById('exportRemain').style.width = (1 - res.progress) * 100 + '%';
            window.setTimeout(function () {
                GetProgress(progressID);
            }, 100);
        }
        else {
            document.getElementById('exportProgress').classList.add('hidden');
            document.getElementById('exportProgress').classList.remove('show');
        }
    });
}

function CheckBeforeExport() {
    let btn = document.getElementById('exportModal').querySelector('.cy-modal-main').querySelector('button');
    if (document.getElementById('exportAll').checked) btn.removeAttribute('disabled');
    else if (CyTransfer.GetToUid('listTable').length > 0) btn.removeAttribute('disabled');
    else btn.setAttribute('disabled', true);
}

window.addEventListener('load', function () {
    CyGrid.Render(tableSchema);
    document.getElementById('btnBack').addEventListener('click', function () {
        document.getElementById('formBack').submit();
    });
    document.getElementById('btnQuery').addEventListener('click', function () {
        CyGrid.Read('gridTable');
    });
    document.getElementById('btnSelect').addEventListener('click', function () {
        if (document.getElementById('tableName').value && document.getElementById('tableID').value)
            document.getElementById('formTable').submit();
    });
    document.getElementById('queryTable').addEventListener('keyup', function () {
        if (event.key == 'Enter')
            document.getElementById('btnQuery').click();
    });
    document.getElementById('btnEdit').addEventListener('click', btnEditClick);
    document.getElementById('btnExport').addEventListener('click', function () {

        fetch(pathBase + '/Home/ReadExportTable', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({})
        }).then(res => res.json())
            .then(res => {
                if (res.success) {
                    BeforeOpenExportModal();
                    CyTransfer.Fill('listTable', res.fromData);
                    CyModal.Open('exportModal');
                }
                else {
                    CyModal.Alert('讀取資料表發生異常：' + res.message );
                }
            }).catch(err => CyModal.Alert('讀取資料發生異常：' + err));
        
    });
    CyModal.Render('editModal', RenderEditModal(), '500*350', '修改資料表描述');
    CyModal.Render('exportModal', RenderExportModal(), '1000*730', '匯出報表檔');
    CyTransfer.Render(transferTable);
    document.getElementById('btnCloseProgress').addEventListener('click', function () {
        document.getElementById('exportProgress').classList.add('hidden');
        document.getElementById('exportProgress').classList.remove('show');
    });
    document.getElementById('queryTable').focus();
});