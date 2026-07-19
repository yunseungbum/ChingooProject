document.addEventListener("DOMContentLoaded", () => {
    const pageElement = document.querySelector(".chat-list-page[data-current-user-id]");
    const roomListElement = document.getElementById("chatRoomList");
    const emptyElement = document.getElementById("chatListEmpty");
    const recordSeparator = String.fromCharCode(0x1e);

    let socket = null;
    let invocationId = 0;

    if (!pageElement || !roomListElement) {
        return;
    }

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

    const createRoomElement = (room) => {
        const item = document.createElement("a");
        item.className = "chat-room-item";
        item.href = `/Chat/Room/${room.roomId}`;
        item.dataset.roomId = room.roomId;

        const main = document.createElement("div");
        main.className = "chat-room-main";

        const recipient = document.createElement("strong");
        recipient.textContent = room.recipientTeamName ?? "";

        const postTitle = document.createElement("span");
        postTitle.textContent = room.postTitle ?? "";

        const lastMessage = document.createElement("p");
        lastMessage.textContent = room.lastMessage ?? "";

        const time = document.createElement("time");
        time.textContent = room.lastMessageAt ?? "";

        const unread = document.createElement("span");
        unread.className = `chat-room-unread ${room.unreadCount > 0 ? "is-visible" : ""}`;
        unread.textContent = room.unreadCount ?? 0;

        const side = document.createElement("div");
        side.className = "chat-room-side";
        side.append(time, unread);

        main.append(recipient, postTitle, lastMessage);
        item.append(main, side);

        return item;
    };

    const upsertRoom = (room) => {
        if (!room?.roomId) {
            return;
        }

        const existingRoom = roomListElement.querySelector(`[data-room-id="${room.roomId}"]`);
        const nextRoom = createRoomElement(room);

        if (existingRoom) {
            existingRoom.replaceWith(nextRoom);
        } else {
            emptyElement?.remove();
        }

        roomListElement.prepend(nextRoom);
    };

    const removeRoom = (roomId) => {
        const roomElement = roomListElement.querySelector(`[data-room-id="${roomId}"]`);
        roomElement?.remove();
    };

    const handleHubPayload = (payload) => {
        if (!payload || payload.type !== 1) {
            return;
        }

        if (payload.target === "ChatRoomUpdated") {
            const [room] = payload.arguments ?? [];
            upsertRoom(room);
            return;
        }

        if (payload.target === "ChatRoomRemoved") {
            const [roomId] = payload.arguments ?? [];
            removeRoom(roomId);
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

        socket.onerror = (event) => {
            console.error(event);
        };
    };

    connect().catch((error) => {
        console.error(error);
    });
});
