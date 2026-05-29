var ClsSettings = {
    GetAll: function () {
        Helper.AjaxCallGet("/api/Setting", {}, "json",
            function (data) {
                console.log(data);
                $("#lnkFacebook").attr("href", data.facebookLink);
            }, function () { });
    }
}

ClsSettings.GetAll();