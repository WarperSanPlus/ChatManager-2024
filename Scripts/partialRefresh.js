let EndSessionAction = '/Accounts/Login';
class PartialRefresh {
    constructor(serviceURL, container, refreshRate, postRefreshCallback = null, instantRefresh = true) {
        this.serviceURL = serviceURL;
        this.container = container;
        this.postRefreshCallback = postRefreshCallback;
        this.refreshRate = refreshRate * 1000;
        this.paused = false;
        this.data = {};

        if (instantRefresh)
            this.refresh(true);

        setInterval(() => { this.refresh() }, this.refreshRate);
    }
    static setTimeOutPage(page) {
        timeOutPage = page;
    }
    pause() { this.paused = true }
    restart() { this.paused = false }
    replaceContent(htmlContent) {
        if (htmlContent !== "") {
            $("#" + this.container).html(htmlContent);
            if (this.postRefreshCallback != null) this.postRefreshCallback();
        }
    }
    replaceData(data = {}) {
        if (data == null)
            data = {};

        this.data = data;
    }
    refresh(forced = false, data = null) {
        if (this.paused)
            return;

        if (data != null)
            this.data = data;

        this.data.forceRefresh = forced;

        let url = this.serviceURL;

        if (url.indexOf("?") == -1)
            url += "?";

        for (const [key, value] of Object.entries(this.data))
            url += `${key}=${value}&`;

        $.ajax({
            url: url,
            dataType: "html",
            success: (htmlContent) => { this.replaceContent(htmlContent) },
            statusCode: {
                403: function () {
                    if (EndSessionAction != "")
                        window.location = EndSessionAction + "?message=Compte bloqué";
                    else
                        alert("Illegal access!");
                }
            }
        });
    }
    command(url, moreCallBack = null) {
        $.ajax({
            url: url,
            method: 'GET',
            success: () => {
                this.refresh(true);
                if (moreCallBack != null)
                    moreCallBack();
            }
        });
    }
    commandPost(url, data = null, callback = null) {
        $.ajax({
            url: url,
            type: 'POST',
            data: data,
            success: () => {
                this.refresh(true);
                if (callback != null)
                    callback();
            }
        });
    }

    confirmedCommand(message, url, moreCallBack = null) {
        bootbox.confirm(message, (result) => { if (result) this.command(url, moreCallBack) });
    }
}