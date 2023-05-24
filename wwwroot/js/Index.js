window.addEventListener('load', function () {
    document.getElementById('ServerName').focus();
    document.getElementById('ServerAuth').addEventListener('change', function (e) {
        let val = e.target.value;
        let servername = document.getElementById('ServerName');
        if (val == '0') {
            servername.value = '.';
            servername.setAttribute('readonly', true);
            document.getElementById('user').setAttribute('readonly', true);
            document.getElementById('pwd').setAttribute('readonly', true);
            document.getElementById('btnSend').focus();
        }
        else {
            servername.value = '';
            servername.removeAttribute('readonly');
            document.getElementById('user').removeAttribute('readonly');
            document.getElementById('pwd').removeAttribute('readonly');
            document.getElementById('ServerName').focus();
        }
    });
    document.getElementById('ServerName').addEventListener('keyup', textKeyUp);
    document.getElementById('user').addEventListener('keyup', textKeyUp);
    document.getElementById('pwd').addEventListener('keyup', textKeyUp);
    document.getElementById('btnSend').addEventListener('click', checkValid);
});



function textKeyUp(e){
    if (e.Key == 'Enter')
        document.getElementById('btnSend').click();
}

function checkValid(e) {
    e.preventDefault();
    CyLoading.Start();
    fetch(pathBase + '/Home/CheckValid/', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(GetData()),
    }).then(res => res.json())
        .then(res => {
            if (res.success)
                document.getElementById('formConnect').submit();
            else {
                CyLoading.Stop();
                CyModal.Alert('連線失敗', 'S');
            }
        }).catch(function () {
            CyLoading.Stop();
            CyModal.Alert('連線失敗', 'S');
        });
}

function GetData() {
    return {
        server: document.getElementById('ServerName').value,
        user: document.getElementById('user').value,
        pwd: document.getElementById('pwd').value
    };
}