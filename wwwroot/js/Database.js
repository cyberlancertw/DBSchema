const databaseSchema = {
    PrimaryKey: 'id',
    ReadAfterRender: true,
    Column: [
        {
            ColumnName: '資料庫編號',
            DataName: 'databaseID',
            Hidden: true
        },
        {
            ColumnName: '資料庫名稱',
            DataName: 'databaseName',
            SortType: 'DatabaseName'
        },
        {
            ColumnName: '已使用空間',
            DataName: 'usedStorage',
            SortType: 'UsedStorage',
            Getter: function (item) {
                return item.usedStorage + ' MB';
            }
        },
        {
            ColumnName: '建立時間',
            DataName: 'createTime',
            SortType: 'CreateTime',
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
        SortType: 'DatabaseName',
        SortDesc: false
    },
    Event: {
        Read: {
            Url: pathBase + '/Home/QueryDatabase',
            QueryData: GetQueryName
        },
        RowSelect: function (item) {
            document.getElementById('databaseName').value = item.databaseName;
            document.getElementById('btnSelect').removeAttribute('disabled');
        },
        RowDeselect: function (item) {
            document.getElementById('databaseName').removeAttribute('value');
            document.getElementById('btnSelect').setAttribute('disabled', true);
        },
        PageReadDone: function () {
            document.getElementById('databaseName').removeAttribute('value');
            document.getElementById('btnSelect').setAttribute('disabled', true);
        },
        RowDoubleClick: function (item) {
            document.getElementById('databaseName').value = item.databaseName;
            document.getElementById('formDatabase').submit();
        }
    },
    MultiSelect: false
}

function GetQueryName() {
    return {
        QName: document.getElementById('queryDatabase').value
    };
}
window.addEventListener('load', function () {
    CyGrid.Render('gridDatabase', databaseSchema);
    document.getElementById('btnBack').addEventListener('click', function () {
        location.href = pathBase + '/Home/';
    });
    document.getElementById('btnQuery').addEventListener('click', function () {
        CyGrid.Read('gridDatabase');
    });
    document.getElementById('btnSelect').addEventListener('click', function () {
        console.log('!');
        if (document.getElementById('databaseName').value)
            document.getElementById('formDatabase').submit();
    });
    document.getElementById('queryDatabase').addEventListener('keyup', function () {
        if (event.key == 'Enter')
            document.getElementById('btnQuery').click();
    });
    document.getElementById('queryDatabase').focus();
});