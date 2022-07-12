library(httr)
library(xml2)


####################
#### 介接程式碼 ####
####################

#---取得 Access Token---#
x=POST("https://tdx.transportdata.tw/auth/realms/TDXConnect/protocol/openid-connect/token",
       encode="form",
       body=list(
         grant_type="client_credentials",
         client_id="YOUR_CLIENT_ID",
         client_secret="YOUR_CLIENT_SECRET"))
access_token=content(x)$access_token



#---介接資料範例---#
# 介接臺鐵站點
url="https://tdx.transportdata.tw/api/basic/v2/Rail/TRA/Station?&%24format=XML"
x=GET(url, add_headers(Accept="application/+json", Authorization=paste("Bearer", access_token)))
x=read_xml(x)

TRA_station=data.frame(StationName=xml_text(xml_find_all(x, xpath = ".//d1:StationName//d1:Zh_tw")),
                       StationUID=xml_text(xml_find_all(x, xpath = ".//d1:StationUID")),
                       StationID=xml_text(xml_find_all(x, xpath = ".//d1:StationID")),
                       LocationCity=xml_text(xml_find_all(x, xpath = ".//d1:LocationCity")),
                       LocationTown=xml_text(xml_find_all(x, xpath = ".//d1:LocationTown")),
                       LocationTownCode=xml_text(xml_find_all(x, xpath = ".//d1:LocationTownCode")),
                       PositionLon=xml_text(xml_find_all(x, xpath = ".//d1:PositionLon")),
                       PositionLat=xml_text(xml_find_all(x, xpath = ".//d1:PositionLat")),
                       StationClass=xml_text(xml_find_all(x, xpath = ".//d1:StationClass")))




####################
#### R TDX 套件 ####
####################

#---使用 R TDX 套件---#
# 安裝devtools套件
install.packages(devtools)

# 自GitHub下載TDX套件
devtools::install_github("ChiaJung-Yeh/NYCU_TDX")

# 載入TDX套件
library(TDX)

# 取得 Access Token (輸入 Client Id 與 Client Secret 兩參數)
access_token=get_token(client_id="YOUR_CLIENT_ID", client_secret="YOUR_CLIENT_SECRET")

# 介接臺鐵站點
Rail_Station(access_token, "TRA")

# 關於 R TDX 套件，詳見以下網站
# https://chiajung-yeh.github.io/TDX_Guide/

