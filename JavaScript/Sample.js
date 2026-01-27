let accesstokenEl;
let apireponseEl;

document.addEventListener("DOMContentLoaded", async () => {
    accesstokenEl = document.getElementById("accesstoken");
    apireponseEl = document.getElementById("apireponse");
    await GetAuthorizationHeader();
    await GetApiResponse();
});

async function GetAuthorizationHeader() {
    const parameter = {
        grant_type: "client_credentials",
        client_id: "XXXXXXXXXX-XXXXXXXX-XXXX-XXXX",
        client_secret: "XXXXXXXX-XXXX-XXXX-XXXX-XXXXXXXXXXXX"
    };

    const authUrl = "https://tdx.transportdata.tw/auth/realms/TDXConnect/protocol/openid-connect/token";

    try {
        const body = new URLSearchParams(parameter);
        const response = await fetch(authUrl, {
            method: "POST",
            headers: {
                "Accept-Encoding": "br,gzip",
                "Content-Type": "application/x-www-form-urlencoded"
            },
            body
        });

        if (!response.ok) {
            throw new Error(`Auth request failed: ${response.status} ${response.statusText}`);
        }

        const data = await response.json();
        accesstokenEl.textContent = JSON.stringify(data);
    } catch (error) {
        console.error("Auth error:", error);
    }
}

async function GetApiResponse() {
    const accesstokenStr = accesstokenEl.textContent;

    const accesstoken = JSON.parse(accesstokenStr);

    if (accesstoken != undefined) {
        try {
            const response = await fetch("https://tdx.transportdata.tw/api/basic/v2/Rail/TRA/LiveTrainDelay?$top=30&$format=JSON", {
                method: "GET",
                headers: {
                    "authorization": "Bearer " + accesstoken.access_token,
                    "Accept-Encoding": "br,gzip"
                }
            });

            if (!response.ok) {
                throw new Error(`API request failed: ${response.status} ${response.statusText}`);
            }

            const data = await response.json();
            apireponseEl.textContent = JSON.stringify(data);
            console.log("Data", data);
        } catch (error) {
            console.error("API error:", error);
        }
    }
}
