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
                    case 167: return '0 至 ' + item.dataLength;
                    case 231: return '0 至 ' + item.dataLength / 2;
                    case 239: return item.dataLength / 2;
                    default: return item.dataLength;
                }
                return;
            }
        },
        {
            ColumnName: '空值',
            DataName: 'isNull',
            Width: 80,
            Getter: function (item) {
                return item.isNull ? '可' : '';
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
            Url: '/Home/QueryColumn',
            QueryData: GetQueryName
        },
        RowSelect: function (item) {
            document.getElementById('columnName').value = item.columnName;
            document.getElementById('columnID').value = item.columnID;
        },
        RowDeselect: function (item) {
            console.log(item);
            document.getElementById('columnName').removeAttribute('value');
            document.getElementById('columnID').removeAttribute('value');
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
window.addEventListener('load', function () {
    CyGrid.Render('gridColumn', columnSchema);
    document.getElementById('btnBack').addEventListener('click', function () {
        document.getElementById('formBack').submit();
    });
    document.getElementById('btnQuery').addEventListener('click', function () {
        CyGrid.Read('gridColumn');
    });
    
    document.getElementById('queryColumn').focus();
});