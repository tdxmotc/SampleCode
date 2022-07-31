package tdxlib

import (
	"encoding/json"
	"fmt"
	"io"
	"io/ioutil"
	"net/http"
	"net/url"
	"strings"
	"time"
)

// genAuthReqData 生成 access token 所需要傳送的 data
func genAuthReqData(cID, cSec string) (bodyReader io.Reader) {
	data := url.Values{}
	data.Set("grant_type", "client_credentials")
	data.Set("client_id", cID)
	data.Set("client_secret", cSec)

	bodyReader = strings.NewReader(data.Encode())
	return
}

// authRespInfo TDX 回傳 token 的結構
type authRespInfo struct {
	AccessToken string `json:"access_token"` // Bearer Token
	ExpiresIn   int    `json:"expires_in"`   // token 的有效時間(秒數)
}

// decodeAuthResp 解析請求授權的回傳資料
func decodeAuthResp(respBody io.ReadCloser) (arInfo authRespInfo, err error) {
	defer respBody.Close()

	jDecoder := json.NewDecoder(respBody)
	err = jDecoder.Decode(&arInfo)
	return
}

// genAccessToken 取得 TDX 的 Token
func (tc *TDXController) getAccessToken() (refreshAfter time.Duration, err error) {
	// 準備 http.Request
	req, _ := http.NewRequest("POST",
		"https://tdx.transportdata.tw/auth/realms/TDXConnect/protocol/openid-connect/token",
		genAuthReqData(tc.cID, tc.cSEC))
	req.Header.Add("content-type", "application/x-www-form-urlencoded")

	// 傳送請求
	hc := &http.Client{}
	resp, err := hc.Do(req)
	if err != nil {
		return
	}
	if resp.StatusCode != http.StatusOK {
		failedMsgBytes, _ := ioutil.ReadAll(resp.Body)
		err = fmt.Errorf("%d %s", resp.StatusCode, failedMsgBytes)
		return
	}

	// 解析回傳資料
	var arInfo authRespInfo
	arInfo, err = decodeAuthResp(resp.Body)
	if err != nil {
		return
	}

	// 更新 authorization
	tc.Lock()
	tc.authorization = "Bearer " + arInfo.AccessToken
	refreshAfter = time.Duration(arInfo.ExpiresIn)*time.Second - updateBufferDuration
	tc.Unlock()
	return
}

// setUpdateTimer 設定下次更新的 timer
func (tc *TDXController) setUpdateTimer(refreshAfter time.Duration) {
	tc.refreshTimer = time.AfterFunc(refreshAfter, tc.updateAccessToken)
}

// updateAccessToken 更新 access token，並自動設定下次更新的排程
func (tc *TDXController) updateAccessToken() {
	refreshAfter, err := tc.getAccessToken()
	if err != nil {
		refreshAfter = authFailedRetryInterval
	}

	// 設定下次更新時間
	tc.setUpdateTimer(refreshAfter)
}
