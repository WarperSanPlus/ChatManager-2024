$(() => {
    //alert("Notifications Handler installed");
    Notification.requestPermission().then((permission) => {
        if (permission === "granted") {
            setInterval(function () {
                $.ajax({
                    url: "/Notifications/Pop",
                    success: messages => {
                        messages.forEach((message) => {
                            var icon = "../Content/UI-Icons/chatRoom.png";
                            var title = "ChatManager";
                            var body = message;
                            new Notification(title, { body, icon });
                            console.log(message)
                        });
                    }
                })
            }, 2000);
        }
    });
})