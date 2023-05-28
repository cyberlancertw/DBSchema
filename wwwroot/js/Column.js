const columnSchema = {
    PrimaryKey: 'columnID',
    ReadAfterRender: true,
    Column: [
        {
            ColumnName: '欄位編號',
            DataName: 'columnID',
            Hidden: true
        },
        {
            ColumnName: '欄位名稱',
            DataName: 'columnName',
            SortType: 'ColumnName'
        },
        {
            ColumnName: '描述',
            DataName: 'description',
        },
        {
            ColumnName: '資料類型',
            DataName: 'dataType',
            Getter: function (item) {
                switch (item.dataType) {
                    case 56: return 'INT';
                    case 167: return 'VARCHAR';
                    case 231: return 'NVARCHAR';
                    case 61: return 'DATETIME';
                    case 104: return 'BIT';
                    case 99: return 'NTEXT';
                    case 106: return 'DECIMAL';
                    case 62: return 'FLOAT';
                    case 52: return 'SMALLINT';
                    case 48: return 'TINYINT';
                    case 40: return 'DATE';
                    case 35: return 'TEXT';
                    case 41: return 'TIME';
                    case 239: return 'NCHAR';
                    case 34: return 'IMAGE';
                    case 241: return 'XML';
                    default: return item.dataType;
                }
                return;
            }
        },
        {
            ColumnName: '範圍',
            DataName: 'dataLength',
            Getter: function (item) {
                switch (item.dataType) {
                    case 48: return '0 至 255';
                    case 56: return '-2,147,483,648 至 2,147,483,647';
                    case 104: return '0 至 1';
                    case 167:
                        if (item.dataLength < 0) return '0 至 MAX';
                        else return '0 至 ' + item.dataLength;
                    case 231:
                        if (item.dataLength < 0) return '0 至 MAX';
                        else return '0 至 ' + item.dataLength / 2;
                    case 239: return item.dataLength / 2;
                    default: return item.dataLength;
                }
                return;
            }
        },
        {
            ColumnName: '預設值',
            DataName: 'defaultValue',
            Width: 80,
            Getter: function (item) {
                return item.defaultValue;
            }
        },
        {
            ColumnName: '空值',
            DataName: 'isNull',
            Width: 80,
            Getter: function (item) {
                return item.isNull ? '可' : '不可';
            }
        },
        {
            ColumnName: '索引',
            DataName: 'indexName',
            Width: 80,
            Getter: function (item) {
                return item.indexName;
            }
        },
        {
            ColumnName: '自動遞增',
            DataName: 'isIdentity',
            Width: 80,
            Getter: function (item) {
                if (item.isIdentity) return '識別';
                else return '';
            }
        },
        {
            ColumnName: '主鍵',
            DataName: 'isPrimaryKey',
            Width: 80,
            Getter: function (item) {
                if (item.isPrimaryKey) {
                    let docFrag = document.createDocumentFragment();
                    let span = document.createElement('span');
                    docFrag.appendChild(span);
                    span.setAttribute('style', 'font-weight:900;color:red;');
                    span.appendChild(document.createTextNode('PK'));
                    return docFrag;
                }
                else return '';
            }
        }
    ],
    Page: {
        Enable: true,
        PageSize: 15,
        PageSizeSelect: [5, 10, 15],
        PositionUp: true
    },
    Sort: {
        SortDefaultEnable: true,
        SortType: 'ColumnName',
        SortDesc: false
    },
    Event: {
        Read: {
            Url: pathBase + '/Home/QueryColumn',
            QueryData: GetQueryName
        },
        RowSelect: function (item) {
            document.getElementById('columnName').value = item.columnName;
            document.getElementById('columnID').value = item.columnID;
            document.getElementById('btnEdit').removeAttribute('disabled');
        },
        RowDeselect: function (item) {
            document.getElementById('columnName').removeAttribute('value');
            document.getElementById('columnID').removeAttribute('value');
            document.getElementById('btnEdit').setAttribute('disabled', true);
        }
    },
    MultiSelect: false
}

function GetQueryName() {
    return {
        QName: document.getElementById('queryColumn').value,
        TableID: document.getElementById('tableID').value
    };
}

function BtnEditClick() {
    let TableID = document.getElementById('tableID').value;
    let ColumnName = document.getElementById('columnName').value;
    let ColumnID = document.getElementById('columnID').value;
    if (!TableID || !ColumnID) return;
    fetch(pathBase + '/Home/GetColumnDescription', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ TableID: TableID, ColumnID: ColumnID })
    }).then(res => res.json()).then(res => {
        if (res.success) {
            document.getElementById('labelColumn').textContent = '[ ' + ColumnName + ' ] 欄位描述：';
            if (res.data) {
                document.getElementById('editDescription').value = res.data;
                document.getElementById('oldDescription').value = res.data;
                CyModal.SetTitle('editModal', '修改欄位描述');
                CyModal.Open('editModal');
            }
            else {
                document.getElementById('editDescription').value = '';
                document.getElementById('oldDescription').value = '';
                CyModal.SetTitle('editModal', '新增欄位描述');
                CyModal.Open('editModal');
            }
        }
        else {
            CyModal.Alert(res.message, 'M', '異常');
        }
    }).catch(res => function () {
        CyModal.Alert('查詢欄位描述發生異常：' + res, 'M', '異常');
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
    let ColumnName = document.getElementById('columnName').value;
    if (!TableName || !TableSchema || !ColumnName) return;

    fetch(pathBase + '/Home/UpdateColumnDescription', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            TableName,
            Description: newValue,
            TableSchema,
            InsertNew: insertNew,
            ColumnName
        })
    }).then(res => res.json()).then(res => {
        if (res.success) {
            CyModal.Alert('修改成功', 'S').then(function () {
                CyGrid.Read('gridColumn');
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
    label.setAttribute('id', 'labelColumn');
    label.appendChild(document.createTextNode('欄位描述'));
    let txtarea = document.createElement('textarea');
    wrap.appendChild(txtarea);
    txtarea.setAttribute('id', 'editDescription');
    txtarea.setAttribute('placeholder', '請輸入欄位描述');
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

window.addEventListener('load', function () {
    CyGrid.Render('gridColumn', columnSchema);
    document.getElementById('btnBack').addEventListener('click', function () {
        document.getElementById('formBack').submit();
    });
    document.getElementById('btnQuery').addEventListener('click', function () {
        CyGrid.Read('gridColumn');
    });
    document.getElementById('queryColumn').addEventListener('keyup', function () {
        if (event.key == 'Enter')
            document.getElementById('btnQuery').click();
    })
    document.getElementById('btnEdit').addEventListener('click', BtnEditClick);
    CyModal.Render('editModal', RenderEditModal(), '500*350', '修改欄位描述');
    document.getElementById('queryColumn').focus();
});