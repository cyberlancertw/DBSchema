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
        }
    ],
    Page: {
        Enable: true,
        PageSize: 10,
        PageSizeSelect: [5, 10, 15],
        PositionUp: false
    },
    Sort: {
        SortDefaultEnable: true,
        SortType: 'DatabaseName',
        SortDesc: false
    },
    Event: {
        Read: {
            Url: '/Home/QueryDatabase',
            //QueryData: ReadStudents
        },
        RowSelect: function (item, datas) {
            console.log(item);
            console.log(datas);
        },
    },
    MultiSelect: false
}


window.addEventListener('load', function () {
    CyGrid.Render('gridDatabase', databaseSchema);
});