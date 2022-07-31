package main

import (
	"context"
	"encoding/json"
	"fmt"
	"io/ioutil"
	"log"
	"net/http"
	"net/url"
	"os"
	"tdx-sample/tdxlib"
)

func main() {
	if len(os.Args) < 3 {
		log.Fatal("請指定 client ID 和 client Secret")
	}

	cID := os.Args[1]  // client id
	cSEC := os.Args[2] // client secret

	tdxCtrl, err := tdxlib.NewTDXController(cID, cSEC)
	if err != nil {
		// 無法成功取得 Access Token
		log.Fatal(err)
	}

	// 準備 API 的請求資料
	const sampleAPIURL string = "https://tdx.transportdata.tw/api/basic/v3/Rail/TRA/Station"
	queryParams := url.Values{}
	queryParams.Set("$top", "3")
	queryParams.Set("$format", "JSON")

	client := &http.Client{}
	req, err := http.NewRequestWithContext(context.Background(),
		"GET", sampleAPIURL+"?"+queryParams.Encode(), http.NoBody)
	if err != nil {
		log.Fatal(err)
	}
	authorization := tdxCtrl.GetAuthorization()
	req.Header.Set("Authorization", authorization) // 設定 bearer token

	// 發送 API 請求
	rsp, err := client.Do(req)
	if err != nil {
		log.Fatal(err)
	} else if rsp.StatusCode != http.StatusOK {
		failedMsg, _ := ioutil.ReadAll(rsp.Body)
		log.Fatalf("%d %s", rsp.StatusCode, failedMsg)
	}

	// 解析 API 到預定好的 struct
	// 為求方便使用 map[string]interface{} 做示範
	var tmpStruct = map[string]interface{}{}
	jDecoder := json.NewDecoder(rsp.Body)
	err = jDecoder.Decode(&tmpStruct)
	if err != nil {
		rsp.Body.Close()
		log.Fatal(err)
	}
	rsp.Body.Close()

	// 顯示成功取得的資料
	jEncoder := json.NewEncoder(os.Stdout)
	jEncoder.SetIndent("", "\t")
	_ = jEncoder.Encode(tmpStruct)

	fmt.Println("\nsuccess : )")
}
