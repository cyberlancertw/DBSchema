const databaseSchema = {
    GridID: 'gridDatabase',
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
                return CyTool.DateTimeString(item.createTime);
            }
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
    CyGrid.Render(databaseSchema);
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