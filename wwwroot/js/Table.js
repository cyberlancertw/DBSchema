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
            document.getElementById('btnSelect').removeAttribute('disabled');
        },
        RowDeselect: function (item) {
            console.log(item);
            document.getElementById('tableName').removeAttribute('value');
            document.getElementById('tableID').removeAttribute('value');
            document.getElementById('btnSelect').setAttribute('disabled', true);
        }
    },
    MultiSelect: false
}

function GetQueryName() {
    return {
        QName: document.getElementById('queryTable').value,
    };
}
window.addEventListener('load', function () {
    CyGrid.Render('gridTable', tableSchema);
    document.getElementById('btnBack').addEventListener('click', function () {
        document.getElementById('formBack').submit();
    });
    document.getElementById('btnQuery').addEventListener('click', function () {
        CyGrid.Read('gridTable');
    });
    this.document.getElementById('btnSelect').addEventListener('click', function () {
        if (document.getElementById('tableName').value && document.getElementById('tableID').value)
            document.getElementById('formTable').submit();
    });
    document.getElementById('queryTable').focus();
});