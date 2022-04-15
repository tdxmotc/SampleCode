$(function () {
    GetAuthorizationHeader();
    
    GetApiResponse();    
});

function GetAuthorizationHeader() {    
    const parameter = {
        grant_type:"client_credentials",
        client_id: "XXXXXXXXXX-XXXXXXXX-XXXX-XXXX",
        client_secret: "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX"
    };
    
    let auth_url = "https://tdx.transportdata.tw/auth/realms/TDXConnect/protocol/openid-connect/token";
        
    $.ajax({
        type: "POST",
        url: auth_url,
        crossDomain:true,
        dataType:'JSON',                
        data: parameter,
        async: false,       
        success: function(data){            
            $("#accesstoken").text(JSON.stringify(data));                            
        },
        error: function (xhr, textStatus, thrownError) {
            
        }
    });          
}

function GetApiResponse(){    
    let accesstokenStr = $("#accesstoken").text();    

    let accesstoken = JSON.parse(accesstokenStr);    

    if(accesstoken !=undefined){
        $.ajax({
            type: 'GET',
            url: 'https://tdx.transportdata.tw/api/basic/v2/Rail/TRA/LiveTrainDelay?$top=30&$format=JSON',             
            headers: {
                "authorization": "Bearer " + accesstoken.access_token,                
              },            
            async: false,
            success: function (Data) {
                $('#apireponse').text(JSON.stringify(Data));                
                console.log('Data', Data);
            },
            error: function (xhr, textStatus, thrownError) {
                console.log('errorStatus:',textStatus);
                console.log('Error:',thrownError);
            }
        });
    }
}