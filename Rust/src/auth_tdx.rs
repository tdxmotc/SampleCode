use std::{collections::HashMap, fs::File, path::Path};
use reqwest::*;
use std::error::Error;
use core::result::Result;
use json::*;

static AUTH_URL: &str = "https://tdx.transportdata.tw/auth/realms/TDXConnect/protocol/openid-connect/token";
static TEST_URL: &str = "https://tdx.transportdata.tw/api/basic/v2/Rail/TRA/LiveTrainDelay?$top=30&$format=JSON";

#[tokio::main]
async fn main() -> Result<(), Box<dyn Error + Send + Sync>> {
    //可以用處純在其他目錄裡的JSON檔案保護使用者的Access Token
    let file_path = Path::new("C:\\Users\\bookw\\Downloads\\tdx-secret.json"); 
    //let file_path = Path::new("C:\\Users\\..\\sample-secret.json"); 
    let file = File::open(file_path).expect("file not found"); 
    let secret: HashMap<String, String> = serde_json::from_reader(file).expect("error while reading");

    let auth_header  = object!{
        "method": "post",
        "headers": {
            "content-type": "application/x-www-form-urlencoded"
        },
        "payload": {
            "grant_type": "client_credentials",
            "client_id": null,
            "client_secret": null
        }
    }; 
    auth_header["client_id"] = secret.get("client_id").unwrap();
    auth_header["client_secret"] = secret.get("client_secret").unwrap();

    let client = Client::new();
    let auth_response = client.post(AUTH_URL).json(&stringify(auth_header)).send().await?;

    let mut data_header = auth_header.get("access_token");

    print!("{:?}", data_header);

    //let data_response = client.get(TEST_URL).bearer_auth(token);

    Ok(())
}


