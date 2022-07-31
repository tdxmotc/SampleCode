package tdxlib

import (
	"errors"
	"sync"
	"time"
)

// updateBufferDuration token 要失效前的更新緩衝時間
const updateBufferDuration = 1 * time.Hour

// authFailedRetryInterval token 更新失敗時，重新嘗試的間隔時間
const authFailedRetryInterval = 3 * time.Minute

// TDXController TDX Controller
//
// 取得 authorization 與自動更新 access token
type TDXController struct {
	tkRWMute      sync.RWMutex
	cID           string // Client Id
	cSEC          string // Client Secret
	authorization string // authorization header value
	refreshTimer  *time.Timer
}

// NewTDXController 新建一個 TDXController
//
// 取得 Access Token 失敗時，會直接回傳錯誤訊息
func NewTDXController(cID, cSEC string) (resTC *TDXController, err error) {
	tc := &TDXController{
		cID:  cID,
		cSEC: cSEC,
	}

	var refreshAfter time.Duration
	refreshAfter, err = tc.getAccessToken()
	if err != nil {
		err = errors.New("init auth failed, err= " + err.Error())
		return
	}
	tc.setUpdateTimer(refreshAfter)

	resTC = tc
	return
}

// GetAuthorization 取得 authorization
func (tc *TDXController) GetAuthorization() (authorization string) {
	tc.tkRWMute.RLock()
	authorization = tc.authorization
	tc.tkRWMute.RUnlock()
	return
}
