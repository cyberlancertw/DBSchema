const tableSchema = {
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
            SortType: 'ColumnCount',
            Width: 100
        },
        {
            ColumnName: '建立時間',
            DataName: 'createTime',
            SortType: 'CreateTime',
            Width: 150,
            Getter: function (item) {
                let t = new Date(item.createTime);
                let y = t.getFullYear();
                let M = t.getMonth() > 8 ? (t.getMonth() + 1) : '0' + (t.getMonth() + 1);
                let d = t.getDate() > 9 ? t.getDate() : '0' + t.getDate();
                let h = t.getHours() > 9 ? t.getHours() : '0' + t.getHours();
                let m = t.getMinutes() > 9 ? t.getMinutes() : '0' + t.getMinutes();
                let s = t.getSeconds() > 9 ? t.getSeconds() : '0' + t.getSeconds();
                return y + '-' + M + '-' + d + " " + h + ":" + m + ":" + s;
            }
        },
        {
            ColumnName: 'Schema',
            DataName: 'schema',
            Hidden: true
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

window.addEventListener('load', function () {
    CyGrid.Render('gridTable', tableSchema);
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
    CyModal.Render('editModal', RenderEditModal(), '500*350', '修改資料表描述');
    document.getElementById('queryTable').focus();
});