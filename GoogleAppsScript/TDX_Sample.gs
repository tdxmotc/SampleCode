var CLIENT_ID = '<YOUR CLIENT ID>';
var CLIENT_SECRET = '<YOUR CLIENT SECRET>';

function doGet(e) {
  var url = 'https://tdx.transportdata.tw/api/basic/v2/Rail/TRA/LiveBoard/Station/1000?$filter=Direction eq 1&$format=JSON'
  var options = {
      "method": "get",
      "headers": { "authorization": "Bearer " + GetAuthorizationToken() }
  };
  var response = UrlFetchApp.fetch(url, options);
  // console.log(response.getResponseCode());
  var outData = JSON.parse(response.getContentText());
  console.log(outData);
  return ContentService.createTextOutput(JSON.stringify(outData)).setMimeType(ContentService.MimeType.JSON);
}

function GetAuthorizationToken() {
  var token_url = "https://tdx.transportdata.tw/auth/realms/TDXConnect/protocol/openid-connect/token";
  var options = {
      "method": "post",
      "headers": {
          "content-type": "application/x-www-form-urlencoded"
      },
      "payload": {
          "grant_type": "client_credentials",
          "client_id": CLIENT_ID,
          "client_secret": CLIENT_SECRET
      }
  };
  var response = UrlFetchApp.fetch(token_url, options);

  outData = JSON.parse(response.getContentText());
  // console.log(outData);
  return outData['access_token'];
}
