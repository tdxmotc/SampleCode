# Golang 的 TDX API 介接範例

使用內建套件實作取得 access token 並自動更新的簡單範例

## Go 版本

需要 1.11 以上的版本，1.10 以下因為沒有 go module 需要再自行做調整 

## 執行範例

### 下載程式碼

```bash
git clone https://github.com/tdxmotc/SampleCode.git
cd SampleCode/Golang
```

### 編譯

執行後會在目錄下生成 tdx-sample 執行檔

```bash
go build
```

### 執行

此範例會呼叫查詢台鐵車站的 API (/v3/Rail/TRA/Station)，請將 `<your client id>` 和 `<your client secret>` 分別帶入你的驗證資料

```bash
./tdx-sample <your client id> <your client secret>
```