# TDX運輸資料流通服務API介接範例程式碼說明


為使開發者能快速在M2M環境下介接使用TDX運輸資料流通服務平臺之交通領域資料服務API，在此提供數種程式語言的範例程式碼提供開發者做參考。

## API使用次數限制
TDX API現階段呼叫頻率限制為 **每個呼叫來源IP每秒最多50次 (不分API金鑰)**。

## API認證機制
TDX API皆使用OIDC Client Credentials流程進行身份認證，認證完成後即取得Access Token，將Access Token帶入即可存取TDX API服務。詳細步驟說明如下:

### 1. 註冊為TDX會員
於<a href="https://tdx.transportdata.tw/register" target="_blank">TDX官網</a>註冊為TDX會員，完成Email驗證、帳號經管理員審核後即可登入TDX網站。

### 2. 取得API金鑰 
登入TDX網站後，於<a href="https://tdx.transportdata.tw/user/dataservice/key" target="_blank">TDX會員中心</a>取得API金鑰(包含Client Id和Client Secret)，可視開發測試需求自行建立多組API金鑰(至多3組)。

### 3. 取得取得Access Token
取得token的url固定為 https<nolink>://tdx.transportdata.tw/auth/realms/TDXConnect/protocol/openid-connect/token ，使用HTTP POST方法、帶入Client Id和Client Secret進行驗證以取得Access token。以下為curl範例:
```
curl --request POST \
     --url 'https://tdx.transportdata.tw/auth/realms/TDXConnect/protocol/openid-connect/token' \
     --header 'content-type: application/x-www-form-urlencoded' \
     --data grant_type=client_credentials \
     --data client_id=YOUR_CLIENT_ID \
     --data client_secret=YOUR_CLIENT_SECRET \
```
data參數說明如下:

| 參數 | 描述 |
| ------ | ------ |
| grant_type | 固定使用"client_credentials" |
| client_id | 您的Client Id，可從TDX會員中心取得 |
| client_secret | 您的Client Secret，可從TDX會員中心取得 |

若帶入的參數正確，可收到HTTP 200回應與訊息:
```
{
    "access_token": "eyJh...",
    "expires_in": 86400,
    "token_type": "Bearer",
    (...省略其他內容)
}
```
回傳訊息欄位說明如下:
| 參數 | 描述 |
| ------ | ------ |
| access_token | 用於存取API服務的token，格式為JWT |
| expires_in | token的有效期限，單位為秒，預設為86400秒(1天) |
| token_type | token類型，固定為"Bearer" |


### 4. 呼叫TDX API服務
將第三步驟取得的Access Token帶入HTTP Header，呼叫TDX API。curl範例如下:
```
curl --request GET \
     --url TDX_API_URI \
     --header 'authorization: Bearer ACCESS_TOKEN'
```

### 5. 重新取得Access Token
待Access Token產生之後，若時間超過有效期限(第三步驟收到回應中的expires_in參數)，需使用Client Id和Client Secret重新取得Access Token。



