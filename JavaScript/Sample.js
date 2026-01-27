let accessTokenEl;
let apiResponseEl;

document.addEventListener("DOMContentLoaded", async () => {
    accessTokenEl = document.getElementById("accesstoken");
    apiResponseEl = document.getElementById("apireponse");
    await getAuthorizationHeader();
    await getApiResponse();
});

async function getAuthorizationHeader() {
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
        accessTokenEl.textContent = JSON.stringify(data);
    } catch (error) {
        console.error("Auth error:", error);
    }
}

async function getApiResponse() {
    const accessTokenStr = accessTokenEl.textContent;

    const accessToken = JSON.parse(accessTokenStr);

    if (accessToken != undefined) {
        try {
            const response = await fetch("https://tdx.transportdata.tw/api/basic/v2/Rail/TRA/LiveTrainDelay?$top=30&$format=JSON", {
                method: "GET",
                headers: {
                    "authorization": "Bearer " + accessToken.access_token,
                    "Accept-Encoding": "br,gzip"
                }
            });

            if (!response.ok) {
                throw new Error(`API request failed: ${response.status} ${response.statusText}`);
            }

            const data = await response.json();
            apiResponseEl.textContent = JSON.stringify(data);
            console.log("Data", data);
        } catch (error) {
            console.error("API error:", error);
        }
    }
}
