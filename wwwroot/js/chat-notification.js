document.addEventListener("DOMContentLoaded", () => {
    const badgeElement = document.getElementById("chatNotificationBadge");
    const recordSeparator = String.fromCharCode(0x1e);

    let socket = null;
    let invocationId = 0;

    if (!badgeElement) {
        return;
    }

    const updateBadge = (count) => {
        const unreadCount = Number(count) || 0;
        badgeElement.textContent = unreadCount;
        badgeElement.classList.toggle("is-visible", unreadCount > 0);
    };

    const sendHubMessage = (target, args) => {
        if (!socket || socket.readyState !== WebSocket.OPEN) {
            return;
        }

        const payload = {
            type: 1,
            invocationId: String(++invocationId),
            target,
            arguments: args
        };

        socket.send(JSON.stringify(payload) + recordSeparator);
    };

    const handleHubPayload = (payload) => {
        if (!payload || payload.type !== 1) {
            return;
        }

        if (payload.target === "UnreadChatCountUpdated") {
            const [count] = payload.arguments ?? [];
            updateBadge(count);
            return;
        }

        if (payload.target === "ChatRoomUpdated") {
            const [room] = payload.arguments ?? [];

            if (room?.unreadTotalCount !== undefined) {
                updateBadge(room.unreadTotalCount);
            }
        }
    };

    const handleSocketMessage = (event) => {
        const messages = String(event.data)
            .split(recordSeparator)
            .filter(Boolean);

        messages.forEach((rawMessage) => {
            try {
                handleHubPayload(JSON.parse(rawMessage));
            } catch (error) {
                console.error(error);
            }
        });
    };

    const connect = async () => {
        const negotiateResponse = await fetch("/chatHub/negotiate?negotiateVersion=1", {
            method: "POST",
            credentials: "same-origin"
        });

        if (!negotiateResponse.ok) {
            throw new Error("SignalR negotiate request failed.");
        }

        const negotiateData = await negotiateResponse.json();
        const connectionToken = encodeURIComponent(negotiateData.connectionToken);
        const protocol = window.location.protocol === "https:" ? "wss" : "ws";
        const socketUrl = `${protocol}://${window.location.host}/chatHub?id=${connectionToken}`;

        socket = new WebSocket(socketUrl);

        socket.onopen = () => {
            socket.send(JSON.stringify({ protocol: "json", version: 1 }) + recordSeparator);
            sendHubMessage("JoinUserInbox", []);
        };

        socket.onmessage = handleSocketMessage;
        socket.onerror = (event) => console.error(event);
    };

    connect().catch((error) => console.error(error));
});
