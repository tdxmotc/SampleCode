use core::result::Result;
use reqwest::header::AUTHORIZATION;
use reqwest::{header::CONTENT_TYPE, *};
use serde_json::*;
use std::error::Error;

static AUTH_URL: &str = "https://tdx.transportdata.tw/auth/realms/TDXConnect/protocol/openid-connect/token";
static TEST_URL: &str = "https://tdx.transportdata.tw/api/basic/v2/Rail/TRA/LiveBoard/Station/1000?$filter=Direction eq 1&$format=JSON";

static CLIENT_ID: &str = "XXXXX-XXXXXXXX-XXXX-XXXX";
static CLIENT_SECRET: &str = "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX";

#[tokio::main]
async fn main() -> Result<(), Box<dyn Error + Send + Sync>> {
    let auth_header = json!({
        "grant_type": "client_credentials",
        "client_id": CLIENT_ID,
        "client_secret": CLIENT_SECRET,
    });

    let client = Client::new();
    let auth_response = client
        .post(AUTH_URL)
        .header(CONTENT_TYPE, "application/x-www-form-urlencoded")
        .form(&auth_header)
        .send()
        .await?
        .text()
        .await?;

    let data_header = auth_response.split_once("\":\"").unwrap().1;
    let access_token = format!("Bearer {}", data_header.split_once("\",").unwrap().0);

    let data_response = client
        .get(TEST_URL)
        .header(AUTHORIZATION, access_token)
        .send()
        .await?
        .text()
        .await?;

    println!("{:?}", data_response);

    Ok(())
}
