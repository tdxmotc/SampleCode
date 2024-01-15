# TDX運輸資料流通服務API介接範例程式碼說明


為使開發者能快速在M2M環境下介接使用TDX運輸資料流通服務平臺之交通領域資料服務API，在此提供數種程式語言的範例程式碼提供開發者做參考。

## API使用次數限制
TDX API呼叫頻率限制已於2024/01/15改為 **每把API金鑰每秒最多呼叫50次**。  
未來導入訂閱機制時，將依會員不同的訂閱等級而有不同的限制次數。  

## API認證機制
TDX API皆使用OIDC Client Credentials流程進行身份認證，認證完成後即取得Access Token，將Access Token帶入即可存取TDX API服務。詳細步驟說明如下:

### 1. 註冊為TDX會員
於<a href="https://tdx.transportdata.tw/register" target="_blank">TDX官網</a>註冊為TDX會員，完成Email驗證、帳號經管理員審核後即可登入TDX網站。

### 2. 取得API金鑰 
登入TDX網站後，於<a href="https://tdx.transportdata.tw/user/dataservice/key" target="_blank">TDX會員中心</a>取得API金鑰(包含Client Id和Client Secret)，可視開發測試需求自行建立多組API金鑰(目前開放至多3組)。

### 3. 取得Access Token
取得Access Token的API為**https<nolink>://tdx.transportdata.tw/auth/realms/TDXConnect/protocol/openid-connect/token**，使用 **HTTP POST** 方法、帶入Client Id和Client Secret進行驗證以取得Access token。以下為curl範例:     
```
curl --request POST \
     --url 'https://tdx.transportdata.tw/auth/realms/TDXConnect/protocol/openid-connect/token' \
     --header 'content-type: application/x-www-form-urlencoded' \
     --data grant_type=client_credentials \
     --data client_id=YOUR_CLIENT_ID \
     --data client_secret=YOUR_CLIENT_SECRET \
```
參數說明如下:

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

待Access Token產生之後，若時間超過有效期限(expires_in參數)，需使用Client Id和Client Secret重新取得Access Token。  
您可在多個服務內同時使用同一組Client Id和Client Secret取得Access Token，新取得的token不會影響已存在token的有效性或時效性。
     
提醒您，若每次呼叫API時都重新取得Access Token，此作法將會提升程式端與TDX環境的網路與運算資源的消耗。   
為了讓平台運算資源能更有效與公平的被使用，TDX已於2023/05/15開始限制 **Access Token API** 可存取次數: **每個IP來源每分鐘最多呼叫20次**。  
     
建議您可在程式端實作Access Token快取機制，方法如下:      
  - 方法1: 程式實作定期自動重新取得Token機制，如程式每4小時或6小時重取一次Access Token，每次呼叫API時皆使用該Token。  
  - 方法2: 程式初始化時取得Token，每次呼叫API時帶入該Token，若發現Token過期再重新取得Token。  

**若有任何Access Token API使用方法或Token快取機制的問題，可於Issues留下您的意見，我們將盡快回覆您。**

### 4. 呼叫TDX API服務
呼叫TDX API時將取得的Access Token帶入HTTP Authorization Bearer Header。curl範例如下:
```
curl --request GET \
     --url TDX_API_URI \
     --header 'authorization: Bearer ACCESS_TOKEN'
```
     
呼叫API時，可視需求加入Accept-Encoding HTTP Header，可有效減少資料回傳量。呼叫歷史資料類型API時，使用此設定將可大幅降低資料傳輸時間。使用方式如下:
```
Accept-Encoding: br,gzip
```
     
## 其他說明
     
### 傳輸加密通訊協定支援狀況  
由於TLS 1.0與TLS 1.1已被證實具有安全風險，為確保網路連線機制的安全性，TDX API與網站僅支援TLS 1.2(含)以上之傳輸加密協定。  
TLS與Ciphers支援狀況可參考SSL Labs工具檢測後的結果:  

![image](https://user-images.githubusercontent.com/44422898/198218706-9dc8baf2-d8aa-44af-be4e-261e8f576ccb.png)
