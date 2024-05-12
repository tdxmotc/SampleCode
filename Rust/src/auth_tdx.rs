use core::result::Result;
use reqwest::{header::CONTENT_TYPE, *};
use serde_json::*;
use std::error::Error;
use std::{collections::HashMap, fs::File, path::Path};

static AUTH_URL: &str = "https://tdx.transportdata.tw/auth/realms/TDXConnect/protocol/openid-connect/token";
//static TEST_URL: &str = "https://tdx.transportdata.tw/api/basic/v2/Rail/TRA/LiveTrainDelay?$top=30&$format=JSON";

#[tokio::main]
async fn main() -> Result<(), Box<dyn Error + Send + Sync>> {
    //可以用處純在其他目錄裡的JSON檔案保護使用者的Access Token
    let file_path = Path::new("C:\\Users\\bookw\\Downloads\\tdx-secret.json");
    //let file_path = Path::new("C:\\Users\\..\\sample-secret.json");
    let file = File::open(file_path).expect("file not found");
    let secret: HashMap<String, String> =
        serde_json::from_reader(file).expect("error while reading");

    let auth_header = json!({
        "grant_type": "client_credentials",
        "client_id": secret.get("client_id").unwrap(),
        "client_secret": secret.get("client_secret").unwrap()
    });

    let client = Client::new();
    let auth_response = client
        .post(AUTH_URL)
        .header(CONTENT_TYPE, "application/x-www-form-urlencoded")
        .form(&auth_header)
        .send()
        .await?;

    let data_header = auth_response.text().await?;
    let access_token = data_header.contains("access_token");

    println!("{:?}", access_token);

    //let data_response = client.get(TEST_URL).bearer_auth(access_token);

    Ok(())
}
