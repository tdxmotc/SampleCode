<?php
// 取得 Access Token
$client_id = 'XXXXXXXXXX-XXXXXXXX-XXXX-XXXX';
$client_secret = 'XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX';

$ch = curl_init();
curl_setopt($ch, CURLOPT_URL, 'https://tdx.transportdata.tw/auth/realms/TDXConnect/protocol/openid-connect/token');
curl_setopt($ch, CURLOPT_POST, 1);
curl_setopt($ch, CURLOPT_POSTFIELDS, 'grant_type=client_credentials&client_id='.$client_id.'&client_secret='.$client_secret);
curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
$result = curl_exec($ch);
curl_close($ch);

$access_token = json_decode($result,1)['access_token'];

// 測試：取得新北市公車到站資料
$ch = curl_init();
curl_setopt($ch, CURLOPT_URL, 'https://tdx.transportdata.tw/api/basic/v2/Bus/EstimatedTimeOfArrival/City/NewTaipei?$top=30&$format=JSON');
curl_setopt($ch, CURLOPT_HTTPHEADER, array('authorization: Bearer '.$access_token));
curl_setopt($ch, CURLOPT_SSL_VERIFYPEER, 0);
curl_setopt($ch, CURLOPT_RETURNTRANSFER, 1);
$busEstimatedTime = curl_exec($ch);
curl_close($ch);
print_r($busEstimatedTime);
